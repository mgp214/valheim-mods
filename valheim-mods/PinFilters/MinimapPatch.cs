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

		[HarmonyPostfix]
		[HarmonyPatch("UpdatePins")]
		public static void UpdatePins(List<Minimap.PinData> ___m_pins) {
			if (___m_pins == null) return;
			foreach (var pin in ___m_pins) {
				if (pin.m_type == Minimap.PinType.None) {
					continue;
				}
				if (!PinFilterManager.Instance.IsPinTypeVisible(pin.m_type)) {
					if (pin.m_uiElement == null) continue;
					UnityEngine.Object.Destroy(pin.m_uiElement.gameObject);
					pin.m_uiElement = null;
				}
			}
		}
	}
}
