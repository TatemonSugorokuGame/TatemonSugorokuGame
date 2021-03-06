using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using KoganeUnityLib;
using SubmarineMirage.Base;
using SubmarineMirage.Service;
using SubmarineMirage.Utility;
using SubmarineMirage.Setting;
using SubmarineMirage.Debug;
using TatemonSugoroku.Scripts;
using TatemonSugoroku.Scripts.Akio;
namespace TatemonSugoroku.Siren {



	/// <summary>
	/// ■ Siren用、デバッグの描画クラス
	///		このスクリプトをOFFにするだけで、デバッグ挙動は無くなります。
	///		つまり、GameSceneの、DebugゲームオブジェクトをDisableにして下さい。
	/// </summary>
	public class SirenDebugView : SMStandardMonoBehaviour {
		protected override void StartAfterInitialize() {
			// 各種モデルを取得
			var inputManager = SMServiceLocator.Resolve<SMInputManager>();
			var allModelManager = AllModelManager.s_instance;
			var tileManager = FindObjectOfType<TileManagerView>();
			var moveArrowManager = FindObjectOfType<MoveArrowManagerView>();
			var dice = FindObjectOfType<DiceManagerView>();
			var pieces = FindObjectOfType<PieceManagerView>();
			var day = FindObjectOfType<DayView>();
			var tatemons = FindObjectOfType<TatemonManagerView>();
			var background = FindObjectOfType<BackgroundView>();
			var camera = FindObjectOfType<GameCameraView>();

#if false
			// 13番のタイル領域変更
			inputManager.GetKey( SMInputKey.Decide )._enabledEvent.AddLast().Subscribe( _ => {
				var tile = tileManager.GetView( 13 );
				var i = ( ( int )tile._areaType + 1 ) % EnumUtils.GetLength<TileAreaType>();
				tile.ChangeArea( ( TileAreaType )i );
			} );
			// 座標から、13番のタイル領域変更
			inputManager.GetKey( SMInputKey.Reset )._enabledEvent.AddLast().Subscribe( _ => {
				var tile = tileManager.GetView( new Vector2Int( 5, 1 ) );
				var i = ( ( int )tile._areaType + 1 ) % EnumUtils.GetLength<TileAreaType>();
				tile.ChangeArea( ( TileAreaType )i );
			} );

			// タイル領域変更
			var areaType = TileAreaType.None;
			inputManager.GetKey( SMInputKey.Finger2 )._enabledEvent.AddLast().Subscribe( _ => {
				var i = ( ( int )areaType + 1 ) % EnumUtils.GetLength<TileAreaType>();
				areaType = ( TileAreaType )i;
			} );
///*
			// 矢印変更
			var arrowType = MoveArrowType.Down;
			inputManager.GetKey( SMInputKey.Finger2 )._enabledEvent.AddLast().Subscribe( _ => {
				var i = ( ( int )arrowType + 1 ) % EnumUtils.GetLength<MoveArrowType>();
				arrowType = ( MoveArrowType )i;
				SMLog.Debug( $"矢印 : {arrowType}" );
			} );

			// タッチしたタイル番号を表示
			inputManager._touchTileID.Subscribe( id => {
				SMLog.Debug( id );
			} );
			// タッチしたタイルを変更
			inputManager._touchTileID
				.Where( id => id != -1 )
				.Subscribe( id => {
					var tile = tileManager.GetView( id );
					tile.ChangeArea( areaType );

					moveArrowManager.Place(
						0, id,
						new List< KeyValuePair<MoveArrowType, MotionStatus> > {
							new KeyValuePair<MoveArrowType, MotionStatus>( MoveArrowType.Down,	MotionStatus.Unmovable ),
							new KeyValuePair<MoveArrowType, MotionStatus>( MoveArrowType.Left,	MotionStatus.Movable ),
							new KeyValuePair<MoveArrowType, MotionStatus>( MoveArrowType.Right,	MotionStatus.Return ),
							new KeyValuePair<MoveArrowType, MotionStatus>( MoveArrowType.Up,	MotionStatus.Unmovable ),
						}
					);
					UTask.Void( async () => {
						using ( var canceler = new SMAsyncCanceler() ) {
							await UTask.Delay( canceler, 1000 );
							moveArrowManager.Hide();
						}
					} );
				} );
//*/

			// サイコロを投げる
			var diceState = DiceState.Hide;
			inputManager.GetKey( SMInputKey.Quit )._enabledEvent.AddLast().Subscribe( _ => {
				var i = ( ( int )diceState + 1 ) % EnumUtils.GetLength<DiceState>();
				diceState = ( DiceState )i;

				UTask.Void( async () => {
					var ii = await dice.Roll();
					SMLog.Debug( $"出目 : {ii}" );
				} );
			} );

			// コマ1の移動
			var piece1 = pieces.GetView( PlayerType.Player1 );
			inputManager._updateEvent.AddLast()
				.Where( _ => !piece1._isMoving )
				.Select( _ => inputManager.GetAxis( SMInputAxis.Move ) )
				.Where( move => move != Vector2Int.zero )
				.Subscribe( move => {
					var tileMove = new Vector2Int(
						move.x < 0 ? 1 : 0 < move.x ? -1 : 0,
						move.y < 0 ? -1 : 0 < move.y ? 1 : 0
					);
					if ( tileMove.x != 0 ) {
						tileMove.y = 0;
					}
					var tilePosition = piece1._tilePosition + tileMove;
					var id = TileManagerView.ToID( tilePosition );
					piece1.Move( id ).Forget();
				} );

			// コマ2の移動
			var piece2 = pieces.GetView( PlayerType.Player2 );
			inputManager._updateEvent.AddLast()
				.Where( _ => !piece2._isMoving )
				.Select( _ => inputManager.GetAxis( SMInputAxis.Debug ) )
				.Where( move => move != Vector2Int.zero )
				.Subscribe( move => {
					var tileMove = new Vector2Int(
						move.x < 0 ? 1 : 0 < move.x ? -1 : 0,
						move.y < 0 ? -1 : 0 < move.y ? 1 : 0
					);
					if ( tileMove.x != 0 ) {
						tileMove.y = 0;
					}
					var tilePosition = piece2._tilePosition + tileMove;
					var id = TileManagerView.ToID( tilePosition );
					piece2.Move( id ).Forget();
				} );
#endif
			// 日時を更新
			inputManager.GetKey( SMInputKey.Reset )._enabledEvent.AddLast().Subscribe( _ => {
				day.UpdateHour();
			} );
			day._hour.Subscribe( h => {
				SMLog.Debug( $"ゲーム内時刻 : {h}" );
			} );
#if false
			///*
			// タッチしたタイルにたてもんを配置
			var isPlace = true;
			inputManager.GetKey( SMInputKey.Finger2 )._enabledEvent.AddLast().Subscribe( _ => {
				isPlace = !isPlace;
				SMLog.Debug( $"たてもん配置するか？ : {isPlace}" );
			} );
			var tatemonPlayer = PlayerType.Player1;
			inputManager._touchTileID
				.Where( id => isPlace )
				.Where( id => id != -1 )
				.Subscribe( id => {
					tatemons.Place( ( int )tatemonPlayer, id, Random.Range( 1, 6 ) );
					SMLog.Debug( $"たてもん配置 : {tatemonPlayer}" );

					var i = ( ( int )tatemonPlayer + 1 ) % EnumUtils.GetLength<PlayerType>();
					tatemonPlayer = ( PlayerType )i;
				} );
//*/
/*
			// 背景絵を変更
			inputManager.GetKey( SMInputKey.Decide )._enabledEvent.AddLast().Subscribe( _ => {
				background.ChangeImage();
			} );
*/

			inputManager.GetKey( SMInputKey.Reset )._enabledEvent.AddLast().Subscribe( _ => {
				camera.SetResultCamera();
			} );
#endif
		}
	}
}