//---------------------------------------------------------------------------------------------------------
// ▽ Submarine Mirage Framework for Unity
//		Copyright (c) 2020 夢想海の水底より(from Seabed of Reverie)
//		Released under the MIT License :
//			https://github.com/FromSeabedOfReverie/SubmarineMirageFrameworkForUnity/blob/master/LICENSE
//---------------------------------------------------------------------------------------------------------
#if PHOTON_UNITY_NETWORKING
namespace SubmarineMirage.Network {
	using Photon.Pun;
	///====================================================================================================
	/// <summary>
	/// ■ 部屋が、接続の、フォトン状態クラス
	/// </summary>
	///====================================================================================================
	public class JoinRoomSMPhotonRoomState : SMPhotonRoomState {
		///------------------------------------------------------------------------------------------------
		/// ● 要素
		///------------------------------------------------------------------------------------------------
		public override SMGameServerRoomType _type => SMGameServerRoomType.Join;



		public JoinRoomSMPhotonRoomState() {
		}



		protected override void Connect()
			=> PhotonNetwork.JoinRoom( _room.ToToken() );

		protected override void Disconnect()
			=> PhotonNetwork.LeaveRoom();
	}
}
#endif