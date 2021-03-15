using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static Cartographer.DataSerializer;

namespace Cartographer {

	[HarmonyPatch(typeof(Minimap))]
	class MinimapPatch {

		private static bool isSketching = false;
		public static bool sketchingIsEnabled = false;
		private static Color unmask = Color.clear;
		private static Color neutralHeight = new Color(31f, 0, 0);
		private static Color mask = Color.white;
		private static Color sketchColor = Color.red;
		private static float radius = 15f;
		private static float fogInset = 5f;
		private static bool hasBackedUpTextures = false;

		private static SketchToggleComponent sketchToggle;
		private static SketchPanelComponent sketchPanel;
		private static TextInputComponent fogInsetInput;
		private static TextInputComponent radiusInput;
		private static Color[] sketchData;
		private static Color[] maskData;

		private static Dictionary<Texture2D, Texture2D> backupTextures;

		[HarmonyReversePatch]
		[HarmonyPatch(typeof(Minimap), "ScreenToWorldPoint")]
		public static Vector3 ScreenToWorldPoint(Minimap instance, Vector3 mousePos) {
			throw new NotImplementedException("It's a stub");
		}

		[HarmonyReversePatch]
		[HarmonyPatch(typeof(Minimap), "WorldToPixel")]
		public static void WorldToPixel(Minimap instance, Vector3 p, out int mx, out int my) {
			throw new NotImplementedException("It's a stub");
		}

		[HarmonyReversePatch]
		[HarmonyPatch(typeof(Minimap), "GetHeight")]
		public static float GetHeight(Minimap instance, int x, int y) {
			throw new NotImplementedException("It's a stub");
		}

		[HarmonyPostfix]
		[HarmonyPatch("Start")]
		public static void StartPostfix(Minimap __instance, Texture2D ___m_mapTexture, Texture2D ___m_fogTexture, Texture2D ___m_forestMaskTexture, Texture2D ___m_heightTexture) {
			UIInputHandler component = __instance.m_mapImageLarge.GetComponent<UIInputHandler>();
			component.m_onRightDown = (Action<UIInputHandler>)Delegate.Combine(component.m_onRightDown, new Action<UIInputHandler>(MinimapPatch.OnMapRightDown));
			component.m_onRightUp = (Action<UIInputHandler>)Delegate.Combine(component.m_onRightUp, new Action<UIInputHandler>(MinimapPatch.OnMapRightUp));

			backupTextures = new Dictionary<Texture2D, Texture2D>();

			sketchPanel = SketchPanelComponent.New(__instance.m_pinRootLarge);
			sketchToggle = SketchToggleComponent.New(sketchPanel.rectTransform);
			sketchToggle = SketchToggleComponent.New(sketchPanel.rectTransform);
			fogInsetInput = TextInputComponent.New(sketchPanel.rectTransform);
			fogInsetInput.inputField.text = fogInset.ToString();
			fogInsetInput.onValueChanged += (value) => {
				if (float.TryParse(value, out float result)) {
					fogInset = Mathf.Clamp(result, radius + 1, 500);
					Plugin.Log.LogDebug($"fogInset changed to {result}");
				}
			};
			radiusInput = TextInputComponent.New(sketchPanel.rectTransform);
			radiusInput.inputField.text = radius.ToString();
			radiusInput.onValueChanged += (value) => {
				if (float.TryParse(value, out float result)) {
					radius = Mathf.Clamp(result, 1, 500);
					Plugin.Log.LogDebug($"radius changed to {result}");
				}
			};
		}

		[HarmonyPostfix]
		[HarmonyPatch("Update")]
		public static void UpdatePostfix(Minimap __instance, Texture2D ___m_mapTexture, Texture2D ___m_fogTexture, Texture2D ___m_forestMaskTexture, Texture2D ___m_heightTexture, bool[] ___m_explored) {
			if (!hasBackedUpTextures) {
				Plugin.Log.LogInfo("initializing cartographer");
				backupTextures[___m_mapTexture] = CopyTexture(___m_mapTexture, TextureFormat.RGBA32);
				backupTextures[___m_forestMaskTexture] = CopyTexture(___m_forestMaskTexture, TextureFormat.RGBA32);
				backupTextures[___m_heightTexture] = CopyTexture(___m_heightTexture, TextureFormat.RFloat);
				hasBackedUpTextures = true;
				LoadMapData(___m_mapTexture, ___m_fogTexture, ___m_heightTexture, ___m_forestMaskTexture, ___m_explored);
			}

			if (Input.GetKeyDown(KeyCode.N)) {
				radius++;
				Plugin.Log.LogDebug("radius: " + radius);
			}
			if (Input.GetKeyDown(KeyCode.B)) {
				radius--;
				Plugin.Log.LogDebug("radius: " + radius);
			}

			if (Input.GetKeyDown(KeyCode.H)) {
				fogInset++;
				Plugin.Log.LogDebug("fogInset: " + fogInset);
			}
			if (Input.GetKeyDown(KeyCode.G)) {
				fogInset--;
				Plugin.Log.LogDebug("fogInset: " + fogInset);
			}


			if (isSketching) {
				var pos = ScreenToWorldPoint(__instance, Input.mousePosition);
				WorldToPixel(__instance, pos, out int x, out int y);
				var fogRadius = Mathf.Clamp(radius - fogInset, 1, 500);
				Plugin.Log.LogDebug($"sketching with r: {radius} & fog r: {fogRadius}");
				if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
					Erase(x, y, radius, ___m_mapTexture, backupTextures[___m_mapTexture]);
					Erase(x, y, radius, ___m_forestMaskTexture, backupTextures[___m_forestMaskTexture]);
					Reexplore(x, y, fogRadius, ___m_explored, ___m_fogTexture);
					Erase(x, y, radius, ___m_heightTexture, backupTextures[___m_heightTexture]);
				} else {
					Sketch(x, y, radius, ___m_mapTexture, sketchColor, sketchData);
					Sketch(x, y, radius, ___m_forestMaskTexture, unmask);
					Sketch(x, y, fogRadius, ___m_fogTexture, unmask, maskData);
					Sketch(x, y, radius, ___m_heightTexture, neutralHeight);
				}
			}
		}
		private static void Erase(int x, int y, float radius, Texture2D texture, Texture2D backupTexture) {
			int r = (int)Mathf.Ceil(radius / Minimap.instance.m_pixelSize);
			for (int i = y - r; i <= y + r; i++) {
				for (int j = x - r; j <= x + r; j++) {
					if (j >= 0 && i >= 0 && j < Minimap.instance.m_textureSize && i < Minimap.instance.m_textureSize && new Vector2((float)(j - x), (float)(i - y)).magnitude <= (float)r) {
						texture.SetPixel(j, i, backupTexture.GetPixel(j, i));
						sketchData[i * Minimap.instance.m_textureSize + j] = Color.clear;
					}
				}
			}
			texture.Apply();
		}

