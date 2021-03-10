using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace PinFilters {
	class PinFilterManager : MonoBehaviour {
		private static object _lockObj = new object();
		private static PinFilterManager _Instance;
		public static PinFilterManager Instance {
			get {
				lock (_lockObj) {
					if (_Instance == null) {
						var instanceGameObject = new GameObject("Pin Filter Manager");
						_Instance = instanceGameObject.AddComponent<PinFilterManager>();
						_Instance.Initialize();
					}
				}
				return _Instance;
			}
		}

		private Dictionary<Minimap.PinType, bool> isTypeVisible;

		public bool allPinsHidden;
		private KeyCode toggleKey = KeyCode.F7;
		private static Color visibleColor = Color.white;
		private static Color hiddenColor = Color.gray;

		private bool CanFilterPinType(Minimap.PinType type) => isTypeVisible.ContainsKey(type);

		public bool IsPinTypeVisible(Minimap.PinType pinType) {
			var canFilterPinType = CanFilterPinType(pinType);
			return (canFilterPinType && isTypeVisible[pinType]) || !canFilterPinType;
		}

		public void Update() {
			if (Input.GetKeyDown(toggleKey)) {
				allPinsHidden = !allPinsHidden;
			}
		}

		private void Initialize() {
			allPinsHidden = true;
			isTypeVisible = new Dictionary<Minimap.PinType, bool>();

			Minimap minimap = Minimap.instance;
			SetUpFilter(minimap.m_selectedIcon0, Minimap.PinType.Icon0);
			SetUpFilter(minimap.m_selectedIcon1, Minimap.PinType.Icon1);
			SetUpFilter(minimap.m_selectedIcon2, Minimap.PinType.Icon2);
			SetUpFilter(minimap.m_selectedIcon3, Minimap.PinType.Icon3);
			SetUpFilter(minimap.m_selectedIcon4, Minimap.PinType.Icon4);
		}

		private void SetUpFilter(Image sourceImage, Minimap.PinType pinType) {
			Minimap minimap = Minimap.instance;

			var buttonGameObject = new GameObject($"{pinType.ToString()} filter button");
			buttonGameObject.transform.SetParent(sourceImage.transform.parent);
			var button = buttonGameObject.AddComponent<Button>();

			var innerElement = new GameObject($"{pinType.ToString()} filter inner element");
			var image = innerElement.AddComponent<Image>();
			image.color = visibleColor;
			innerElement.transform.SetParent(buttonGameObject.transform);
			innerElement.transform.localScale = Vector3.one * 0.25f;

			buttonGameObject.transform.position = sourceImage.rectTransform.position - new Vector3(60, 0);

			button.onClick.AddListener(() => {
				isTypeVisible[pinType] = !isTypeVisible[pinType];
				image.color = isTypeVisible[pinType] ? visibleColor : hiddenColor;
			});

			isTypeVisible[pinType] = true;

		}
	}
}
