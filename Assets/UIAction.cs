using UnityEngine;
using UnityEngine.SceneManagement;

public class UIAction : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private float transitionDuration = 0.5f; // Thời gian chuyển cảnh
    [SerializeField] private bool useCutscene = true; // Có sử dụng cutscene không
    public GameObject RankingPanel;
    public GameObject SettingsPanel;

    // Update is called once per frame

    public void LoadScene(string nameScene)
    {
        if (useCutscene && CutsceneManager.Instance != null)
        {
            CutsceneManager.Instance.PlayCutscene(nameScene, transitionDuration);
        }
        else
        {
            SceneManager.LoadScene(nameScene);
        }
        Debug.Log("Load Scene " + nameScene);
    }
    public void Ranking()
    {
        RankingPanel.SetActive(true);
    }
    public void CloseRanking()
    {
        RankingPanel.SetActive(false);
    }
    public void Settings()
    {
        SettingsPanel.SetActive(true);
    }
    public void CloseSettings()
    {
        SettingsPanel.SetActive(false);
    }
}
