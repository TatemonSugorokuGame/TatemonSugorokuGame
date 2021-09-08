//---------------------------------------------------------------------------------------------------------
// ▽ Submarine Mirage Framework for Unity
//		Copyright (c) 2020 夢想海の水底より(from Seabed of Reverie)
//		Released under the MIT License :
//			https://github.com/FromSeabedOfReverie/SubmarineMirageFrameworkForUnity/blob/master/LICENSE
//---------------------------------------------------------------------------------------------------------
namespace SubmarineMirage.Network {
	using System.Collections.Generic;
	using Cysharp.Threading.Tasks;
	using UniRx;
	using Base;
	///====================================================================================================
	/// <summary>
	/// ■ ゲームサーバーのインターフェース
	/// </summary>
	///====================================================================================================
	public interface ISMGameServer : ISMStandardBase {
		///------------------------------------------------------------------------------------------------
		/// ● 要素
		///------------------------------------------------------------------------------------------------
		/// <summary>接続の型</summary>
		public SMGameServerType _type { get; }

		/// <summary>プレイヤー名</summary>
		string _playerName { get; set; }

		/// <summary>プレイヤー数のイベント</summary>
		ReactiveProperty<int> _playerCountEvent { get; }
		/// <summary>部屋一覧のイベント</summary>
		ReactiveProperty< List<SMGameServerRoom> > _roomsEvent { get; }
		/// <summary>失敗のイベント</summary>
		ReactiveProperty<string> _errorEvent { get; }

		/// <summary>全て接続完了か？</summary>
		public bool _isConnected { get; }
		/// <summary>サーバーか？</summary>
		public bool _isServer { get; }
		/// <summary>活動中か？</summary>
		public bool _isActive { get; set; }

		///------------------------------------------------------------------------------------------------
		/// ● 接続
		///------------------------------------------------------------------------------------------------
		/// <summary>● オンライン接続</summary>
		UniTask<bool> ConnectOnline();
		/// <summary>● オフライン接続</summary>
		UniTask<bool> ConnectOffline();
		/// <summary>● 接続解除</summary>
		UniTask<bool> Disconnect();

		/// <summary>● 控室に入室</summary>
		UniTask<bool> EnterLobby();
		/// <summary>● 部屋を作成</summary>
		UniTask<bool> CreateRoom( string name, string password, int maxPlayerCount );
		/// <summary>● 部屋に入室</summary>
		UniTask<bool> EnterRoom( SMGameServerRoom room );
	}
}