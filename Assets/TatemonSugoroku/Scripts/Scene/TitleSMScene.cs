using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using SubmarineMirage.Service;
using SubmarineMirage.Audio;
using SubmarineMirage.Scene;
using SubmarineMirage.Utility;
using SubmarineMirage.Setting;
using SubmarineMirage.Debug;
namespace TatemonSugoroku.Scripts {



	/// <summary>
	/// ■ タイトルシーンのクラス
	/// </summary>
	public class TitleSMScene : MainSMScene {
		const float OPENING_SECONDS = 30;

		SMSceneManager _sceneManager;
		float _noInputSeconds;



		/// <summary>
		/// ● コンストラクタ
		/// </summary>
		public TitleSMScene() {
			// シーン初期化
			_enterEvent.AddLast( async canceler => {
				_sceneManager = SMServiceLocator.Resolve<SMSceneManager>();
				var audioManager = SMServiceLocator.Resolve<SMAudioManager>();
				audioManager.Play( SMBGM.Title ).Forget();

				UTask.Void( async () => {
					_noInputSeconds = OPENING_SECONDS;
					await UTask.WaitUntil( _asyncCancelerOnExit, () => _noInputSeconds <= 0 );
					_sceneManager.GetFSM<UISMScene>().ChangeState<UINoneSMScene>().Forget();
					_sceneManager.GetFSM<MainSMScene>().ChangeState<OpeningSMScene>().Forget();
				} );

				await UTask.DontWait();
			} );

			// シーン終了
			_exitEvent.AddFirst( async canceler => {
				await UTask.DontWait();
			} );

			// 更新（非同期的に実行）
			_asyncUpdateEvent.AddLast( async canceler => {
				await UTask.DontWait();
			} );

			// 物理更新
			_fixedUpdateEvent.AddLast().Subscribe( _ => {
			} );

			// 更新
			_updateEvent.AddLast().Subscribe( _ => {
				_noInputSeconds -= Time.deltaTime;
				if ( !( _sceneManager.GetFSM<UISMScene>()._state is UINoneSMScene ) ) {
					_noInputSeconds = OPENING_SECONDS;
				}
			} );

			// 後更新
			_lateUpdateEvent.AddLast().Subscribe( _ => {
			} );

			// 破棄
			_disposables.AddFirst( () => {
			} );
		}
	}
}