using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using KoganeUnityLib;
using SubmarineMirage.Base;
using SubmarineMirage.Service;
using SubmarineMirage.Audio;
using SubmarineMirage.Scene;
using SubmarineMirage.Utility;
using SubmarineMirage.Setting;
using SubmarineMirage.Debug;
namespace TatemonSugoroku.Scripts {



	public class UICredit : SMStandardMonoBehaviour {
		[SerializeField] RectTransform _topText;
		readonly SMAsyncCanceler _canceler = new SMAsyncCanceler();
		SMSceneManager _sceneManager { get; set; }
		SMAudioManager _audioManager { get; set; }


		void Start() {
			_disposables.AddFirst( () => {
				_canceler.Dispose();
			} );
		}



		protected override void StartAfterInitialize() {
			_sceneManager = SMServiceLocator.Resolve<SMSceneManager>();
			_audioManager = SMServiceLocator.Resolve<SMAudioManager>();

			var buttons = GetComponentsInChildren<Button>( true );
			buttons.ForEach( b => {
				b.onClick.AddListener( () => {

					switch ( b.name ) {
						case "ButtonEnd":
							UTask.Void( async () => {
								await _audioManager.Play( SMSE.Decide );
								_sceneManager.GetFSM<MainSMScene>().ChangeState<TitleSMScene>().Forget();
							} );
							break;
					}
				} );
			} );

			Scroll().Forget();
		}



		async UniTask Scroll() {
			var scene = _sceneManager.GetFSM<MainSMScene>().GetState<CreditSMScene>();
			await UTask.WaitWhile( _canceler, () => scene._bgmSeconds == 0 );

			await _topText
				.DOAnchorPos( new Vector3( 0, 11670 ), scene._bgmSeconds - 5 )
				.SetEase( Ease.Linear )
				.Play()
				.ToUniTask( _canceler );

			await UTask.Delay( _canceler, 5000 );

			_sceneManager.GetFSM<MainSMScene>().ChangeState<TitleSMScene>().Forget();
		}
	}
}