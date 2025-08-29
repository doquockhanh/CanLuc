using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Gameplay.Focus
{
	/// <summary>
	/// UI Panel hiển thị thông tin khi focus vào Focusable object
	/// </summary>
	public class FocusInfoPanel : MonoBehaviour
	{
		[Header("UI References")]
		[SerializeField] private Canvas canvas;
		[SerializeField] private RectTransform panelRect;
		[SerializeField] private TextMeshProUGUI descriptionText;
		
		[Header("Animation")]
		[SerializeField] private float fadeInDuration = 0.2f;
		[SerializeField] private float fadeOutDuration = 0.1f;
		
		[Header("Mouse Follow")]
		[SerializeField] private Vector2 mouseOffset = new Vector2(20, 0); // Offset để panel nằm bên phải chuột
		
		private CanvasGroup canvasGroup;
		private Camera worldCamera;
		private FocusableInfo currentInfo;
		
		void Awake()
		{
			canvasGroup = GetComponent<CanvasGroup>();
			if (canvasGroup == null)
				canvasGroup = gameObject.AddComponent<CanvasGroup>();
				
			worldCamera = Camera.main;
			if (canvas == null)
				canvas = GetComponentInParent<Canvas>();
				
			// Ẩn panel ban đầu
			HidePanel();
		}
		
		void Update()
		{
			// Cập nhật vị trí theo chuột nếu panel đang hiển thị
			if (gameObject.activeSelf && currentInfo != null)
			{
				UpdatePosition(Vector3.zero); // worldPosition không còn cần thiết
			}
		}
		
		public void ShowPanel(FocusableInfo info, Vector3 worldPosition)
		{
			if (info == null) return;
			
			currentInfo = info;
				
			if (descriptionText != null)
				descriptionText.text = info.ActionDescription;
			
			// Hiển thị với animation
			gameObject.SetActive(true);
			StartCoroutine(FadeIn());
		}
		
		public void HidePanel()
		{
			if (!gameObject.activeSelf) return;
			
			StartCoroutine(FadeOut());
		}
		
		public void UpdatePosition(Vector3 worldPosition)
		{
			if (worldCamera == null || canvas == null) return;
			
			// Sử dụng vị trí chuột thay vì world position
			Vector3 mouseScreenPos = Input.mousePosition;
			
			// Chuyển đổi sang canvas position
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				canvas.transform as RectTransform,
				mouseScreenPos,
				canvas.worldCamera,
				out Vector2 localPoint
			);
			
			// Thêm offset từ FocusableInfo
			if (currentInfo != null)
			{
				localPoint += currentInfo.Offset;
			}
			
			// Đặt panel bên phải chuột (gốc trên bên trái của panel = vị trí chuột + offset)
			localPoint += mouseOffset;
			
			// Đơn giản: luôn đặt panel bên phải chuột
			// Nếu cần kiểm tra vượt màn hình, có thể thêm logic sau
			
			panelRect.anchoredPosition = localPoint;
		}
		

		
		private System.Collections.IEnumerator FadeIn()
		{
			canvasGroup.alpha = 0f;
			float elapsed = 0f;
			
			while (elapsed < fadeInDuration)
			{
				elapsed += Time.deltaTime;
				canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
				yield return null;
			}
			
			canvasGroup.alpha = 1f;
		}
		
		private System.Collections.IEnumerator FadeOut()
		{
			float startAlpha = canvasGroup.alpha;
			float elapsed = 0f;
			
			while (elapsed < fadeOutDuration)
			{
				elapsed += Time.deltaTime;
				canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
				yield return null;
			}
			
			canvasGroup.alpha = 0f;
			gameObject.SetActive(false);
		}
	}
}
