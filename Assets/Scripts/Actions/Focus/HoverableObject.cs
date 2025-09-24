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

	public void OnMouseEnter()
	{
		if (!enableHover || !showFocusInfoPanel) return;

		isHovered = true;
		HoverManager.Instance.OnObjectHoverEnter(gameObject);
	}

	public void OnMouseExit()
	{
		if (!enableHover || !showFocusInfoPanel) return;

		isHovered = false;
		HoverManager.Instance.OnObjectHoverExit(gameObject);
	}

	public void SetHoverEnabled(bool enabled)
	{
		enableHover = enabled;
		if (!enabled && isHovered)
		{
			HoverManager.Instance.OnObjectHoverExit(gameObject);
			isHovered = false;
		}
	}

	public void SetShowFocusInfoPanel(bool show)
	{
		showFocusInfoPanel = show;
		if (!show && isHovered)
		{
			HoverManager.Instance.OnObjectHoverExit(gameObject);
		}
	}

	public bool IsHovered => isHovered;
}

