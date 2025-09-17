using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectUIManager : MonoBehaviour
{
	[SerializeField] private Transform floorsRoot;
	[SerializeField] private GameObject floorGroupPrefab;
	[SerializeField] private GameObject levelButtonPrefab;

	void OnEnable()
	{
		BuildUI();
	}

	public void BuildUI()
	{
		for (int i = floorsRoot.childCount - 1; i >= 0; i--)
		{
			Destroy(floorsRoot.GetChild(i).gameObject);
		}
		var world = GameProgressManager.Instance.CurrentWorld;
		foreach (var floor in world.floors.OrderBy(f => f.floorId))
		{
			var floorGO = Instantiate(floorGroupPrefab, floorsRoot);
			var levelsRoot = floorGO.transform;
			foreach (var level in floor.levels.OrderBy(l => l.levelId))
			{
				var btnGO = Instantiate(levelButtonPrefab, levelsRoot);
				var btn = btnGO.GetComponent<Button>();
				if (btn != null)
				{
					btn.interactable = level.isUnlocked;
					btn.onClick.RemoveAllListeners();
					int fId = floor.floorId;
					int lId = level.levelId;
					btn.onClick.AddListener(() => GameProgressManager.Instance.LoadLevel(fId, lId));
				}
				var text = btnGO.GetComponentInChildren<UnityEngine.UI.Text>(true);
				if (text != null)
				{
					text.text = $"{floor.floorId}-{level.levelId}";
				}
				var passedIcon = btnGO.transform.Find("Passed");
				if (passedIcon != null)
				{
					passedIcon.gameObject.SetActive(level.isPassed);
				}
				var lockedIcon = btnGO.transform.Find("Locked");
				if (lockedIcon != null)
				{
					lockedIcon.gameObject.SetActive(!level.isUnlocked);
				}
			}
		}
	}
}


