using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InnerFires {
	[BepInPlugin(pluginGuid, pluginName, pluginVersion)]
	public class Plugin : BaseUnityPlugin {
		public const string pluginGuid = "com.banana.valheim.torchduration";
		public const string pluginName = "Torch Duration Extender";
		public const string pluginVersion = "0.1";

		public void Awake() {
			new Harmony(pluginGuid).PatchAll();
		}
	}
}
