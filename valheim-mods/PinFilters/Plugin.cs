using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinFilters {
	[BepInPlugin(pluginGuid, pluginName, pluginVersion)]
	public class Plugin : BaseUnityPlugin {
		public const string pluginGuid = "com.banana.valheim.pinfilters";
		public const string pluginName = "Pin Filters";
		public const string pluginVersion = "1.0";

		public void Awake() {
			new Harmony(pluginGuid).PatchAll();
		}
	}
}
