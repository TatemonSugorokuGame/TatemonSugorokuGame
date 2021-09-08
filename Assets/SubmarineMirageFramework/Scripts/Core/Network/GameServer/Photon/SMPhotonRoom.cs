//---------------------------------------------------------------------------------------------------------
// ▽ Submarine Mirage Framework for Unity
//		Copyright (c) 2020 夢想海の水底より(from Seabed of Reverie)
//		Released under the MIT License :
//			https://github.com/FromSeabedOfReverie/SubmarineMirageFrameworkForUnity/blob/master/LICENSE
//---------------------------------------------------------------------------------------------------------
#if PHOTON_UNITY_NETWORKING
namespace SubmarineMirage.Network {
	using Photon.Realtime;
	using KoganeUnityLib;
	using Debug;
	///====================================================================================================
	/// <summary>
	/// ■ フォトンの部屋クラス
	/// </summary>
	///====================================================================================================
	public class SMPhotonRoom : SMGameServerRoom {
		const string PASSWORD_SPLIT_TEXT = ":password:";

		protected override string _wrapText => PASSWORD_SPLIT_TEXT;



		public SMPhotonRoom( string name, string password, int maxPlayerCount )
			: base( name, password, maxPlayerCount
		) {}

		public SMPhotonRoom( RoomInfo room ) {
			var ns = room.Name.Split( PASSWORD_SPLIT_TEXT );
			_name = ns[0];
			if ( ns.Length == 2 )	{ _password = ns[1]; }

			_playerCount = room.PlayerCount;
			_maxPlayerCount = room.MaxPlayers;
			
		}

		public override void Dispose() {
		}



		public override string ToToken()
			=> $"{_name}{PASSWORD_SPLIT_TEXT}{_password}";
	}
}
#endif