using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

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
		[SerializeField] private GameObject followHelpPanel;
		[SerializeField] private TextMeshProUGUI followHelpText;

		[Header("Input")]
		[SerializeField] private KeyCode accumulateKey = KeyCode.Space;
		[SerializeField] private float freeMoveSpeed = 10f;

		[Header("State (Read-Only)")]
		[SerializeField] private GameObject currentFocused;

		private ForceAccumulator focusedAccumulator;
		private IFocusable[] focusedListeners;

		[Header("Registry")]
		[SerializeField] private List<GameObject> registeredObjects = new List<GameObject>();

		// Camera follow state for execution cycle
		private List<GameObject> cameraCycleTargets;
		private int cameraCycleIndex = -1;
		[SerializeField] private bool followActive = false;
		[SerializeField] private bool freeCameraMode = false; // true: WASD control; false: follow target
		
		// Camera movement lock for DirectActionObject
		private bool cameraMovementLocked = false;

		// Events
		public System.Action OnExecuteAllRegistered;

		void Awake()
		{
			if (worldCamera == null)
			{
				worldCamera = Camera.main;
			}
		}

		void Update()
		{
			// Ưu tiên xử lý khi đang ở chế độ follow/cycle
			if (followActive)
			{
				HandleFollowModeInputs();
				UpdateFollowHelpPanel();
				return; // Không xử lý focus/accumulate mặc định khi đang follow
			}

			HandleFocusSelection();
			HandleAccumulation();
			UpdateFollowHelpPanel();
		}

		void LateUpdate()
		{
			// WASD luôn hoạt động khi không follow
			if (!followActive)
			{
				UpdateCameraFreeMove();
				return;
			}
			// Khi đang follow: nếu free mode -> WASD, ngược lại -> follow target
			if (freeCameraMode)
			{
				UpdateCameraFreeMove();
			}
			else
			{
				UpdateCameraFollow();
			}
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

		private void HandleFollowModeInputs()
		{
			// Space: toggle giữa follow target và free camera (WASD)
			if (Input.GetKeyDown(accumulateKey))
			{
				freeCameraMode = !freeCameraMode;
				return;
			}

			// ESC: thoát chế độ follow/cycle hoàn toàn
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				ExitFollowMode();
				return;
			}

			// Left click: chuyển mục tiêu tiếp theo
			if (Input.GetMouseButtonDown(0))
			{
				if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
				AdvanceCameraCycle();
			}
		}

		private void ExitFollowMode()
		{
			followActive = false;
			freeCameraMode = false;
			cameraCycleTargets = null;
			cameraCycleIndex = -1;
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

			// Không còn cập nhật FocusInfoPanel khi focus/unfocus
			// UpdateFocusInfoPanel();
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

			// Không còn cập nhật FocusInfoPanel khi focus/unfocus
			// UpdateFocusInfoPanel();
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
			// Snapshot danh sách để dùng cho camera cycle
			List<GameObject> snapshot = new List<GameObject>(registeredObjects);
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

			// Thông báo cho GameManager biết rằng tất cả action đã được thực thi
			OnExecuteAllRegistered?.Invoke();

			// Khởi động camera cycle theo snapshot
			StartCameraCycle(snapshot);
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
				// Tìm IActionInfo (có thể là FocusableInfo hoặc DirectActionInfo)
				var actionInfo = currentFocused.GetComponent<IActionInfo>();
				if (actionInfo != null)
				{
					// Gọi method ShowPanel qua reflection - vị trí sẽ được cập nhật tự động theo chuột
					var showPanelMethod = focusInfoPanelRef.GetType().GetMethod("ShowPanel");
					if (showPanelMethod != null)
					{
						showPanelMethod.Invoke(focusInfoPanelRef, new object[] { actionInfo, Vector3.zero });
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

		private void HideFocusInfoPanel()
		{
			if (focusInfoPanelRef == null) return;
			var hidePanelMethod = focusInfoPanelRef.GetType().GetMethod("HidePanel");
			if (hidePanelMethod != null)
			{
				hidePanelMethod.Invoke(focusInfoPanelRef, null);
			}
		}

		// ================= Camera Cycle Logic =================
		private void StartCameraCycle(List<GameObject> targets)
		{
			if (targets == null) return;
			// Lọc null
			targets.RemoveAll(t => t == null);
			if (targets.Count == 0)
			{
				followActive = false;
				cameraCycleTargets = null;
				cameraCycleIndex = -1;
				freeCameraMode = false;
				return;
			}

			cameraCycleTargets = targets;
			cameraCycleIndex = 0;
			followActive = true;
			freeCameraMode = false; // bắt đầu ở chế độ follow
			// Ẩn FocusInfoPanel khi bắt đầu follow/cycle (CommanderExecuteAction)
			// Sử dụng HoverManager nếu có, ngược lại dùng method cũ
			if (HoverManager.Instance != null)
			{
				HoverManager.Instance.HideFocusInfoPanel();
			}
			else
			{
				HideFocusInfoPanel();
			}
			JumpCameraToCurrentTarget();
		}

		private void AdvanceCameraCycle()
		{
			if (!followActive || cameraCycleTargets == null || cameraCycleTargets.Count == 0) return;
			cameraCycleIndex = (cameraCycleIndex + 1) % cameraCycleTargets.Count;
			JumpCameraToCurrentTarget();
		}

		private GameObject GetCurrentCameraTarget()
		{
			if (!followActive || cameraCycleTargets == null || cameraCycleTargets.Count == 0) return null;
			if (cameraCycleIndex < 0 || cameraCycleIndex >= cameraCycleTargets.Count) return null;
			// Bỏ qua mục tiêu null và tự động tiến tới mục tiêu hợp lệ tiếp theo
			int safeguard = 0;
			while (safeguard < cameraCycleTargets.Count)
			{
				var t = cameraCycleTargets[cameraCycleIndex];
				if (t != null) return t;
				cameraCycleIndex = (cameraCycleIndex + 1) % cameraCycleTargets.Count;
				safeguard++;
			}
			return null;
		}

		private void JumpCameraToCurrentTarget()
		{
			if (worldCamera == null) return;
			var target = GetCurrentCameraTarget();
			if (target == null)
			{
				followActive = false;
				freeCameraMode = false;
				return;
			}
			Vector3 pos = target.transform.position;
			pos.z = worldCamera.transform.position.z;
			worldCamera.transform.position = pos;
		}

		private void UpdateCameraFollow()
		{
			if (!followActive || worldCamera == null) return;
			var target = GetCurrentCameraTarget();
			if (target == null)
			{
				followActive = false;
				freeCameraMode = false;
				return;
			}
			Vector3 pos = target.transform.position;
			pos.z = worldCamera.transform.position.z;
			worldCamera.transform.position = pos;
		}

		private void UpdateCameraFreeMove()
		{
			if (worldCamera == null || cameraMovementLocked) return;
			Vector3 delta = Vector3.zero;
			if (Input.GetKey(KeyCode.A)) delta.x -= 1f;
			if (Input.GetKey(KeyCode.D)) delta.x += 1f;
			if (Input.GetKey(KeyCode.S)) delta.y -= 1f;
			if (Input.GetKey(KeyCode.W)) delta.y += 1f;
			delta = delta.normalized * freeMoveSpeed * Time.deltaTime;
			Vector3 pos = worldCamera.transform.position + delta;
			pos.z = worldCamera.transform.position.z;
			worldCamera.transform.position = pos;
		}

		private void UpdateFollowHelpPanel()
		{
			if (followHelpPanel == null) return;
			if (followHelpPanel.activeSelf != followActive)
			{
				followHelpPanel.SetActive(followActive);
			}
			if (!followActive) return;
			if (followHelpText != null)
			{
				string status;
				if (freeCameraMode)
				{
					status = "Free Camera (WASD)";
				}
				else
				{
					var target = GetCurrentCameraTarget();
					int total = cameraCycleTargets != null ? cameraCycleTargets.Count : 0;
					int index = cameraCycleIndex >= 0 ? (cameraCycleIndex + 1) : 0;
					string targetName = target != null ? target.name : "<none>";
					status = $"Follow: {targetName} ({index}/{total})";
				}
				followHelpText.text = status;
			}
		}

		#region Camera Movement Lock Methods

		/// <summary>
		/// Locks camera movement (WASD controls)
		/// </summary>
		public void LockCameraMovement()
		{
			cameraMovementLocked = true;
		}

		/// <summary>
		/// Unlocks camera movement (WASD controls)
		/// </summary>
		public void UnlockCameraMovement()
		{
			cameraMovementLocked = false;
		}

		#endregion
	}
}


