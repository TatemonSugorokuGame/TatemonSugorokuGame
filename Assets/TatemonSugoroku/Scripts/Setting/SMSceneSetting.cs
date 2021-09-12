using System;
using SubmarineMirage.FSM;
using SubmarineMirage.Scene;
using Sample;
namespace TatemonSugoroku.Scripts {



	/// <summary>
	/// ■ シーンデータの設定クラス
	///		登録すると、SMSceneManagerから遷移できる。
	/// </summary>
	public class SMSceneSetting : BaseSMSceneSetting {

		/// <summary>
		/// ● 設定
		/// </summary>
		public override void Setup() {
			_datas = new SMFSMGenerateList {
				{
					// アプリ終了まで、永続読込のシーン
					new Type[] { typeof( ForeverSMScene ), },
					typeof( ForeverSMScene )
				}, {
					// アクティブ状態とする、メインシーン
					// ここに、Unityシーンと対応する、クラスを登録（シーン名 + SMScene）
					new Type[] {
						typeof( UnknownSMScene ),

						// たてもんすごろくのシーン
						typeof( TitleSMScene ),
						typeof( CreditSMScene ),
						typeof( NetworkSMScene ),
						typeof( GameSMScene ),
						typeof( ResultSMScene ),

						// テストシーン
						typeof( Siren.SirenSMScene ),

						// サンプルのシーン
						typeof( SampleTitleSMScene ),
						typeof( SampleGameSMScene ),
						typeof( SampleGameOverSMScene ),
						typeof( SampleGameClearSMScene ),
					},
					typeof( MainSMScene )
				}, {
					// UI配置専用のシーン
					new Type[] {
						typeof( UINoneSMScene ),
						// たてもんすごろくの操作説明のシーン
						typeof( UIHelpSMScene ),
					},
					typeof( UISMScene )
				}, {
					// デバッグ用のシーン
					new Type[] { typeof( DebugSMScene ), },
					typeof( DebugSMScene )
				},
			};
		}
	}
}