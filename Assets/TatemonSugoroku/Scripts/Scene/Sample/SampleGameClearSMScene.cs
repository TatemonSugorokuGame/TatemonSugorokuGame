using UniRx;
using Cysharp.Threading.Tasks;
using SubmarineMirage.Service;
using SubmarineMirage.Data;
using SubmarineMirage.Scene;
using SubmarineMirage.Utility;
using SubmarineMirage.Debug;
using TatemonSugoroku.Scripts;
namespace Sample {



	/// <summary>
	/// ■ ゲームクリアシーンのクラス
	/// </summary>
	public class SampleGameClearSMScene : MainSMScene {

		/// <summary>
		/// ● コンストラクタ
		/// </summary>
		public SampleGameClearSMScene() {

			// シーン初期化
			_enterEvent.AddLast( async canceler => {
				SMLog.Debug( $"やった！ゲームクリアです。" );

				// 勝敗をセーブ
				var allDataManager = SMServiceLocator.Resolve<SMAllDataManager>();
				var playDatas = allDataManager.Get<PlayDataManager>();
//				playDatas._currentData._winCount++;
				await playDatas.SaveCurrentData();

				await UTask.DontWait();
			} );


			// 非同期更新
			_asyncUpdateEvent.AddLast( async canceler => {
				// 5秒後に、タイトルシーンへ遷移
				SMLog.Debug( $"5秒後に、タイトルシーンへ遷移します。" );
				await UTask.Delay( canceler, 5000 );

				_owner.GetFSM<MainSMScene>().ChangeState<SampleTitleSMScene>().Forget();
			} );



			// シーン終了
			_exitEvent.AddFirst( async canceler => {
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