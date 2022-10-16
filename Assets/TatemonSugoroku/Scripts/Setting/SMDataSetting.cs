using System.Collections.Generic;
using SubmarineMirage.File;
using SubmarineMirage.Data;
using SubmarineMirage.Setting;
using Sample;
namespace TatemonSugoroku.Scripts {



	/// <summary>
	/// ■ 登録データの設定クラス
	/// </summary>
	public class SMDataSetting : BaseSMDataSetting {

		/// <summary>
		/// ● 設定
		/// </summary>
		public override void Setup() {
			base.Setup();

			_datas = new Dictionary<SMDataSettingType, List<IBaseSMDataManager>> {
				{
					// セーブデータを登録
					SMDataSettingType.Save,
					new List<IBaseSMDataManager> {
						// 設定データ
						new SettingDataManager(),
						// プレイデータ
						new PlayDataManager(),
					}
				}, {
					// サーバーデータを登録
					SMDataSettingType.Server,
					new List<IBaseSMDataManager> {
					}
				}, {
					// マスターデータを登録
					SMDataSettingType.Master,
					new List<IBaseSMDataManager> {
						// サンプルのアイテムデータ
//						new SMCSVDataManager<string, SampleItemData>(
//							"", "SampleItem", SMFileLocation.Resource, 1 ),
						// 操作説明のデータ
						new SMCSVDataManager<int, SetsumeiItemData>(
							"", "SetsumeiItem", SMFileLocation.Resource, 1 ),
						// オープニングデータ
						new SMCSVDataManager<int, OpeningData>(
							"", "Opening", SMFileLocation.Resource, 1 ),
						// オープニングキャラクターデータ
						new SMCSVDataManager<string, OpeningCharacterData>(
							"", "OpeningCharacter", SMFileLocation.Resource, 1 ),
						// クレジットデータ
						new SMCSVDataManager<int, CreditData>(
							"", "Credit", SMFileLocation.Resource, 1 ),
					}
				},
			};
		}
	}
}