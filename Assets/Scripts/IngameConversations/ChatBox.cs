using UnityEngine;

/// <summary>
/// Component đơn giản để đánh dấu và quản lý chat box
/// </summary>
public class ChatBox : MonoBehaviour
{
    [Header("Chat Box Settings")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.3f;
    
    private CanvasGroup canvasGroup;
    private bool isFading = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        // Fade in khi bắt đầu
        FadeIn();
    }

    public void FadeIn()
    {
        if (isFading) return;
        
        StartCoroutine(FadeCoroutine(0f, 1f, fadeInDuration));
    }

    public void FadeOut()
    {
        if (isFading) return;
        
        StartCoroutine(FadeCoroutine(1f, 0f, fadeOutDuration, true));
    }

    private System.Collections.IEnumerator FadeCoroutine(float from, float to, float duration, bool destroyAfter = false)
    {
        isFading = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / duration);
            canvasGroup.alpha = alpha;
            yield return null;
        }

        canvasGroup.alpha = to;
        isFading = false;

        if (destroyAfter)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Cập nhật vị trí chat box theo Transform
    /// </summary>
    public void UpdatePosition(Transform targetTransform, Vector3 offset)
    {
        if (targetTransform == null) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        RectTransform rectTransform = GetComponent<RectTransform>();
        
        // Chuyển đổi world position sang screen position
        Vector3 screenPos = Camera.main.WorldToScreenPoint(targetTransform.position + offset);
        
        // Chuyển đổi screen position sang local position của Canvas
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPos,
            canvas.worldCamera,
            out localPos
        );

        rectTransform.localPosition = localPos;
    }
}
