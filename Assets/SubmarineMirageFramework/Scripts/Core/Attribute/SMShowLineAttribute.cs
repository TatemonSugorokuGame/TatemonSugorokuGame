//---------------------------------------------------------------------------------------------------------
// ▽ Submarine Mirage Framework for Unity
//		Copyright (c) 2020 夢想海の水底より(from Seabed of Reverie)
//		Released under the MIT License :
//			https://github.com/FromSeabedOfReverie/SubmarineMirageFrameworkForUnity/blob/master/LICENSE
//---------------------------------------------------------------------------------------------------------
namespace SubmarineMirage {
	using System;



	[AttributeUsage(
		AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true
	)]
	public class SMShowLineAttribute : SMShowAttribute {
	}
}