using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Cartographer.DataSerializer;
using UnityEngine.UI;

namespace Cartographer {

	[HarmonyPatch(typeof(Minimap))]
	class MinimapPatch {

		private static bool isSketching = false;
		public static bool sketchingIsEnabled = false;
		private static Color sketchColor = Color.red;
		private static float radius = 15f;
		private static bool hasInitialized = false;

		private static RectTransform sizeSliderRect;

		private static RawImage sketchImage;
		private static Texture2D sketchTexture;
		private static RectTransform panelRect;

		public const int sketchDataSize = 4096;
		public static float pixelSize;

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
		[HarmonyPatch(typeof(Minimap), "WorldToMapPoint")]
		public static void WorldToMapPoint(Minimap instance, Vector3 p, out float x, out float y) {
			throw new NotImplementedException("It's a stub");
		}

		[HarmonyPostfix]
		[HarmonyPatch("Start")]
		public static void StartPostfix(Minimap __instance, Texture2D ___m_mapTexture, Texture2D ___m_fogTexture, Texture2D ___m_forestMaskTexture, Texture2D ___m_heightTexture) {
			UIInputHandler component = __instance.m_mapImageLarge.GetComponent<UIInputHandler>();
			component.m_onRightDown = (Action<UIInputHandler>)Delegate.Combine(component.m_onRightDown, new Action<UIInputHandler>(MinimapPatch.OnMapRightDown));
			component.m_onRightUp = (Action<UIInputHandler>)Delegate.Combine(component.m_onRightUp, new Action<UIInputHandler>(MinimapPatch.OnMapRightUp));

			var font = Minimap.instance.m_biomeNameLarge.font;
			var fontSize = Minimap.instance.m_biomeNameLarge.fontSize;
			var fontStyle = Minimap.instance.m_biomeNameLarge.fontStyle;

			var panel = new GameObject("sketch panel");
			panelRect = panel.AddComponent<RectTransform>();
			var panelImg = panel.AddComponent<Image>();
			var panelVlg = panel.AddComponent<VerticalLayoutGroup>();
			panelRect.SetParent(Minimap.instance.m_mapImageLarge.rectTransform, false);

			panelRect.anchorMax = new Vector2(1, 0.5f);
			panelRect.anchorMin = new Vector2(1, 0.5f);
			panelRect.pivot = new Vector2(1, 0.5f);
			panelRect.offsetMin = new Vector2(-100, -100);
			panelRect.offsetMax = new Vector2(0, 100);

			panelImg.color = new Color(0.33f, 0.33f, 0.33f, 0.33f);

			panelVlg.padding = new RectOffset(5, 5, 5, 5);
			panelVlg.spacing = 5f;
			panelVlg.childAlignment = TextAnchor.UpperLeft;
			panelVlg.childControlHeight = false;
			panelVlg.childControlWidth = true;
			panelVlg.childForceExpandHeight = false;
			panelVlg.childForceExpandWidth = true;

			var sketchEnableObj = new GameObject();
			var sketchEnableBtn = sketchEnableObj.AddComponent<Button>();
			var sketchEnableRect = sketchEnableObj.AddComponent<RectTransform>();

			sketchEnableRect.SetParent(panelRect, false);
			var sketchEnableTxtObj = new GameObject();
			var sketchEnableTxt = sketchEnableTxtObj.AddComponent<Text>();
			sketchEnableTxt.rectTransform.SetParent(sketchEnableRect, false);
			sketchEnableTxt.rectTransform.anchorMin = Vector2.zero;
			sketchEnableTxt.rectTransform.anchorMax = Vector2.one;
			sketchEnableTxt.rectTransform.pivot = Vector2.one * 0.5f;
			sketchEnableTxt.font = font;
			sketchEnableTxt.fontSize = fontSize;
			sketchEnableTxt.fontStyle = fontStyle;
			sketchEnableTxt.text = "Sketch";
			sketchEnableTxt.alignment = TextAnchor.UpperLeft;
			sketchEnableTxt.color = Color.gray;

			sketchEnableTxt.color = Color.gray;
			sketchEnableBtn.onClick.AddListener(() => {
				sketchingIsEnabled = !sketchingIsEnabled;
				Plugin.Log.LogDebug("toggling sketchingIsEnabled: " + sketchingIsEnabled);
				sketchEnableTxt.text = sketchingIsEnabled ? "Sketching" : "Sketch";
				sketchEnableTxt.color = sketchingIsEnabled ? Color.white : Color.gray;
			});
			Debug.Log("sizeSliderObj");
			var sizeSliderObj = new GameObject();
			var sizeSliderSlider = sizeSliderObj.AddComponent<Slider>();
			sizeSliderRect = sizeSliderObj.GetComponent<RectTransform>();

			Debug.Log("sizeSliderFillObj");
			var sizeSliderFillObj = new GameObject();
			var sizeSliderFillRect = sizeSliderFillObj.AddComponent<RectTransform>();
			sizeSliderFillRect.anchorMax = new Vector2(1, 0.75f);
			sizeSliderFillRect.anchorMin = new Vector2(0, 0.25f);
			sizeSliderFillRect.pivot = new Vector2(0.5f, 0.5f);
			sizeSliderFillRect.offsetMin = new Vector2(5, 0);
			sizeSliderFillRect.offsetMax = new Vector2(5, 0);
			var sizeSliderFillImg = sizeSliderFillObj.AddComponent<Image>();
			sizeSliderFillImg.color = Color.gray;

			sizeSliderFillRect.SetParent(sizeSliderRect, false);
			sizeSliderSlider.fillRect = sizeSliderFillRect;
			var sizeSliderSlideArealObj = new GameObject();
			var sizeSliderSlideAreaRect = sizeSliderSlideArealObj.AddComponent<RectTransform>();
			sizeSliderSlideAreaRect.anchorMax = Vector2.one;
			sizeSliderSlideAreaRect.anchorMin = Vector2.zero;
			sizeSliderSlideAreaRect.pivot = new Vector2(0.5f, 0.5f);
			sizeSliderSlideAreaRect.offsetMin = new Vector2(10, 0);
			sizeSliderSlideAreaRect.offsetMax = new Vector2(10, 0);

			sizeSliderSlideAreaRect.SetParent(sizeSliderRect, false);
			sizeSliderSlider.handleRect = sizeSliderFillRect;
			var sizeSliderHandlelObj = new GameObject();
			var sizeSliderHandleRect = sizeSliderHandlelObj.AddComponent<RectTransform>();
			sizeSliderHandleRect.anchorMax = new Vector2(-5, 0.75f);
			sizeSliderHandleRect.anchorMin = new Vector2(5, 0.25f);
			sizeSliderHandleRect.pivot = new Vector2(0.5f, 0.5f);
			sizeSliderHandleRect.offsetMin = new Vector2(10, 0);
			sizeSliderHandleRect.offsetMax = new Vector2(10, 0);
			var sizeSliderHandleImg = sizeSliderHandlelObj.AddComponent<Image>();
			sizeSliderHandleImg.color = Color.gray;

			sizeSliderHandleRect.SetParent(sizeSliderSlideAreaRect, false);
			sizeSliderSlider.handleRect = sizeSliderHandleRect;
			sizeSliderSlider.minValue = 1;
			sizeSliderSlider.maxValue = 200f;
			sizeSliderSlider.onValueChanged.AddListener((value) => {
				Debug.Log("radius changed to " + value);
				radius = value;
			});
			sizeSliderRect.SetParent(panelRect, false);

		}

