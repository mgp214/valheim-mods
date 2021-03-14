using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cartographer {

	[BepInPlugin(pluginGuid, pluginName, pluginVersion)]
	public class Plugin : BaseUnityPlugin {
		public const string pluginGuid = "banana.cartographer";
		public const string pluginName = "Cartographer";
		public const string pluginVersion = "1.0.0.6";
		public static ConfigEntry<string> logLevelConfig;
		public static ManualLogSource Log;

		public void Awake() {
			logLevelConfig = Config.Bind("General", "log level", "Info", "Set minimum priority for Cartographer's logging messages to appear in the BepInEx console. Can be one of Debug, Info, Warning, Error");
			Log = Logger;
			new Harmony(pluginGuid).PatchAll();
		}
	}
}
