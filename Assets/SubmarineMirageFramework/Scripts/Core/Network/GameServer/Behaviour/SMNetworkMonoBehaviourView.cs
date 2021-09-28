//---------------------------------------------------------------------------------------------------------
// ▽ Submarine Mirage Framework for Unity
//		Copyright (c) 2020 夢想海の水底より(from Seabed of Reverie)
//		Released under the MIT License :
//			https://github.com/FromSeabedOfReverie/SubmarineMirageFrameworkForUnity/blob/master/LICENSE
//---------------------------------------------------------------------------------------------------------
namespace SubmarineMirage.Network {
	using System.Collections.Generic;
	using UnityEngine;
	using UniRx;
	using KoganeUnityLib;
	using Base;
	///====================================================================================================
	/// <summary>
	/// ■ ネットワークのモノ動作のビュークラス
	/// </summary>
	///====================================================================================================
	public abstract class SMNetworkMonoBehaviourView : SMStandardMonoBehaviour, ISMNetworkMonoBehaviourView {
		///------------------------------------------------------------------------------------------------
		/// ● 要素
		///------------------------------------------------------------------------------------------------
		ISMNetworkMonoBehaviourView _view	{ get; set; }

		[SerializeField] SMNetworkSendType _sendTypeSetter = SMNetworkSendType.None;
		[SerializeField] SMNetworkTransformType _useTransformTypeSetter = SMNetworkTransformType.None;
		[SerializeField] bool _isUseScaleSetter = false;
		[SerializeField] bool _isUseTeleportSetter = false;
		[SerializeField] bool _isUseAnimatorSetter = false;

		public SMNetworkSendType _sendType				=> _sendTypeSetter;
		public SMNetworkTransformType _useTransformType	=> _useTransformTypeSetter;
		public bool _isUseScale		=> _isUseScaleSetter;
		public bool _isUseTeleport	=> _isUseTeleportSetter;
		public bool _isUseAnimator	=> _isUseAnimatorSetter;

		public object _networkView	=> _view?._networkView;
		public object _owner		=> _view?._owner;

		public SMNetworkObjectType _objectType	=> _view?._objectType ?? SMNetworkObjectType.Other;
		public int _networkID		=> _view?._networkID ?? -1;
		public int _ownerID			=> _view?._ownerID ?? -1;
		public string _ownerName	=> _view?._ownerName ?? string.Empty;

		public Subject< List<object> > _sendStreamEvent		=> _view?._sendStreamEvent;
		public Subject< List<object> > _receiveStreamEvent	=> _view?._receiveStreamEvent;
		public Subject<SMGameServerSendData> _receiveEvent	=> _view?._receiveEvent;

		///------------------------------------------------------------------------------------------------
		/// ● 生成、削除
		///------------------------------------------------------------------------------------------------
		protected override void Awake() {
#if PHOTON_UNITY_NETWORKING
			_view = GetComponent<SMPhotonMonoBehaviourView>();
#endif
			base.Awake();
		}

		void Reset() {
			ApplyComponents();
		}

		///------------------------------------------------------------------------------------------------
		/// ● 情報を送受信
		///------------------------------------------------------------------------------------------------
		public void Send<TTarget, TData>( TTarget target, TData data )
			where TTarget : class
			where TData : SMGameServerSendData
		{
			_view?.Send( target, data );
		}

		///------------------------------------------------------------------------------------------------
		/// ● 部品を適用
		///------------------------------------------------------------------------------------------------
		public void ApplyComponents() {
#if PHOTON_UNITY_NETWORKING
			var v = gameObject.GetOrAddComponent<SMPhotonMonoBehaviourView>();
			v.ApplyComponents();
#endif
		}
	}
}