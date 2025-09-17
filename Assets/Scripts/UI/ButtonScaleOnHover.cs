using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonScaleOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Vector3 targetScale = new Vector3(1.2f, 1.2f, 1f); // Kích thước khi hover
    public float scaleSpeed = 5f;

    private Vector3 originalScale;
    private Vector3 currentTargetScale;

    private void Awake()
    {
        originalScale = transform.localScale;
        currentTargetScale = originalScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, currentTargetScale, Time.unscaledDeltaTime * scaleSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        currentTargetScale = targetScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        currentTargetScale = originalScale;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        currentTargetScale = originalScale; // Khi click thì thu nhỏ lại
        transform.localScale = originalScale; // Đặt lại ngay lập tức
    }
}
