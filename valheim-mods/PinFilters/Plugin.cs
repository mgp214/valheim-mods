using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PinFilters {
	[BepInPlugin(pluginGuid, pluginName, pluginVersion)]
	public class Plugin : BaseUnityPlugin {
		public const string pluginGuid = "banana.pinfilters";
		public const string pluginName = "Pin Filters";
		public const string pluginVersion = "1.2.1";

		private ConfigEntry<string> toggleKeyConfig;
		public static KeyCode toggleKeyCode = KeyCode.None;

		public void Awake() {
			toggleKeyConfig = Config.Bind("General", "Global Toggle Key", "F7", "hotkey to toggle global pin visibility. Leave blank or set to None to disable.");
			Enum.TryParse<KeyCode>(toggleKeyConfig.Value, out toggleKeyCode);
			Debug.Log($"Configuring Pin Filter global toggle to key {toggleKeyCode}.");

			new Harmony(pluginGuid).PatchAll();
		}
	}
}
