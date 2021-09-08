//---------------------------------------------------------------------------------------------------------
// ▽ Submarine Mirage Framework for Unity
//		Copyright (c) 2020 夢想海の水底より(from Seabed of Reverie)
//		Released under the MIT License :
//			https://github.com/FromSeabedOfReverie/SubmarineMirageFrameworkForUnity/blob/master/LICENSE
//---------------------------------------------------------------------------------------------------------
#if PHOTON_UNITY_NETWORKING
namespace SubmarineMirage.Network {
	using Photon.Realtime;
	using Photon.Pun;
	///====================================================================================================
	/// <summary>
	/// ■ 部屋が、作成の、フォトン状態クラス
	/// </summary>
	///====================================================================================================
	public class CreateRoomSMPhotonRoomState : SMPhotonRoomState {
		///------------------------------------------------------------------------------------------------
		/// ● 要素
		///------------------------------------------------------------------------------------------------
		public override SMGameServerRoomType _type => SMGameServerRoomType.Create;



		public CreateRoomSMPhotonRoomState() {
		}



		protected override void Connect() {
			var option = new RoomOptions();
			option.MaxPlayers = ( byte )_room._maxPlayerCount;
			PhotonNetwork.CreateRoom( _room.ToToken(), option );
		}

		protected override void Disconnect()
			=> PhotonNetwork.LeaveRoom();
	}
}
#endif