//---------------------------------------------------------------------------------------------------------
// ▽ Submarine Mirage Framework for Unity
//		Copyright (c) 2020 夢想海の水底より(from Seabed of Reverie)
//		Released under the MIT License :
//			https://github.com/FromSeabedOfReverie/SubmarineMirageFrameworkForUnity/blob/master/LICENSE
//---------------------------------------------------------------------------------------------------------
#define TestNetwork
#if PHOTON_UNITY_NETWORKING
namespace SubmarineMirage.Network {
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using UnityEngine;
	using Cysharp.Threading.Tasks;
	using UniRx;
	using Photon.Realtime;
	using Photon.Pun;
	using KoganeUnityLib;
	using Service;
	using FSM;
	using Extension;
	using Utility;
	using Setting;
	using Debug;
	///====================================================================================================
	/// <summary>
	/// ■ フォトンの管理クラス
	/// </summary>
	///====================================================================================================
	public class SMPhotonManager : SMMonoBehaviourPunCallbacks, ISMGameServer {
		///------------------------------------------------------------------------------------------------
		/// ● 要素
		///------------------------------------------------------------------------------------------------
		[SMShow] public SMFSM _masterFSM	{ get; private set; }
		[SMShow] public SMFSM _roomFSM	{ get; private set; }
		public SMPhotonMasterState _masterState	=> _masterFSM._state as SMPhotonMasterState;
		public SMPhotonRoomState _roomState		=> _roomFSM._state as SMPhotonRoomState;

		/// <summary>接続の型</summary>
		[SMShow] public SMGameServerType _type => _masterState?._type ?? SMGameServerType.Disconnect;

		/// <summary>プレイヤー名</summary>
		[SMShow] public string _playerName {
			get => PhotonNetwork.NickName;
			private set => PhotonNetwork.NickName = value;
		}

		/// <summary>プレイヤー数のイベント</summary>
		public ReactiveProperty<int> _playerCountEvent	{ get; private set; } = new ReactiveProperty<int>();
		/// <summary>現在の部屋のイベント</summary>
		public Subject<SMGameServerRoom> _currentRoomEvent { get; private set; }
			= new Subject<SMGameServerRoom>();
		/// <summary>部屋一覧のイベント</summary>
		public Subject< List<SMGameServerRoom> > _roomsEvent	{ get; private set; }
			= new Subject< List<SMGameServerRoom> >();
		/// <summary>失敗のイベント</summary>
		public Subject<string> _errorEvent		{ get; private set; } = new Subject<string>();

		/// <summary>全て接続完了か？</summary>
		[SMShow] public bool _isConnect =>
			_roomState._status == SMGameServerStatus.Connect &&
			PhotonNetwork.IsConnectedAndReady;
		/// <summary>サーバーか？</summary>
		[SMShow] public bool _isServer => PhotonNetwork.IsMasterClient;	// Photonの場合、マスタークライアントか？
		/// <summary>活動中か？</summary>
		[SMShow] public bool _isActive {
			// Photonの場合、RPC送信を実行中か？
			get => PhotonNetwork.IsMessageQueueRunning;
			set => PhotonNetwork.IsMessageQueueRunning = value;
		}

		SMDisplayLog _displayLog { get; set; }

		readonly SMAsyncCanceler _canceler = new SMAsyncCanceler();

		///------------------------------------------------------------------------------------------------
		/// ● 作成、削除
		///------------------------------------------------------------------------------------------------
		public static SMPhotonManager Create() {
			var go = new GameObject( nameof( SMPhotonManager ) );
			go.DontDestroyOnLoad();
			var photonObject = go.AddComponent<SMPhotonManager>();
#if TestNetwork
			SMLog.Debug( $"作成 : {nameof( SMPhotonManager )}", SMLogTag.Server );
#endif
			return photonObject;
		}

