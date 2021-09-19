using System.Linq;
using UnityEngine;
using KoganeUnityLib;
using SubmarineMirage.Base;
namespace TatemonSugoroku.Scripts {



	public class UITurn : SMStandardMonoBehaviour {
		GameObject[] _turns;



		void Start() {
			_turns = this.GetChildren( true );
			ChangeTurn( null );
		}



		public void ChangeTurn( int? playerID ) {
			_turns.ForEach( go => go.SetActive( false ) );

			if ( !playerID.HasValue )	{ return; }
			_turns[playerID.Value].SetActive( true );
		}
	}
}