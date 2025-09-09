using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class FocusManager : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Camera worldCamera;
	[SerializeField] private MonoBehaviour focusInfoPanelRef;
	[SerializeField] private GameObject followHelpPanel;
	[SerializeField] private TextMeshProUGUI followHelpText;

	[Header("Input")]
	[SerializeField] private KeyCode accumulateKey = KeyCode.Space;

	[Header("State (Read-Only)")]
	[SerializeField] private GameObject currentFocused;

	private ForceAccumulator focusedAccumulator;
	private IFocusable[] focusedListeners;

	[Header("Registry")]
	[SerializeField] private List<GameObject> registeredObjects = new List<GameObject>();

	[Header("Audio")]
	[SerializeField] private AudioSource audioSource;
	[SerializeField] private AudioClip audioClip;

	// Camera controller reference
	private CameraController cameraController;

	// Events
	public System.Action OnExecuteAllRegistered;

	void Awake()
	{
		if (worldCamera == null)
		{
			worldCamera = Camera.main;
		}

		audioSource.clip = audioClip;

		// Subscribe to focus changes to keep internal state (accumulator) in sync
		FocusableBase.OnFocusChanged += HandleGlobalFocusChanged;

		// Tìm hoặc tạo CameraController
		cameraController = FindFirstObjectByType<CameraController>();
		if (cameraController == null)
		{
			// Tạo CameraController nếu chưa có
			GameObject cameraControllerObj = new GameObject("CameraController");
			cameraController = cameraControllerObj.AddComponent<CameraController>();
		}

		// Thiết lập camera và UI cho CameraController
		cameraController.SetCamera(worldCamera);
		cameraController.SetFollowHelpUI(followHelpPanel, followHelpText);
	}

	private void OnDestroy()
	{
		FocusableBase.OnFocusChanged -= HandleGlobalFocusChanged;
	}

	void Update()
	{
		// Ưu tiên xử lý khi đang ở chế độ follow/cycle
		if (cameraController != null && cameraController.IsFollowActive)
		{
			return; // Không xử lý focus/accumulate mặc định khi đang follow
		}

		HandleFocusSelection();
		HandleAccumulation();
	}

	// LateUpdate đã được chuyển sang CameraController

	private void HandleFocusSelection()
	{
		// Focus selection is now handled by FocusableBase.OnMouseDown
		// Only handle unfocus on right click here for convenience
		if (Input.GetMouseButtonDown(1))
		{
			if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
			FocusableBase.ClearFocus();
		}
	}

	// Camera input handling đã được chuyển sang CameraController

	private void HandleAccumulation()
	{
		// If focused object is a registry trigger, Space executes all registered
		if (currentFocused != null && Input.GetKeyDown(accumulateKey))
		{
			// Nếu là điểm kích hoạt registry (marker) hoặc chỉ huy, thực thi tất cả đăng ký
			if (currentFocused.GetComponent<ExecuteRegistryOnAccumulate>() != null ||
				currentFocused.GetComponent("CommanderExecuteAction") != null)
			{
				if (registeredObjects.Count > 0)
				{
					currentFocused.GetComponent<CommanderExecuteAction>().Execute(true);
					StartCoroutine(ExecuteAllRegistered());
					return;
				}
				else
				{
					currentFocused.GetComponent<CommanderExecuteAction>().Execute(false);
					return;
				}

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

		if (Input.GetKeyDown(accumulateKey))
		{
			audioSource.Play();
		}

		if (Input.GetKeyUp(accumulateKey))
		{
			audioSource.Stop();
		}
	}

	public void SetFocus(GameObject target)
	{
		FocusableBase.SetFocus(target);
	}

	public void Unfocus()
	{
		FocusableBase.ClearFocus();
	}

	public GameObject GetCurrentFocus() => FocusableBase.CurrentGameObject;
	public float GetCurrentForce() => focusedAccumulator != null ? focusedAccumulator.CurrentForce : 0f;
	public float ConsumeCurrentForce() => focusedAccumulator != null ? focusedAccumulator.Consume() : 0f;

	private void HandleGlobalFocusChanged(FocusableBase previous, FocusableBase current)
	{
		currentFocused = current != null ? current.gameObject : null;
		focusedAccumulator = currentFocused != null ? currentFocused.GetComponent<ForceAccumulator>() : null;
		focusedListeners = currentFocused != null ? currentFocused.GetComponentsInChildren<IFocusable>(true) : null;
	}

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

	public IEnumerator ExecuteAllRegistered()
	{
		CleanupRegistry();
		// Snapshot danh sách để dùng cho camera cycle
		List<GameObject> snapshot = new List<GameObject>(registeredObjects);
		for (int i = 0; i < registeredObjects.Count; i++)
		{
			yield return new WaitForSeconds(1f);
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

		// Khởi động camera cycle theo snapshot
		if (cameraController != null)
		{
			cameraController.StartCameraCycle(snapshot);
		}
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

	private void HideFocusInfoPanel()
	{
		if (focusInfoPanelRef == null) return;
		var hidePanelMethod = focusInfoPanelRef.GetType().GetMethod("HidePanel");
		if (hidePanelMethod != null)
		{
			hidePanelMethod.Invoke(focusInfoPanelRef, null);
		}
	}
}


