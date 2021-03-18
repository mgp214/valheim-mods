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
public class ToggleComponent {
	public class ValueChangedEvent : UnityEvent<bool> { }
	private static Texture2D disabledTexture = LoadTextureAsset("disabled.png");
	private static Vector2 half = new Vector2(0.5f, 0.5f);

	public ValueChangedEvent onValueChanged;

	public bool Value { get; private set; }

	private RawImage image;
	private RawImage disabledImage;
	private RectTransform rectTransform;
	private GameObject root;
	private Button button;

	public ToggleComponent(RectTransform parent, Vector2 size, string imageAssetPath, bool value) {
		root = new GameObject();
		button = root.AddComponent<Button>();
		rectTransform = root.AddComponent<RectTransform>();
		var imageObj = new GameObject();
		var imageObjRect = imageObj.AddComponent<RectTransform>();
		image = imageObj.AddComponent<RawImage>();
		var disabledImageObj = new GameObject();
		var disabledImageObjRect = disabledImageObj.AddComponent<RectTransform>();
		disabledImage = disabledImageObj.AddComponent<RawImage>();

		// rectTransform.offsetMin = -size / 2;
		// rectTransform.offsetMax = size / 2;
		// rectTransform.sizeDelta = size;
		// disabledImageObjRect.sizeDelta = size;
		// imageObjRect.sizeDelta = size;

		disabledImageObjRect.anchorMin = half;
		disabledImageObjRect.anchorMax = half;
		imageObjRect.anchorMin = half;
		imageObjRect.anchorMax = half;

		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
		disabledImageObjRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
		disabledImageObjRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
		imageObjRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
		imageObjRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

		// disabledImageObjRect.offsetMin = half;
		// disabledImageObjRect.offsetMax = half;
		// imageObjRect.offsetMin = half;
		// imageObjRect.offsetMax = half;

		imageObjRect.SetParent(rectTransform, false);
		disabledImageObjRect.SetParent(rectTransform, false);
		rectTransform.SetParent(parent, false);

		disabledImage.texture = disabledTexture;
		image.texture = LoadTextureAsset(imageAssetPath);

		// image.SetNativeSize();
		// disabledImage.SetNativeSize();

		Value = value;
		disabledImageObj.SetActive(!Value);

		onValueChanged = new ValueChangedEvent();

		button.onClick.AddListener(() => {
			Value = !Value;
			disabledImageObj.SetActive(!Value);
			onValueChanged.Invoke(Value);
		});
	}


}