using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace PinFilters {

	[HarmonyPatch(typeof(Minimap))]
	class MinimapPatch {

		private static Vector3 offscreen = new Vector3(10000, 10000);
		private static Dictionary<Minimap.PinData, Vector3> savedPinPositions = new Dictionary<Minimap.PinData, Vector3>();

		[HarmonyReversePatch]
		[HarmonyPatch(typeof(Minimap), "WorldToMapPoint")]
		public static void WorldToMapPoint(Minimap instance, Vector3 p, out float mx, out float my) {
			throw new NotImplementedException("It's a stub");
		}

		[HarmonyPrefix]
		[HarmonyPatch("IsPointVisible")]
		public static bool IsPointVisible(Vector3 p, RawImage map, ref bool __result) {
			if (PinFilterManager.Instance.allPinsHidden) {
				__result = true;
				return true;
			}
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch("UpdatePins")]
		public static void UpdatePinsPrefix(List<Minimap.PinData> ___m_pins) {
			if (___m_pins == null) return;
			foreach (var pin in ___m_pins) {
				if (pin.m_type == Minimap.PinType.None) continue;

				if (pin.m_uiElement == null) continue;
				if (PinFilterManager.Instance.IsPinTypeVisible(pin.m_type)) {
					if (savedPinPositions.ContainsKey(pin)) {
						pin.m_uiElement.localPosition = savedPinPositions[pin];
						savedPinPositions.Remove(pin);
					}
				}
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch("UpdatePins")]
		public static void UpdatePinsPostfix(List<Minimap.PinData> ___m_pins) {
			var pinStates = new Dictionary<Minimap.PinData, bool>();

			if (___m_pins == null) return;
			foreach (var pin in ___m_pins) {
				if (pin.m_type == Minimap.PinType.None) {
					continue;
				}

				if (pin.m_uiElement == null) continue;
				if (!PinFilterManager.Instance.IsPinTypeVisible(pin.m_type)) {
					if (!savedPinPositions.ContainsKey(pin)) {
						savedPinPositions[pin] = pin.m_uiElement.localPosition;
					}
					pin.m_uiElement.position = offscreen;
				}
			}
		}
	}
}
