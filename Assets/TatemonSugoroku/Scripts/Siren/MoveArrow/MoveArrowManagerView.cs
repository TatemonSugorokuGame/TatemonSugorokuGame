using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using KoganeUnityLib;
using SubmarineMirage.Base;
using SubmarineMirage.Service;
using SubmarineMirage.Audio;
using SubmarineMirage.Extension;
using SubmarineMirage.Utility;
using SubmarineMirage.Setting;
using TatemonSugoroku.Scripts.Akio;
namespace TatemonSugoroku.Scripts {



	/// <summary>
	/// ■ 移動矢印管理の描画クラス
	/// </summary>
	public class MoveArrowManagerView : SMStandardMonoBehaviour {
		public static readonly Dictionary<MoveArrowType, Vector2Int> ARROW_TYPE_TO_ADD_TILE_POSITION
			= new Dictionary<MoveArrowType, Vector2Int> {
				{ MoveArrowType.Up,     new Vector2Int( 0, -1 ) },
				{ MoveArrowType.Right,  new Vector2Int( 1, 0 ) },
				{ MoveArrowType.Down,   new Vector2Int( 0, 1 ) },
				{ MoveArrowType.Left,   new Vector2Int( -1, 0 ) }
			};

		/// <summary>矢印タイプから、角度に変換</summary>
		public static readonly Dictionary<MoveArrowType, Quaternion> ARROW_TYPE_TO_ROTATION
			= new Dictionary<MoveArrowType, Quaternion> {
				{ MoveArrowType.Up,     Quaternion.Euler( 0, 90, 0 ) },
				{ MoveArrowType.Right,  Quaternion.Euler( 0, 180, 0 ) },
				{ MoveArrowType.Down,   Quaternion.Euler( 0, 270, 0 ) },
				{ MoveArrowType.Left,   Quaternion.Euler( 0, 0, 0 ) }
			};

		readonly Dictionary<MoveArrowType, MoveArrowView> _views
			= new Dictionary<MoveArrowType, MoveArrowView>();
		PieceManagerView _pieceManager { get; set; }

		[SerializeField] GameObject _prefab;
		[SerializeField] public Color _disableColor = Color.gray;
		[SerializeField] Color _enableColor = Color.yellow;
		[SerializeField] Color _flashColor = Color.white;

		public Color _color { get; private set; }
		Tween _colorTween { get; set; }

		bool _isActive { get; set; }

		SMAudioManager _audioManager { get; set; }



		void Start() {
			_pieceManager = FindObjectOfType<PieceManagerView>();

			EnumUtils.GetValues<MoveArrowType>().ForEach( type => {
				var go = _prefab.Instantiate( transform );
				var v = go.GetComponent<MoveArrowView>();
				v.Setup( type, _disableColor );
				_views[type] = v;
			} );

			_color = _enableColor;
			_colorTween = DOTween.To(
				() => _color,
				c => {
					_color = c;
					_views.ForEach( pair => pair.Value.UpdateColor( c ) );
				},
				_flashColor,
				1
			)
			.SetEase( Ease.InOutQuart )
			.SetLoops( -1, LoopType.Yoyo )
			.Play();

			_disposables.AddFirst( () => {
				_colorTween?.Kill();
			} );

			UTask.Void( async () => {
				_audioManager = await SMServiceLocator.WaitResolve<SMAudioManager>();
			} );
		}



		public MoveArrowView GetView( MoveArrowType type )
			=> _views[type];



		public void Place( int playerID, int tileID,
							IEnumerable< KeyValuePair<MoveArrowType, MotionStatus> > arrowDatas, bool isInputTurn
		) {
			Hide();
			_isActive = true;

			if ( isInputTurn ) {
				_audioManager.Play( SMSE.Place ).Forget();

				arrowDatas.ForEach( pair => {
					var type = pair.Key;
					var state = pair.Value;
					var tilePosition =
						TileManagerView.ToTilePosition( tileID ) + ARROW_TYPE_TO_ADD_TILE_POSITION[type];
					var data = new MoveArrowData( tilePosition, type, state );

					var v = GetView( type );
					v.Place( data );
				} );
			}

			_pieceManager.Move( playerID, tileID ).Forget();
		}

		public void Hide() {
			_isActive = false;
			_views.ForEach( pair => pair.Value.Hide() );
		}
	}
}