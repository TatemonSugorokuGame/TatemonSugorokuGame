using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using KoganeUnityLib;
using SubmarineMirage.Base;
using SubmarineMirage.Service;
using SubmarineMirage.Data;
using SubmarineMirage.Audio;
using SubmarineMirage.Scene;
using SubmarineMirage.Utility;
using SubmarineMirage.Setting;
using SubmarineMirage.Debug;
namespace TatemonSugoroku.Scripts {



	public class UIOpening : SMStandardMonoBehaviour {
		[SerializeField] GameObject _speakersTop;
		readonly Dictionary<string, Image> _speakers = new Dictionary<string, Image>();
		readonly Dictionary<string, Vector3> _speakerDefaultPositions = new Dictionary<string, Vector3>();
		string _lastSpeakerName = string.Empty;
		[SerializeField] Text _subtitles;
		[SerializeField] Image _info;

		readonly SMAsyncCanceler _canceler = new SMAsyncCanceler();
		readonly SMAsyncCanceler _infoCanceler = new SMAsyncCanceler();
		SMSceneManager _sceneManager { get; set; }
		SMAudioManager _audioManager { get; set; }


		void Start() {
			_disposables.AddFirst( () => {
				_canceler.Dispose();
				_infoCanceler.Dispose();
			} );
		}



		protected override void StartAfterInitialize() {
			_sceneManager = SMServiceLocator.Resolve<SMSceneManager>();
			_audioManager = SMServiceLocator.Resolve<SMAudioManager>();

			var buttons = GetComponentsInChildren<Button>( true );
			buttons.ForEach( b => {
				b.onClick.AddListener( () => {

					switch ( b.name ) {
						case "ButtonEnd":
							UTask.Void( async () => {
								await _audioManager.Play( SMSE.Decide );
								if ( _canceler._isDispose )	{ return; }
								_sceneManager.GetFSM<MainSMScene>().ChangeState<TitleSMScene>().Forget();
							} );
							break;
					}
				} );
			} );

			_speakersTop.GetChildren().ForEach( go => {
				var i = go.GetComponent<Image>();
				i.color = new Color( i.color.r, i.color.g, i.color.b, 0 );
				_speakers[go.name] = i;
				go.SetActive( false );
				_speakerDefaultPositions[go.name] = go.GetComponent<RectTransform>().anchoredPosition3D;
			} );

			SetSubtitles( string.Empty );

			_info
				.DOFade( 0.1f, 1.5f )
				.SetEase( Ease.InOutSine )
				.SetLoops( -1, LoopType.Yoyo )
				.Play()
				.ToUniTask( _infoCanceler )
				.Forget();

			Play().Forget();
		}



		async UniTask Play() {
			await UTask.Delay( _canceler, 3000 );

			var allDataManager = SMServiceLocator.Resolve<SMAllDataManager>();
			var characterData = allDataManager.Get<string, OpeningCharacterData>();
			var openingData = allDataManager.Get<int, OpeningData>();

			foreach ( var data in openingData.GetAlls() ) {
				switch ( data._tag ) {
					case OpeningData.Tag.BGM:
						var bgm = data._target.ToEnum<SMBGM>();
//						_audioManager._bgmVolume = 0.4f;
						_audioManager._bgm.Play( bgm ).Forget();
						break;

					case OpeningData.Tag.Show:
						await StopMove( _lastSpeakerName );
						await SetSpeaker( data._target, true );
						break;

					case OpeningData.Tag.Hide:
						await StopMove( data._target );
						await SetSpeaker( data._target, false );
						break;

					case OpeningData.Tag.Talk:
						var text = string.Empty;
						if ( !data._target.IsNullOrEmpty() ) {
							text = $"（{characterData.Get( data._target ).ToUGUI()}）";
							if ( data._target != _lastSpeakerName ) {
								await StopMove( _lastSpeakerName );
							}
							await SetSpeaker( data._target, true );
							await SetMove( data._target );
						}
						text += data._subtitles;
						SetSubtitles( text );
						await _audioManager._voice.Play( data._voice.Value );
						if ( _canceler._isDispose )	{ return; }
//						await UTask.Delay( _canceler, 1000 );
						SetSubtitles( string.Empty );
						break;
				}
			}

			await StopMove( _lastSpeakerName );
			await UTask.Delay( _canceler, 1000 );
			_sceneManager.GetFSM<MainSMScene>().ChangeState<TitleSMScene>().Forget();
		}



		async UniTask SetSpeaker( string name, bool isActive ) {
			if ( name.IsNullOrEmpty() )	{ return; }
			if ( isActive == _speakers[name].gameObject.activeSelf ) { return; }


			if ( isActive ) {
				_speakers[name].gameObject.SetActive( isActive );
			}

			await _speakers[name]
				.DOFade( isActive ? 1 : 0, 1 )
				.SetEase( Ease.InOutSine )
				.Play()
				.ToUniTask( _canceler );

			if ( !isActive ) {
				_speakers[name].gameObject.SetActive( isActive );
			}
		}

		async UniTask SetMove( string name ) {
			if ( name.IsNullOrEmpty() )		{ return; }
			if ( name == _lastSpeakerName )	{ return; }


//			await StopMove( _lastSpeakerName );
			await UTask.DontWait();

			var rect = _speakers[name].GetComponent<RectTransform>();
			rect
				.DOAnchorPos3D( _speakerDefaultPositions[name] + new Vector3( 0, 50, 0 ), 1 )
				.SetEase( Ease.InOutSine )
				.SetLoops( -1, LoopType.Yoyo )
				.Play()
				.ToUniTask( _canceler )
				.Forget();

			_lastSpeakerName = name;
		}

		async UniTask StopMove( string name ) {
			if ( name.IsNullOrEmpty() )	{ return; }

			_canceler.Cancel();
			var rect = _speakers[name].GetComponent<RectTransform>();
			await rect
				.DOAnchorPos3D( _speakerDefaultPositions[name], 0.5f )
				.SetEase( Ease.Linear )
				.Play()
				.ToUniTask( _canceler );
		}



		void SetSubtitles( string text ) {
			_subtitles.text = text;
			_subtitles.GetParent().SetActive( !text.IsNullOrEmpty() );
		}
	}
}