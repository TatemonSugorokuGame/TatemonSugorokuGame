using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using DG.Tweening;
using SubmarineMirage.Base;
using SubmarineMirage.Service;
using SubmarineMirage.Scene;
using SubmarineMirage.Utility;
using SubmarineMirage.Setting;
using SubmarineMirage.Debug;
using TatemonSugoroku.Scripts;
namespace TatemonSugoroku.Siren {



	/// <summary>
	/// ■ テスト用、回転クラス
	/// </summary>
	public class TestRotate : SMStandardMonoBehaviour {
		readonly SMAsyncCanceler _canceler = new SMAsyncCanceler();



		void Start() {
			_disposables.AddFirst( () => {
				SMLog.Warning( "Dispose" );
				_canceler.Dispose();
			} );

			Rotate().Forget();
		}



		async UniTask Rotate() {
			_canceler._cancelEvent.AddLast().Subscribe( _ => Debug.LogWarning( $"Cancel : {_isDispose}" ) );

			UTask.Void( async () => {
				await UTask.WaitWhile( _canceler, () => !Input.GetKey( KeyCode.Return ) );
				var sceneManager = SMServiceLocator.Resolve<SMSceneManager>();
				sceneManager.GetFSM<MainSMScene>().ChangeState<TitleSMScene>().Forget();
//				_canceler.Cancel();
			} );

			SMLog.Debug( $"Rotate Start : {_isDispose}" );

			await transform
				.DORotate( new Vector3( 0, 360, 0 ), 1, RotateMode.LocalAxisAdd )
				.SetEase( Ease.Linear )
				.SetLoops( -1, LoopType.Restart )
				.Play()
				.ToUniTask( _canceler );

			SMLog.Debug( $"Rotate End : {_isDispose}" );
		}
	}
}