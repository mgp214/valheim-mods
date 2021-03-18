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
		public enum DataType { Sketch }
		public static void Save(Color[] colors, DataType type) {
			var path = GetPath(type);
			Plugin.Log.LogInfo($"saving {type} data to {path}");
			var texture = new Texture2D(Minimap.instance.m_textureSize, Minimap.instance.m_textureSize, TextureFormat.RGBA32, false);
			texture.SetPixels(colors);
			var bytes = texture.EncodeToPNG();
			File.WriteAllBytes(path, bytes);
		}

		public static void Save(Texture2D texture, DataType type) {
			var path = GetPath(type);
			Plugin.Log.LogInfo($"saving {type} data to {path}");
			var bytes = texture.EncodeToPNG();
			File.WriteAllBytes(path, bytes);
		}

		public static Color[] LoadAsArray(DataType type) {
			var path = GetPath(type);
			Plugin.Log.LogInfo($"loading {type} data from {path}");
			var bytes = File.ReadAllBytes(path);
			var texture = new Texture2D(Minimap.instance.m_textureSize, Minimap.instance.m_textureSize, TextureFormat.RGBA32, false);
			texture.LoadImage(bytes);
			return texture.GetPixels();
		}

		public static Texture2D Load(DataType type) {
			var path = GetPath(type);
			Plugin.Log.LogInfo($"loading {type} data from {path}");
			var bytes = File.ReadAllBytes(path);
			var texture = new Texture2D(Minimap.instance.m_textureSize, Minimap.instance.m_textureSize, TextureFormat.RGBA32, false);
			texture.wrapMode = TextureWrapMode.Clamp;
			texture.LoadImage(bytes);
			return texture;
		}

		private static string GetPath(DataType type) {
			var worldGeneratorType = typeof(WorldGenerator);
			var worldField = worldGeneratorType.GetField("m_world", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			var world = (World)worldField.GetValue(WorldGenerator.instance);
			var worldName = world.m_name;
			Plugin.Log.LogDebug("Got current world name: " + worldName);
			return $"{Utils.GetSaveDataPath()}/worlds/{worldName}_cartographer_{type.ToString().Split('.').Last()}.png";
		}

		private static string GetAssetPath(string filename) {
			return $"{BepInEx.Paths.PluginPath}/cartographer/assets/{filename}";
		}

		public static Texture2D LoadTextureAsset(string name) {
			var path = GetAssetPath(name);
			try {
				var texture = new Texture2D(1, 1);
				var bytes = File.ReadAllBytes(path);
				Plugin.Log.LogDebug($"Loaded image asset {name}");
				texture.LoadImage(bytes);
				return texture;
			} catch (Exception e) {
				Plugin.Log.LogError($"Couldn't load asset {path}: {e.GetType().ToString()}");
			}
			return Texture2D.whiteTexture;
		}
	}
}