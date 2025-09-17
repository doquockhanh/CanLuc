using System.Text;
using TMPro;
using UnityEngine;

public class MissionUIController : MonoBehaviour
{
	[SerializeField] private float refreshInterval = 0.5f;

	[SerializeField] private TextMeshProUGUI missionText;

	[SerializeField] private Color doneColor = new Color(0.2f, 0.8f, 0.2f, 1f);
	[SerializeField] private Color pendingColor = Color.white;

	private float timer;

	private void Update()
	{
		timer += Time.unscaledDeltaTime;
		if (timer < refreshInterval) return;
		timer = 0f;
		RefreshUI();
	}

	private void RefreshUI()
	{
		if (MissionManager.Instance == null)
		{
			SetText("(Không tìm thấy MissionManager)");
			return;
		}

		// Đảm bảo danh sách nhiệm vụ luôn cập nhật khi runtime có thay đổi
		MissionManager.Instance.CollectAllMissionsInScene();

		var builder = new StringBuilder();
		var missionsField = typeof(MissionManager).GetField("sceneMissions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		var list = missionsField?.GetValue(MissionManager.Instance) as System.Collections.Generic.List<MissionBase>;
		if (list == null || list.Count == 0)
		{
			SetText("(Khong co nhiem vu)");
			return;
		}

		for (int i = 0; i < list.Count; i++)
		{
			var mission = list[i];
			if (mission == null) continue;
			bool done = false;
			try { done = mission.IsDone(); }
			catch (System.Exception ex) { Debug.LogError($"[MissionUI] Lỗi IsDone() ở {mission.name}: {ex.Message}"); }

			var lineBuilder = new StringBuilder();
			lineBuilder.Append(i + 1).Append(". ")
				.Append(mission.MissionName)
				.Append(" — ")
				.Append(done ? "Hoan thanh" : "Chua xong");

			var colorHex = ColorUtility.ToHtmlStringRGBA(done ? doneColor : pendingColor);
			builder.Append("<color=#").Append(colorHex).Append(">")
				.Append(lineBuilder.ToString())
				.Append("</color>");

			if (i < list.Count - 1) builder.Append('\n');
		}

		SetText(builder.ToString());
	}

	private void SetText(string value)
	{
		if (missionText != null)
		{
			missionText.text = value;
		}
	}
}


