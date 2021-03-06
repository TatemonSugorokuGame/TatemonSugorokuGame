//---------------------------------------------------------------------------------------------------------
// ▽ Submarine Mirage Framework for Unity
//		Copyright (c) 2020 夢想海の水底より(from Seabed of Reverie)
//		Released under the MIT License :
//			https://github.com/FromSeabedOfReverie/SubmarineMirageFrameworkForUnity/blob/master/LICENSE
//---------------------------------------------------------------------------------------------------------
namespace SubmarineMirage.Audio {
	using Cysharp.Threading.Tasks;
	using Base;
	///====================================================================================================
	/// <summary>
	/// ■ 音源管理のインタフェース
	///		管理クラスの配列に入れる際に、ジェネリッククラスだと安定しない為、作成。
	/// </summary>
	///====================================================================================================
	public interface IBaseSMAudioSource : ISMStandardBase {
		///------------------------------------------------------------------------------------------------
		/// ● 仮想関数
		///------------------------------------------------------------------------------------------------
		/// <summary>● 設定</summary>
		void Setup();
		/// <summary>● 停止（再生中音）</summary>
		UniTask Stop();
	}
}