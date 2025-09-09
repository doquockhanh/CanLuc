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

	/// <summary>
	/// Đăng ký một GameObject có thể hover
	/// </summary>
	public void RegisterHoverable(GameObject obj, IHoverable hoverable)
	{
		if (obj != null && hoverable != null)
		{
			hoverableObjects[obj] = hoverable;
		}
	}

	/// <summary>
	/// Hủy đăng ký một GameObject
	/// </summary>
	public void UnregisterHoverable(GameObject obj)
	{
		if (obj != null)
		{
			hoverableObjects.Remove(obj);
		}
	}

	/// <summary>
	/// Xử lý khi mouse enter vào một object
	/// </summary>
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

	/// <summary>
	/// Xử lý khi mouse exit khỏi một object
	/// </summary>
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

	/// <summary>
	/// Ẩn FocusInfoPanel ngay lập tức
	/// </summary>
	public void HideFocusInfoPanel()
	{
		if (focusInfoPanel != null)
		{
			focusInfoPanel.HidePanel();
		}
		currentHoveredObject = null;
	}

	/// <summary>
	/// Lấy object đang được hover
	/// </summary>
	public GameObject GetCurrentHoveredObject() => currentHoveredObject;

	/// <summary>
	/// Kiểm tra xem có object nào đang được hover không
	/// </summary>
	public bool IsAnyObjectHovered() => currentHoveredObject != null;
}

