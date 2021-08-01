//---------------------------------------------------------------------------------------------------------
// ▽ Submarine Mirage Framework for Unity
//		Copyright (c) 2020 夢想海の水底より(from Seabed of Reverie)
//		Released under the MIT License :
//			https://github.com/FromSeabedOfReverie/SubmarineMirageFrameworkForUnity/blob/master/LICENSE
//---------------------------------------------------------------------------------------------------------
namespace SubmarineMirage {
	using System;
	using UnityEngine;
	using Cysharp.Threading.Tasks;
	using UniRx;
	using Base;
	using Service;
	using Task;
	using File;
	using Data;
	using Scene;
	using Extension;
	using Utility;
	using Setting;
	using Debug;
	using SystemTask = System.Threading.Tasks.Task;
	///====================================================================================================
	/// <summary>
	/// ■ 海底の蜃気楼フレームワークのクラス
	///		フレームワークの開始地点を定義。
	/// </summary>
	///====================================================================================================
	public class SubmarineMirageFramework : SMStandardBase, ISMService {
		///------------------------------------------------------------------------------------------------
		/// ● 要素
		///------------------------------------------------------------------------------------------------
		[SMShow] public bool _isInitialized	{ get; private set; }
		readonly SMAsyncCanceler _asyncCanceler = new SMAsyncCanceler();

		///------------------------------------------------------------------------------------------------
		/// ● 作成、削除
		///------------------------------------------------------------------------------------------------
		/// <summary>
		/// ● コンストラクタ
		/// </summary>
		public SubmarineMirageFramework() {
			SMLog.s_isEnable = SMDebugManager.IS_DEVELOP;	// 真っ先に指定しないと、ログが出ない

			_disposables.AddFirst( () => {
				_isInitialized = false;
				_asyncCanceler.Dispose();
			} );
		}

		/// <summary>
		/// ● アプリ終了
		///		安全に全機能を解放し、ゲームを終了させる。
		/// </summary>
		public static void Shutdown( bool isApplicationQuit = true ) {
			try {
				SMServiceLocator.Dispose();

				if ( !isApplicationQuit )			{ return; }
				if ( SMDebugManager.s_isPlayTest )	{ return; }

				if ( SMDebugManager.IS_UNITY_EDITOR ) {
					UnityEditor.EditorApplication.isPlaying = false;
				} else {
					Application.Quit();
				}

			} catch ( OperationCanceledException ) {
			} catch ( Exception e ) {
				SMLog.Error( e );
				throw;
			}
		}

		/// <summary>
		/// ● フレームワークの開始地点
		///		ここから、フレームワークが実行される。
		/// </summary>
// TODO : もっと良い開始名を考える
		public async UniTask TakeOff( Func<UniTask> initializePluginEvent, Func<UniTask> registerSettingsEvent )
		{
			try {
				await Initialize( initializePluginEvent, registerSettingsEvent );
			} catch ( OperationCanceledException ) {
			} catch ( Exception e ) {
				SMLog.Error( e );
				throw;
			}
		}

		/// <summary>
		/// ● 初期化
		///		フレームワークの全機能を登録する。
		/// </summary>
		async UniTask Initialize( Func<UniTask> initializePluginEvent, Func<UniTask> registerSettingsEvent )
		{
			await SystemTask.Delay( 1, _asyncCanceler.ToToken() );

			_disposables.AddFirst( Observable.OnceApplicationQuit().Subscribe( _ => Shutdown( false ) ) );

			await initializePluginEvent();

			var taskManager = SMServiceLocator.Register( new SMTaskManager() );
			SMServiceLocator.Register( new SMMainSetting() );
			SMServiceLocator.Register( new SMDecorationManager() );
			SMServiceLocator.Register( new SMDebugManager() );
			SMServiceLocator.Register( new SMDisplayLog() );
			SMServiceLocator.Register( new SMCoroutineManager() );
			SMServiceLocator.Register( new SMInputManager() );
			SMServiceLocator.Register( new SMUnityTagManager() );
			SMServiceLocator.Register( new SMUnityLayerManager() );
			SMServiceLocator.Register( new SMFileManager() );
			SMServiceLocator.Register( new SMTimeManager() );
			SMServiceLocator.Register( new SMNetworkManager() );
			SMServiceLocator.Register( new SMAllDataManager() );

			await registerSettingsEvent();

			new SMSplashScreenWaiter();
			SMServiceLocator.Register( new SMSceneManager() );

			await taskManager.Initialize();

			if ( SMDebugManager.s_isPlayTest ) {
				var test = await SMServiceLocator.WaitResolve<IBaseSMTest>( _asyncCanceler );
				await test.Initialize();
			}

			_isInitialized = true;
		}
	}
}