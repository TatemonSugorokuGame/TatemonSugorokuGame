using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using Cysharp.Threading.Tasks;
using SubmarineMirage;
using SubmarineMirage.Service;
using SubmarineMirage.File;
using SubmarineMirage.Data.Raw;
using SubmarineMirage.Data.Save;
using SubmarineMirage.Extension;
using SubmarineMirage.Utility;
using SubmarineMirage.Setting;
namespace TatemonSugoroku.Scripts {
	///========================================================================================================
	/// <summary>
	/// ■ プレイ情報のクラス
	///		暗号化され、保存される。
	/// </summary>
	///========================================================================================================
	public class PlayData : BaseSMSaveData {
		///----------------------------------------------------------------------------------------------------
		/// ● 要素
		///----------------------------------------------------------------------------------------------------
		/// <summary>保存番号</summary>
		[SMShow] public int _saveID;
		/// <summary>一度でも保存したか？</summary>
		[SMShow] public bool _isSave;
		/// <summary>日付</summary>
		[SMShow] public DateTime _date;

		/// <summary>スクリーンショット画像の、生情報一覧</summary>
		public List<SMTextureRawData> _pictureRawData = new List<SMTextureRawData>();

		/// <summary>スクリーンショット画像の、情報一覧</summary>
		// Sprite型は、そのまま保存できない
		[IgnoreDataMember] public List<Sprite> _pictures = new List<Sprite>();

		///----------------------------------------------------------------------------------------------------
		/// ● 作成、削除
		///----------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● コンストラクタ（読込用）
		/// </summary>
		public PlayData() {
		}

		/// <summary>
		/// ● コンストラクタ（書込用）
		/// </summary>
		public PlayData( int saveID ) {
			_saveID = saveID;
		}

		/// <summary>
		/// ● 解放（補助）
		/// </summary>
		protected override void DisposeSub() {
			base.DisposeSub();

			ResetRawData();
			_pictures.Clear();
		}

		/// <summary>
		/// ● 生情報をリセット
		/// </summary>
		void ResetRawData() {
			_pictureRawData.ForEach( d => d.Dispose() );
			_pictureRawData.Clear();
		}

		///----------------------------------------------------------------------------------------------------
		/// ● 読み書き
		///----------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● 読込
		/// </summary>
		public override async UniTask Load() {
			// 写真生情報から、スプライトを読込
			_pictures = _pictureRawData
				.Select( data => data.ToSprite() )
				.ToList();
			ResetRawData();

			await base.Load();
		}

		/// <summary>
		/// ● 保存
		/// </summary>
		public override async UniTask Save() {
			var loader = SMServiceLocator.Resolve<SMFileManager>().Get<SMFileLoader>();

			// 生情報に変換
			ResetRawData();
			_pictureRawData = _pictures
				.Select( data => data.ToRawData( SMTextureRawData.Type.JPG, 100 ) ) // JPG形式に変換
				.ToList();

			// フォルダ内の、元々ある写真を削除
			var path = Path.Combine( SMMainSetting.SAVE_EXTERNAL_PATH, SMMainSetting.PICTURE_FILE_PATH, $"{_saveID}" );
			PathSMUtility.Delete( path );

			// 写真をJPG形式で非同期保存
			await _pictureRawData.Select( async ( data, i ) => {
				var name = string.Format( SMMainSetting.PICTURE_FILE_NAME_FORMAT, i + 1 );
				var p = Path.Combine( SMMainSetting.PICTURE_FILE_PATH, $"{_saveID}", name );
				await loader.SaveExternal( p, data._data );
			} );

			_isSave = true;

			await base.Save();
		}

		///----------------------------------------------------------------------------------------------------
		/// ● 登録、解除
		///----------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● 写真を登録
		/// </summary>
		public void RegisterPicture( Sprite sprite )
			=> _pictures.Add( sprite );
	}
}