using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace InnerFires {

	[HarmonyPatch(typeof(Fireplace), "UpdateFireplace")]
	class FireplacePatch {
		private const double durationFactor = 10d;

		[HarmonyReversePatch]
		[HarmonyPatch(typeof(Fireplace), "GetTimeSinceLastUpdate")]
		public static double GetTimeSinceLastUpdate(Fireplace instance) {
			throw new NotImplementedException("It's a stub");
		}

		public static bool Prefix(Fireplace __instance, ref ZNetView ___m_nview) {
			if (!___m_nview.IsValid()) {
				return true;
			}
			if (___m_nview.IsOwner()) {
				float fuel = ___m_nview.GetZDO().GetFloat("fuel", 0f);
				double timeSinceLastUpdate = FireplacePatch.GetTimeSinceLastUpdate(__instance);
				if (__instance.IsBurning()) {
//					Debug.Log("Burning speed multiplied by " + durationFactor);
					float fuelBurnt = (float)(timeSinceLastUpdate / (double)__instance.m_secPerFuel / durationFactor);
					fuel -= fuelBurnt;
					if (fuel <= 0f) {
						fuel = 0f;
					}
					___m_nview.GetZDO().Set("fuel", fuel);
				}
			}
			return true;
		}

	}
}
