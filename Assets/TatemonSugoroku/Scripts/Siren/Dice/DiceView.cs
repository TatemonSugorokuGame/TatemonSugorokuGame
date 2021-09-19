#define TestDice
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using KoganeUnityLib;
using SubmarineMirage;
using SubmarineMirage.Base;
using SubmarineMirage.Service;
using SubmarineMirage.Audio;
using SubmarineMirage.Network;
using SubmarineMirage.Utility;
using SubmarineMirage.Setting;
using SubmarineMirage.Debug;
namespace TatemonSugoroku.Scripts {



	/// <summary>
	/// ■ サイコロの描画クラス
	/// </summary>
	public class DiceView : SMNetworkMonoBehaviourView {
		Rigidbody _rigidbody		{ get; set; }
		Transform[] _transforms		{ get; set; }
		Vector3 _firstPosition		{ get; set; }
		ParticleSystem _particle	{ get; set; }

		[SMShow] int _diceID			{ get; set; }
		[SMShow] DiceState _state		{ get; set; }
		[SMShow] public Vector3 _power	{ get; set; }
		[SMShow] public int _value		{ get; set; }

		SMAudioManager _audioManager		{ get; set; }
		SMGameServerModel _gameServerModel	{ get; set; }
		SMDisplayLog _displayLog			{ get; set; }

		readonly SMAsyncCanceler _canceler = new SMAsyncCanceler();



		protected override void StartAfterInitialize() {
			base.StartAfterInitialize();

			_audioManager = SMServiceLocator.Resolve<SMAudioManager>();
			_gameServerModel = SMServiceLocator.Resolve<SMNetworkManager>()._gameServerModel;
			_displayLog = SMServiceLocator.Resolve<SMDisplayLog>();

			_rigidbody = GetComponent<Rigidbody>();
			_transforms = transform.GetChildren( true )
				.Where( ( go, i ) => i < 6 )
				.Select( go => go.transform )
				.ToArray();
			_particle = GetComponentInChildren<ParticleSystem>( true );

			_state = DiceState.Roll;
			ChangeState( DiceState.Hide ).Forget();

			_receiveEvent.Subscribe( Receive );


			_disposables.AddFirst( () => {
				_canceler.Dispose();
			} );
		}



		protected override void UpdateAfterInitialize() {
			switch ( _state ) {
				case DiceState.Rotate:
					if ( _objectType == SMNetworkObjectType.Mine ) {
						transform.position = _firstPosition;
					}
					break;
			}
#if TestDice
			_displayLog.Add( $"Dice {_diceID} : {_value}, {_state}" );
#endif
		}

		void OnCollisionEnter( Collision collision ) {
			_particle.transform.position = collision.contacts.First().point;
			_particle.Play();
			_audioManager?.Play( SMSE.DiceHit ).Forget();
		}

		async UniTask ChangeState( DiceState state ) {
			if ( _state == state )	{ return; }
			_state = state;

			_canceler.Cancel();
			_value = -1;

			switch ( _state ) {
				case DiceState.Hide:
					gameObject.SetActive( false );
					if ( _objectType != SMNetworkObjectType.Mine )	{ return; }

					transform.position = _firstPosition;
					return;

				case DiceState.Rotate:
					gameObject.SetActive( true );
					if ( _objectType != SMNetworkObjectType.Mine )	{ return; }

					transform.position = _firstPosition;
					_rigidbody.useGravity = false;
					_rigidbody.AddTorque(
						new Vector3(
							Random.Range( -10, 10 ),
							Random.Range( -10, 10 ),
							Random.Range( -10, 10 )
						),
						ForceMode.Impulse
					);
					return;

				case DiceState.Roll:
					gameObject.SetActive( true );
					if ( _objectType != SMNetworkObjectType.Mine )	{ return; }

					var tempPower = _power;
					if ( tempPower == default ) {
						tempPower = new Vector3(
							Random.Range( -1, 1 ),
							Random.Range( -1, 1 ),
							Random.Range( -1, 1 )
						).normalized * 10;
//						tempPower = transform.forward * 10,
//						tempPower = new Vector3( -1, 0.1f, 1 ) * 10,
					}
					_rigidbody.useGravity = true;
					_rigidbody.AddForce(
						tempPower,
						ForceMode.Impulse
					);
					await UTask.WaitWhile( _canceler, () => !_rigidbody.IsSleeping() );
					await UTask.Delay( _canceler, 500 );
					CalculateValue();
					SendDiceValue();
					return;
			}

			void CalculateValue() {
				var maxY = 0f;
				var value = 0;
				_transforms
					.Where( t => t.position.y > maxY )
					.ForEach( t => {
						maxY = t.position.y;
						value = t.gameObject.name.ToInt();
					} );
				_value = value;
			}
		}



		public void SendDiceID( int diceID ) {
			if ( _objectType != SMNetworkObjectType.Mine )	{ return; }

			Send(
				new SMGameServerSendTarget( SMGameServerSendTargetType.All ),
				new IDSendData( diceID )
			);
		}

		void SendDiceValue() {
			if ( _objectType != SMNetworkObjectType.Mine ) { return; }

			Send(
				new SMGameServerSendTarget( SMGameServerSendTargetType.All ),
				new ValueSendData( _value )
			);
		}

		public void SendChangeState( DiceState state ) {
			Send(
				new SMGameServerSendTarget( SMGameServerSendTargetType.All ),
				new ChangeStateSendData( state )
			);
		}



		void Receive( SMGameServerSendData data ) {
			switch ( data ) {
				case IDSendData idData:
					_diceID = idData._diceID;
					switch ( _diceID ) {
						case 0:	transform.position = new Vector3( -1, 5, 0 );	break;
						case 1:	transform.position = new Vector3( 1, 5, 0 );	break;
					}
					_firstPosition = transform.position;

					var manager = FindObjectOfType<DiceManagerView>();
					manager.Register( this );
					break;

				case ValueSendData valueData:
					_value = valueData._value;
					break;

				case ChangeStateSendData changeStateData:
					ChangeState( changeStateData._state ).Forget();
					break;
			}
		}



		public class IDSendData : SMGameServerSendData {
			[SMShow] public int _diceID;

			public IDSendData() {
			}
			public IDSendData( int diceID ) {
				_diceID = diceID;
			}
		}

		public class ValueSendData : SMGameServerSendData {
			[SMShow] public int _value;

			public ValueSendData() {
			}
			public ValueSendData( int value ) {
				_value = value;
			}
		}

		public class ChangeStateSendData : SMGameServerSendData {
			[SMShow] public DiceState _state;

			public ChangeStateSendData() {
			}
			public ChangeStateSendData( DiceState state ) {
				_state = state;
			}
		}
	}
}