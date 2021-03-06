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
	using Cysharp.Threading.Tasks;
	using Photon.Pun;
	using FSM;
	using Extension;
	using Utility;
	using Setting;
	using Debug;
	///====================================================================================================
	/// <summary>
	/// ■ フォトンの状態クラス
	/// </summary>
	///====================================================================================================
	public abstract class SMPhotonState : SMState {
		///------------------------------------------------------------------------------------------------
		/// ● 要素
		///------------------------------------------------------------------------------------------------
		public new SMPhotonServerModel _owner { get; private set; }

		/// <summary>接続状態</summary>
		[SMShowLine] public SMGameServerStatus _status { get; protected set; }

		[SMShow] public GameServerSMException _error { get; protected set; }

		protected string _registerEventKey => this.GetAboutName();

		protected abstract Type _disconnectStateType { get; }

		///------------------------------------------------------------------------------------------------
		/// ● 作成、削除
		///------------------------------------------------------------------------------------------------
		public SMPhotonState() {
			_enterEvent.AddLast( _registerEventKey, async canceler => {
#if TestNetwork
				SMLog.Debug( $"サーバー接続中 : {this.GetAboutName()}", SMLogTag.Server );
#endif
				_status = SMGameServerStatus.Disconnect;
				_error = null;
				if ( _owner._masterState._status == SMGameServerStatus.Connect ) {
					await UTask.WaitWhile( canceler, () => !PhotonNetwork.IsConnectedAndReady );
				}
				if ( Connect() ) {
					await UTask.WaitWhile( canceler, () => _status == SMGameServerStatus.Disconnect );
				} else {
					_status = SMGameServerStatus.Error;
				}
			} );

			_asyncUpdateEvent.AddLast( _registerEventKey, async canceler => {
				await UTask.WaitWhile( canceler, () => _status == SMGameServerStatus.Connect );
				_fsm.ChangeState( _disconnectStateType ).Forget();
			} );

			_exitEvent.AddLast( _registerEventKey, async canceler => {
#if TestNetwork
				SMLog.Debug( $"サーバー接続切断中 : {this.GetAboutName()}", SMLogTag.Server );
#endif
				if ( _status == SMGameServerStatus.Connect ) {
					if ( Disconnect() ) {
						await UTask.WaitWhile( canceler, () => _status == SMGameServerStatus.Connect );
					} else {
						_status = SMGameServerStatus.Error;
					}
				}
			} );
		}

		public override void Setup( object owner, SMFSM fsm ) {
			base.Setup( owner, fsm );
			_owner = base._owner as SMPhotonServerModel;
		}



		protected abstract bool Connect();

		protected abstract bool Disconnect();



		public async UniTask<bool> WaitConnect() {
			await UTask.WaitWhile( _asyncCancelerOnExit, () => _status == SMGameServerStatus.Disconnect );
			return _status == SMGameServerStatus.Connect;
		}



		/// <summary>
		/// ● 接続成功（呼戻）
		/// </summary>
		public void OnConnect() {
			_status = SMGameServerStatus.Connect;
			SMLog.Debug( $"サーバー接続成功 : {this.GetAboutName()}", SMLogTag.Server );
		}

		/// <summary>
		/// ● 接続の切断（呼戻）
		/// </summary>
		public void OnDisconnect() {
			_status = SMGameServerStatus.Disconnect;
			SMLog.Debug( $"サーバー接続切断 : {this.GetAboutName()}", SMLogTag.Server );
		}

		/// <summary>
		/// ● 接続失敗（呼戻）
		/// </summary>
		public void OnError( GameServerSMException error ) {
			_status = SMGameServerStatus.Error;
			_error = error;

			SMLog.Error( $"{_error}", SMLogTag.Server );
			_owner._errorEvent.OnNext( _error );
		}
	}
}
#endif