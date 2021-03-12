using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cartographer {

	[BepInPlugin(pluginGuid, pluginName, pluginVersion)]
	public class Plugin : BaseUnityPlugin {
		public const string pluginGuid = "banana.cartographer";
		public const string pluginName = "Cartographer";
		public const string pluginVersion = "1.0.0";

		public void Awake() {
			new Harmony(pluginGuid).PatchAll();
		}
	}
}
