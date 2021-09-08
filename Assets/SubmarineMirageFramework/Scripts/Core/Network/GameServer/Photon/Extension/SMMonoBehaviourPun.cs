//---------------------------------------------------------------------------------------------------------
// ▽ Submarine Mirage Framework for Unity
//		Copyright (c) 2020 夢想海の水底より(from Seabed of Reverie)
//		Released under the MIT License :
//			https://github.com/FromSeabedOfReverie/SubmarineMirageFrameworkForUnity/blob/master/LICENSE
//---------------------------------------------------------------------------------------------------------
#if PHOTON_UNITY_NETWORKING
namespace SubmarineMirage.Network {
	using System;
	using Photon.Pun;
	using Base;
	using Event;
	using Extension;
	using Debug;
	using Debug.ToString;
	///====================================================================================================
	/// <summary>
	/// ■ フォトンのモノ動作クラス
	/// </summary>
	///====================================================================================================
	public abstract class SMMonoBehaviourPun : MonoBehaviourPun, ISMStandardBase {
		///------------------------------------------------------------------------------------------------
		/// ● 要素
		///------------------------------------------------------------------------------------------------
		/// <summary>識別番号</summary>
		[SMShowLine] public uint _id { get; private set; }
		/// <summary>破棄の一覧</summary>
		public SMDisposable _disposables { get; private set; } = new SMDisposable();
		/// <summary>破棄済か？</summary>
		[SMShowLine] public bool _isDispose => _disposables._isDispose;
		/// <summary>デバッグ文字列の生成者</summary>
		public SMToStringer _toStringer { get; private set; }

		///------------------------------------------------------------------------------------------------
		/// ● 作成、削除
		///------------------------------------------------------------------------------------------------
		protected virtual void Awake() {
			_id = SMIDCounter.GetNewID( this );
			_toStringer = new SMToStringer( this );
			SetToString();

			_disposables.AddFirst( () => {
				_toStringer.Dispose();
				gameObject.Destroy();
			} );
		}

		public void Dispose() => _disposables.Dispose();

		protected void OnDestroy() => Dispose();

		///------------------------------------------------------------------------------------------------
		/// ● エラー
		///------------------------------------------------------------------------------------------------
		protected void CheckDisposeError( string name ) {
			if ( !_isDispose ) { return; }

			throw new ObjectDisposedException(
				$"{this}", $"既に解放済\n{this.GetAboutName()}.{name}" );
		}

		///------------------------------------------------------------------------------------------------
		/// ● 文字列に変換
		///------------------------------------------------------------------------------------------------
		public virtual void SetToString() {}

		public override string ToString() => ToString( 0 );

		public virtual string ToString( int indent, bool isUseHeadIndent = true )
			=> _toStringer.Run( indent, isUseHeadIndent );

		public virtual string ToLineString( int indent = 0 ) => _toStringer.RunLine( indent );
	}
}
#endif