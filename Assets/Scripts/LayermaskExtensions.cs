using UnityEngine;
public static class LayerMaskExtensions
{
	/// <summary> Converts given layermask to layer number </summary>
	/// <returns> Layer number </returns>
	/// <assert> Mask represents multiple layers </assert>
	public static int MaskToLayer(this LayerMask mask )
	{
		var bitmask = mask.value;
 
		UnityEngine.Assertions.Assert.IsFalse( ( bitmask & ( bitmask - 1 ) ) != 0,
			"MaskToLayer() was passed an invalid mask containing multiple layers." );
 
		int result = bitmask > 0 ? 0 : 31;
		while( bitmask > 1 )
		{
			bitmask = bitmask >> 1;
			result++;
		}
		return result;
	}
 
	/// <summary> Converts layermask to layer number </summary>
	internal static int ToLayer( this LayerMask mask )
	{
		return MaskToLayer( mask );
	}
}