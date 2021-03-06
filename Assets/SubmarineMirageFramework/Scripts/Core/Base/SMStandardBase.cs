//---------------------------------------------------------------------------------------------------------
// ▽ Submarine Mirage Framework for Unity
//		Copyright (c) 2020 夢想海の水底より(from Seabed of Reverie)
//		Released under the MIT License :
//			https://github.com/FromSeabedOfReverie/SubmarineMirageFrameworkForUnity/blob/master/LICENSE
//---------------------------------------------------------------------------------------------------------
namespace SubmarineMirage.Base {
	using System;
	using Event;
	using Extension;
	using Debug;
	using Debug.ToString;



	public abstract class SMStandardBase : BaseSM, ISMStandardBase {
		public SMDisposable _disposables	{ get; private set; } = new SMDisposable();
		[SMShowLine] public bool _isDispose => _disposables._isDispose;
		public SMToStringer _toStringer	{ get; private set; }



		public SMStandardBase() {
			_toStringer = new SMToStringer( this );
			SetToString();
			_disposables.AddFirst( () => {
				_toStringer.Dispose();
			} );
		}

		public override void Dispose() => _disposables.Dispose();



		protected void CheckDisposeError( string name ) {
			if ( !_isDispose )	{ return; }

			throw new ObjectDisposedException(
				$"{this}", $"既に解放済\n{this.GetAboutName()}.{name}" );
		}



		public virtual void SetToString() {}

		public override string ToString( int indent, bool isUseHeadIndent = true )
			=> _toStringer.Run( indent, isUseHeadIndent );

		public override string ToLineString( int indent = 0 ) => _toStringer.RunLine( indent );
	}
}