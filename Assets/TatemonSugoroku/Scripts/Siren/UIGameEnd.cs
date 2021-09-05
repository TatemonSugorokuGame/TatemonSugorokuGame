using System.Linq;
using UnityEngine;
using KoganeUnityLib;
using SubmarineMirage.Base;
namespace TatemonSugoroku.Scripts {



	public class UIGameEnd : SMStandardMonoBehaviour {
		CanvasGroup _group { get; set; }


		void Start() {
			_group = GetComponentInChildren<CanvasGroup>();
			SetActive( false );
		}



		public void SetActive( bool isActive ) {
			_group.alpha = isActive ? 1 : 0;

			if ( !isActive )	{ return; }
			var uiHowToPlay = FindObjectOfType<UIHowToPlay>();
			uiHowToPlay.Hide();
		}
	}
}