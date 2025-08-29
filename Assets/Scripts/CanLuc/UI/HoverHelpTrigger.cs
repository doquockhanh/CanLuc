using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay.Focus
{
	/// <summary>
	/// Gắn lên UI Image (hoặc bất kỳ UI element nào có raycast) để
	/// hiển thị panel hướng dẫn khi hover và ẩn khi rời khỏi.
	/// Panel sẽ hiện tại vị trí cố định (anchoredPosition) nếu được cấu hình.
	/// </summary>
	public class HoverHelpTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[Header("References")]
		[SerializeField] private GameObject helpPanel; // Panel bạn tự setup trong scene
		[SerializeField] private RectTransform panelRect; // RectTransform của panel (nếu muốn đặt vị trí cố định)

		[Header("Fixed Position")]
		[SerializeField] private Vector2 anchoredPosition = Vector2.zero; // Vị trí cố định của panel trong canvas

		private void Awake()
		{
			// Nếu chưa chỉ định panelRect nhưng có helpPanel, cố gắng lấy RectTransform
			if (panelRect == null && helpPanel != null)
			{
				panelRect = helpPanel.GetComponent<RectTransform>();
			}
			// Đảm bảo panel tắt ban đầu để tránh hiển thị ngoài ý muốn
			if (helpPanel != null)
			{
				helpPanel.SetActive(false);
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (helpPanel == null) return;
			// Đặt vị trí cố định nếu có panelRect
			if (panelRect != null)
			{
				panelRect.anchoredPosition = anchoredPosition;
			}
			helpPanel.SetActive(true);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (helpPanel == null) return;
			helpPanel.SetActive(false);
		}
	}
}
