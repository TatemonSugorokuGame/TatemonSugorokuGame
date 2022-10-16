using System.Collections.Generic;
using KoganeUnityLib;
using SubmarineMirage;
using SubmarineMirage.Data;
using SubmarineMirage.Setting;
namespace TatemonSugoroku.Scripts {



	public class CreditData : SMCSVData<int> {
		public enum Tag {
			Headline,
			Normal,
		}


		public override int _registerKey => _dataID;

		[SMShow] public int _dataID		{ get; private set; }
		[SMShow] public Tag _tag		{ get; private set; }
		[SMShow] public string _text	{ get; private set; }



		public override void Setup( string fileName, int index, List<string> texts ) {
			_dataID	= index;
			_tag	= texts[0].ToEnumOrDefault( Tag.Normal );
			_text	= texts[1];
		}
	}
}