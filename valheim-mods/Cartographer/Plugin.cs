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
using UnityEngine;

namespace Cartographer {

	[BepInPlugin(pluginGuid, pluginName, pluginVersion)]
	public class Plugin : BaseUnityPlugin {
		public const string pluginGuid = "banana.cartographer";
		public const string pluginName = "Cartographer";
		public const string pluginVersion = "1.0.0.111";
		public static ConfigEntry<string> logLevelConfig;
		private static ConfigEntry<string> sketchColorsConfig;
		public static List<Color> sketchColors;

		public static ManualLogSource Log;

		private void LoadColors() {
			sketchColors = new List<Color>();
			foreach (var colorString in sketchColorsConfig.Value.Split(',')) {
				var wasSuccessful = ColorUtility.TryParseHtmlString(colorString.Trim(), out var color);
				if (wasSuccessful) {
					if (color.a != 1) {
						color.a = 1;
						Log.LogWarning("Do not include alpha component in sketch colors. Use the built-in transparency features of Cartographer's UI. ignoring alpha...");
					}

					sketchColors.Add(color);
					Log.LogDebug($"Loaded sketch color {colorString}");
				} else {
					Log.LogWarning($"Failed to load sketch color {colorString}");
				}
			}
		}

		public void Awake() {
			logLevelConfig = Config.Bind("General", "log level", "Info", "Set minimum priority for Cartographer's logging messages to appear in the BepInEx console. Can be one of Debug, Info, Warning, Error");
			sketchColorsConfig = Config.Bind("Sketch", "sketch colors", "#912c2c,#996b00,#225e09,#355a7a,#533769,#e0e0e0,#808080,#262626", "Comma-separated list of hex color codes for the colors you can select from.");
			Log = Logger;
			LoadColors();
			new Harmony(pluginGuid).PatchAll();
		}
	}
}
