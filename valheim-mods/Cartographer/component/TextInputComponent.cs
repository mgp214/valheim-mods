using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Cartographer {
	class TextInputComponent : BaseComponent {
		public static TextInputComponent New(RectTransform parent) {
			var go = new GameObject();
			var component = go.AddComponent<TextInputComponent>();
			component.Initialize(parent);
			return component;
		}
		public InputField inputField;
		public UnityAction<string> onValueChanged;

		public override void Initialize(RectTransform parent) {
			base.Initialize(parent);
			inputField = gameObject.AddComponent<InputField>();
			inputField.onValueChanged.AddListener((value) => {
				onValueChanged?.Invoke(value);
			});
		}
	}
}