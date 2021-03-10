using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExtendedExploreRadius {

	[HarmonyPatch(typeof(Minimap), "UpdateExplore")]
	class MinimapPatch {
		private const float newExploreRadius = 150f;

		[HarmonyReversePatch]
		[HarmonyPatch(typeof(Minimap), "Explore", new Type[] { typeof(Vector3), typeof(float) })]
		public static void Explore(Minimap instance, Vector3 playerPosition, float radius) {
			throw new NotImplementedException("It's a stub");
		}

		public static bool Prefix(Player player, Minimap __instance, ref float ___m_exploreTimer, float ___m_exploreInterval) {
			___m_exploreTimer += Time.deltaTime;
			if (___m_exploreTimer > ___m_exploreInterval) {
				___m_exploreTimer = 0f;
				MinimapPatch.Explore(__instance, player.transform.position, newExploreRadius);
			}
			return true;
		}
	}
}