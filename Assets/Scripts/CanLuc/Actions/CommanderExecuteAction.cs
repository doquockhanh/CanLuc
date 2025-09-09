using TMPro;
using UnityEngine;

public class CommanderExecuteAction : FocusableBase
{
	[Header("Audio Settings")]
	[SerializeField] private AudioSource audioSource;
	[SerializeField] private AudioClip shootSound;
	[SerializeField] private TextMeshProUGUI text;

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
}




