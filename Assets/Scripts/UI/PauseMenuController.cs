using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;
    public Button pauseButton;
    public Button resumeButton;
    public Button mainMenuButton;

    [Header("Settings")]
    public KeyCode pauseKey = KeyCode.Escape;
    public float panelAnimationDuration = 0.3f;

    private bool isPaused = false;

    void Start()
    {
        // Setup button events
        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);


        // Ẩn panel ban đầu
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            ShowPanelAnimation();
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            HidePanelAnimation();
        }
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void reloadScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    private void ShowPanelAnimation()
    {
        pausePanel.transform.localScale = Vector3.zero;
        pausePanel.transform.DOScale(Vector3.one, panelAnimationDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }
    
    private void HidePanelAnimation()
    {
        pausePanel.transform.DOScale(Vector3.zero, panelAnimationDuration)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() => pausePanel.SetActive(false));
    }
}
