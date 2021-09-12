using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UniRx;
using KoganeUnityLib;
using SubmarineMirage.Base;
using SubmarineMirage.Service;
using SubmarineMirage.Audio;
using SubmarineMirage.Scene;
using SubmarineMirage.Network;
using SubmarineMirage.Utility;
using SubmarineMirage.Setting;
using SubmarineMirage.Debug;
namespace TatemonSugoroku.Scripts {



	public class UINetworkError : SMStandardMonoBehaviour {
		[SerializeField] Text _info;

		SMSceneManager _sceneManager { get; set; }
		SMAudioManager _audioManager { get; set; }
		SMNetworkManager _networkManager { get; set; }
		ISMGameServer _gameServer { get; set; }



		protected override void StartAfterInitialize() {
			gameObject.DontDestroyOnLoad();

			_sceneManager = SMServiceLocator.Resolve<SMSceneManager>();
			_audioManager = SMServiceLocator.Resolve<SMAudioManager>();
			_networkManager = SMServiceLocator.Resolve<SMNetworkManager>();
			_gameServer = _networkManager._gameServer;

			SetError( string.Empty );

			var buttons = GetComponentsInChildren<Button>( true );
			buttons.ForEach( b => {
				b.onClick.AddListener( () => {
					UTask.Void( async () => {
						switch ( b.name ) {
							case "ButtonClose": {
								_audioManager.Play( SMSE.Decide ).Forget();
								SetError( string.Empty );

								var mainFSM = _sceneManager.GetFSM<MainSMScene>();
								if ( mainFSM._state is GameSMScene ) {
									mainFSM.ChangeState<TitleSMScene>().Forget();
								}
								return;
							}
						}

						await UTask.DontWait();
					} );
				} );
			} );

			_disposables.AddFirst(
				_gameServer._errorEvent.Subscribe( t => SetError( t ) )
			);
		}



		public void SetError( string text ) {
			_info.text = text;
			gameObject.SetActive( !text.IsNullOrEmpty() );
		}
	}
}