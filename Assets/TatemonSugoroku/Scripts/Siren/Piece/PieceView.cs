//#define TestPiece
using System.Linq;
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

		PieceType _type { get; set; }
		/// <summary>プレイヤーのタイプ</summary>
		public PlayerType _playerType { get; private set; }

		/// <summary>コマが配置されている、タイル番号</summary>
		public int _tileID { get; private set; }
		/// <summary>コマが配置されている、タイル位置</summary>
		public Vector2Int _tilePosition { get; private set; }
		readonly LinkedList<int> _moveTileIDs = new LinkedList<int>();

		public bool _isMoving { get; private set; }

		SMAudioManager _audioManager { get; set; }
		float _moveSESecond { get; set; }

		readonly SMAsyncCanceler _canceler = new SMAsyncCanceler();



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
				_moveTileIDs.Clear();
				_canceler.Dispose();
				_isMoving = false;
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
			var tileID = TileManagerView.ToID( tilePosition );
			_moveTileIDs.Enqueue( tileID );

			await UpdateMove();
		}

		async UniTask UpdateMove() {
			if ( _isMoving ) {
				await WaitMove();
				return;
			}

			try {
				_isMoving = true;
				_particles.ForEach( p => p.gameObject.SetActive( true ) );

				while ( !_moveTileIDs.IsEmpty() ) {
					var lastTileID = _tileID;
					_tileID = _moveTileIDs.Dequeue();
					_tilePosition = TileManagerView.ToTilePosition( _tileID );

					if ( lastTileID != _tileID && _moveSESecond < Time.time ) {
						_audioManager.Play( SMSE.Walk ).Forget();
// TODO : SMSE.Walkの秒数を手打ちしてる・・・
						_moveSESecond = Time.time + 1.515f;
					}

					var targetRealPosition = TileManagerView.ToRealPosition( _tilePosition ) + _offset;
					var distance = Vector3.Distance( targetRealPosition, transform.position );
					var jumpCount = Mathf.RoundToInt( distance ) * 2;
					var duration = distance / 4;

					await transform
						.DOJump( targetRealPosition, 0.2f, jumpCount, duration )
						.SetEase( Ease.Linear )
						.Play()
						.ToUniTask( _canceler );
					transform.position = targetRealPosition;
				}

			} finally {
				if ( !_isDispose ) {
					_moveTileIDs.Clear();
					_particles.ForEach( p => p.gameObject.SetActive( false ) );
					_isMoving = false;
				}
			}
		}

		public UniTask WaitMove()
			=> UTask.WaitWhile( _canceler, () => _isMoving );



		public void Place( int tileID )
			=> Place( TileManagerView.ToTilePosition( tileID ) );

		public void Place( Vector2Int tilePosition ) {
			_canceler.Cancel();

			_tilePosition = tilePosition;
			_tileID = TileManagerView.ToID( _tilePosition );

			transform.position = TileManagerView.ToRealPosition( _tilePosition ) + _offset;
		}



		public void Show( int tileID ) {
			Place( tileID );
			gameObject.SetActive( true );
		}

		public void Hide() {
			gameObject.SetActive( false );
		}
	}
}