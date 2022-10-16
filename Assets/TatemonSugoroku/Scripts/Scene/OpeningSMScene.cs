using UniRx;
using Cysharp.Threading.Tasks;
using SubmarineMirage.Service;
using SubmarineMirage.Data;
using SubmarineMirage.Audio;
using SubmarineMirage.Scene;
using SubmarineMirage.Utility;
using SubmarineMirage.Setting;
namespace TatemonSugoroku.Scripts {



	/// <summary>
	/// ■ オープニングシーンのクラス
	/// </summary>
	public class OpeningSMScene : MainSMScene {
		SMAudioManager _audioManager;


		/// <summary>
		/// ● コンストラクタ
		/// </summary>
		public OpeningSMScene() {

			// シーン初期化
			_enterEvent.AddLast( async canceler => {
				_audioManager = SMServiceLocator.Resolve<SMAudioManager>();
				_audioManager._bgsVolume = 0.4f;
				_audioManager._voiceVolume = 1;
				_audioManager.Play( SMBGS.Wind ).Forget();
			} );

			// シーン終了
			_exitEvent.AddFirst( async canceler => {
				var dataManager = SMServiceLocator.Resolve<SMAllDataManager>().Get<SettingDataManager>();
				var data = dataManager.Get();
				_audioManager._bgmVolume = data._bgmVolume;
				_audioManager._bgsVolume = data._bgsVolume;
				_audioManager._voiceVolume = data._voiceVolume;
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