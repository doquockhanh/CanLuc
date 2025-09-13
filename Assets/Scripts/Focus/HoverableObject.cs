using UnityEngine;


[RequireComponent(typeof(Collider2D))]
public class HoverableObject : MonoBehaviour, IHoverable
{
	[Header("Hover Settings")]
	[SerializeField] private bool enableHover = true;
	[SerializeField] private bool showFocusInfoPanel = true;

	private bool isHovered = false;

	void Start()
	{
		// Đăng ký với HoverManager
		HoverManager.Instance.RegisterHoverable(gameObject, this);
	}

	void OnDestroy()
	{
		// Hủy đăng ký khi object bị destroy
		if (HoverManager.Instance != null)
		{
			HoverManager.Instance.UnregisterHoverable(gameObject);
		}
	}

	/// <summary>
	/// Unity event - được gọi khi mouse enter vào object
	/// </summary>
	public void OnMouseEnter()
	{
		if (!enableHover || !showFocusInfoPanel) return;

		isHovered = true;
		HoverManager.Instance.OnObjectHoverEnter(gameObject);
	}

	/// <summary>
	/// Unity event - được gọi khi mouse exit khỏi object
	/// </summary>
	public void OnMouseExit()
	{
		if (!enableHover || !showFocusInfoPanel) return;

		isHovered = false;
		HoverManager.Instance.OnObjectHoverExit(gameObject);
	}

	/// <summary>
	/// Public method để bật/tắt hover functionality
	/// </summary>
	public void SetHoverEnabled(bool enabled)
	{
		enableHover = enabled;
		if (!enabled && isHovered)
		{
			HoverManager.Instance.OnObjectHoverExit(gameObject);
			isHovered = false;
		}
	}

	/// <summary>
	/// Public method để bật/tắt hiển thị FocusInfoPanel
	/// </summary>
	public void SetShowFocusInfoPanel(bool show)
	{
		showFocusInfoPanel = show;
		if (!show && isHovered)
		{
			HoverManager.Instance.OnObjectHoverExit(gameObject);
		}
	}

	/// <summary>
	/// Kiểm tra xem object có đang được hover không
	/// </summary>
	public bool IsHovered => isHovered;
}

