using UnityEngine;
using UnityEngine.SceneManagement;

public class RedirectToScene : MonoBehaviour
{
    [SerializeField] private string firstScene = "HomePage";

    private void Start()
    {
        // Đảm bảo managers đã tồn tại
        Debug.Log("Persistent Scene loaded, managers are ready.");

        // Load HomePage additive, để PersistentScene vẫn giữ lại
        SceneManager.LoadScene(firstScene, LoadSceneMode.Single);
    }
}