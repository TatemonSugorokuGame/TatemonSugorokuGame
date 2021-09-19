using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using KoganeUnityLib;
using SubmarineMirage.Base;
using SubmarineMirage.Service;
using SubmarineMirage.Audio;
using SubmarineMirage.Network;
using SubmarineMirage.Extension;
using SubmarineMirage.Utility;
using SubmarineMirage.Setting;
using SubmarineMirage.Debug;
namespace TatemonSugoroku.Scripts {



	/// <summary>
	/// ■ サイコロ管理の描画クラス
	/// </summary>
	public class DiceManagerView : SMStandardMonoBehaviour {
		public static readonly int MAX_COUNT = 2;

		
		Vector3 _power { get; set; }

		readonly List<DiceView> _views = new List<DiceView>();
		readonly SMAsyncCanceler _canceler = new SMAsyncCanceler();

		public int _total { get; private set; }

		public readonly Subject<int> _totalEvent = new Subject<int>();
		public readonly ReactiveProperty<DiceState> _state = new ReactiveProperty<DiceState>();



		protected override void StartAfterInitialize() {
			base.StartAfterInitialize();

			var gameServerModel = SMServiceLocator.Resolve<SMNetworkManager>()._gameServerModel;
			if ( gameServerModel._isServer ) {
				MAX_COUNT.Times( i => {
					var go = gameServerModel.Instantiate( "Prefab/Dice" );
					go.SetParent( transform );
					var view = go.GetComponent<DiceView>();
					view.SendDiceID( i );
				} );
			}

			_disposables.AddFirst( () => {
				_totalEvent.Dispose();
				_canceler.Dispose();
			} );
		}

		public UniTask WaitSetup()
			=> UTask.WaitWhile( _canceler, () => _views.Count != MAX_COUNT );



		public void Register( DiceView dice )
			=> _views.Add( dice );



		public List<int> GetValues()
			=> _views
				.Select( v => v._value )
				.ToList();



		public void SetPower( Vector3 power = default ) {
			_power = power;
			_views.ForEach( v => v._power = power );
		}



		public async UniTask ChangeState( DiceState state, bool isInputTurn ) {
			_canceler.Cancel();
			_state.Value = state;

			if ( _state.Value == DiceState.Roll ) {
				_views.ForEach( v => v._value = -1 );
			}
			if ( isInputTurn ) {
				_views.ForEach( v => v.SendChangeState( _state.Value ) );
			}
			if ( _state.Value == DiceState.Roll ) {
				await UTask.WaitWhile( _canceler, () => _views.Any( v => v._value == -1 ) );
				_total = _views.Sum( v => v._value );
				_totalEvent.OnNext( _total );
			}
		}

		public async UniTask<int> Roll( bool isInputTurn ) {
			await UTask.DelayFrame( _canceler, 1 );

			var audioManager = await SMServiceLocator.WaitResolve<SMAudioManager>();
			audioManager.Play( SMSE.Dice ).Forget();

			await ChangeState( DiceState.Roll, isInputTurn );
			UTask.Void( async () => {
				await UTask.Delay( _canceler, 5000 );
				await ChangeState( DiceState.Hide, isInputTurn );
			} );
			return _total;
		}
	}
}