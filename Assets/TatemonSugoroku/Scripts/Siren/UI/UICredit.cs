using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using KoganeUnityLib;
using SubmarineMirage.Base;
using SubmarineMirage.Service;
using SubmarineMirage.Audio;
using SubmarineMirage.Data;
using SubmarineMirage.Scene;
using SubmarineMirage.Utility;
using SubmarineMirage.Setting;
using SubmarineMirage.Debug;
namespace TatemonSugoroku.Scripts {



	public class UICredit : SMStandardMonoBehaviour {
		[SerializeField] RectTransform _topText;
		[SerializeField] Text _headlineText;
		[SerializeField] Text _normalText;
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

			_headlineText.gameObject.SetActive( false );
			_normalText.gameObject.SetActive( false );

			Scroll().Forget();
		}



		async UniTask Scroll() {
			var allDataManager = SMServiceLocator.Resolve<SMAllDataManager>();
			var creditData = allDataManager.Get<int, CreditData>();

			var endObjects = _topText.GetChildren()
				.Select( go => go.transform );
			creditData.GetAlls().ForEach( data => {
				GameObject go = null;
				switch ( data._tag ) {
					case CreditData.Tag.Headline:
						go = Instantiate( _headlineText.gameObject, _topText );
						break;
					case CreditData.Tag.Normal:
						go = Instantiate( _normalText.gameObject, _topText );
						break;
				}
				go.SetActive( true );
				var text = go.GetComponent<Text>();
				text.text = data._text;
			} );
			endObjects.ForEach( t => t.SetAsLastSibling() );
			await UTask.DelayFrame( _canceler, 2 );

			var scene = _sceneManager.GetFSM<MainSMScene>().GetState<CreditSMScene>();
			await UTask.WaitWhile( _canceler, () => scene._bgmSeconds == 0 );

			var endY = _topText.sizeDelta.y - 1080;
			await _topText
				.DOAnchorPos( new Vector3( 0, endY ), scene._bgmSeconds - 5 )
				.SetEase( Ease.Linear )
				.Play()
				.ToUniTask( _canceler );

			await UTask.Delay( _canceler, 5000 );

			_sceneManager.GetFSM<MainSMScene>().ChangeState<TitleSMScene>().Forget();
		}
	}
}