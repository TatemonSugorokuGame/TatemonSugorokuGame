//---------------------------------------------------------------------------------------------------------
// ▽ Submarine Mirage Framework for Unity
//		Copyright (c) 2020 夢想海の水底より(from Seabed of Reverie)
//		Released under the MIT License :
//			https://github.com/FromSeabedOfReverie/SubmarineMirageFrameworkForUnity/blob/master/LICENSE
//---------------------------------------------------------------------------------------------------------
namespace SubmarineMirage.Network {
	using Base;
	using Data;
	///====================================================================================================
	/// <summary>
	/// ■ ゲームサーバーの送信情報クラス
	/// </summary>
	///====================================================================================================
	public abstract class SMGameServerSendData : SMLightBase, ISMSerializeData {
		///------------------------------------------------------------------------------------------------
		/// ● 要素
		///------------------------------------------------------------------------------------------------
		/// <summary>送信秒数</summary>
		[SMShow] public double _sendSeconds;
		/// <summary>送信者</summary>
		[SMShow] public object _sender;
		/// <summary>視野</summary>
		[SMShow] public object _view;

		///------------------------------------------------------------------------------------------------
		/// ● 生成、破棄
		///------------------------------------------------------------------------------------------------
		public override void Dispose() {}
	}
}