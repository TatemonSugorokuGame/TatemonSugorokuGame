//---------------------------------------------------------------------------------------------------------
// ▽ Submarine Mirage Framework for Unity
//		Copyright (c) 2020 夢想海の水底より(from Seabed of Reverie)
//		Released under the MIT License :
//			https://github.com/FromSeabedOfReverie/SubmarineMirageFrameworkForUnity/blob/master/LICENSE
//---------------------------------------------------------------------------------------------------------
#if PHOTON_UNITY_NETWORKING
namespace SubmarineMirage.Network {
	using System;
	using Photon.Realtime;
	using Extension;
	///====================================================================================================
	/// <summary>
	/// ■ マスターサーバーの、フォトン状態クラス
	/// </summary>
	///====================================================================================================
	public abstract class SMPhotonMasterState : SMPhotonState {
		///------------------------------------------------------------------------------------------------
		/// ● 要素
		///------------------------------------------------------------------------------------------------
		[SMShow] public abstract SMGameServerType _type	{ get; }

		protected override Type _disconnectStateType => typeof( DisconnectSMPhotonMasterState );

		///------------------------------------------------------------------------------------------------
		/// ● 作成、削除
		///------------------------------------------------------------------------------------------------
		public SMPhotonMasterState() {
		}



		/// <summary>
		/// ● 接続の切断（呼戻）
		/// </summary>
		public void OnDisconnect( DisconnectCause cause ) {
			switch ( cause ) {
				case DisconnectCause.None:
				case DisconnectCause.DisconnectByClientLogic:
				case DisconnectCause.DisconnectByServerLogic:
					OnDisconnect();
					return;

				default:
					OnError(
						new GameServerSMException(
							(
								!_owner._networkManager._isConnect	? SMGameServerErrorType.NoNetwork
																	: SMGameServerErrorType.Other
							),
							cause,
							$"サーバー接続失敗 : {this.GetAboutName()}.{nameof( OnDisconnect )}",
							true
						)
					);
					return;
			}
		}
	}
}
#endif