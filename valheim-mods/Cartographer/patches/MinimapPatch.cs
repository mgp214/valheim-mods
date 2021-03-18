using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Cartographer.DataSerializer;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Cartographer {

	[HarmonyPatch(typeof(Minimap))]
	class MinimapPatch {

		private static bool isSketching = false;
		public static bool sketchingIsEnabled = false;
		private static Color sketchColor = Color.white;
		private static float radius = 2f;
		private static bool hasInitialized = false;
		private static RawImage sketchImage;
		private static Texture2D sketchTexture;
		private static RectTransform panelRect;

		private static float sketchResolutionFactor = 2;
		private static int SketchSize { get => (int)((float)Minimap.instance.m_textureSize * sketchResolutionFactor); }

		[HarmonyReversePatch]
		[HarmonyPatch(typeof(Minimap), "ScreenToWorldPoint")]
		public static Vector3 ScreenToWorldPoint(Minimap instance, Vector3 mousePos) {
			throw new NotImplementedException("It's a stub");
		}

		// [HarmonyReversePatch]
		// [HarmonyPatch(typeof(Minimap), "WorldToPixel")]
		// public static void WorldToPixel(Minimap instance, Vector3 p, out int mx, out int my) {
		// 	throw new NotImplementedException("It's a stub");
		// }

		public static void WorldToPixel(Vector3 p, out int x, out int y) {
			int offset = SketchSize / 2;
			x = Mathf.RoundToInt(p.x / Minimap.instance.m_pixelSize * sketchResolutionFactor + (float)offset);
			y = Mathf.RoundToInt(p.z / Minimap.instance.m_pixelSize * sketchResolutionFactor + (float)offset);
		}

		[HarmonyReversePatch]
		[HarmonyPatch(typeof(Minimap), "WorldToMapPoint")]
		public static void WorldToMapPoint(Minimap instance, Vector3 p, out float x, out float y) {
			throw new NotImplementedException("It's a stub");
		}

		private static RectTransform CreateTextButton(out Text text, out Button button, string label = null, Font font = null, FontStyle style = FontStyle.Normal, int fontSize = 10, UnityAction onClick = null) {
			var obj = new GameObject();
			var rect = obj.AddComponent<RectTransform>();
			button = obj.AddComponent<Button>();
			if (onClick != null)
				button.onClick.AddListener(onClick);

			var textObj = new GameObject();
			text = textObj.AddComponent<Text>();
			text.text = label;
			text.rectTransform.SetParent(rect, false);
			if (font != null) text.font = font;
			text.fontStyle = style;
			text.fontSize = fontSize;
			text.alignment = TextAnchor.UpperCenter;
			text.color = Color.white;

			return rect;
		}

		private static RectTransform CreateImageButton(out RawImage image, out RawImage disabledImage, out Button button, bool startEnabled = false, UnityAction onClick = null) {
			var obj = new GameObject();
			var rect = obj.AddComponent<RectTransform>();
			button = obj.AddComponent<Button>();
			image = obj.AddComponent<RawImage>();
			var imgObj = new GameObject();
			var imgRect = imgObj.AddComponent<RectTransform>();
			var xObj = new GameObject();
			var xRect = xObj.AddComponent<RectTransform>();
			disabledImage = xObj.AddComponent<RawImage>();
			xRect.SetParent(rect, false);
			disabledImage.enabled = !startEnabled;
			if (onClick != null)
				button.onClick.AddListener(onClick);
			return rect;
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
			panelRect.offsetMin = new Vector2(-75, -100);
			panelRect.offsetMax = new Vector2(0, 100);

			panelImg.color = new Color(0.33f, 0.33f, 0.33f, 0.33f);

			panelVlg.padding = new RectOffset(5, 5, 5, 5);
			panelVlg.spacing = 5f;
			panelVlg.childAlignment = TextAnchor.UpperCenter;
			panelVlg.childControlHeight = false;
			panelVlg.childControlWidth = false;
			panelVlg.childForceExpandHeight = false;
			panelVlg.childForceExpandWidth = false;

			// var sketchEnableObj = new GameObject();
			// var sketchEnableBtn = sketchEnableObj.AddComponent<Button>();
			// var sketchEnableRect = sketchEnableObj.AddComponent<RectTransform>();
			// sketchEnableRect.offsetMin = Vector2.zero;
			// sketchEnableRect.offsetMax = Vector2.up * 20;

			// sketchEnableRect.pivot = Vector2.one * 0.5f;

			// sketchEnableRect.SetParent(panelRect, false);
			// var sketchEnableTxtObj = new GameObject();
			// var sketchEnableTxt = sketchEnableTxtObj.AddComponent<Text>();
			// sketchEnableTxt.rectTransform.SetParent(sketchEnableRect, false);
			// sketchEnableTxt.rectTransform.pivot = Vector2.one * 0.5f;
			// sketchEnableTxt.font = font;
			// sketchEnableTxt.fontSize = fontSize;
			// sketchEnableTxt.fontStyle = fontStyle;
			// sketchEnableTxt.text = "Sketch";
			// sketchEnableTxt.alignment = TextAnchor.UpperCenter;
			// sketchEnableTxt.color = Color.gray;

			// sketchEnableTxt.color = Color.gray;
			// sketchEnableBtn.onClick.AddListener(() => {
			// 	sketchingIsEnabled = !sketchingIsEnabled;
			// 	Plugin.Log.LogDebug("toggling sketchingIsEnabled: " + sketchingIsEnabled);
			// 	sketchEnableTxt.text = sketchingIsEnabled ? "Sketching" : "Sketch";
			// 	sketchEnableTxt.color = sketchingIsEnabled ? Color.white : Color.gray;
			// });

			// var sketchEnableRect = CreateTextButton(out var sketchEnableText, out var sketchEnableBtn, "Sketch", font: font, style: fontStyle, fontSize: fontSize);
			// sketchEnableRect_text.SetParent(panelRect, false);
			// var sketchEnableRect = CreateImageButton(out var image, out var disabledImage, out var button);
			// sketchEnableRect.SetParent(panelRect, false);
			// sketchEnableRect.offsetMin = new Vector2(-10, -10);
			// sketchEnableRect.offsetMax = new Vector2(10, 10);
			// // image.color = Color.gray;
			// image.texture = DataSerializer.LoadTextureAsset("pencil.png");
			// disabledImage.texture = DataSerializer.LoadTextureAsset("disabled.png");
			// button.onClick.AddListener(() => {
			// 	sketchingIsEnabled = !sketchingIsEnabled;
			// 	disabledImage.enabled = !sketchingIsEnabled;
			// 	Plugin.Log.LogDebug("toggling sketchingIsEnabled: " + sketchingIsEnabled);
			// 	// sketchEnableText.text = sketchingIsEnabled ? "Sketching" : "Sketch";
			// 	// image.color = sketchingIsEnabled ? Color.white : Color.gray;
			// });

			var sketchEnableToggle = new ToggleComponent(panelRect, new Vector2(30, 30), "pencil.png", false);
			sketchEnableToggle.onValueChanged.AddListener((val) => {
				sketchingIsEnabled = val;
				Plugin.Log.LogDebug("toggling sketchingIsEnabled: " + sketchingIsEnabled);
			});

			var colorSwatchGrpObj = new GameObject();
			var colorSwatchGrpRect = colorSwatchGrpObj.AddComponent<RectTransform>();
			colorSwatchGrpRect.offsetMin = new Vector2(colorSwatchGrpRect.offsetMin.x, -10);
			colorSwatchGrpRect.offsetMax = new Vector2(colorSwatchGrpRect.offsetMax.x, 10);
			var colorSwatchGrpHlg = colorSwatchGrpObj.AddComponent<HorizontalLayoutGroup>();
			colorSwatchGrpRect.SetParent(panelRect, false);

			foreach (var color in Plugin.sketchColors) {
				AddColorSwatch(colorSwatchGrpRect, color, ColorUtility.ToHtmlStringRGB(color));
			}

			var sizeObj = new GameObject();
			var sizeRect = sizeObj.AddComponent<RectTransform>();
			sizeRect.offsetMin = new Vector2(colorSwatchGrpRect.offsetMin.x, -10);
			sizeRect.offsetMax = new Vector2(colorSwatchGrpRect.offsetMax.x, 10);
			var sizeHlg = sizeObj.AddComponent<HorizontalLayoutGroup>();
			// sizeRect.SetParent(panelRect, false);

			var sizeDisplayObj = new GameObject();
			var sizeDisplayRect = sizeDisplayObj.AddComponent<RectTransform>();
			sizeDisplayRect.offsetMin = new Vector2(-50, -50);
			sizeDisplayRect.offsetMax = new Vector2(50, 50);
			var sizeDisplayTxt = sizeDisplayObj.AddComponent<Text>();
			sizeDisplayTxt.resizeTextForBestFit = true;
			sizeDisplayTxt.text = radius.ToString();

			var sizeDownBtnObj = new GameObject();
			var sizeDownRect = sizeDownBtnObj.AddComponent<RectTransform>();
			sizeDownRect.offsetMin = new Vector2(-50, -50);
			sizeDownRect.offsetMax = new Vector2(50, 50);
			var sizeDownBtn = sizeDownBtnObj.AddComponent<Button>();
			var sizeDownBtnTxtObj = new GameObject();
			var sizeDownBtnTxt = sizeDownBtnTxtObj.AddComponent<Text>();
			sizeDownBtnTxt.text = "-";

			sizeDownBtn.onClick.AddListener(() => {
				radius--;
				sizeDisplayTxt.text = radius.ToString();
				Plugin.Log.LogDebug("radius: " + radius);
			});

			sizeDownRect.SetParent(sizeRect, false);
			sizeDisplayRect.SetParent(sizeRect, false);

			var sizeUpBtnObj = new GameObject();
			var sizeUpRect = sizeUpBtnObj.AddComponent<RectTransform>();
			sizeDownRect.offsetMin = new Vector2(-50, -50);
			sizeDownRect.offsetMax = new Vector2(50, 50);
			var sizeUpBtn = sizeUpBtnObj.AddComponent<Button>();
			var sizeUpBtnTxtObj = new GameObject();
			var sizeUpBtnTxt = sizeUpBtnTxtObj.AddComponent<Text>();
			sizeUpBtnTxt.text = "+";

			sizeUpBtn.onClick.AddListener(() => {
				radius++;
				sizeDisplayTxt.text = radius.ToString();
				Plugin.Log.LogDebug("radius: " + radius);
			});

			sizeUpRect.SetParent(sizeRect, false);
		}

		private static void AddColorSwatch(RectTransform parent, Color color, string label) {
			var swatch = new GameObject();
			var swatchRect = swatch.AddComponent<RectTransform>();
			var swatchImg = swatch.AddComponent<Image>();
			swatchImg.color = color;
			var swatchBtn = swatch.AddComponent<Button>();

			swatchRect.offsetMin = new Vector2(-8, -8);
			swatchRect.offsetMax = new Vector2(8, 8);
			swatchRect.SetParent(parent, false);

			swatchBtn.onClick.AddListener(() => {
				sketchColor = color;
				Plugin.Log.LogDebug($"sketchColor: {label}");
			});
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

		static void Initialize() {
			Plugin.Log.LogInfo("initializing cartographer");
			var sketchImageObj = new GameObject("Sketch Image Overlay");
			var sketchCanvasGroup = sketchImageObj.AddComponent<CanvasGroup>();
			sketchCanvasGroup.interactable = false;
			sketchCanvasGroup.blocksRaycasts = false;
			sketchImage = sketchImageObj.AddComponent<RawImage>();
			var sketchRect = sketchImage.rectTransform;
			var mapRect = Minimap.instance.m_mapImageLarge.rectTransform;

			sketchRect.SetParent(mapRect.parent, false);
			// sketchRect.SetSiblingIndex(mapRect.GetSiblingIndex());
			sketchRect.anchorMin = mapRect.anchorMin;
			sketchRect.anchorMax = mapRect.anchorMax;
			sketchRect.position = mapRect.position;
			sketchRect.offsetMax = mapRect.offsetMax;
			sketchRect.offsetMin = mapRect.offsetMin;

			sketchTexture = LoadMapData();
			sketchImage.texture = sketchTexture;
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

			// BELOW only executes when everything is loaded in.
			if (Player.m_localPlayer == null) return;
			if (!hasInitialized) {
				hasInitialized = true;
				Initialize();
			}

			// if (Input.GetKeyDown(KeyCode.N)) {
			// 	if (Input.GetKey(KeyCode.LeftShift)) {
			// 		radius--;
			// 		Plugin.Log.LogDebug("radius: " + radius);
			// 	} else {
			// 		radius++;
			// 		Plugin.Log.LogDebug("radius: " + radius);
			// 	}
			// }
			if (Input.GetKeyDown(KeyCode.LeftBracket)) {
				radius--;
				Plugin.Log.LogDebug("radius: " + radius);
			}
			if (Input.GetKeyDown(KeyCode.RightBracket)) {
				radius++;
				Plugin.Log.LogDebug("radius: " + radius);
			}

			if (Input.GetKeyDown(KeyCode.End)) {
				sketchTexture = ResetMapData();
				sketchImage.texture = sketchTexture;
			}

			CenterSketch(Player.m_localPlayer.transform.position + ___m_mapOffset, ___m_largeZoom);

			if (isSketching) {
				var pos = ScreenToWorldPoint(__instance, Input.mousePosition);
				WorldToPixel(pos, out int x, out int y);
				Plugin.Log.LogDebug($"sketching with r: {radius}");
				if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
					Sketch(x, y, radius, sketchTexture, Color.clear);
				} else {
					Sketch(x, y, radius, sketchTexture, sketchColor);
				}
			}
		}

		private static void Sketch(int x, int y, float radius, Texture2D texture, Color color) {
			int r = (int)Mathf.Ceil(radius);
			for (int i = y - r; i <= y + r; i++) {
				for (int j = x - r; j <= x + r; j++) {
					if (j >= 0 && i >= 0 && j < SketchSize && i < SketchSize && new Vector2((float)(j - x), (float)(i - y)).magnitude <= (float)r) {
						if (j == 0 || j == SketchSize - 1 || i == 0 || i == SketchSize - 1) continue;
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
				hasInitialized = false;
			} catch (Exception e) {
				Plugin.Log.LogError("Couldn't save map data for this world.");
				Plugin.Log.LogDebug(e);
			}
		}

		public static Texture2D LoadMapData() {
			Plugin.Log.LogInfo("loading map data");
			Texture2D texture;
			try {
				texture = DataSerializer.Load(DataType.Sketch);
				Plugin.Log.LogDebug("loaded image file");
			} catch (Exception e) {
				Plugin.Log.LogWarning($"Couldn't load map data for this world: {e.GetType().ToString()}. It might just not have been generated yet.");
				Plugin.Log.LogWarning($"Creating blank sketch data with size {SketchSize}x{SketchSize}");
				return ResetMapData();
			}
			return texture;
		}

		private static Texture2D ResetMapData() {
			Plugin.Log.LogWarning($"Creating blank sketch data with size {SketchSize}x{SketchSize}");
			var texture = new Texture2D(SketchSize, SketchSize, TextureFormat.RGBA32, false);
			texture.wrapMode = TextureWrapMode.Clamp;
			var pixels = new Color[SketchSize * SketchSize];
			for (var i = 0; i < pixels.Length; i++) {
				pixels[i] = Color.clear;
			}
			texture.SetPixels(pixels);
			texture.Apply();
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