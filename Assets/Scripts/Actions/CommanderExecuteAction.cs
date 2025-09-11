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
				StartCameraCycleForAccumulatedObjects();
			}
		}
	}

	private void StartCameraCycleForAccumulatedObjects()
	{
		var cameraController = CameraController.Instance != null ? CameraController.Instance : FindFirstObjectByType<CameraController>();
		if (cameraController == null) return;

		var focusables = FindObjectsByType<FocusableBase>(FindObjectsSortMode.None);
		var objectsToCycle = new System.Collections.Generic.List<GameObject>();
		for (int i = 0; i < focusables.Length; i++)
		{
			var acc = focusables[i].GetComponent<ForceAccumulator>();
			if (acc != null && acc.CurrentForce > 0f)
			{
				objectsToCycle.Add(focusables[i].gameObject);
			}
		}

		if (objectsToCycle.Count > 0)
		{
			cameraController.StartCameraCycle(objectsToCycle);
		}
	}
}




