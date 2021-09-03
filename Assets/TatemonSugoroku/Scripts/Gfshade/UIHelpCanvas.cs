using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SubmarineMirage.Utility;
using SubmarineMirage.Service;
using SubmarineMirage.Data;
using SubmarineMirage.Data.Save;
using SubmarineMirage.Scene;
using SubmarineMirage.Audio;
using SubmarineMirage.Setting;
using SubmarineMirage;
using UnityEngine.UI;
using KoganeUnityLib;
using UniRx;
using Cysharp.Threading.Tasks;

namespace TatemonSugoroku.Scripts
{
    public class UIHelpCanvas : MonoBehaviour
    {
        [SerializeField]
        Transform _PageTop;

        [SerializeField]
        Button _Prev;

        [SerializeField]
        Button _Next;

        [SerializeField]
        Button _Close;

        [SerializeField]
        Button _End;


        readonly List<GameObject> _pages = new List<GameObject>();
        readonly SMAsyncCanceler _canceler = new SMAsyncCanceler();


        private void SetExplanations()
        {
            foreach ( Transform t in _PageTop ) {
                t.gameObject.SetActive( false );

                if ( t.gameObject.name.Contains( "PC" ) ) {
                    switch ( SMMainSetting.PLATFORM ) {
                        case SMPlatformType.Windows:
                        case SMPlatformType.MacOSX:
                        case SMPlatformType.Linux:
                        case SMPlatformType.WebGL:
                            _pages.Add( t.gameObject );
                            break;
                    }

                } else if ( t.gameObject.name.Contains( "Smartphone" ) ) {
                    switch ( SMMainSetting.PLATFORM ) {
                        case SMPlatformType.Android:
                        case SMPlatformType.IOS:
                            _pages.Add( t.gameObject );
                            break;
                    }

                } else {
                    _pages.Add( t.gameObject );
                }
			}
        }

        private void Awake()
        {
            var f = SMServiceLocator.Resolve<SubmarineMirageFramework>();
            var initf = f.ObserveEveryValueChanged(f => f._isInitialized).Where(b => b).First().Publish();
            AsyncSubject<Unit> init = new AsyncSubject<Unit>();

            var pageRP = new ReactiveProperty<int>(0);
            var page = init.SelectMany(pageRP);

            initf.Subscribe(_ => {
                var n = f._isInitialized;
                SetExplanations();
                init.OnNext(Unit.Default);
                init.OnCompleted();
            });

            SMAudioManager audioManager = null;
            SMAllDataManager dataManager = null;
            SettingDataManager setting = null;
            UTask.Void( async () => {
                audioManager = await SMServiceLocator.WaitResolve<SMAudioManager>();
                dataManager = await SMServiceLocator.WaitResolve<SMAllDataManager>();
                setting = dataManager.Get<SettingDataManager>();
            } );

            page.Subscribe(UpdateText);
            page.Select(p => p > 0).Subscribe(prev => _Prev.interactable = prev);
            page.Select(p => p < _pages.Count - 1).Subscribe(next => _Next.interactable = next);
            _Prev.OnClickAsObservable().Subscribe(_ => {
                pageRP.Value--;
                audioManager?.Play( SMSE.Decide ).Forget();
            } );
            _Next.OnClickAsObservable().Subscribe(_ => {
                pageRP.Value++;
                audioManager?.Play( SMSE.Decide ).Forget();
            } );

            var sceneManager = SMServiceLocator.Resolve<SMSceneManager>();
            _Close.OnClickAsObservable().Subscribe( _ => {
                UTask.Void( async () => {
                    audioManager?.Play( SMSE.Decide ).Forget();
                    var data = setting.Get();
                    if ( !data._isShowHelp ) {
                        data._isShowHelp = true;
                        await setting._saveEvent.Run( dataManager._asyncCancelerOnDispose );
                    }
                    await UTask.Delay( _canceler, 500 );
                    sceneManager.GetFSM<UISMScene>().ChangeState<UINoneSMScene>().Forget();
                } );
            } );
            _End.OnClickAsObservable().Subscribe( _ => {
                UTask.Void( async () => {
                    audioManager?.Play( SMSE.Decide ).Forget();
                    await UTask.Delay( _canceler, 500 );
                    sceneManager.GetFSM<UISMScene>().ChangeState<UINoneSMScene>().Forget();
                    sceneManager.GetFSM<MainSMScene>().ChangeState<TitleSMScene>().Forget();
                } );
            } );
            _End.interactable = !( sceneManager.GetFSM<MainSMScene>()._state is TitleSMScene );

            initf.Connect();
        }

        private void UpdateText(int i)
        {
            _pages.ForEach( go => go.SetActive( false ) );
            _pages[i].SetActive( true );
        }

		private void OnDestroy() {
            _canceler.Dispose();
		}
	}
}
