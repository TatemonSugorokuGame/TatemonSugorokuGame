//---------------------------------------------------------------------------------------------------------
// ▽ Submarine Mirage Framework for Unity
//		Copyright (c) 2020 夢想海の水底より(from Seabed of Reverie)
//		Released under the MIT License :
//			https://github.com/FromSeabedOfReverie/SubmarineMirageFrameworkForUnity/blob/master/LICENSE
//---------------------------------------------------------------------------------------------------------
namespace SubmarineMirage.Extension {
	using UniRx;
	using Base;
	using Service;



	public class MainThreadDispatcherSMExtension : SMStandardMonoBehaviour, ISMService {
		public readonly Subject<Unit> _onGUIEvent = new Subject<Unit>();



		protected override void Awake() {
			base.Awake();

			_disposables.AddFirst( () => {
				_onGUIEvent.OnCompleted();
				_onGUIEvent.Dispose();
			} );
		}



		void OnGUI() {
			if ( _isDispose )	{ return; }

			_onGUIEvent.OnNext( Unit.Default );
		}
	}
}