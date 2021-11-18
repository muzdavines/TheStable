/*
* Copyright (c) Mad Pixel Machine
* http://www.madpixelmachine.com/
*/
using UnityEngine;

namespace EnergyBarToolkit
{
	public static class MadGameObjectDestroy
	{
		public static void SafeDestroy(Object go)
		{
			if (Application.isPlaying)
			{
				GameObject.Destroy(go);
			}
			else
			{
#if UNITY_EDITOR
				if (go != null)
				{
#if !(UNITY_4 || UNITY_5 || UNITY_2017 || UNITY_2018_1)
					if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(go))
						UnityEditor.PrefabUtility.UnpackPrefabInstance(UnityEditor.PrefabUtility.GetNearestPrefabInstanceRoot((go as GameObject)),
							UnityEditor.PrefabUnpackMode.Completely, UnityEditor.InteractionMode.AutomatedAction);
#endif
					Object.DestroyImmediate(go);
				}
#endif
			}
		}
	}
}
