using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UniRx;
using KoganeUnityLib;
using SubmarineMirage.Base;
using SubmarineMirage.Utility;
using SubmarineMirage.Debug;
namespace TatemonSugoroku.Scripts {



	/// <summary>
	/// ■ 背景の描画クラス
	/// </summary>
	public class BackgroundView : SMStandardMonoBehaviour {
		readonly Dictionary<DayState, SpriteRenderer> _images = new Dictionary<DayState, SpriteRenderer>();
		readonly SMAsyncCanceler _canceler = new SMAsyncCanceler();



		void Start() {
			var rs = GetComponentsInChildren<SpriteRenderer>( true );
			rs.ForEach( r => {
				r.gameObject.SetActive( true );
				r.color = new Color( 1, 1, 1, 0 );
				var s = r.gameObject.name.ToEnum<DayState>();
				_images[s] = r;
			} );
			_images[DayState.Sun].color = Color.white;

			var day = FindObjectOfType<DayView>();
			day._state.Subscribe( state => ChangeImage( state ).Forget() );

			_disposables.AddFirst( () => {
				_canceler.Dispose();
			} );
		}



		/// <summary>
		/// ● 画像を変更
		/// </summary>
		async UniTask ChangeImage( DayState state ) {
			await _images[state]
				.DOFade( 1, 3 )
				.SetEase( Ease.InQuart )
				.Play()
				.ToUniTask( _canceler );
		}
	}
}