using UniRx;
using Cysharp.Threading.Tasks;
using SubmarineMirage.Service;
using SubmarineMirage.Audio;
using SubmarineMirage.Scene;
using SubmarineMirage.Network;
using SubmarineMirage.Utility;
using SubmarineMirage.Setting;
namespace TatemonSugoroku.Scripts {



	/// <summary>
	/// ■ ネットワークシーンのクラス
	/// </summary>
	public class NetworkSMScene : MainSMScene {

		/// <summary>
		/// ● コンストラクタ
		/// </summary>
		public NetworkSMScene() {

			// シーン初期化
			_enterEvent.AddLast( async canceler => {
				var audioManager = SMServiceLocator.Resolve<SMAudioManager>();
				audioManager.Play( SMBGS.Wind ).Forget();

				var gameServer = SMServiceLocator.Resolve<SMNetworkManager>()._gameServerModel;
				if ( await gameServer.Disconnect() ) {
				}
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