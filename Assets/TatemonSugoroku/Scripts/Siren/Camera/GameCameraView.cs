using UnityEngine;
using UniRx;
using KoganeUnityLib;
using SubmarineMirage.Base;
using SubmarineMirage.Service;
using SubmarineMirage.Extension;
using SubmarineMirage.Utility;
using SubmarineMirage.Setting;
namespace TatemonSugoroku.Scripts {



	/// <summary>
	/// ■ ゲームシーンのカメラの描画クラス
	/// </summary>
	public class GameCameraView : SMStandardMonoBehaviour {
		GameCameraState _state { get; set; }

		[SerializeField] Vector3 _offset = new Vector3( 0, 1, 0 );
		Vector3 _lookAtPosition { get; set; }
		Vector3 _angles;

		SMInputManager _inputManager { get; set; }



		protected override void StartAfterInitialize() {
			_angles = transform.eulerAngles;
			_angles.z = 0;

			_inputManager = SMServiceLocator.Resolve<SMInputManager>();
			_disposables.AddFirst(
				_inputManager.GetKey( SMInputKey.Finger2 )._enablingEvent.AddLast()
					.Where( _ => _state == GameCameraState.Input )
					.Subscribe( _ => {
						var axis = _inputManager.GetAxis( SMInputAxis.Rotate );
						var add = new Vector3( -axis.y, axis.x, 0 ) * 200 * Time.deltaTime;
						RotateCamera( add );
					} )
			);
		}



		protected override void Update() {
			base.Update();

			switch ( _state ) {
				case GameCameraState.Result:
					RotateCamera( new Vector3( 0, -10 * Time.deltaTime, 0 ) );
					break;
			}

			transform.LookAt( _lookAtPosition + _offset );
		}



		void RotateCamera( Vector3 addAxis ) {
			_angles += addAxis;
			_angles = new Vector3(
				Mathf.Clamp( _angles.x, 12, 89 ),
				Mathf.Repeat( _angles.y, 360 ),
				0
			);
			transform.eulerAngles = _angles;
			transform.position = transform.forward * -10;

			transform.LookAt( _lookAtPosition + _offset );
		}



		public void LookAt( Vector3 position )
			=> _lookAtPosition = position;



		void ChangeState( GameCameraState state ) {
			_state = state;
		}



		// ゲームマネージャーさん、これ呼んで下さい・・・。
		public void SetResultCamera() {
			_angles.x = 30;
			_offset = Vector3.up * 3;
			_state = GameCameraState.Result;
		}
	}
}