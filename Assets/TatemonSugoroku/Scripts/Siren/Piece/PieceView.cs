//#define TestPiece
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using KoganeUnityLib;
using SubmarineMirage.Base;
using SubmarineMirage.Service;
using SubmarineMirage.Audio;
using SubmarineMirage.Utility;
using SubmarineMirage.Setting;
using SubmarineMirage.Debug;
namespace TatemonSugoroku.Scripts {



	/// <summary>
	/// ■ コマの描画クラス
	/// </summary>
	public class PieceView : SMStandardMonoBehaviour {
		/// <summary>タイルマップの範囲内の大きさ</summary>
		static readonly Vector2Int MAX_SIZE = TileManagerView.MAX_SIZE - Vector2Int.one;

		Renderer[] _renderers { get; set; }
		ParticleSystem[] _particles { get; set; }

		[SerializeField] Vector3 _offset = new Vector3( 0, 0.5f, 0 );
		[SerializeField] float _duration = 5;
		readonly SMAsyncCanceler _canceler = new SMAsyncCanceler();

		PieceType _type { get; set; }
		/// <summary>プレイヤーのタイプ</summary>
		public PlayerType _playerType { get; private set; }

		/// <summary>コマが配置されている、タイル番号</summary>
		public int _tileID { get; private set; }
		/// <summary>コマが配置されている、タイル位置</summary>
		public Vector2Int _tilePosition { get; private set; }

		public bool _isMoving { get; private set; }

		SMAudioManager _audioManager { get; set; }
		float _moveSESecond { get; set; }


		public void Setup( PieceType type, PlayerType playerType ) {
			_renderers = GetComponentsInChildren<SpriteRenderer>( true );
			_particles = GetComponentsInChildren<ParticleSystem>( true );
			_particles.ForEach( p => p.gameObject.SetActive( false ) );

			_type = type;
			_playerType = playerType;

			var tileID = 0;
			switch ( playerType ) {
				case PlayerType.Player1:	tileID = 0;								break;
				case PlayerType.Player2:	tileID = TileManagerView.MAX_ID - 1;	break;
			}
			_tileID = tileID;
			_tilePosition = TileManagerView.ToTilePosition( tileID );
			transform.position = TileManagerView.ToRealPosition( _tilePosition ) + _offset;

			if ( _type == PieceType.Dummy ) {
				_renderers.ForEach( r => {
					var c = r.material.color;
					c.a = 0.5f;
					r.material.color = c;
				} );
				Hide();
			}

			_disposables.AddFirst( () => {
				_canceler.Dispose();
			} );

			UTask.Void( async () => {
				_audioManager = await SMServiceLocator.WaitResolve<SMAudioManager>();
			} );
		}



		protected override void Update() {
			base.Update();

			var p = Camera.main.transform.position;
			p.y = 0;
			transform.rotation = Quaternion.LookRotation( p );
		}



		public async UniTask Move( int tileID ) {
			var tilePosition = TileManagerView.ToTilePosition( tileID );
			await Move( tilePosition );
		}

		public async UniTask Move( Vector2Int tilePosition ) {
			_canceler.Cancel();

			_tilePosition = tilePosition;
/*
// tileIDで、値をClampできないので、コメント
			_tilePosition = new Vector2Int(
				Mathf.Clamp( _tilePosition.x, TileManagerView.MIN_SIZE.x, MAX_SIZE.y ),
				Mathf.Clamp( _tilePosition.y, TileManagerView.MIN_SIZE.y, MAX_SIZE.y )
			);
*/
			_tileID = TileManagerView.ToID( _tilePosition );

			var targetRealPosition = TileManagerView.ToRealPosition( _tilePosition ) + _offset;
			var distance = Vector3.Distance( targetRealPosition, transform.position );
			var jumpCount = Mathf.RoundToInt( distance ) * 3;
			var duration = distance / 2;

			_particles.ForEach( p => p.gameObject.SetActive( true ) );

			if ( _moveSESecond < Time.time ) {
				_audioManager.Play( SMSE.Walk ).Forget();
				_moveSESecond = Time.time + 2;
			}

			try {
				_isMoving = true;
				await transform
					.DOJump( targetRealPosition, 0.2f, jumpCount, duration )
					.SetEase( Ease.Linear )
					.Play()
					.ToUniTask( _canceler );
			} finally {
				_isMoving = false;
			}
			transform.position = targetRealPosition;

			_particles.ForEach( p => p.gameObject.SetActive( false ) );
		}



		public void Place( int tileID )
			=> Place( TileManagerView.ToTilePosition( tileID ) );

		public void Place( Vector2Int tilePosition ) {
			_canceler.Cancel();

			_tilePosition = tilePosition;
/*
// tileIDで、値をClampできないので、コメント
			_tilePosition = new Vector2Int(
				Mathf.Clamp( _tilePosition.x, TileManagerView.MIN_SIZE.x, MAX_SIZE.y ),
				Mathf.Clamp( _tilePosition.y, TileManagerView.MIN_SIZE.y, MAX_SIZE.y )
			);
*/
			_tileID = TileManagerView.ToID( _tilePosition );

			transform.position = TileManagerView.ToRealPosition( _tilePosition ) + _offset;
		}



		public void PlaceArrowPosition( int tileID ) {
			Place( tileID );
			gameObject.SetActive( true );
			_audioManager.Play( SMSE.Place ).Forget();
		}

		public void Hide() {
			gameObject.SetActive( false );
		}
	}
}