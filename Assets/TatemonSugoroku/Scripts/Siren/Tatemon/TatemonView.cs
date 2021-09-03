using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniRx;
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
	/// ■ たてもんの描画クラス
	/// </summary>
	public class TatemonView : SMStandardMonoBehaviour {
		readonly Dictionary<string, SpriteRenderer> _renderers = new Dictionary<string, SpriteRenderer>();
		readonly List<ParticleSystem> _particles = new List<ParticleSystem>();
		PieceManagerView _pieceManager { get; set; }
		SMAudioManager _audioManager { get; set; }

		TatemonState _state { get; set; }
		/// <summary>プレイヤーのタイプ</summary>
		public PlayerType _playerType { get; private set; }

		/// <summary>手番の回数</summary>
		public int _turnID { get; private set; }
		/// <summary>タイル番号</summary>
		public int _tileID { get; private set; }
		/// <summary>タイル位置</summary>
		public Vector2Int _tilePosition { get; private set; }

		public float _maxSpeed		{ get; set; }
		int _rotatePower	{ get; set; }

		SMVoice _voice { get; set; }

		/// <summary>タイルが配置済みか？</summary>
		public bool _isPlaced => _state != TatemonState.None;
		bool _isNearPiece { get; set; }
		float _placeEndSecond { get; set; }

		readonly SMAsyncCanceler _canceler = new SMAsyncCanceler();



		public void Setup( PlayerType playerType, int turnID, float maxSpeed ) {
			_playerType = playerType;
			_turnID = turnID;
			_maxSpeed = maxSpeed;

			gameObject.name = $"Tatemon {_playerType} {_turnID}";

			_pieceManager = FindObjectOfType<PieceManagerView>();

			var rs = GetComponentsInChildren<SpriteRenderer>();
			rs.ForEach( r => {
				_renderers[r.gameObject.name] = r;
			} );
			GetComponentsInChildren<ParticleSystem>().ForEach( p => {
				_particles.Add( p );
			} );

			_state = TatemonState.Place;
			ChangeState( TatemonState.None );

			_disposables.AddFirst( () => {
				_canceler.Dispose();
			} );

			UTask.Void( async () => {
				_audioManager = await SMServiceLocator.WaitResolve<SMAudioManager>();
			} );
		}



		protected override void Update() {
			base.Update();

			if ( _placeEndSecond < Time.time ) {
				var isNear = _pieceManager._views.Any( v => {
					var delta = v.transform.position - transform.position;
					var distance = delta.magnitude;
					return distance < 1;
				} );

				if ( _isNearPiece != isNear ) {
					_isNearPiece = isNear;
					_renderers.ForEach( pair => {
						var c = pair.Value.color;
						c.a = _isNearPiece ? 0.5f : 1;
						pair.Value.color = c;
					} );
				}
			}

			_renderers["Number"].transform.rotation = Quaternion.Euler( 90, 0, 0 );
			_renderers["Number"].color = Color.white;
		}



		public void ChangeState( TatemonState state ) {
			if ( _state == state )	{ return; }
			_state = state;

			transform.position = TileManagerView.ToRealPosition( _tilePosition ) + new Vector3( 0, 0.5f, 0 );
			transform.localScale = TileManagerView.REAL_SCALE;

			switch ( _state ) {
				case TatemonState.None:
					_canceler.Cancel();
					gameObject.SetActive( false );
					break;

				case TatemonState.Place:
					gameObject.SetActive( true );
					var scale = transform.localScale;
					transform.localScale = Vector3.zero;
					transform
						.DOScale( scale, 1 )
						.SetEase( Ease.OutBounce )
						.Play();
					_audioManager?.Play( SMSE.Tatemon ).Forget();
					_audioManager?.Play( _voice ).Forget();
					Rotate().Forget();
					_placeEndSecond = Time.time + 3;
					break;
			}
		}



		async UniTask Rotate() {
			_canceler.Cancel();

			var speed =
				Mathf.Pow( _rotatePower, 2 ) / Mathf.Pow( TatemonManagerView.MAX_ROTATE_POWER, 2 )
				* _maxSpeed;
			var duration = 1 / speed;

			await transform
				.DORotate( new Vector3( 0, 360, 0 ), duration, RotateMode.LocalAxisAdd )
				.SetEase( Ease.Linear )
				.SetLoops( -1, LoopType.Restart )
				.Play()
				.ToUniTask( _canceler );
		}



		///----------------------------------------------------------------------------------------------------
		/// ● 配置
		///----------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● タイル番号に、たてもんを配置
		/// </summary>
		public void Place( int tileID, int rotatePower, Sprite tatemon, Sprite aura, Sprite number, SMVoice voice )
			=> Place( TileManagerView.ToTilePosition( tileID ), rotatePower, tatemon, aura, number, voice );

		/// <summary>
		/// ● タイル位置に、たてもんを配置
		/// </summary>
		public void Place( Vector2Int tilePosition, int rotatePower,
							Sprite tatemon, Sprite aura, Sprite number, SMVoice voice
		) {
			_renderers["Body"].sprite = tatemon;
			_renderers["Aura"].sprite = aura;
			_renderers["Number"].sprite = number;
			_voice = voice;

//			_particles.ForEach( p => p.Play() );

			_tilePosition = tilePosition;
			_tileID = TileManagerView.ToID( _tilePosition );
			_rotatePower = rotatePower;
			ChangeState( TatemonState.Place );
		}
	}
}