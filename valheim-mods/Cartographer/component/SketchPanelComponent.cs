// using System;
// using System.Collections.Generic;
// using HarmonyLib;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using UnityEngine;
// using UnityEngine.UIElements;
// using UnityEngine.EventSystems;

// namespace Cartographer {
// 	class SketchPanelComponent : BaseComponent {
// 		public VisualElement verticalLayoutGroup;
// 		public RectTransform rectTransform;
// 		private Image image;
// 		public static SketchPanelComponent New(RectTransform parent) {
// 			var go = new GameObject();
// 			var component = go.AddComponent<SketchPanelComponent>();
// 			component.Initialize(parent);
// 			return component;
// 		}

// 		public override void Initialize(RectTransform parent) {
// 			base.Initialize(parent);
// 			verticalLayoutGroup = gameObject.AddComponent<VisualElement>();
// 			verticalLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
// 			verticalLayoutGroup.padding = new RectOffset(10, 10, 10, 10);
// 			image = gameObject.AddComponent<Image>();
// 			image.color = new Color(0.25f, 0.25f, 0.25f, 0.25f);
// 			rectTransform = image.rectTransform;
// 			rectTransform.anchorMax = new Vector2(1, 0.5f);
// 			rectTransform.anchorMin = new Vector2(1, 0.5f);
// 			rectTransform.pivot = new Vector2(1, 0f);
// 			rectTransform.offsetMin = new Vector2(-200, -200);
// 		}

// 		public void Update() {
// 			var updated = false;
// 			var changeAmount = Time.deltaTime * 15f;
// 			if (Input.GetKey(KeyCode.LeftArrow)) {
// 				if (Input.GetKey(KeyCode.LeftShift)) {
// 					rectTransform.offsetMin += Vector2.left * changeAmount;
// 				} else {
// 					rectTransform.offsetMax += Vector2.left * changeAmount;
// 				}
// 				updated = true;
// 			}
// 			if (Input.GetKey(KeyCode.RightArrow)) {
// 				if (Input.GetKey(KeyCode.LeftShift)) {
// 					rectTransform.offsetMin += Vector2.right * changeAmount;
// 				} else {
// 					rectTransform.offsetMax += Vector2.right * changeAmount;
// 				}
// 				updated = true;
// 			}
// 			if (Input.GetKey(KeyCode.UpArrow)) {
// 				if (Input.GetKey(KeyCode.LeftShift)) {
// 					rectTransform.offsetMin += Vector2.up * changeAmount;
// 				} else {
// 					rectTransform.offsetMax += Vector2.up * changeAmount;
// 				}
// 				updated = true;
// 			}
// 			if (Input.GetKey(KeyCode.DownArrow)) {
// 				if (Input.GetKey(KeyCode.LeftShift)) {
// 					rectTransform.offsetMin += Vector2.down * changeAmount;
// 				} else {
// 					rectTransform.offsetMax += Vector2.down * changeAmount;
// 				}
// 				updated = true;
// 			}

// 			if (updated) {
// 				Plugin.Log.LogDebug($"min: {rectTransform.offsetMin}, max: {rectTransform.offsetMax}");
// 			}
// 		}
// 	}
// }