//---------------------------------------------------------------------------------------------------------
// ▽ Submarine Mirage Framework for Unity
//		Copyright (c) 2020 夢想海の水底より(from Seabed of Reverie)
//		Released under the MIT License :
//			https://github.com/FromSeabedOfReverie/SubmarineMirageFrameworkForUnity/blob/master/LICENSE
//---------------------------------------------------------------------------------------------------------
#if PHOTON_UNITY_NETWORKING
namespace SubmarineMirage.Network {
	using Photon.Pun;
	using Utility;
	///====================================================================================================
	/// <summary>
	/// ■ マスターサーバーが、オフライン接続の、フォトン状態クラス
	/// </summary>
	///====================================================================================================
	public class OfflineSMPhotonMasterState : SMPhotonMasterState {
		///------------------------------------------------------------------------------------------------
		/// ● 要素
		///------------------------------------------------------------------------------------------------
		[SMShow] public override SMGameServerType _type => SMGameServerType.Offline;



		public OfflineSMPhotonMasterState() {
			_enterEvent.AddLast( _registerEventKey, async canceler => {
				if ( _status == SMGameServerStatus.Connect ) {
					await _owner._roomFSM.ChangeState<CreateRoomSMPhotonRoomState>();
				}
			} );

			_exitEvent.AddFirst( _registerEventKey, async canceler => {
				if ( !( _owner._roomState is DisconnectSMPhotonRoomState ) ) {
					await _owner._roomFSM.ChangeState<DisconnectSMPhotonRoomState>();
				}
			} );
			_exitEvent.AddLast( _registerEventKey, async canceler => {
				PhotonNetwork.OfflineMode = false;
				await UTask.DontWait();
			} );
		}



		protected override void Connect()
			=> PhotonNetwork.OfflineMode = true;

		protected override void Disconnect()
			=> PhotonNetwork.Disconnect();
	}
}
#endif