		protected override void Awake() {
			base.Awake();

			var fsm = SMFSM.Generate(
				this,
				new SMFSMGenerateList {
					{
						new SMPhotonMasterState[] {
							new DisconnectSMPhotonMasterState(),
							new OfflineSMPhotonMasterState(),
							new OnlineSMPhotonMasterState(),
						},
						typeof( SMPhotonMasterState )
					}, {
						new SMPhotonRoomState[] {
							new DisconnectSMPhotonRoomState(),
							new LobbySMPhotonRoomState(),
							new CreateRoomSMPhotonRoomState(),
							new JoinRoomSMPhotonRoomState(),
						},
						typeof( SMPhotonRoomState )
					}
				}
			);
			_masterFSM = fsm.GetFSM<SMPhotonMasterState>();
			_roomFSM = fsm.GetFSM<SMPhotonRoomState>();

			_masterFSM.ChangeState<DisconnectSMPhotonMasterState>().Forget();
			_roomFSM.ChangeState<DisconnectSMPhotonRoomState>().Forget();

			UTask.Void( async () => {
				_displayLog = await SMServiceLocator.WaitResolve<SMDisplayLog>( _canceler );
			} );


			_disposables.AddFirst( () => {
				_canceler.Dispose();
				_playerCountEvent.Dispose();
				_roomsEvent.Dispose();
				_errorEvent.Dispose();
				_masterFSM.Dispose();
				_roomFSM.Dispose();
			} );
		}



#if TestNetwork
		void LateUpdate() {
			// デバッグ表示を設定
			_displayLog.Add( Color.cyan );
			_displayLog.Add( $"● {this.GetAboutName()}" );
			_displayLog.Add( Color.white );

			_displayLog.Add( $"{nameof( _masterState )} : {_masterState.ToLineString()}" );
			_displayLog.Add( $"{nameof( _roomState )} : {_roomState.ToLineString()}" );
			_displayLog.Add( $"{nameof( _isConnect )} : {_isConnect}" );
		}
#endif

		///------------------------------------------------------------------------------------------------
		/// ● 接続
		///------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● オンライン接続
		/// </summary>
		public async UniTask<bool> ConnectOnline( string playerName ) {
			if ( !( _masterState is OnlineSMPhotonMasterState ) ) {
				_playerName = playerName;
				await _masterFSM.ChangeState<OnlineSMPhotonMasterState>();
			}
			if ( _masterState is OnlineSMPhotonMasterState ) {
				return await _masterState.WaitConnect();
			} else {
				return false;
			}
		}

		/// <summary>
		/// ● オフライン接続
		/// </summary>
		public async UniTask<bool> ConnectOffline( string playerName ) {
			if ( !( _masterState is OfflineSMPhotonMasterState ) ) {
				_playerName = playerName;
				await _masterFSM.ChangeState<OfflineSMPhotonMasterState>();
			}
			if ( _masterState is OfflineSMPhotonMasterState ) {
				return await _masterState.WaitConnect();
			} else {
				return false;
			}
		}

		/// <summary>
		/// ● 接続解除
		/// </summary>
		public async UniTask<bool> Disconnect() {
			if ( !( _masterState is DisconnectSMPhotonMasterState ) ) {
				await _masterFSM.ChangeState<DisconnectSMPhotonMasterState>();
			}
			return true;
		}

		/// <summary>
		/// ● 控室に入室
		/// </summary>
		public async UniTask<bool> EnterLobby() {
			if ( !( _masterState is OnlineSMPhotonMasterState ) )	{ return false; }

			if ( !( _roomState is LobbySMPhotonRoomState ) ) {
				await _roomFSM.ChangeState<LobbySMPhotonRoomState>();
			}
			if ( _roomState is LobbySMPhotonRoomState ) {
				return await _roomState.WaitConnect();
			} else {
				return false;
			}
		}

		/// <summary>
		/// ● 部屋を作成
		/// </summary>
		public async UniTask<bool> CreateRoom( string name, string password, int maxPlayerCount ) {
			if ( _masterState is DisconnectSMPhotonMasterState )	{ return false; }

			SMPhotonRoom photonRoom;
			try {
				photonRoom = new SMPhotonRoom( name, password, maxPlayerCount );
			} catch ( Exception e ) {
				var et = $"{e}";
				_errorEvent.OnNext( et );
				SMLog.Error( et, SMLogTag.Server );
				return false;
			}

			if ( photonRoom.ToToken() != _roomState._room?.ToToken() ) {
				var state = _roomFSM.GetState<CreateRoomSMPhotonRoomState>();
				state._room = photonRoom;
				await _roomFSM.ChangeState<CreateRoomSMPhotonRoomState>();
			}
			if ( _roomState is CreateRoomSMPhotonRoomState ) {
				return await _roomState.WaitConnect();
			} else {
				return false;
			}
		}

		/// <summary>
		/// ● 部屋に入室
		/// </summary>
		public async UniTask<bool> EnterRoom( SMGameServerRoom room, string inputPassword ) {
			if ( !( _masterState is OnlineSMPhotonMasterState ) )	{ return false; }

			var photonRoom = room as SMPhotonRoom;

			if ( !photonRoom.IsEqualPassword( inputPassword ) ) {
				var et = $"パスワード不一致 : {inputPassword}";
				_errorEvent.OnNext( et );
				SMLog.Error( et, SMLogTag.Server );
				return false;
			}

			if ( photonRoom.ToToken() != _roomState._room?.ToToken() ) {
				var state = _roomFSM.GetState<JoinRoomSMPhotonRoomState>();
				state._room = photonRoom;
				await _roomFSM.ChangeState<JoinRoomSMPhotonRoomState>();
			}
			if ( _roomState is JoinRoomSMPhotonRoomState ) {
				return await _roomState.WaitConnect();
			} else {
				return false;
			}
		}

