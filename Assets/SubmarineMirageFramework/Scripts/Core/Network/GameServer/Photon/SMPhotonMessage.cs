//---------------------------------------------------------------------------------------------------------
// ▽ Submarine Mirage Framework for Unity
//		Copyright (c) 2020 夢想海の水底より(from Seabed of Reverie)
//		Released under the MIT License :
//			https://github.com/FromSeabedOfReverie/SubmarineMirageFrameworkForUnity/blob/master/LICENSE
//---------------------------------------------------------------------------------------------------------
#if PHOTON_UNITY_NETWORKING
namespace SubmarineMirage.Network {
	using Photon.Pun;
	using Photon.Realtime;
	using Base;
	///====================================================================================================
	/// <summary>
	/// ■ フォトンの伝言クラス
	/// </summary>
	///====================================================================================================
	public class SMPhotonMessage : SMLightBase {
		///------------------------------------------------------------------------------------------------
		/// ● 要素
		///------------------------------------------------------------------------------------------------
		/// <summary>送信者</summary>
		[SMShowLine] public Player _sender { get; private set; }
		/// <summary>視野</summary>
		[SMShowLine] public PhotonView _view { get; private set; }
		/// <summary>送信秒数</summary>
		[SMShowLine] public double _sendSeconds { get; private set; }

		///------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● コンストラクタ
		/// </summary>
		///------------------------------------------------------------------------------------------------
		public SMPhotonMessage( PhotonMessageInfo photonMessage ) {
			_sendSeconds = photonMessage.SentServerTime;
			_sender = photonMessage.Sender;
			_view = photonMessage.photonView;
		}

		public override void Dispose() {
		}
	}
}
#endif