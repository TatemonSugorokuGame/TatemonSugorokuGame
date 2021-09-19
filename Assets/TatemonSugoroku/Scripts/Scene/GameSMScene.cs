using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using SubmarineMirage.Service;
using SubmarineMirage.Network;
using SubmarineMirage.Scene;
using SubmarineMirage.Utility;
using SubmarineMirage.Setting;
namespace TatemonSugoroku.Scripts {



	/// <summary>
	/// ■ ゲームシーンのクラス
	/// </summary>
	public class GameSMScene : MainSMScene {

		/// <summary>
		/// ● コンストラクタ
		/// </summary>
		public GameSMScene() {
			// シーン初期化
			_enterEvent.AddLast( async canceler => {
				await UTask.DontWait();
			} );

			// シーン終了
			_exitEvent.AddFirst( async canceler => {
				var gameServer = SMServiceLocator.Resolve<SMNetworkManager>()._gameServerModel;
				if ( await gameServer.Disconnect() ) {
				}
			} );

			// 更新（非同期的に実行）
			_asyncUpdateEvent.AddLast( async canceler => {
				var game = Object.FindObjectOfType<Akio.MainGameManager>();
				game.DoGame( canceler.ToToken() ).Forget();
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