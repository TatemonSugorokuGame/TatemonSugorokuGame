using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using KoganeUnityLib;
using SubmarineMirage.Base;
using SubmarineMirage.Service;
using SubmarineMirage.Task;
using SubmarineMirage.Data;
using SubmarineMirage.Audio;
using SubmarineMirage.Scene;
using SubmarineMirage.Utility;
using SubmarineMirage.Setting;
namespace TatemonSugoroku.Scripts {



	public class UITitle : SMStandardMonoBehaviour {
		readonly SMAsyncCanceler _canceler = new SMAsyncCanceler();



		protected override void StartAfterInitialize() {
			var sceneManager = SMServiceLocator.Resolve<SMSceneManager>();
			var taskManager = SMServiceLocator.Resolve<SMTaskManager>();
			var audioManager = SMServiceLocator.Resolve<SMAudioManager>();

			var buttons = GetComponentsInChildren<Button>( true );
			buttons.ForEach( b => {
				b.onClick.AddListener( () => {
					UTask.Void( async () => {
						switch ( b.name ) {
							case "ButtonTourism":
								await audioManager.Play( SMSE.Decide );
								Application.OpenURL( "https://uozu-kanko.jp/jyantokoi-uozu-fest/" );
								break;

							case "ButtonUOZU":
								await audioManager.Play( SMSE.Decide );
								Application.OpenURL( "https://detail.uozugame.com/" );
								break;

							case "ButtonFramework":
								await audioManager.Play( SMSE.Decide );
								Application.OpenURL(
									"https://github.com/FromSeabedOfReverie/SubmarineMirageFrameworkForUnity" );
								break;

							case "ButtonStart":
								await audioManager.Play( SMSE.Title );
								sceneManager.GetFSM<MainSMScene>().ChangeState<GameSMScene>().Forget();
								break;

							case "ButtonHelp":
								await audioManager.Play( SMSE.Decide );
								sceneManager.GetFSM<UISMScene>().ChangeState<UIHelpSMScene>().Forget();
								break;

							case "ButtonCredit":
								await audioManager.Play( SMSE.Decide );
								sceneManager.GetFSM<MainSMScene>().ChangeState<CreditSMScene>().Forget();
								break;

							case "ButtonEnd":
								await audioManager.Play( SMSE.Decide );
								taskManager.Finalize().Forget();
								break;
						}
					} );
				} );
			} );

			ShowHelp().Forget();
		}



		async UniTask ShowHelp() {
			var setting = SMServiceLocator.Resolve<SMAllDataManager>().Get<SettingDataManager>().Get();
			var audioManager = SMServiceLocator.Resolve<SMAudioManager>();
			if ( setting._isShowHelp )	{ return; }

			await UTask.Delay( _canceler, 1000 );

			audioManager.Play( SMSE.Decide ).Forget();
			var sceneManager = SMServiceLocator.Resolve<SMSceneManager>();
			sceneManager.GetFSM<UISMScene>().ChangeState<UIHelpSMScene>().Forget();

			await UTask.DontWait();
		}
	}
}