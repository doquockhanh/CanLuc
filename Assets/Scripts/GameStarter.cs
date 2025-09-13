using UnityEngine;

public class GameStarter : MonoBehaviour
{
    [SerializeField] private GameObject cutsceneManagerObject;

    private void Start()
    {
        if (cutsceneManagerObject != null && !cutsceneManagerObject.activeInHierarchy)
        {
            cutsceneManagerObject.SetActive(true);
        }
    }
}