		private static void CenterSketch(Vector3 centerPoint, float largeZoom) {
			WorldToMapPoint(Minimap.instance, centerPoint, out float x, out float y);
			sketchImage.uvRect = Minimap.instance.m_mapImageLarge.uvRect;
			Rect uvRectLarge = Minimap.instance.m_mapImageSmall.uvRect;
			RectTransform mapRectLarge = Minimap.instance.m_mapImageLarge.transform as RectTransform;
			uvRectLarge.width = largeZoom * mapRectLarge.rect.width / mapRectLarge.rect.height;
			uvRectLarge.height = largeZoom;
			uvRectLarge.center = new Vector2(x, y);
			sketchImage.uvRect = uvRectLarge;
		}

		[HarmonyPostfix]
		[HarmonyPatch("Update")]
		public static void UpdatePostfix(
				Minimap __instance,
				Texture2D ___m_mapTexture,
				Texture2D ___m_fogTexture,
				Texture2D ___m_forestMaskTexture,
				Texture2D ___m_heightTexture,
				bool[] ___m_explored,
				Vector3 ___m_mapOffset,
				float ___m_largeZoom) {

			if (!hasInitialized) {
				Plugin.Log.LogInfo("initializing cartographer");
				hasInitialized = true;

				var sketchImageObj = new GameObject("Sketch Image Overlay");
				var sketchCanvasGroup = sketchImageObj.AddComponent<CanvasGroup>();
				sketchCanvasGroup.interactable = false;
				sketchCanvasGroup.blocksRaycasts = false;
				sketchImage = sketchImageObj.AddComponent<RawImage>();
				var sketchRect = sketchImage.rectTransform;
				var mapRect = Minimap.instance.m_mapImageLarge.rectTransform;
				sketchRect.SetParent(mapRect.parent, false);
				sketchRect.anchorMin = mapRect.anchorMin;
				sketchRect.anchorMax = mapRect.anchorMax;
				sketchRect.position = mapRect.position;
				sketchRect.offsetMax = mapRect.offsetMax;
				sketchRect.offsetMin = mapRect.offsetMin;

				sketchTexture = LoadMapData(sketchTexture);
				sketchImage.texture = sketchTexture;
				pixelSize = Minimap.instance.m_pixelSize / ((float)Minimap.instance.m_textureSize / (float)sketchDataSize);
			}

			if (Input.GetKeyDown(KeyCode.N)) {
				radius++;
				Plugin.Log.LogDebug("radius: " + radius);
			}
			if (Input.GetKeyDown(KeyCode.B)) {
				radius--;
				Plugin.Log.LogDebug("radius: " + radius);
			}
			// BELOW only executes when everything is loaded in.
			if (Player.m_localPlayer == null) return;
			UpdateRect(panelRect);

			CenterSketch(Player.m_localPlayer.transform.position + ___m_mapOffset, ___m_largeZoom);

			if (isSketching) {
				var pos = ScreenToWorldPoint(__instance, Input.mousePosition);
				WorldToPixel(__instance, pos, out int x, out int y);
				Plugin.Log.LogDebug($"sketching with r: {radius}");
				if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
					Sketch(x, y, radius, sketchTexture, Color.clear);
				} else {
					Sketch(x, y, radius, sketchTexture, sketchColor);
				}
			}
		}

