//---------------------------------------------------------------------------------------------------------
// ▽ Submarine Mirage Framework for Unity
//		Copyright (c) 2020 夢想海の水底より(from Seabed of Reverie)
//		Released under the MIT License :
//			https://github.com/FromSeabedOfReverie/SubmarineMirageFrameworkForUnity/blob/master/LICENSE
//---------------------------------------------------------------------------------------------------------
#if PHOTON_UNITY_NETWORKING
namespace SubmarineMirage.Network {
	using System;
	using UniRx;
	using Photon.Pun;
	using Utility;
	///====================================================================================================
	/// <summary>
	/// ■ 部屋の、フォトン状態クラス
	/// </summary>
	///====================================================================================================
	public abstract class SMPhotonRoomState : SMPhotonState {
		///------------------------------------------------------------------------------------------------
		/// ● 要素
		///------------------------------------------------------------------------------------------------
		/// <summary>部屋の型</summary>
		public abstract SMGameServerRoomType _type { get; }
		/// <summary>部屋</summary>
		public SMPhotonRoom _room { get; set; }

		/// <summary>プレイヤー数</summary>
		public int _playerCount => PhotonNetwork.CurrentRoom?.PlayerCount ?? 0;

		protected override Type _disconnectStateType => typeof( DisconnectSMPhotonRoomState );

		///------------------------------------------------------------------------------------------------
		/// ● 作成、削除
		///------------------------------------------------------------------------------------------------
		public SMPhotonRoomState() {
			_exitEvent.AddLast( _registerEventKey, async canceler => {
				_room = null;
				await UTask.DontWait();
			} );

			_disposables.AddFirst( () =>
				_owner._playerCountEvent
					.Where( _ => _room != null )
					.Subscribe( i => _room._playerCount = i )
			);
		}



		/// <summary>
		/// ● 部屋失敗（呼戻）
		/// </summary>
		public void OnError( short returnCode, string message )
			=> OnError( $"{nameof( returnCode )} : {returnCode}\n{message}" );
	}
}
#endif