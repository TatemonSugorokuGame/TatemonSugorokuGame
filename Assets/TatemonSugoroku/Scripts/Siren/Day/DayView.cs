using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using SubmarineMirage.Base;
using SubmarineMirage.Service;
using SubmarineMirage.Audio;
using SubmarineMirage.Utility;
using SubmarineMirage.Setting;
namespace TatemonSugoroku.Scripts {
	///========================================================================================================
	/// <summary>
	/// ■ 日時の描画クラス
	///		ターン数ではなく、1日の時間で、環境を遷移させます。
	///		太陽光、花火、たてもん回転など、様々な物に影響を与えます。
	/// </summary>
	///========================================================================================================
	public class DayView : SMStandardMonoBehaviour {
		///----------------------------------------------------------------------------------------------------
		/// ● 要素
		///----------------------------------------------------------------------------------------------------
		/// <summary>1プレイヤーが行える、最大手番</summary>
		const int MAX_PLAYER_TURN = 7;
		/// <summary>全プレイヤーを総合した、最大手番</summary>
		const int MAX_ALL_TURN = MAX_PLAYER_TURN * 2;
		/// <summary>ゲーム開始時の、ゲーム内時刻</summary>
		const int FIRST_HOUR = 12;
		/// <summary>ゲーム終了時の、ゲーム内時刻</summary>
		const int END_HOUR = 21;

		/// <summary>日没の開始時刻</summary>
		public const int FIRST_SUNSET_HOUR = 18;
		/// <summary>完全に日没する時刻</summary>
		public const int END_SUNSET_HOUR = 20;
		/// <summary>日没に要する時間</summary>
		public const int SUNSET_HOUR = END_SUNSET_HOUR - FIRST_SUNSET_HOUR;

		/// <summary>時間が進む速度</summary>
		float _hourVeloctiy { get; set; }
		int turnCount { get; set; }

		SMAudioManager _audioManager { get; set; }

		/// <summary>現在時刻</summary>
		public readonly ReactiveProperty<float> _hour = new ReactiveProperty<float>();
		/// <summary>日の強さの割合</summary>
		public readonly ReactiveProperty<float> _sunsetRate = new ReactiveProperty<float>();
		public readonly ReactiveProperty<DayState> _state = new ReactiveProperty<DayState>();

		///----------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● 初期化
		/// </summary>
		///----------------------------------------------------------------------------------------------------
		void Start() {
			_hour.Value = FIRST_HOUR;
			var delta = END_HOUR - FIRST_HOUR;
			_hourVeloctiy = ( float )delta / MAX_PLAYER_TURN;
			_sunsetRate.Value = 1;

			_disposables.AddFirst( () => {
				_hour.Dispose();
				_sunsetRate.Dispose();
				_state.Dispose();
			} );

			UTask.Void( async () => {
				_audioManager = await SMServiceLocator.WaitResolve<SMAudioManager>();
				ChangeAudio( _state.Value );
			} );

			_state
				.Where( s => _audioManager != null )
				.Subscribe( s => ChangeAudio( s ) );
		}



		void ChangeAudio( DayState state ) {
			switch ( state ) {
				case DayState.Sun:
					_audioManager.Play( SMBGM.Game ).Forget();
					_audioManager.Play( SMBGS.Daytime ).Forget();
					break;
				case DayState.Evening:
					_audioManager.Play( SMBGS.Evening ).Forget();
					break;
				case DayState.Night:
					_audioManager.Play( SMBGM.NightGame ).Forget();
					_audioManager.Play( SMBGS.Wind ).Forget();
					break;
			}
		}



		///----------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● 時間を更新
		///		時間を手番分進めます。
		///		全プレイヤーの手番終了後、呼んで下さい。
		/// </summary>
		///----------------------------------------------------------------------------------------------------
		public void UpdateHour() {
			turnCount += 1;
			switch ( turnCount ) {
				case 0:
				case 1:
				case 2:
					_state.Value = DayState.Sun;
					break;

				case 3:
				case 4:
					_state.Value = DayState.Evening;
					break;

				case 5:
				case 6:
					_state.Value = DayState.Night;
					break;
			}

			_hour.Value = Mathf.Clamp( _hour.Value + _hourVeloctiy, FIRST_HOUR, END_HOUR );

			var delta = END_SUNSET_HOUR - _hour.Value;
			_sunsetRate.Value = Mathf.Clamp01( delta / SUNSET_HOUR );
		}
	}
}