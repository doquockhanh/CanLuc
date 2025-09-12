using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SlidePanelController : MonoBehaviour
{
    public RectTransform panel;      // Panel UI cần trượt
    public Button toggleButton;      // Nút bấm để mở/đóng
    public float slideTime = 0.5f;   // Thời gian trượt

    private bool isShown = false;
    private Vector2 hiddenPos;
    private Vector2 shownPos;
    private Coroutine moveCoroutine;
    public GameObject background;

    void Start()
    {
        shownPos = panel.anchoredPosition;
        hiddenPos = new Vector2(shownPos.x, shownPos.y + Screen.height);
        toggleButton.onClick.AddListener(TogglePanel);
    }

    public void TogglePanel()
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        if (!isShown)
        {
            panel.gameObject.SetActive(true); // Hiện panel trước khi trượt xuống
            moveCoroutine = StartCoroutine(MovePanel(panel, hiddenPos, shownPos, slideTime, true));
        }
        else
        {
            // panel.gameObject.SetActive(true); // Hiện panel trước khi trượt xuống
            // moveCoroutine = StartCoroutine(MovePanel(panel, hiddenPos, shownPos, slideTime, true));
        }
        //isShown = !isShown;
        background.SetActive(true);
    }

    IEnumerator MovePanel(RectTransform rect, Vector2 from, Vector2 to, float duration, bool setActiveAfter)
    {
        float elapsed = 0f;
        rect.anchoredPosition = from;
        while (elapsed < duration)
        {
            rect.anchoredPosition = Vector2.Lerp(from, to, elapsed / duration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        rect.anchoredPosition = to;

        if (!setActiveAfter)
            rect.gameObject.SetActive(false); // Ẩn panel sau khi trượt lên
    }
    public void CloseButton()
    {
        //background.SetActive(false);
        moveCoroutine = StartCoroutine(MovePanel(panel, shownPos, hiddenPos, slideTime, false));
    }
}