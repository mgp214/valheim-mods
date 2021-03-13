using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Cartographer {

	[HarmonyPatch(typeof(Minimap))]
	class MinimapPatch {

		private static bool isDrawing = false;
		private static bool drawingIsEnabled = false;
		private static Color unmask = new Color(0, 0, 0, 0);
		private static Color neutralHeight = new Color(31f, 0, 0);
		private static Color mask = Color.white;
		private static Color drawColor = Color.red;
		private static float radius = 10f;
		private static float fogInset = 5f;

		private static bool hasBackedUpTextures = false;

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
		}

		[HarmonyPostfix]
		[HarmonyPatch("Update")]
		public static void UpdatePostfix(Minimap __instance, Texture2D ___m_mapTexture, Texture2D ___m_fogTexture, Texture2D ___m_forestMaskTexture, Texture2D ___m_heightTexture, bool[] ___m_explored) {
			if (!hasBackedUpTextures) {
				backupTextures[___m_mapTexture] = CopyTexture(___m_mapTexture, TextureFormat.RGBA32);
				backupTextures[___m_forestMaskTexture] = CopyTexture(___m_forestMaskTexture, TextureFormat.RGBA32);
				backupTextures[___m_heightTexture] = CopyTexture(___m_heightTexture, TextureFormat.RFloat);
				hasBackedUpTextures = true;
			}
			if (Input.GetKeyDown(KeyCode.P)) {
				drawingIsEnabled = !drawingIsEnabled;
				Debug.Log("drawingIsEnabled toggled, new value: " + drawingIsEnabled);
			}
			if (isDrawing) {
				var pos = ScreenToWorldPoint(__instance, Input.mousePosition);
				WorldToPixel(__instance, pos, out int x, out int y);
				var fogRadius = Mathf.Clamp(radius - fogInset, 1, 500);
				// Debug.Log($"drawing position {pos}, r: {radius}, fog: {fogRadius}");
				if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
					EraseRadius(x, y, radius, ___m_mapTexture, backupTextures[___m_mapTexture]);
					EraseRadius(x, y, radius, ___m_forestMaskTexture, backupTextures[___m_forestMaskTexture]);
					Reexplore(x, y, fogRadius, ___m_explored, ___m_fogTexture);
					EraseRadius(x, y, radius, ___m_heightTexture, backupTextures[___m_heightTexture]);
				} else {
					DrawRadius(x, y, radius, ___m_mapTexture, drawColor);
					DrawRadius(x, y, radius, ___m_forestMaskTexture, unmask);
					DrawRadius(x, y, fogRadius, ___m_fogTexture, unmask);
					DrawRadius(x, y, radius, ___m_heightTexture, neutralHeight);
				}
			}
		}
		private static void EraseRadius(int x, int y, float radius, Texture2D texture, Texture2D backupTexture) {
			int r = (int)Mathf.Ceil(radius / Minimap.instance.m_pixelSize);
			for (int i = y - r; i <= y + r; i++) {
				for (int j = x - r; j <= x + r; j++) {
					if (j >= 0 && i >= 0 && j < Minimap.instance.m_textureSize && i < Minimap.instance.m_textureSize && new Vector2((float)(j - x), (float)(i - y)).magnitude <= (float)r) {
						texture.SetPixel(j, i, backupTexture.GetPixel(j, i));
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
						}
					}
				}
			}
			fogTexture.Apply();
		}

		private static void DrawRadius(int x, int y, float radius, Texture2D texture, Color color) {
			int r = (int)Mathf.Ceil(radius / Minimap.instance.m_pixelSize);
			for (int i = y - r; i <= y + r; i++) {
				for (int j = x - r; j <= x + r; j++) {
					if (j >= 0 && i >= 0 && j < Minimap.instance.m_textureSize && i < Minimap.instance.m_textureSize && new Vector2((float)(j - x), (float)(i - y)).magnitude <= (float)r) {
						texture.SetPixel(j, i, color);
					}
				}
			}
			texture.Apply();
		}


		[HarmonyPrefix]
		[HarmonyPatch("OnMapMiddleClick")]
		public static bool OnMapMiddleClickPrefix(Minimap __instance) {
			if (drawingIsEnabled) return true;
			return false;
		}

		public static void OnMapRightUp(UIInputHandler handler) {
			if (!drawingIsEnabled) return;
			Debug.Log("stop drawing.");
			isDrawing = false;
		}

		public static void OnMapRightDown(UIInputHandler handler) {
			if (!drawingIsEnabled) return;
			Debug.Log("start drawing.");
			isDrawing = true;
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