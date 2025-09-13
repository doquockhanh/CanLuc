using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }

    [Header("Cutscene Settings")]
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Color fadeColor = Color.black;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (fadePanel == null)
        {
            Debug.LogError("CutsceneManager: FadePanel chưa được gán! Vui lòng gán FadePanel trong Inspector.");
            return;
        }

        fadePanel.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
    }

    public void PlayCutscene(string nextSceneName, float duration = 2f)
    {
        if (Instance == null)
        {
            Debug.LogError("CutsceneManager: Instance chưa được khởi tạo! Vui lòng đảm bảo CutsceneManager đã được thêm vào scene.");
            return;
        }

        if (fadePanel == null)
        {
            Debug.LogError("CutsceneManager: FadePanel chưa được gán! Vui lòng gán FadePanel trong Inspector.");
            return;
        }

        StartCoroutine(CutsceneSequence(nextSceneName, duration));
    }

    private IEnumerator CutsceneSequence(string nextSceneName, float duration)
    {
        // Fade in
        yield return StartCoroutine(FadeIn());

        // Load next scene
        SceneManager.LoadScene(nextSceneName);
        
        // Đợi một frame để scene mới load xong
        yield return null;

        // Wait for cutscene duration
        yield return new WaitForSeconds(duration);

        // Fade out
        yield return StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        fadePanel.raycastTarget = true;
        float elapsedTime = 0f;
        Color startColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
        Color targetColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1);

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            fadePanel.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        fadePanel.color = targetColor;
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color startColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1);
        Color targetColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            fadePanel.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        fadePanel.color = targetColor;
        fadePanel.raycastTarget = false;
    }
} 
