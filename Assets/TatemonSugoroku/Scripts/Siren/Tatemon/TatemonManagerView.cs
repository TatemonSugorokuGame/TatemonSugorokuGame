using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using KoganeUnityLib;
using SubmarineMirage.Base;
using SubmarineMirage.Extension;
using SubmarineMirage.Setting;
using SubmarineMirage.Debug;
namespace TatemonSugoroku.Scripts {



	/// <summary>
	/// ■ たてもん管理の描画クラス
	/// </summary>
	public class TatemonManagerView : SMStandardMonoBehaviour {
		/// <summary>最大の1プレイヤーの手番</summary>
		const int MAX_PLAYER_TURN = 7;
		public const int MAX_ROTATE_POWER = 12;

		static readonly List<SMVoice> VOICES = new List<SMVoice> {
			SMVoice.Wasshoi1,
			SMVoice.Wasshoi2,
			SMVoice.Wasshoi3,
			SMVoice.Wasshoi4,
		};


		[SerializeField] GameObject _prefab;
		[SerializeField] List<Sprite> _auraSpritesSetter;
		[SerializeField] List<Sprite> _tatemonSpritesSetter;
		[SerializeField] List<Sprite> _numberSpritesSetter;
		[SerializeField] float _maxSpeed = 4;

		readonly Dictionary<PlayerType, Sprite> _auraSprites = new Dictionary<PlayerType, Sprite>();
		readonly Dictionary<int, Sprite> _tatemonSprites = new Dictionary<int, Sprite>();
		readonly Dictionary<int, Sprite> _numberSprites = new Dictionary<int, Sprite>();

		readonly Dictionary<PlayerType, List<TatemonView>> _views
			= new Dictionary<PlayerType, List<TatemonView>>();
		/// <summary>プレイヤーごとの、現在の手番回数</summary>
		readonly Dictionary<PlayerType, int> _turnCounts = new Dictionary<PlayerType, int>();



		void Start() {
			_auraSpritesSetter.ForEach( ( s, i ) => {
				_auraSprites[( PlayerType )i] = s;
			} );
			_tatemonSpritesSetter.ForEach( s => {
				var i = s.name.Replace( "tatemon", "" ).ToInt();
				_tatemonSprites[i] = s;
			} );
			_numberSpritesSetter.ForEach( s => {
				var i = s.name.ToInt();
				_numberSprites[i] = s;
			} );

			EnumUtils.GetValues<PlayerType>().ForEach( type => {
				_views[type] = Enumerable.Range( 0, MAX_PLAYER_TURN )
					.Select( i => {
						var go = _prefab.Instantiate( transform );
						var v = go.GetComponent<TatemonView>();
						v.Setup( type, i, _maxSpeed );
						return v;
					} )
					.ToList();
			} );

			EnumUtils.GetValues<PlayerType>()
				.ForEach( type => _turnCounts[type] = 0 );
		}



		///----------------------------------------------------------------------------------------------------
		/// ● 取得
		///----------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● プレイヤー番号の、たてもんの一覧を取得
		/// </summary>
		public TatemonView GetView( PlayerType playerType, int turnID )
			=> _views[playerType][turnID];

		/// <summary>
		/// ● 全たてもんの一覧を取得
		/// </summary>
		public IEnumerable<TatemonView> GetViews()
			=> _views.SelectMany( pair => pair.Value );

		public SMVoice GetRandomVoice() {
			var i = UnityEngine.Random.Range( 0, VOICES.Count );
			return VOICES[i];
		}

		///----------------------------------------------------------------------------------------------------
		/// ● 配置
		///----------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● プレイヤー番号と、タイル番号の場所に、たてもんを配置
		///		何個目を配置するかは、自動計算。
		/// </summary>
		public void Place( int playerID, int tileID, int rotatePower ) {
			var tilePosition = TileManagerView.ToTilePosition( tileID );
			Place( playerID, tilePosition, rotatePower );
		}

		/// <summary>
		/// ● プレイヤー番号と、タイル位置に、たてもんを配置
		///		何個目を配置するかは、自動計算。
		/// </summary>
		public void Place( int playerID, Vector2Int tilePosition, int rotatePower ) {
			var type = ( PlayerType )playerID;
			var turnID = _turnCounts[type];

			if ( turnID >= MAX_PLAYER_TURN ) {
				throw new InvalidOperationException( string.Join( "\n",
					$"",
					$"最大ターン数以上に、配置が要求されました。",
					$"これ以上、たてもんを配置できません。",
					$"{nameof( turnID )} : {turnID}",
					$"{nameof( MAX_PLAYER_TURN )} : {MAX_PLAYER_TURN}",
					$"{this}"
				) );
			}

			_views
				.SelectMany( pair => pair.Value )
				.Where( v => v._tilePosition == tilePosition )
				.ForEach( v => v.ChangeState( TatemonState.None ) );

			var m = GetView( type, turnID );
			_turnCounts[type] += 1;

			var tatemonSprite = _tatemonSprites[rotatePower];
			var auraSprite = _auraSprites[type];
			var numberSprite = _numberSprites[rotatePower];
			var voice = GetRandomVoice();

//			SMLog.Debug( $"{nameof( rotatePower )} : {rotatePower}" );
			m.Place( tilePosition, rotatePower, tatemonSprite, auraSprite, numberSprite, voice );
		}
	}
}