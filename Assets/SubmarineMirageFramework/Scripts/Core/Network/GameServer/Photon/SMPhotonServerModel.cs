//---------------------------------------------------------------------------------------------------------
// ▽ Submarine Mirage Framework for Unity
//		Copyright (c) 2020 夢想海の水底より(from Seabed of Reverie)
//		Released under the MIT License :
//			https://github.com/FromSeabedOfReverie/SubmarineMirageFrameworkForUnity/blob/master/LICENSE
//---------------------------------------------------------------------------------------------------------
//#define TestNetwork
#if PHOTON_UNITY_NETWORKING
namespace SubmarineMirage.Network {
	using System;
	using UnityEngine;
	using Cysharp.Threading.Tasks;
	using UniRx;
	using Photon.Realtime;
	using Photon.Pun;
	using ExitGames.Client.Photon;
	using Service;
	using FSM;
	using Extension;
	using Utility;
	using Setting;
	using Debug;
	///====================================================================================================
	/// <summary>
	/// ■ フォトンサーバーのモデルクラス
	/// </summary>
	///====================================================================================================
	public class SMPhotonServerModel : SMGameServerModel {
		///------------------------------------------------------------------------------------------------
		/// ● 要素
		///------------------------------------------------------------------------------------------------
		/// <summary>管理クラスの、フォトンイベント番号</summary>
		public const byte MANAGER_PHOTON_EVENT_CODE = 1;


		/// <summary>接続の型</summary>
		[SMShow] public override SMGameServerType _type => _masterState?._type ?? SMGameServerType.Disconnect;

		/// <summary>プレイヤー名</summary>
		[SMShow] public override string _playerName {
			get => PhotonNetwork.NickName;
			protected set => PhotonNetwork.NickName = value;
		}
		/// <summary>プレイヤー数</summary>
		[SMShow] public override int _playerCount	=> PhotonNetwork.CurrentRoom?.PlayerCount ?? 0;

		/// <summary>全て接続完了か？</summary>
		[SMShow] public override bool _isConnect =>
			_roomState._status == SMGameServerStatus.Connect &&
			PhotonNetwork.IsConnectedAndReady;
		/// <summary>サーバーか？</summary>
		// Photonの場合、マスタークライアントか？
		[SMShow] public override bool _isServer => PhotonNetwork.IsMasterClient;
		/// <summary>活動中か？</summary>
		[SMShow] public override bool _isActive {
			// Photonの場合、RPC送信を実行中か？
			get => PhotonNetwork.IsMessageQueueRunning;
			set => PhotonNetwork.IsMessageQueueRunning = value;
		}
		/// <summary>鍵部屋か？</summary>
		[SMShow] public override bool _isLockRoom {
			get => _roomState._room?._isLock ?? false;
			set {
				if ( _roomState._room != null ) {
					_roomState._room._isLock = value;
				}
			}
		}

		[SMShow] public SMFSM _masterFSM	{ get; private set; }
		[SMShow] public SMFSM _roomFSM		{ get; private set; }
		public SMPhotonMasterState _masterState	=> _masterFSM._state as SMPhotonMasterState;
		public SMPhotonRoomState _roomState		=> _roomFSM._state as SMPhotonRoomState;

		public SMNetworkManager _networkManager { get; private set; }
		SMDisplayLog _displayLog { get; set; }

		readonly SMAsyncCanceler _canceler = new SMAsyncCanceler();

