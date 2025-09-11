using TMPro;
using UnityEngine;

public class CommanderExecuteAction : FocusableBase
{
	[Header("Audio Settings")]
	[SerializeField] private AudioSource audioSource;
	[SerializeField] private AudioClip shootSound;
	[SerializeField] private TextMeshProUGUI text;

	protected override void Awake()
	{
		base.Awake();
		// Commander không cần tích lực
		canAccumulateForce = false;
	}

	public void Execute(bool success)
	{
		if (success)
		{
			audioSource.PlayOneShot(shootSound);
			text.text = "Bắt đầu!!";
		}
		else
		{
			text.text = "Đi chuẩn bị đi?";
		}
	}

	protected override void Update()
	{
		base.Update();

		// Commander: trong Prepare phase, nhấn Space để chuyển Battle phase
		if (GameManager.Instance != null && GameManager.Instance.IsInPreparePhase())
		{
			if (Input.GetKeyDown(KeyCode.Space) && isFocused)
			{
				GameManager.Instance.StartBattlePhase();
				Execute(true);
				// Camera sẽ tự xử lý theo GamePhase trong CameraController
			}
		}
	}
}




