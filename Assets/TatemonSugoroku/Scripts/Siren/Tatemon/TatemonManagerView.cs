using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using KoganeUnityLib;
using SubmarineMirage.Base;
using SubmarineMirage.Extension;
using SubmarineMirage.Debug;
namespace TatemonSugoroku.Scripts {



	/// <summary>
	/// ■ たてもん管理の描画クラス
	/// </summary>
	public class TatemonManagerView : SMStandardMonoBehaviour {
		/// <summary>最大の1プレイヤーの手番</summary>
		const int MAX_PLAYER_TURN = 7;

		[SerializeField] GameObject _prefab;
		[SerializeField] float _speedRate = 1;

		readonly Dictionary<PlayerType, List<TatemonView>> _views
			= new Dictionary<PlayerType, List<TatemonView>>();
		/// <summary>プレイヤーごとの、現在の手番回数</summary>
		readonly Dictionary<PlayerType, int> _turnCounts = new Dictionary<PlayerType, int>();



		void Start() {
			EnumUtils.GetValues<PlayerType>().ForEach( type => {
				_views[type] = Enumerable.Range( 0, MAX_PLAYER_TURN )
					.Select( i => {
						var go = _prefab.Instantiate( transform );
						var v = go.GetComponent<TatemonView>();
						v.Setup( type, i, _speedRate );
						return v;
					} )
					.ToList();
			} );

			EnumUtils.GetValues<PlayerType>()
				.ForEach( type => _turnCounts[type] = 0 );

/*
			firework._launch.Subscribe( _ => {
				GetViews()
					.Where( m => m._isPlaced )
					.ForEach( m => {
						m._speedRate *= 2;
						m.Rotate();
					} );
			} );
*/
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

			var m = GetView( type, turnID );
			_turnCounts[type] += 1;

			m.Place( tilePosition, rotatePower );
		}
	}
}