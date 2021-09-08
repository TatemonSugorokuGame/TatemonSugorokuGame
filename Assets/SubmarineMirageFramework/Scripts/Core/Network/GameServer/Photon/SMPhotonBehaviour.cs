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
	using Photon.Pun;
	using Photon.Realtime;
	///====================================================================================================
	/// <summary>
	/// ■ フォトンのモノ動作クラス
	/// </summary>
	///====================================================================================================
	public abstract class SMPhotonBehaviour : SMMonoBehaviourPun, IPunInstantiateMagicCallback, IPunObservable {
		///------------------------------------------------------------------------------------------------
		/// ● 要素
		///------------------------------------------------------------------------------------------------
		/// <summary>視野</summary>
		public PhotonView _view => photonView;

		///------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● 視野を検索
		/// </summary>
		///------------------------------------------------------------------------------------------------
		protected PhotonView FindView( int viewID )
			=> PhotonView.Find( viewID );

		///------------------------------------------------------------------------------------------------
		/// ● 生成完了（呼戻）
		///------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● 生成完了（呼戻）
		/// </summary>
		public void OnPhotonInstantiate( PhotonMessageInfo info ) {
			OnNetworkInstantiate( new SMPhotonMessage( info ) );
		}

		/// <summary>
		/// ● 生成完了（呼戻）
		/// </summary>
		protected virtual void OnNetworkInstantiate( SMPhotonMessage info ) {
		}

		///------------------------------------------------------------------------------------------------
		/// ● RPCを送信
		///------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● RPCを送信
		/// </summary>
		public void RPC( Player targetPlayer, Enum methodType, params object[] parameters ) {
			var methodID = ( int )( object )methodType;
			parameters = Serialize( parameters );
			_view.RPC( $"{nameof( OnTopRPC )}", targetPlayer, methodID, parameters );
		}

		/// <summary>
		/// ● RPCを送信
		/// </summary>
		public void RPC( RpcTarget target, Enum methodType, params object[] parameters ) {
			var methodID = ( int )( object )methodType;
			parameters = Serialize( parameters );
			_view.RPC( $"{nameof( OnTopRPC )}", target, methodID, parameters );
		}

		/// <summary>
		/// ● RPCをオフライン送信
		/// </summary>
		public void RPCToLocal( Enum methodType, params object[] parameters ) {
			var methodID = ( int )( object )methodType;
			parameters = Serialize( parameters );
			OnTopRPC( methodID, parameters,
				new PhotonMessageInfo( PhotonNetwork.LocalPlayer, PhotonNetwork.ServerTimestamp, _view )
			);
		}

		/// <summary>
		/// ● RPCを、他者に送信するか、自分にオフライン送信
		/// </summary>
		public void RPCToOtherOrLocalMine( Player targetPlayer, Enum methodType, params object[] parameters ) {
			if ( _view.IsMine )	{ RPCToLocal( methodType, parameters ); }
			else				{ RPC( targetPlayer, methodType, parameters ); }
		}

		/// <summary>
		/// ● RPCを、他者に送信するか、自分にオフライン送信
		/// </summary>
		public void RPCToOtherOrLocalMine( RpcTarget target, Enum methodType, params object[] parameters ) {
			if ( _view.IsMine )	{ RPCToLocal( methodType, parameters ); }
			else				{ RPC( target, methodType, parameters ); }
		}

		///------------------------------------------------------------------------------------------------
		/// ● RPCを受信（呼戻）
		///------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● 最初にRPCを受信（呼戻）
		/// </summary>
		[PunRPC] public void OnTopRPC( int methodType, object[] parameters, PhotonMessageInfo info ) {
			parameters = Deserialize( parameters );
			OnRPC( methodType, parameters, new SMPhotonMessage( info ) );
		}

		/// <summary>
		/// ● RPCを受信（呼戻）
		/// </summary>
		public virtual void OnRPC( int methodType, object[] parameters, SMPhotonMessage info ) {
		}

		///------------------------------------------------------------------------------------------------
		/// ● 直列化
		///------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● 直列化
		/// </summary>
		object[] Serialize( object[] parameters )
			=> parameters.Select( data => {
				switch ( data ) {
					case Type t:	return $"{nameof( Type )}:{t.FullName}";
				}
				return data;
			} )
			.ToArray();

		/// <summary>
		/// ● 非直列化
		/// </summary>
		object[] Deserialize( object[] parameters )
			=> parameters.Select( data => {
				switch ( data ) {
					case string s:
						var ss = s.Split( ':' );
						if ( ss[0] == nameof( Type ) )	{ return Type.GetType( ss[1] ); }
						break;
				}
				return data;
			} )
			.ToArray();

		///------------------------------------------------------------------------------------------------
		/// ● 直列化の視野（呼戻）
		///------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● 直列化の視野（呼戻）
		/// </summary>
		public void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info ) {
			OnSerializeView( stream, new SMPhotonMessage( info ) );
		}

		/// <summary>
		/// ● 直列化の視野（呼戻）
		/// </summary>
		protected virtual void OnSerializeView( PhotonStream stream, SMPhotonMessage info ) {
		}
	}
}
#endif