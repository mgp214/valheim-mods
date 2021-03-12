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
		public KeyCode toggleKey = KeyCode.None;

		private Sprite filterSprite;
		private Sprite checkedSprite;
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
			toggleKey = Plugin.toggleKeyCode;
			allPinsHidden = true;
			isTypeVisible = new Dictionary<Minimap.PinType, bool>();

			Minimap minimap = Minimap.instance;
			var temporaryPinPrefab = Instantiate<GameObject>(minimap.m_pinPrefab);
			checkedSprite = temporaryPinPrefab.transform.Find("Checked").gameObject.GetComponent<Image>().sprite;
			Destroy(temporaryPinPrefab);
			filterSprite = minimap.m_icons.Find(x => x.m_name == Minimap.PinType.Icon3).m_icon;
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
			image.sprite = filterSprite;
			innerElement.transform.SetParent(buttonGameObject.transform);
			innerElement.transform.localScale = Vector3.one * 0.5f;

			var checkElement = new GameObject($"{pinType.ToString()} filter checkmark");
			var checkImage = checkElement.AddComponent<Image>();
			checkImage.sprite = checkedSprite;
			checkElement.transform.SetParent(buttonGameObject.transform);
			checkElement.transform.localScale = Vector3.one * 0.5f;
			checkElement.SetActive(false);

			buttonGameObject.transform.position = sourceImage.rectTransform.position - new Vector3(45, 0);

			button.onClick.AddListener(() => {
				isTypeVisible[pinType] = !isTypeVisible[pinType];
				checkImage.gameObject.SetActive(!isTypeVisible[pinType]);
			});

			isTypeVisible[pinType] = true;
		}
	}
}
