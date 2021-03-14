using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
using UnityEngine.UI;
using System.Collections;
using System.IO;

namespace Cartographer {
	class DataSerializer {
		public enum DataType { Sketch, Mask }
		public static void Save(Color[] colors, DataType type) {
			var path = GetPath(type);
			Plugin.Log.LogInfo($"saving {type} data to {path}");
			var texture = new Texture2D(Minimap.instance.m_textureSize, Minimap.instance.m_textureSize, TextureFormat.RGBA32, false);
			texture.SetPixels(colors);
			var bytes = texture.EncodeToPNG();
			File.WriteAllBytes(path, bytes);
		}

		public static Color[] Load(DataType type) {
			var path = GetPath(type);
			Plugin.Log.LogInfo($"loading {type} data from {path}");
			var bytes = File.ReadAllBytes(path);
			var texture = new Texture2D(Minimap.instance.m_textureSize, Minimap.instance.m_textureSize, TextureFormat.RGBA32, false);
			texture.LoadImage(bytes);
			return texture.GetPixels();
		}

		private static string GetPath(DataType type) {
			var worldGeneratorType = typeof(WorldGenerator);
			var worldField = worldGeneratorType.GetField("m_world", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			var world = (World)worldField.GetValue(WorldGenerator.instance);
			var worldName = world.m_name;
			Plugin.Log.LogDebug("Got current world name: " + worldName);
			return $"{Utils.GetSaveDataPath()}/worlds/{worldName}_cartographer_{type.ToString().Split('.').Last()}.png";
		}
	}
}