using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý danh sách các nhiệm vụ trong scene.
/// Thu thập tự động tất cả component kế thừa MissionBase trong scene.
/// </summary>
public class MissionManager : MonoBehaviour
{
	public static MissionManager Instance { get; private set; }

	[SerializeField] private List<MissionBase> sceneMissions = new List<MissionBase>();

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else if (Instance != this)
		{
			Destroy(gameObject);
			return;
		}
	}

	private void Start()
	{
		CollectAllMissionsInScene();
	}

	/// <summary>
	/// Thu thập toàn bộ MissionBase đang hiện hữu trong scene hiện tại.
	/// </summary>
	public void CollectAllMissionsInScene()
	{
		sceneMissions.Clear();
		var missions = FindObjectsByType<MissionBase>(FindObjectsSortMode.None);
		if (missions != null)
		{
			sceneMissions.AddRange(missions);
		}
	}

	/// <summary>
	/// Trả về true nếu tất cả nhiệm vụ đều hoàn thành (hoặc không có nhiệm vụ nào).
	/// </summary>
	public bool AreAllMissionsDone()
	{
		for (int i = 0; i < sceneMissions.Count; i++)
		{
			var mission = sceneMissions[i];
			if (mission == null) continue;
			if (!mission.IsDone())
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Lấy danh sách tất cả nhiệm vụ trong scene
	/// </summary>
	public List<MissionBase> GetAllMissions()
	{
		return new List<MissionBase>(sceneMissions);
	}

	/// <summary>
	/// Lấy số lượng nhiệm vụ đã hoàn thành
	/// </summary>
	public int GetCompletedMissionsCount()
	{
		int completed = 0;
		for (int i = 0; i < sceneMissions.Count; i++)
		{
			var mission = sceneMissions[i];
			if (mission != null && mission.IsDone())
			{
				completed++;
			}
		}
		return completed;
	}

	/// <summary>
	/// Lấy tổng số nhiệm vụ
	/// </summary>
	public int GetTotalMissionsCount()
	{
		return sceneMissions.Count;
	}
}


