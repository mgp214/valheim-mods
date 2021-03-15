using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Cartographer {
	class SketchToggleComponent : BaseComponent {

		public static SketchToggleComponent New(RectTransform parent) {
			var go = new GameObject();
			var component = go.AddComponent<SketchToggleComponent>();
			component.Initialize(parent);
			return component;
		}

		Text text;
		public void SetEnabled(bool value) {
			text.color = value ? Color.white : Color.gray;
			text.text = value ? "Sketching" : "Sketch";
		}
		public override void Initialize(RectTransform parent) {
			base.Initialize(parent);
			text = gameObject.AddComponent<Text>();
			text.rectTransform.anchorMax = Vector2.one;
			text.rectTransform.anchorMin = Vector2.one;
			text.resizeTextForBestFit = true;
			text.rectTransform.offsetMin = new Vector2(-50, -15);
			text.rectTransform.offsetMax = new Vector2(50, 15);
			text.rectTransform.pivot = Vector2.one;
			text.text = "Sketch";
			text.fontSize = Minimap.instance.m_biomeNameLarge.fontSize;
			text.font = Minimap.instance.m_biomeNameLarge.font;
			text.fontStyle = Minimap.instance.m_biomeNameLarge.fontStyle;

			var et = gameObject.AddComponent<EventTrigger>();
			var entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerClick;
			et.triggers.Add(entry);
			entry.callback.AddListener((data) => {
				Plugin.Log.LogDebug($"sketching toggle clicked, new value of sketcingIsEnabled: {MinimapPatch.sketchingIsEnabled}");
				MinimapPatch.sketchingIsEnabled = !MinimapPatch.sketchingIsEnabled;
				SetEnabled(MinimapPatch.sketchingIsEnabled);
			});
		}
	}

}