//#define NetworkTurn
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using KoganeUnityLib;
using SubmarineMirage;
using SubmarineMirage.Service;
using SubmarineMirage.Network;
using SubmarineMirage.Utility;
using SubmarineMirage.Setting;
using SubmarineMirage.Debug;
namespace TatemonSugoroku.Scripts {



	/// <summary>
	/// ■ ネットワーク手番の描画クラス
	/// </summary>
	public class NetworkTurnView : SMNetworkMonoBehaviourView {
		[SMShow] readonly List<PlayerType> _inputPlayers = new List<PlayerType>();
		[SMShow] readonly List<PlayerType> _readyPlayers = new List<PlayerType>();

		[SMShow] int _currentTurnID	{ get; set; }

		[SMShow] public bool _isInputTurn	=> _inputPlayers.Any( p => ( int )p == _currentTurnID );
		[SMShow] bool _isReady				=> _readyPlayers.Count == EnumUtils.GetLength<PlayerType>();

		public readonly Subject<int> _inputTileIDEvent = new Subject<int>();

		SMGameServerModel _gameServerModel	{ get; set; }
		SMInputManager _inputManager		{ get; set; }
		SMDisplayLog _displayLog { get; set; }

		readonly SMAsyncCanceler _canceler = new SMAsyncCanceler();



		protected override void StartAfterInitialize() {
			base.StartAfterInitialize();

			var networkManager = SMServiceLocator.Resolve<SMNetworkManager>();
			_gameServerModel = networkManager._gameServerModel;
			_inputManager = SMServiceLocator.Resolve<SMInputManager>();
			_displayLog = SMServiceLocator.Resolve<SMDisplayLog>();

			_disposables.AddFirst( () => {
				_canceler.Dispose();

				_inputPlayers.Clear();
				_readyPlayers.Clear();
				_currentTurnID = -1;
				_inputTileIDEvent.Dispose();
			} );

			_disposables.AddFirst(
				_receiveEvent.Subscribe( d => Receive( d ) )
			);

			_disposables.AddFirst(
				_inputManager._touchTileID
					.Where( _ => _isInputTurn )
					.Subscribe( i => _inputTileIDEvent.OnNext( i ) )
			);

			SendInputPlayer();
			var gameManager = FindObjectOfType<Akio.MainGameManager>();
			gameManager.networkTurnView = this;
		}



		protected override void UpdateAfterInitialize() {
			base.UpdateAfterInitialize();
#if NetworkTurn
			_displayLog.Add( string.Join( " : ",
				$"{nameof( _isInputTurn )}",
				$"{_isInputTurn}",
				$"{_currentTurnID}",
				string.Join( ",", _inputPlayers.Select( i => $"{i}" ) )
			) );
			_displayLog.Add( string.Join( " : ",
				$"{nameof( _isReady )}",
				$"{_isReady}",
				string.Join( ",", _readyPlayers.Select( i => $"{i}" ) )
			) );
#endif
		}



		void SendInputPlayer() {
			if ( _gameServerModel._isServer ) {
				var s = ( int )DateTime.Now.TimeOfDay.TotalSeconds;
				UnityEngine.Random.InitState( s );
				var i = UnityEngine.Random.Range( 0, EnumUtils.GetLength<PlayerType>() );
				var serverPlayer = ( PlayerType )i;
				var otherPlayer = ( PlayerType )( ( i + 1 ) % EnumUtils.GetLength<PlayerType>() );
#if NetworkTurn
				SMLog.Debug( $"順番 : {i}, {serverPlayer}, {otherPlayer}" );
#endif
				Send(
					new SMGameServerSendTarget( SMGameServerSendTargetType.Server ),
					new InputPlayerSendData( serverPlayer )
				);
				Send(
					new SMGameServerSendTarget(
						_gameServerModel._type == SMGameServerType.Online	? SMGameServerSendTargetType.Other
																			: SMGameServerSendTargetType.Server
					),
					new InputPlayerSendData( otherPlayer )
				);
			}
		}

		void SendReady() {
			_inputPlayers.ForEach( p => {
				Send(
					new SMGameServerSendTarget( SMGameServerSendTargetType.All ),
					new ReadySendData( p )
				);
			} );
		}

		void SendChangeTurn( int turnID ) {
			if ( _gameServerModel._isServer ) {
				Send(
					new SMGameServerSendTarget( SMGameServerSendTargetType.All ),
					new ChangeTurnSendData( turnID )
				);
			}
		}

		public void SendInputTileID( int tileID ) {
			if ( !_isInputTurn )	{ return; }

			Send(
				new SMGameServerSendTarget( SMGameServerSendTargetType.Other ),
				new InputTileSendData( tileID )
			);
		}



		void Receive( SMGameServerSendData data ) {
			switch ( data ) {
				case InputPlayerSendData inputPlayerData:
					_inputPlayers.Add( inputPlayerData._playerType );
					break;

				case ReadySendData readyData:
					_readyPlayers.Add( readyData._playerType );
					break;

				case ChangeTurnSendData changeTurntData:
					_currentTurnID = changeTurntData._turnID;
					break;

				case InputTileSendData inputTileData:
					_inputTileIDEvent.OnNext( inputTileData._tileID );
					break;
			}
		}



		public async UniTask WaitReady() {
			await UTask.WaitWhile( _canceler, () => _inputPlayers.IsEmpty() );
			SendReady();
			await UTask.WaitWhile( _canceler, () => !_isReady );
			_readyPlayers.Clear();
		}

		public async UniTask WaitChangeTurn( int turnID ) {
			SendChangeTurn( turnID );
			await UTask.WaitWhile( _canceler, () => _currentTurnID != turnID );
			await WaitReady();
		}



		public class InputPlayerSendData : SMGameServerSendData {
			[SMShow] public PlayerType _playerType;

			public InputPlayerSendData() {
			}
			public InputPlayerSendData( PlayerType playerType ) {
				_playerType = playerType;
			}
		}

		public class ReadySendData : SMGameServerSendData {
			[SMShow] public PlayerType _playerType;

			public ReadySendData() {
			}
			public ReadySendData( PlayerType playerType ) {
				_playerType = playerType;
			}
		}

		public class ChangeTurnSendData : SMGameServerSendData {
			[SMShow] public int _turnID;

			public ChangeTurnSendData() {
			}
			public ChangeTurnSendData( int turnID ) {
				_turnID = turnID;
			}
		}

		public class InputTileSendData : SMGameServerSendData {
			[SMShow] public int _tileID;

			public InputTileSendData() {
			}
			public InputTileSendData( int tileID ) {
				_tileID = tileID;
			}
		}
	}
}