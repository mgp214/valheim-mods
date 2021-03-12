using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Cartographer {
	class DrawManager : MonoBehaviour {
		public static DrawManager Instance { get; private set; }

		public static void Create() {
			var instanceGameObject = new GameObject("Cartographer Draw Manager");
			Instance = instanceGameObject.AddComponent<DrawManager>();
			Instance.Initialize();
		}

		private Color drawColor = Color.white;

		public bool IsDrawing { get; private set; }

		private void Initialize() {
			IsDrawing = false;
			// CreateDrawButton(Minimap.instance.m_publicPosition);
			CreateDrawButton(Minimap.instance.m_selectedIcon4);
		}

		private void Update() {
			if (Input.GetKeyDown(KeyCode.P)) {
				IsDrawing = !IsDrawing;
				// checkImage.gameObject.SetActive(IsDrawing);
			}
		}

		private void CreateDrawButton(Image sourceImage) {
			Debug.Log("Creating draw button");
			var buttonGameObject = new GameObject("Draw button");
			buttonGameObject.transform.SetParent(sourceImage.transform.parent);
			buttonGameObject.transform.position = sourceImage.rectTransform.position + new Vector3(-150, 0);
			var button = buttonGameObject.AddComponent<Button>();

			var innerElement = new GameObject($"Draw button inner element");
			var image = innerElement.AddComponent<Image>();
			image.sprite = Minimap.instance.m_icons.Find(x => x.m_name == Minimap.PinType.Icon3).m_icon;
			innerElement.transform.SetParent(buttonGameObject.transform);
			innerElement.transform.localScale = Vector3.one * 0.5f;

			var checkElement = new GameObject($"Draw button checkmark");
			var checkImage = checkElement.AddComponent<Image>();
			checkImage.color = drawColor;
			checkElement.transform.SetParent(buttonGameObject.transform);
			checkElement.transform.localScale = Vector3.one * 0.5f;
			checkElement.SetActive(false);


			button.onClick.AddListener(() => {
				IsDrawing = !IsDrawing;
				checkImage.gameObject.SetActive(IsDrawing);
			});
		}
	}
}