		///------------------------------------------------------------------------------------------------
		/// ● 作成、削除
		///------------------------------------------------------------------------------------------------
		public SMPhotonServerModel( SMNetworkManager networkManager ) {
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

			_networkManager = networkManager;
			_displayLog = SMServiceLocator.Resolve<SMDisplayLog>();
#if TestNetwork
			// デバッグ表示を設定
			_disposables.AddFirst(
				_networkManager._updateEvent.AddLast().Subscribe( _ => {
					_displayLog?.Add( Color.cyan );
					_displayLog?.Add( $"● {this.GetAboutName()}" );
					_displayLog?.Add( Color.white );

					_displayLog?.Add( $"{nameof( _masterState )} : {_masterState?.ToLineString()}" );
					_displayLog?.Add( $"{nameof( _roomState )} : {_roomState?.ToLineString()}" );
					_displayLog?.Add( $"{nameof( _isConnect )} : {_isConnect}" );
					_displayLog?.Add( $"{nameof( _isServer )} : {_isServer}" );
					_displayLog?.Add( $"{nameof( _isLockRoom )} : {_isLockRoom}" );
				} )
			);
#endif

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
		/// ● ネットワーク接続
		/// </summary>
		public override async UniTask<bool> Connect( bool isOnline, string playerName ) {
			var type = isOnline ? typeof( OnlineSMPhotonMasterState ) : typeof( OfflineSMPhotonMasterState );

// TODO : type比較が、継承を考慮してないかも？
			if ( _masterState.GetType() != type ) {
				_playerName = playerName;
				await _masterFSM.ChangeState( type );
			}
			if ( _masterState.GetType() == type ) {
				return await _masterState.WaitConnect();
			} else {
				return false;
			}
		}

		/// <summary>
		/// ● 接続解除
		/// </summary>
		public override async UniTask<bool> Disconnect() {
			if ( !( _masterState is DisconnectSMPhotonMasterState ) ) {
				await _masterFSM.ChangeState<DisconnectSMPhotonMasterState>();
			}
			return true;
		}

		/// <summary>
		/// ● 控室に入室
		/// </summary>
		public override async UniTask<bool> EnterLobby() {
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
		public override async UniTask<bool> CreateRoom( string name, string password, int maxPlayerCount ) {
			if ( _masterState is DisconnectSMPhotonMasterState )	{ return false; }

			SMPhotonRoom photonRoom;
			try {
				photonRoom = new SMPhotonRoom( name, password, maxPlayerCount );
			} catch ( GameServerSMException e ) {
				_errorEvent.OnNext( e );
				SMLog.Error( e, SMLogTag.Server );
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
		public override async UniTask<bool> EnterRoom( SMGameServerRoom room, string inputPassword ) {
			if ( !( _masterState is OnlineSMPhotonMasterState ) )	{ return false; }

			SMPhotonRoom photonRoom;
			try {
				photonRoom = room as SMPhotonRoom;
				if ( !photonRoom.IsEqualPassword( inputPassword ) ) {
					throw new GameServerSMException(
						SMGameServerErrorType.MismatchPassword,
						null,
						string.Join( "\n",
							$"{this.GetAboutName()}.{nameof( EnterRoom )} : パスワード不一致",
							$"{nameof( inputPassword )} : {inputPassword}"
						),
						false
					);
				}
				if ( photonRoom.IsFull() ) {
					throw new GameServerSMException(
						SMGameServerErrorType.FullRoom,
						null,
						string.Join( "\n",
							$"{this.GetAboutName()}.{nameof( EnterRoom )} : 部屋が満室",
							$"{nameof( room._maxPlayerCount )} : {room._maxPlayerCount}"
						),
						false
					);
				}
			} catch ( GameServerSMException e ) {
				_errorEvent.OnNext( e );
				SMLog.Error( e, SMLogTag.Server );
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
		/// ● 情報を送受信
		///------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● サーバーに情報送信
		/// </summary>
		public override void Send<T>( SMGameServerSendTarget target, T data,
										SMGameServerSendType sendType = SMGameServerSendType.Reliable
		) {
			data._sendSeconds = 0;
			data._sender = null;
			data._view = null;
			var rawData = new object[] {
				typeof( T ).FullName,
				SerializerSMUtility.Serialize( data ),
			};

			var option = new RaiseEventOptions();
			switch ( target._type ) {
				case SMGameServerSendTargetType.All:
					option.Receivers = ReceiverGroup.All;
					break;
				case SMGameServerSendTargetType.AllByNetwork:
					if ( SMDebugManager.IS_DEVELOP ) {
						SMLog.Warning(
							string.Join( "\n",
								$"{this.GetAboutName()}.{nameof( Send )} : 変換を妥協",
								$"未対応 : {SMGameServerSendTargetType.AllByNetwork}",
								$"矯正 : {SMGameServerSendTargetType.All}"
							),
							SMLogTag.Server
						);
					}
					option.Receivers = ReceiverGroup.All;
					break;
				case SMGameServerSendTargetType.Server:
					option.Receivers = ReceiverGroup.MasterClient;
					break;
				case SMGameServerSendTargetType.Other:
					option.Receivers = ReceiverGroup.Others;
					break;
			}
			option.CachingOption = (
				target._isKeep	? EventCaching.AddToRoomCache
								: EventCaching.DoNotCache
			);
			var sendOption = (
				sendType == SMGameServerSendType.Reliable	? SendOptions.SendReliable
															: SendOptions.SendUnreliable
			);

// TODO : オフラインでも、ちゃんと呼ばれる？
			PhotonNetwork.RaiseEvent( MANAGER_PHOTON_EVENT_CODE, rawData, option, sendOption );
		}

		/// <summary>
		/// ● サーバーから情報受信
		/// </summary>
		public void Receive( EventData photonEvent ) {
			if ( photonEvent.Code != MANAGER_PHOTON_EVENT_CODE )	{ return; }

			var rawData = photonEvent.CustomData as object[];
			var type = Type.GetType( rawData[0] as string );
			var data = SerializerSMUtility.Deserialize( type, rawData[1] as byte[] ) as SMGameServerSendData;
			data._sendSeconds = 0;
			data._sender = null;
			data._view = null;

			_receiveEvent.OnNext( data );
		}

		///------------------------------------------------------------------------------------------------
		/// ● ゲーム物の生成、破棄
		///------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● ゲーム物を生成
		/// </summary>
		public override GameObject Instantiate( string name, Vector3 position = default,
												Quaternion rotation = default
		) {
			return PhotonNetwork.Instantiate( name, position, rotation );
		}

		/// <summary>
		/// ● ゲーム物を破棄
		/// </summary>
		public override void Destroy( GameObject gameObject ) {
			PhotonNetwork.Destroy( gameObject );
		}

		///------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● ネットワーク送信情報を破棄
		/// </summary>
		///------------------------------------------------------------------------------------------------
		public override void RemoveSendData( object target ) {
			switch ( target ) {
				case PhotonView view:
					PhotonNetwork.RemoveRPCs( view );
					break;

				case Player player:
					PhotonNetwork.RemoveRPCs( player );
					break;
			}
		}

		///------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● ネットワーク描画を検索
		/// </summary>
		///------------------------------------------------------------------------------------------------
		public override object FindNetworkView( int networkID )
			=> PhotonView.Find( networkID );
	}
}
#endif