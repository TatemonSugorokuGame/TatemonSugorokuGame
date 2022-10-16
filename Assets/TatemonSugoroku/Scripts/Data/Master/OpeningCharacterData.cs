using System.Collections.Generic;
using UnityEngine;
using KoganeUnityLib;
using SubmarineMirage;
using SubmarineMirage.Data;
namespace TatemonSugoroku.Scripts {



	public class OpeningCharacterData : SMCSVData<string> {
		public override string _registerKey => _tag;

		[SMShow] public int _dataID		{ get; private set; }
		[SMShow] public string _tag		{ get; private set; }
		[SMShow] public string _name	{ get; private set; }
		[SMShow] public Color _color	{ get; private set; }



		public override void Setup( string fileName, int index, List<string> texts ) {
			_dataID	= index;
			_tag	= texts[0];
			_name	= texts[1];
			_color = new Color(
				texts[2].ToInt() / 255f,
				texts[3].ToInt() / 255f,
				texts[4].ToInt() / 255f
			);
		}

		public string ToUGUI() {
			return $"<color=#{_color.EncodeColor()}>{_name}</color>";
		}
	}
}