		private static void Reexplore(int x, int y, float radius, bool[] explored, Texture2D fogTexture) {
			int r = (int)Mathf.Ceil(radius / Minimap.instance.m_pixelSize);
			for (int i = y - r; i <= y + r; i++) {
				for (int j = x - r; j <= x + r; j++) {
					if (j >= 0 && i >= 0 && j < Minimap.instance.m_textureSize && i < Minimap.instance.m_textureSize && new Vector2((float)(j - x), (float)(i - y)).magnitude <= (float)r) {
						var isExplored = j >= 0 && j < Minimap.instance.m_textureSize && i >= 0 && j < Minimap.instance.m_textureSize && explored[i * Minimap.instance.m_textureSize + j];
						if (!isExplored) {
							fogTexture.SetPixel(j, i, mask);
							maskData[i * Minimap.instance.m_textureSize + j] = mask;
						}
					}
				}
			}
			fogTexture.Apply();
		}

		private static void Sketch(int x, int y, float radius, Texture2D texture, Color color, Color[] saveTo = null) {
			int r = (int)Mathf.Ceil(radius / Minimap.instance.m_pixelSize);
			for (int i = y - r; i <= y + r; i++) {
				for (int j = x - r; j <= x + r; j++) {
					if (j >= 0 && i >= 0 && j < Minimap.instance.m_textureSize && i < Minimap.instance.m_textureSize && new Vector2((float)(j - x), (float)(i - y)).magnitude <= (float)r) {
						texture.SetPixel(j, i, color);
						if (saveTo != null) {
							saveTo[i * Minimap.instance.m_textureSize + j] = color;
						}
					}
				}
			}
			texture.Apply();
		}


		[HarmonyPrefix]
		[HarmonyPatch("OnMapRightClick")]
		public static bool OnMapRightClickPrefix(Minimap __instance) {
			if (sketchingIsEnabled) return false;
			return true;
		}

		[HarmonyPostfix]
		[HarmonyPatch("SaveMapData")]
		public static void SaveMapDataPrefix(Minimap __instance) {
			try {
				DataSerializer.Save(sketchData, DataType.Sketch);
				DataSerializer.Save(maskData, DataType.Mask);
			} catch (Exception e) {
				Plugin.Log.LogError("Couldn't save cartographer data for this world.");
				Plugin.Log.LogDebug(e);
			}
		}

		public static void LoadMapData(Texture2D mapTexture, Texture2D fogTexture, Texture2D heightTexture, Texture2D forestTexture, bool[] explored) {
			var expectedSize = Minimap.instance.m_textureSize * Minimap.instance.m_textureSize;
			Plugin.Log.LogInfo("loading cartographer sketch data");
			try {
				sketchData = DataSerializer.Load(DataType.Sketch);
				maskData = DataSerializer.Load(DataType.Mask);
				Plugin.Log.LogDebug("loaded image file");
				int changedPixels = 0;
				for (var i = 0; i < expectedSize; i++) {
					if (sketchData[i].a != 0) {
						var x = i % Minimap.instance.m_textureSize;
						var y = (int)(i / Minimap.instance.m_textureSize);
						mapTexture.SetPixel(x, y, sketchData[i]);
						fogTexture.SetPixel(x, y, maskData[i]);
						forestTexture.SetPixel(x, y, unmask);
						heightTexture.SetPixel(x, y, neutralHeight);
						changedPixels++;
					}
				}
				mapTexture.Apply();
				fogTexture.Apply();
				forestTexture.Apply();
				heightTexture.Apply();
				Plugin.Log.LogDebug($"applied {changedPixels} sketch pixels to world map");
			} catch (Exception e) {
				Plugin.Log.LogWarning("Couldn't load cartographer data for this world. It might just not have been generated yet.");
				Plugin.Log.LogDebug(e);
				sketchData = new Color[expectedSize];
				maskData = new Color[expectedSize];
			}
		}


		public static void OnMapRightUp(UIInputHandler handler) {
			if (!sketchingIsEnabled) return;
			Plugin.Log.LogDebug("stop sketch stroke.");
			isSketching = false;
		}

		public static void OnMapRightDown(UIInputHandler handler) {
			if (!sketchingIsEnabled) return;
			Plugin.Log.LogDebug("start sketch stroke.");
			isSketching = true;
		}

		private static Texture2D CopyTexture(Texture2D source, TextureFormat format) {
			var copy = new Texture2D(source.width, source.height, format, false);
			copy.wrapMode = TextureWrapMode.Clamp;
			copy.SetPixels(source.GetPixels());
			copy.Apply();
			return copy;
		}
	}
}