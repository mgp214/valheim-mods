// using System;
// using System.Collections.Generic;
// using HarmonyLib;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using UnityEngine;
// using UnityEngine.UI;

// namespace Cartographer {
// 	class BaseComponent : MonoBehaviour {
// 		public virtual void Initialize(RectTransform parent) {
// 			gameObject.name = this.GetType().Name;
// 			Plugin.Log.LogDebug($"Created {gameObject.name}");
// 			transform.SetParent(parent, false);
// 		}
// 	}
// }