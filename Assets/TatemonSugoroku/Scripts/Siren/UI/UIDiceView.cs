using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UniRx;
using SubmarineMirage.Base;
using SubmarineMirage.Utility;
using SubmarineMirage.Debug;
namespace TatemonSugoroku.Scripts {



	/// <summary>
	/// ■ 花火画面の描画クラス
	/// </summary>
	public class UIDiceView : SMStandardMonoBehaviour {
		CanvasGroup _group { get; set; }
		Text _text { get; set; }
		public Button _button { get; private set; }
		DiceManagerView _diceManager { get; set; }
		readonly SMAsyncCanceler _canceler = new SMAsyncCanceler();



		void Start() {
			_group = GetComponent<CanvasGroup>();
			_group.alpha = 0;
			_group.blocksRaycasts = false;
			_group.interactable = false;

			_text = GetComponentInChildren<Text>();
			_button = GetComponentInChildren<Button>();
			_diceManager = FindObjectOfType<DiceManagerView>();
			/*
			_diceManager._totalEvent.Subscribe( i => {
				_text.text = $"{i}";
			} );
			*/

			_disposables.AddFirst( () => {
				_canceler.Dispose();
			} );
		}



		public async UniTask ChangeState( DiceState state ) {
			await UTask.DelayFrame( _canceler, 1 );

			_text.text = $"サイコロ";

			switch ( state ) {
				case DiceState.Hide:
					_group.alpha = 0;
					_group.blocksRaycasts = false;
					_group.interactable = false;
					break;

				case DiceState.Rotate:
					_group.alpha = 1;
					_group.blocksRaycasts = true;
					_group.interactable = true;
					break;

				case DiceState.Roll:
					var handler = _button.onClick.GetAsyncEventHandler( _canceler.ToToken() );
					await handler.OnInvokeAsync();

					_group.alpha = 1;
					_group.blocksRaycasts = true;
					_group.interactable = false;

					_diceManager.SetPower(
						new Vector3(
							Random.Range( -1, 1 ),
							Random.Range( -1, 1 ),
							Random.Range( -1, 1 )
						).normalized * 10
					);
					break;
			}
		}
	}
}