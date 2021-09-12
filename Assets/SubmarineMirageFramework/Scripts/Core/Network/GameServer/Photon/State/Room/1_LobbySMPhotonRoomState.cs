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
		[SMShow] public override SMGameServerRoomType _type => SMGameServerRoomType.Join;

		/// <summary>部屋の一覧</summary>
		[SMShow] readonly Dictionary<string, SMPhotonRoom> _rooms = new Dictionary<string, SMPhotonRoom>();



		public LobbySMPhotonRoomState() {
			_exitEvent.AddLast( _registerEventKey, async canceler => {
				_rooms.Clear();
				SendRooms();
				await UTask.DontWait();
			} );


			_disposables.AddFirst( () => {
				_rooms.Clear();
			} );
		}



		protected override void Connect()
			=> PhotonNetwork.JoinLobby();

		protected override void Disconnect()
			=> PhotonNetwork.LeaveLobby();



		public void OnUpdateRoom( List<RoomInfo> differenceRooms ) {
			differenceRooms.ForEach( i => {
				if ( i.RemovedFromList && i.PlayerCount == 0 ) {
					_rooms.Remove( i.Name );
					return;
				}

				try {
					_rooms[i.Name] = new SMPhotonRoom( i );
				} catch ( Exception e ) {
					SMLog.Error( e, SMLogTag.Server );
					// 他人の部屋のエラーは、伝達しない
				}
			} );

			SendRooms();
		}

		public void SendRooms() {
			_owner._roomsEvent.OnNext(
				_rooms
					.Select( pair => pair.Value as SMGameServerRoom )
					.ToList()
			);
		}
	}
}
#endif