		///------------------------------------------------------------------------------------------------
		/// ● 呼戻
		///------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● マスターサーバーに接続成功（呼戻）
		/// </summary>
		public override void OnConnectedToMaster() {
			base.OnConnectedToMaster();
			_masterState.OnConnect();
		}

		/// <summary>
		/// ● マスターサーバーの接続切断（呼戻）
		/// </summary>
		public override void OnDisconnected( DisconnectCause cause ) {
			base.OnDisconnected( cause );
			_masterState.OnDisconnect( cause );
		}


		/// <summary>
		/// ● 控室に参加成功（呼戻）
		/// </summary>
		public override void OnJoinedLobby() {
			base.OnJoinedLobby();
			_roomState.OnConnect();
		}

		/// <summary>
		/// ● 控室から退室成功（呼戻）
		/// </summary>
		public override void OnLeftLobby() {
			base.OnLeftLobby();
			_roomState.OnDisconnect();
		}

		/// <summary>
		/// ● 部屋一覧を更新（呼戻）
		/// </summary>
		public override void OnRoomListUpdate( List<RoomInfo> roomList ) {
			base.OnRoomListUpdate( roomList );
			( _roomState as LobbySMPhotonRoomState ).OnUpdateRoom( roomList );
		}

		/// <summary>
		/// ● 部屋の作成成功（呼戻）
		/// </summary>
		public override void OnCreatedRoom() {
			base.OnCreatedRoom();
			_roomState.OnConnect();
		}

		/// <summary>
		/// ● 部屋の作成失敗（呼戻）
		/// </summary>
		public override void OnCreateRoomFailed( short returnCode, string message ) {
			base.OnCreateRoomFailed( returnCode, message );
			_roomState.OnError( returnCode, message );
		}

		/// <summary>
		/// ● 部屋に参加成功（呼戻）
		/// </summary>
		public override void OnJoinedRoom() {
			base.OnJoinedRoom();
			_roomState.OnConnect();
		}

		/// <summary>
		/// ● 部屋に参加失敗（呼戻）
		/// </summary>
		public override void OnJoinRoomFailed( short returnCode, string message ) {
			base.OnJoinRoomFailed( returnCode, message );
			_roomState.OnError( returnCode, message );
		}

		/// <summary>
		/// ● 部屋から退室成功（呼戻）
		/// </summary>
		public override void OnLeftRoom() {
			base.OnLeftRoom();
			_roomState.OnDisconnect();
		}

		/// <summary>
		/// ● プレイヤー入室（呼戻）
		/// </summary>
		public override void OnPlayerEnteredRoom( Player newPlayer ) {
			base.OnPlayerEnteredRoom( newPlayer );
			OnUpdatePlayer();
		}

		/// <summary>
		/// ● プレイヤー退室（呼戻）
		/// </summary>
		public override void OnPlayerLeftRoom( Player otherPlayer ) {
			base.OnPlayerLeftRoom( otherPlayer );
			OnUpdatePlayer();
		}

		/// <summary>
		/// ● プレイヤー更新（呼戻）
		/// </summary>
		public void OnUpdatePlayer() {
			_playerCountEvent.Value = PhotonNetwork.CurrentRoom?.PlayerCount ?? 0;
		}

		///------------------------------------------------------------------------------------------------
		/// ● ゲーム物の生成、破棄
		///------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● ゲーム物を生成
		/// </summary>
		public GameObject Instantiate( string name, Vector3 position, Quaternion rotation,
										byte group = 0, object[] data = null
		) => PhotonNetwork.Instantiate( name, position, rotation, group, data );

		/// <summary>
		/// ● ゲーム物を破棄
		/// </summary>
		public void Destroy( GameObject gameObject )
			=> PhotonNetwork.Destroy( gameObject );

		///------------------------------------------------------------------------------------------------
		/// ● RPC通信を破棄
		///------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● RPC通信を破棄
		/// </summary>
		public void RemoveRPCs( PhotonView view )
			=> PhotonNetwork.RemoveRPCs( view );

		/// <summary>
		/// ● RPC通信を破棄
		/// </summary>
		public void RemoveRPCs( Player player )
			=> PhotonNetwork.RemoveRPCs( player );
	}
}
#endif