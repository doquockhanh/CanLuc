using UnityEngine;
using System.Collections.Generic;

public class HoverManager : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private FocusInfoPanel focusInfoPanel;

	public static HoverManager Instance { get; private set; }
	private Dictionary<GameObject, IHoverable> hoverableObjects = new Dictionary<GameObject, IHoverable>();
	private GameObject currentHoveredObject;

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else if (Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		// Tìm FocusInfoPanel nếu chưa được assign
		if (focusInfoPanel == null)
		{
			focusInfoPanel = FindFirstObjectByType<FocusInfoPanel>();
		}
	}

	public void RegisterHoverable(GameObject obj, IHoverable hoverable)
	{
		if (obj != null && hoverable != null)
		{
			hoverableObjects[obj] = hoverable;
		}
	}

	public void UnregisterHoverable(GameObject obj)
	{
		if (obj != null)
		{
			hoverableObjects.Remove(obj);
		}
	}

	public void OnObjectHoverEnter(GameObject obj)
	{
		if (obj == null) return;

		// Ẩn panel cũ nếu có
		if (currentHoveredObject != null && currentHoveredObject != obj)
		{
			OnObjectHoverExit(currentHoveredObject);
		}

		currentHoveredObject = obj;

		// Hiển thị FocusInfoPanel nếu object có IActionInfo
		if (focusInfoPanel != null)
		{
			var actionInfo = obj.GetComponent<IActionInfo>();
			if (actionInfo != null)
			{
				focusInfoPanel.ShowPanel(actionInfo, obj.transform.position);
			}
		}
	}

	public void OnObjectHoverExit(GameObject obj)
	{
		if (obj == null) return;

		// Chỉ ẩn panel nếu đây là object đang được hover
		if (currentHoveredObject == obj)
		{
			currentHoveredObject = null;

			// Ẩn FocusInfoPanel
			if (focusInfoPanel != null)
			{
				focusInfoPanel.HidePanel();
			}
		}
	}

	public void HideFocusInfoPanel()
	{
		if (focusInfoPanel != null)
		{
			focusInfoPanel.HidePanel();
		}
		currentHoveredObject = null;
	}

	public GameObject GetCurrentHoveredObject() => currentHoveredObject;

	public bool IsAnyObjectHovered() => currentHoveredObject != null;
}

