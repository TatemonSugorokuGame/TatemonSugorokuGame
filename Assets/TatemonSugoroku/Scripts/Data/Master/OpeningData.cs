using System.Collections.Generic;
using KoganeUnityLib;
using SubmarineMirage;
using SubmarineMirage.Data;
using SubmarineMirage.Setting;
namespace TatemonSugoroku.Scripts {



	public class OpeningData : SMCSVData<int> {
		public enum Tag {
			BGM,
			Show,
			Hide,
			Talk,
		}


		public override int _registerKey => _dataID;

		[SMShow] public int _dataID			{ get; private set; }
		[SMShow] public Tag _tag			{ get; private set; }
		[SMShow] public string _target		{ get; private set; }
		[SMShow] public string _subtitles	{ get; private set; }
		[SMShow] public SMVoice? _voice		{ get; private set; }



		public override void Setup( string fileName, int index, List<string> texts ) {
			_dataID		= index;
			_tag = texts[0].ToEnum<Tag>();
			_target		= texts[1];
			_subtitles	= texts[2];

			if ( !texts[3].IsNullOrEmpty() ) {
				_voice = texts[3].ToEnum<SMVoice>();
			}
		}
	}
}