using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using SubmarineMirage.Service;
using SubmarineMirage.Network;
using SubmarineMirage.Scene;
using SubmarineMirage.Utility;
using SubmarineMirage.Setting;
using TatemonSugoroku.Scripts.Akio;
namespace TatemonSugoroku.Scripts {



	/// <summary>
	/// ■ ゲームシーンのクラス
	/// </summary>
	public class GameSMScene : MainSMScene {

		/// <summary>
		/// ● コンストラクタ
		/// </summary>
		public GameSMScene() {
			SMGameServerModel gameServerModel = null;
			MainGameManager gameManager = null;

			// シーン初期化
			_enterEvent.AddLast( async canceler => {
				gameServerModel = SMServiceLocator.Resolve<SMNetworkManager>()._gameServerModel;
				gameManager = Object.FindObjectOfType<MainGameManager>();
				await UTask.DontWait();
			} );

			// シーン終了
			_exitEvent.AddFirst( async canceler => {
				gameManager.isEndGame = true;
				if ( await gameServerModel.Disconnect() ) {
				}
			} );

			// 更新（非同期的に実行）
			_asyncUpdateEvent.AddLast( async canceler => {
				gameManager.DoGame( canceler.ToToken() ).Forget();
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