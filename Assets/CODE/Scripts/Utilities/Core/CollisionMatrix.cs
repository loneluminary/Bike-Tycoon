using System.Collections.Generic;
using UnityEngine;

public static class CollisionMatrix
{
	private static Dictionary<int, int> _masksByLayer;
	
	private static bool _initialized;

	private static void Initialize()
	{
		_initialized = true;
		
		_masksByLayer = new Dictionary<int, int>();
		for (int i = 0; i < 32; i++)
		{
			int num = 0;
			for (int j = 0; j < 32; j++)
			{ 
				if (!Physics.GetIgnoreLayerCollision(i, j)) num |= 1 << j;
			}
			_masksByLayer.Add(i, num);
		}
	}

	public static int MaskByLayer(int layer)
	{
		if(!_initialized) Initialize();
		
		return _masksByLayer[layer];
	}
}