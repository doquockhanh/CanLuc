using UnityEngine;

/// <summary>
/// Base class cho một nhiệm vụ trong scene.
/// Kế thừa MonoBehaviour để có thể gắn lên GameObject.
/// </summary>
public abstract class MissionBase : MonoBehaviour
{
	[SerializeField] private string missionName = "Nhiệm vụ";

	/// <summary>
	/// Tên nhiệm vụ hiển thị trên UI.
	/// </summary>
	public string MissionName => string.IsNullOrWhiteSpace(missionName) ? name : missionName;


	/// <summary>
	/// Trả về true nếu nhiệm vụ đã hoàn thành.
	/// </summary>
	public abstract bool IsDone();
}


