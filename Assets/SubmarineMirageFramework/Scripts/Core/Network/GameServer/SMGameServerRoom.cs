//---------------------------------------------------------------------------------------------------------
// ▽ Submarine Mirage Framework for Unity
//		Copyright (c) 2020 夢想海の水底より(from Seabed of Reverie)
//		Released under the MIT License :
//			https://github.com/FromSeabedOfReverie/SubmarineMirageFrameworkForUnity/blob/master/LICENSE
//---------------------------------------------------------------------------------------------------------
namespace SubmarineMirage.Network {
	using System;
	using UnityEngine;
	using Base;
	using Extension;
	using Debug;
	///====================================================================================================
	/// <summary>
	/// ■ ゲームサーバーの部屋クラス
	/// </summary>
	///====================================================================================================
	public abstract class SMGameServerRoom : SMLightBase {
		[SMShow] protected abstract string _wrapText { get; }

		[SMShowLine] public string _name { get; protected set; }
		[SMShowLine] public string _password { get; protected set; }

		[SMShowLine] public int _playerCount { get; set; }
		[SMShowLine] public int _maxPlayerCount { get; protected set; }

		[SMShowLine] public bool _isActive { get; protected set; }



		public SMGameServerRoom() {
			Wrap();
		}

		public SMGameServerRoom( string name, string password, int maxPlayerCount ) {
			_name = name;
			_password = password;
			_maxPlayerCount = maxPlayerCount;
			_isActive = true;

			Wrap();
		}



		void Wrap() {
			_maxPlayerCount = Mathf.Clamp( _maxPlayerCount, 0, SMNetworkManager.MAX_PLAYERS );

			_name = _name ?? string.Empty;
			_password = _password ?? string.Empty;

			if ( _name.Contains( _wrapText ) ) {
				throw new InvalidOperationException( string.Join( "\n",
					$"{this.GetAboutName()}.{nameof( Wrap )} : 部屋名に、禁則文字を使用",
					$"{nameof( _name )} : {_name}"
				) );
			}
			if ( _password.Contains( _wrapText ) ) {
				throw new InvalidOperationException( string.Join( "\n",
					$"{this.GetAboutName()}.{nameof( Wrap )} : パスワードに、禁則文字を使用",
					$"{nameof( _password )} : {_password}"
				) );
			}
		}



		public abstract bool IsEqualPassword( string password );

		public abstract string ToToken();
	}
}