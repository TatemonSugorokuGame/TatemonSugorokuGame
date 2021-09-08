//---------------------------------------------------------------------------------------------------------
// ▽ Submarine Mirage Framework for Unity
//		Copyright (c) 2020 夢想海の水底より(from Seabed of Reverie)
//		Released under the MIT License :
//			https://github.com/FromSeabedOfReverie/SubmarineMirageFrameworkForUnity/blob/master/LICENSE
//---------------------------------------------------------------------------------------------------------
#if PHOTON_UNITY_NETWORKING
namespace SubmarineMirage.Network {
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using Cysharp.Threading.Tasks;
	using UniRx;
	using Photon.Realtime;
	using Photon.Pun;
	using KoganeUnityLib;
	using FSM;
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
		public SMFSM _masterFSM	{ get; private set; }
		public SMFSM _roomFSM	{ get; private set; }
		public SMPhotonMasterState _masterState	=> _masterFSM._state as SMPhotonMasterState;
		public SMPhotonRoomState _roomState		=> _roomFSM._state as SMPhotonRoomState;

		/// <summary>接続の型</summary>
		public SMGameServerType _type => _masterState?._type ?? SMGameServerType.Disconnect;

		/// <summary>プレイヤー名</summary>
		public string _playerName {
			get => PhotonNetwork.NickName;
			set => PhotonNetwork.NickName = value;
		}

		/// <summary>プレイヤー数のイベント</summary>
		public ReactiveProperty<int> _playerCountEvent	{ get; private set; } = new ReactiveProperty<int>();
		/// <summary>部屋一覧のイベント</summary>
		public ReactiveProperty< List<SMGameServerRoom> > _roomsEvent	{ get; private set; }
			= new ReactiveProperty< List<SMGameServerRoom> >();
		/// <summary>失敗のイベント</summary>
		public ReactiveProperty<string> _errorEvent		{ get; private set; } = new ReactiveProperty<string>();

		/// <summary>全て接続完了か？</summary>
		public bool _isConnected => _roomState._status == SMGameServerStatus.Connect;
		/// <summary>サーバーか？</summary>
		public bool _isServer => PhotonNetwork.IsMasterClient;	// Photonの場合、マスタークライアントか？
		/// <summary>活動中か？</summary>
		public bool _isActive {
			// Photonの場合、RPC送信を実行中か？
			get => PhotonNetwork.IsMessageQueueRunning;
			set => PhotonNetwork.IsMessageQueueRunning = value;
		}

		readonly SMAsyncCanceler _canceler = new SMAsyncCanceler();

		///------------------------------------------------------------------------------------------------
		/// ● 作成、削除
		///------------------------------------------------------------------------------------------------
		public static SMPhotonManager Create() {
			var go = new GameObject( nameof( SMPhotonManager ) );
			go.DontDestroyOnLoad();
			var photonObject = go.AddComponent<SMPhotonManager>();

			SMLog.Debug( $"作成 : {nameof( SMPhotonManager )}", SMLogTag.Server );

			return photonObject;
		}

		protected override void Awake() {
			base.Awake();

			_playerName = $"プレイヤー{UnityEngine.Random.Range( 0, 99999999 )}";

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


			_disposables.AddFirst( () => {
				_canceler.Dispose();
				_playerCountEvent.Dispose();
				_roomsEvent.Dispose();
				_errorEvent.Dispose();
				_masterFSM.Dispose();
				_roomFSM.Dispose();
			} );
		}

		///------------------------------------------------------------------------------------------------
		/// ● 接続
		///------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● オンライン接続
		/// </summary>
		public async UniTask<bool> ConnectOnline() {
			if ( !( _masterState is OnlineSMPhotonMasterState ) ) {
				await _masterFSM.ChangeState<OnlineSMPhotonMasterState>();
			}
			return await _masterState.WaitConnect();
		}

		/// <summary>
		/// ● オフライン接続
		/// </summary>
		public async UniTask<bool> ConnectOffline() {
			if ( !( _masterState is OfflineSMPhotonMasterState ) ) {
				await _masterFSM.ChangeState<OfflineSMPhotonMasterState>();
			}
			return await _masterState.WaitConnect();
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
			return await _roomState.WaitConnect();
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
				_errorEvent.Value = $"{e}";
				SMLog.Error( _errorEvent.Value, SMLogTag.Server );
				return false;
			}

			if ( photonRoom.ToToken() != _roomState._room?.ToToken() ) {
				var state = _roomFSM.GetState<CreateRoomSMPhotonRoomState>();
				state._room = photonRoom;
				await _roomFSM.ChangeState<CreateRoomSMPhotonRoomState>();
			}

			return await _roomState.WaitConnect();
		}

		/// <summary>
		/// ● 部屋に入室
		/// </summary>
		public async UniTask<bool> EnterRoom( SMGameServerRoom room ) {
			if ( !( _masterState is OnlineSMPhotonMasterState ) )	{ return false; }

			var photonRoom = room as SMPhotonRoom;

			if ( photonRoom.ToToken() != _roomState._room?.ToToken() ) {
				var state = _roomFSM.GetState<JoinRoomSMPhotonRoomState>();
				state._room = photonRoom;
				await _roomFSM.ChangeState<JoinRoomSMPhotonRoomState>();
			}

			return await _roomState.WaitConnect();
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
			_playerCountEvent.Value += 1;
		}

		/// <summary>
		/// ● プレイヤー退室（呼戻）
		/// </summary>
		public override void OnPlayerLeftRoom( Player otherPlayer ) {
			base.OnPlayerLeftRoom( otherPlayer );
			_playerCountEvent.Value -= 1;
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