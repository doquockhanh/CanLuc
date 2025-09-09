using UnityEngine;
using TMPro;

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
	private IActionInfo currentInfo;
	private Coroutine fadeRoutine;

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

	public void ShowPanel(IActionInfo info, Vector3 worldPosition)
	{
		if (info == null) return;
		currentInfo = info;
		if (descriptionText != null)
			descriptionText.text = info.GetFullDescription();

		// Dừng fade cũ nếu đang chạy
		if (fadeRoutine != null)
		{
			StopCoroutine(fadeRoutine);
			fadeRoutine = null;
		}

		// Kích hoạt và chạy fade in
		gameObject.SetActive(true);
		fadeRoutine = StartCoroutine(FadeTo(1f, fadeInDuration, deactivateOnEnd: false));
	}

	public void HidePanel()
	{
		if (!gameObject.activeSelf)
		{
			return;
		}
		// Dừng fade cũ nếu đang chạy
		if (fadeRoutine != null)
		{
			StopCoroutine(fadeRoutine);
			fadeRoutine = null;
		}
		// Chạy fade out và tắt
		fadeRoutine = StartCoroutine(FadeTo(0f, fadeOutDuration, deactivateOnEnd: true));
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

	private System.Collections.IEnumerator FadeTo(float targetAlpha, float duration, bool deactivateOnEnd)
	{
		float startAlpha = canvasGroup.alpha;
		float elapsed = 0f;
		if (duration <= 0f)
		{
			canvasGroup.alpha = targetAlpha;
			if (deactivateOnEnd && targetAlpha <= 0f)
			{
				gameObject.SetActive(false);
			}
			yield break;
		}
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
			yield return null;
		}
		canvasGroup.alpha = targetAlpha;
		if (deactivateOnEnd && targetAlpha <= 0f)
		{
			gameObject.SetActive(false);
		}
		fadeRoutine = null;
	}
}
