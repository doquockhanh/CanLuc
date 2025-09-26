using UnityEngine;
using UnityEngine.SceneManagement;

public class RedirectToScene : MonoBehaviour
{
    [Header("Instance Loaind Scene")]
    [SerializeField] private string instanceLoadScene = "MainMenu";
    [SerializeField] private bool isLoadInstance = true;

    private void Start()
    {
        if (isLoadInstance) {
            SceneManager.LoadScene(instanceLoadScene, LoadSceneMode.Single);
        }
    }

    public void GoToScene(string sceneName) {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}