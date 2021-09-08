//---------------------------------------------------------------------------------------------------------
// ▽ Submarine Mirage Framework for Unity
//		Copyright (c) 2020 夢想海の水底より(from Seabed of Reverie)
//		Released under the MIT License :
//			https://github.com/FromSeabedOfReverie/SubmarineMirageFrameworkForUnity/blob/master/LICENSE
//---------------------------------------------------------------------------------------------------------
#if PHOTON_UNITY_NETWORKING
namespace SubmarineMirage.Network {
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using Photon.Realtime;
	using Photon.Pun;
	using Utility;
	using Setting;
	using Debug;
	///====================================================================================================
	/// <summary>
	/// ■ 部屋が、控室の、フォトン状態クラス
	/// </summary>
	///====================================================================================================
	public class LobbySMPhotonRoomState : SMPhotonRoomState {
		///------------------------------------------------------------------------------------------------
		/// ● 要素
		///------------------------------------------------------------------------------------------------
		public override SMGameServerRoomType _type => SMGameServerRoomType.Join;

		/// <summary>部屋の一覧</summary>
		public List<SMPhotonRoom> _rooms { get; private set; } = new List<SMPhotonRoom>();



		public LobbySMPhotonRoomState() {
			_exitEvent.AddLast( _registerEventKey, async canceler => {
				_rooms.Clear();
				_owner._roomsEvent.Value = _rooms
					.Select( r => r as SMGameServerRoom )
					.ToList();

				await UTask.DontWait();
			} );
		}



		protected override void Connect()
			=> PhotonNetwork.JoinLobby();

		protected override void Disconnect()
			=> PhotonNetwork.LeaveLobby();



		public void OnUpdateRoom( List<RoomInfo> roomList ) {
			_rooms = roomList
				.Where( r => !r.RemovedFromList )
				.Select( r => {
					try {
						return new SMPhotonRoom( r );
					} catch ( Exception e ) {
						SMLog.Error( e, SMLogTag.Server );
						return null;
					}
				} )
				.Where( r => r != null )
				.ToList();

			_owner._roomsEvent.Value = _rooms
				.Select( r => r as SMGameServerRoom )
				.ToList();
		}
	}
}
#endif