		static void UpdateRect(RectTransform rectTransform) {
			var updated = false;
			var changeAmount = Time.deltaTime * 25f;
			if (Input.GetKey(KeyCode.LeftArrow)) {
				if (Input.GetKey(KeyCode.LeftShift)) {
					rectTransform.offsetMin += Vector2.left * changeAmount;
				} else {
					rectTransform.offsetMax += Vector2.left * changeAmount;
				}
				updated = true;
			}
			if (Input.GetKey(KeyCode.RightArrow)) {
				if (Input.GetKey(KeyCode.LeftShift)) {
					rectTransform.offsetMin += Vector2.right * changeAmount;
				} else {
					rectTransform.offsetMax += Vector2.right * changeAmount;
				}
				updated = true;
			}
			if (Input.GetKey(KeyCode.UpArrow)) {
				if (Input.GetKey(KeyCode.LeftShift)) {
					rectTransform.offsetMin += Vector2.up * changeAmount;
				} else {
					rectTransform.offsetMax += Vector2.up * changeAmount;
				}
				updated = true;
			}
			if (Input.GetKey(KeyCode.DownArrow)) {
				if (Input.GetKey(KeyCode.LeftShift)) {
					rectTransform.offsetMin += Vector2.down * changeAmount;
				} else {
					rectTransform.offsetMax += Vector2.down * changeAmount;
				}
				updated = true;
			}

			if (updated) {
				Plugin.Log.LogDebug($"min: {rectTransform.offsetMin}, max: {rectTransform.offsetMax}");
			}
		}
		private static void Sketch(int x, int y, float radius, Texture2D texture, Color color) {
			int r = (int)Mathf.Ceil(radius / Minimap.instance.m_pixelSize);
			for (int i = y - r; i <= y + r; i++) {
				for (int j = x - r; j <= x + r; j++) {
					if (j >= 0 && i >= 0 && j < Minimap.instance.m_textureSize && i < Minimap.instance.m_textureSize && new Vector2((float)(j - x), (float)(i - y)).magnitude <= (float)r) {
						if (j == 0 || j == Minimap.instance.m_textureSize - 1 || i == 0 || i == Minimap.instance.m_textureSize - 1) continue;
						texture.SetPixel(j, i, color);
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
				DataSerializer.Save(sketchTexture, DataType.Sketch);
			} catch (Exception e) {
				Plugin.Log.LogError("Couldn't save map data for this world.");
				Plugin.Log.LogDebug(e);
			}
		}

		public static Texture2D LoadMapData(Texture2D sketchTexture) {
			var expectedSize = Minimap.instance.m_textureSize * Minimap.instance.m_textureSize;
			Plugin.Log.LogInfo("loading map data");
			Texture2D texture;
			try {
				texture = DataSerializer.Load(DataType.Sketch);
				Plugin.Log.LogDebug("loaded image file");
			} catch (Exception e) {
				Plugin.Log.LogWarning("Couldn't load map data for this world. It might just not have been generated yet.");
				Plugin.Log.LogDebug(e);
				Plugin.Log.LogWarning("Creating blank sketch data");

				texture = new Texture2D(Minimap.instance.m_textureSize, Minimap.instance.m_textureSize, TextureFormat.RGBA32, false);
				texture.wrapMode = TextureWrapMode.Clamp;
				var pixels = new Color[Minimap.instance.m_textureSize * Minimap.instance.m_textureSize];
				for (var i = 0; i < pixels.Length; i++) {
					pixels[i] = Color.clear;
				}
				texture.SetPixels(pixels);
				texture.Apply();
			}
			return texture;
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
	}
}