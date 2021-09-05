using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using KoganeUnityLib;
using SubmarineMirage.Base;
using SubmarineMirage.Extension;
namespace TatemonSugoroku.Scripts {



	/// <summary>
	/// ■ コマ管理の描画クラス
	/// </summary>
	public class PieceManagerView : SMStandardMonoBehaviour {
		/// <summary>移動方向タイプから、タイル方向への変換</summary>
		public static readonly Dictionary<PieceMoveType, Vector2Int> MOVE_TYPE_TO_TILE_DIRECTION
			= new Dictionary<PieceMoveType, Vector2Int> {
				{ PieceMoveType.None,   new Vector2Int( 0, 0 ) },
				{ PieceMoveType.Down,   new Vector2Int( 0, 1 ) },
				{ PieceMoveType.Left,   new Vector2Int( -1, 0 ) },
				{ PieceMoveType.Right,  new Vector2Int( 1, 0 ) },
				{ PieceMoveType.Up,     new Vector2Int( 0, -1 ) },
			};
		/// <summary>プレイヤー数</summary>
		public static readonly int PLAYER_COUNT = EnumUtils.GetLength<PlayerType>();


		[SerializeField] GameObject _prefab;
		[SerializeField] List<Sprite> _pieceSpritesSetter;

		readonly Dictionary<PlayerType, Sprite> _pieceSprites = new Dictionary<PlayerType, Sprite>();

		public readonly List<PieceView> _views = new List<PieceView>();
		readonly List<PieceView> _dummyViews = new List<PieceView>();



		void Start() {
			_pieceSpritesSetter.ForEach( ( s, i ) => {
				_pieceSprites[( PlayerType )i] = s;
			} );

			EnumUtils.GetValues<PlayerType>().ForEach( type => {
				var go = _prefab.Instantiate( transform );
				var sprite = go.GetComponentInChildren<SpriteRenderer>();
				sprite.sprite = _pieceSprites[type];
				var v = go.GetComponent<PieceView>();
				v.Setup( PieceType.Body, type );
				_views.Add( v );

				go = _prefab.Instantiate( transform );
				sprite = go.GetComponentInChildren<SpriteRenderer>();
				sprite.sprite = _pieceSprites[type];
				v = go.GetComponent<PieceView>();
				v.Setup( PieceType.Dummy, type );
				_dummyViews.Add( v );
			} );
		}



		public PieceView GetView( PlayerType type )
			=> _views[( int )type];

		public PieceView GetDummyView( PlayerType type )
			=> _dummyViews[( int )type];



		public async UniTask Move( int playerID, int tileID ) {
			var v = GetView( ( PlayerType )playerID );
			await v.Move( tileID );
		}

		public async UniTask Move( int playerID, Vector2Int tilePosition ) {
			var v = GetView( ( PlayerType )playerID );
			await v.Move( tilePosition );
		}

		public async UniTask WaitMoves() {
			await _views.Select( v => v.WaitMove() );
			await _dummyViews.Select( v => v.WaitMove() );
		}



		public void Place( int playerID, int tileID ) {
			var v = GetView( ( PlayerType )playerID );
			v.Place( tileID );
		}

		public void Place( int playerID, Vector2Int tilePosition ) {
			var v = GetView( ( PlayerType )playerID );
			v.Place( tilePosition );
		}



		public void PlaceDummy( int playerID ) {
			var tileID = GetView( ( PlayerType )playerID )._tileID;
			var v = GetDummyView( ( PlayerType )playerID );
			v.Show( tileID );
		}

		public void HideDummies() {
			_dummyViews.ForEach( v => v.Hide() );
		}
	}
}