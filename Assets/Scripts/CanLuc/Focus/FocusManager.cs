using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Gameplay.Focus
{
	/// <summary>
	/// Central controller that handles click-to-focus and Space-to-accumulate.
	/// Attach to a singleton GameObject in the scene.
	/// </summary>
	public class FocusManager : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] private Camera worldCamera;
		[SerializeField] private MonoBehaviour focusInfoPanelRef;

		[Header("Input")]
		[SerializeField] private KeyCode accumulateKey = KeyCode.Space;

		[Header("State (Read-Only)")]
		[SerializeField] private GameObject currentFocused;

		private ForceAccumulator focusedAccumulator;
		private IFocusable[] focusedListeners;

		[Header("Registry")]
		[SerializeField] private List<GameObject> registeredObjects = new List<GameObject>();

		void Awake()
		{
			if (worldCamera == null)
			{
				worldCamera = Camera.main;
			}
		}

		void Update()
		{
			HandleFocusSelection();
			HandleAccumulation();
		}

		private void HandleFocusSelection()
		{
			if (Input.GetMouseButtonDown(0))
			{
				// Ignore if clicking on UI
				if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

				Vector3 mouseWorld = worldCamera.ScreenToWorldPoint(Input.mousePosition);
				Vector2 point = new Vector2(mouseWorld.x, mouseWorld.y);
				Collider2D col = Physics2D.OverlapPoint(point);
				if (col != null)
				{
					SetFocus(col.gameObject);
				}
				else
				{
					// Click vào vùng trống để unfocus
					Unfocus();
				}
			}

			// Right click để unfocus
			if (Input.GetMouseButtonDown(1))
			{
				// Ignore if clicking on UI
				if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

				Unfocus();
			}
		}

		private void HandleAccumulation()
		{
			// If focused object is a registry trigger, Space executes all registered
			if (currentFocused != null && Input.GetKeyDown(accumulateKey))
			{
				// Nếu là điểm kích hoạt registry (marker) hoặc chỉ huy, thực thi tất cả đăng ký
				if (currentFocused.GetComponent<ExecuteRegistryOnAccumulate>() != null ||
					currentFocused.GetComponent("CommanderExecuteAction") != null)
				{
					ExecuteAllRegistered();
					return;
				}
			}

			if (focusedAccumulator == null) return;

			// On first press, register the focused object
			if (Input.GetKeyDown(accumulateKey))
			{
				Register(currentFocused);
			}

			// While held, continue accumulating for focused object
			if (Input.GetKey(accumulateKey))
			{
				focusedAccumulator.Accumulate(Time.deltaTime);
			}
		}

		public void SetFocus(GameObject target)
		{
			if (target == currentFocused) return;

			GameObject previous = currentFocused;
			IFocusable[] previousListeners = focusedListeners;

			currentFocused = target;
			focusedAccumulator = currentFocused != null ? currentFocused.GetComponent<ForceAccumulator>() : null;
			focusedListeners = currentFocused != null ? currentFocused.GetComponentsInChildren<IFocusable>(true) : null;

			// Notify previous
			if (previous != null && previousListeners != null)
			{
				for (int i = 0; i < previousListeners.Length; i++)
				{
					previousListeners[i].OnDefocused(currentFocused);
				}
			}

			// Notify new
			if (currentFocused != null && focusedListeners != null)
			{
				for (int i = 0; i < focusedListeners.Length; i++)
				{
					focusedListeners[i].OnFocused(previous);
				}
			}

			// Update UI panel
			UpdateFocusInfoPanel();
		}

		public void Unfocus()
		{
			if (currentFocused == null) return;

			GameObject previous = currentFocused;
			IFocusable[] previousListeners = focusedListeners;

			// Reset focus state
			currentFocused = null;
			focusedAccumulator = null;
			focusedListeners = null;

			// Notify previous object that it's no longer focused
			if (previous != null && previousListeners != null)
			{
				for (int i = 0; i < previousListeners.Length; i++)
				{
					previousListeners[i].OnDefocused(null);
				}
			}

			// Update UI panel
			UpdateFocusInfoPanel();
		}

		public GameObject GetCurrentFocus() => currentFocused;
		public float GetCurrentForce() => focusedAccumulator != null ? focusedAccumulator.CurrentForce : 0f;
		public float ConsumeCurrentForce() => focusedAccumulator != null ? focusedAccumulator.Consume() : 0f;

		/// <summary>
		/// Public method để unfocus từ bên ngoài (UI button, etc.)
		/// </summary>
		public void UnfocusCurrent() => Unfocus();

		public void Register(GameObject obj)
		{
			if (obj == null) return;
			if (!registeredObjects.Contains(obj))
			{
				registeredObjects.Add(obj);
			}
			CleanupRegistry();
		}

		public void Unregister(GameObject obj)
		{
			if (obj == null) return;
			registeredObjects.Remove(obj);
			CleanupRegistry();
		}

		public void ClearRegistry()
		{
			registeredObjects.Clear();
		}

		public int RegisteredCount => registeredObjects.Count;

		public void ExecuteAllRegistered()
		{
			CleanupRegistry();
			for (int i = 0; i < registeredObjects.Count; i++)
			{
				GameObject obj = registeredObjects[i];
				if (obj == null) continue;
				var acc = obj.GetComponent<ForceAccumulator>();
				if (acc == null) continue;
				float force = acc.Consume();
				if (force <= 0f) continue;
				var actions = obj.GetComponentsInChildren<IForceAction>(true);
				for (int a = 0; a < actions.Length; a++)
				{
					actions[a].Execute(force);
				}
			}
			// Optional: clear after execution
			ClearRegistry();
		}

		private void CleanupRegistry()
		{
			for (int i = registeredObjects.Count - 1; i >= 0; i--)
			{
				if (registeredObjects[i] == null)
				{
					registeredObjects.RemoveAt(i);
				}
			}
		}

		private void UpdateFocusInfoPanel()
		{
			if (focusInfoPanelRef == null) return;

			if (currentFocused != null)
			{
				var focusableInfo = currentFocused.GetComponent("FocusableInfo");
				if (focusableInfo != null)
				{
					// Gọi method ShowPanel qua reflection - vị trí sẽ được cập nhật tự động theo chuột
					var showPanelMethod = focusInfoPanelRef.GetType().GetMethod("ShowPanel");
					if (showPanelMethod != null)
					{
						showPanelMethod.Invoke(focusInfoPanelRef, new object[] { focusableInfo, Vector3.zero });
					}
				}
				else
				{
					// Gọi method HidePanel qua reflection
					var hidePanelMethod = focusInfoPanelRef.GetType().GetMethod("HidePanel");
					if (hidePanelMethod != null)
					{
						hidePanelMethod.Invoke(focusInfoPanelRef, null);
					}
				}
			}
			else
			{
				// Gọi method HidePanel qua reflection
				var hidePanelMethod = focusInfoPanelRef.GetType().GetMethod("HidePanel");
				if (hidePanelMethod != null)
				{
					hidePanelMethod.Invoke(focusInfoPanelRef, null);
				}
			}
		}
	}
}


