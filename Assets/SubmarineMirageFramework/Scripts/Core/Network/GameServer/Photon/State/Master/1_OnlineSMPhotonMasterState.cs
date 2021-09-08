//---------------------------------------------------------------------------------------------------------
// ▽ Submarine Mirage Framework for Unity
//		Copyright (c) 2020 夢想海の水底より(from Seabed of Reverie)
//		Released under the MIT License :
//			https://github.com/FromSeabedOfReverie/SubmarineMirageFrameworkForUnity/blob/master/LICENSE
//---------------------------------------------------------------------------------------------------------
#if PHOTON_UNITY_NETWORKING
namespace SubmarineMirage.Network {
	using Photon.Pun;
	using Setting;
	///====================================================================================================
	/// <summary>
	/// ■ マスターサーバーが、オンライン接続の、フォトン状態クラス
	/// </summary>
	///====================================================================================================
	public class OnlineSMPhotonMasterState : SMPhotonMasterState {
		///------------------------------------------------------------------------------------------------
		/// ● 要素
		///------------------------------------------------------------------------------------------------
		public override SMGameServerType _type => SMGameServerType.Online;



		public OnlineSMPhotonMasterState() {
			_enterEvent.AddLast( _registerEventKey, async canceler => {
				if ( _status == SMGameServerStatus.Connect ) {
					await _owner._roomFSM.ChangeState<LobbySMPhotonRoomState>();
				}
			} );

			_exitEvent.AddFirst( _registerEventKey, async canceler => {
				if ( !( _owner._roomState is DisconnectSMPhotonRoomState ) ) {
					await _owner._roomFSM.ChangeState<DisconnectSMPhotonRoomState>();
				}
			} );
		}



		protected override void Connect() {
			PhotonNetwork.OfflineMode = false;
			PhotonNetwork.GameVersion = SMMainSetting.APPLICATION_VERSION;
			PhotonNetwork.ConnectUsingSettings();
		}

		protected override void Disconnect()
			=> PhotonNetwork.Disconnect();
	}
}
#endif