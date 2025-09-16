using UnityEngine;

/// <summary>
/// Nhiệm vụ: Đạt điểm tối thiểu.
/// Hoàn thành khi ScoreManager.CurrentScore >= requiredScore.
/// </summary>
public class ScoreThresholdMission : MissionBase
{
	[SerializeField] private int requiredScore = 100;

	public int RequiredScore
	{
		get => requiredScore;
		set => requiredScore = Mathf.Max(0, value);
	}

	public override bool IsDone()
	{
		if (ScoreManager.Instance == null)
		{
			Debug.LogWarning("[ScoreThresholdMission] ScoreManager.Instance == null");
			return false;
		}

		return ScoreManager.Instance.CurrentScore >= requiredScore;
	}
}


