using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using SubmarineMirage.Base;
using SubmarineMirage.Service;
using SubmarineMirage.Audio;
using SubmarineMirage.Scene;
using SubmarineMirage.Setting;
namespace TatemonSugoroku.Scripts {



	public class UIHowToPlay : SMStandardMonoBehaviour {
		protected override void StartAfterInitialize() {
			var sceneManager = SMServiceLocator.Resolve<SMSceneManager>();
			var audioManager = SMServiceLocator.Resolve<SMAudioManager>();

			var button = GetComponentInChildren<Button>();
			button.onClick.AddListener( () => {
				audioManager.Play( SMSE.Decide ).Forget();
				sceneManager.GetFSM<UISMScene>().ChangeState<UIHelpSMScene>().Forget();
			} );
		}

		public void Hide() {
			gameObject.SetActive( false );
		}
	}
}