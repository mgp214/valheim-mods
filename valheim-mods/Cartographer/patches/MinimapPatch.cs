using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Cartographer {

	[HarmonyPatch(typeof(Minimap))]
	class MinimapPatch {

		[HarmonyReversePatch]
		[HarmonyPatch(typeof(Minimap), "ScreenToWorldPoint")]
		public static Vector3 ScreenToWorldPoint(Minimap instance, Vector3 mousePos) {
			throw new NotImplementedException("It's a stub");
		}

		[HarmonyPostfix]
		[HarmonyPatch("Start")]
		public static void StartPostfix(Minimap __instance) {
			UIInputHandler component = __instance.m_mapImageLarge.GetComponent<UIInputHandler>();
			component.m_onRightDown = (Action<UIInputHandler>)Delegate.Combine(component.m_onRightDown, new Action<UIInputHandler>(MinimapPatch.OnMapRightDown));
			DrawManager.Create();
		}

		[HarmonyPrefix]
		[HarmonyPatch("OnMapMiddleClick")]
		public static bool OnMapMiddleClickPrefix(Minimap __instance) {
			if (!DrawManager.Instance.IsDrawing) return false;
			Debug.Log("stop drawing!");
			return true;
		}

		public static void OnMapRightDown(UIInputHandler handler) {
			if (!DrawManager.Instance.IsDrawing) return;
			//TODO: implement drawing
			Debug.Log("start drawing!");
		}
	}
}