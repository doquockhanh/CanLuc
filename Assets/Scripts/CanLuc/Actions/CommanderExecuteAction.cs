using System.Collections;
using TMPro;
using UnityEngine;

namespace Gameplay.Focus
{
	/// <summary>
	/// Gắn vào GameObject chỉ huy. Khi đối tượng này đang được focus,
	/// nhấn phím tích lũy (accumulateKey) sẽ yêu cầu FocusManager
	/// thực thi toàn bộ các đối tượng đã được đăng ký (registry).
	/// Đây là component đánh dấu (marker), không chứa logic riêng.
	/// </summary>
	public class CommanderExecuteAction : MonoBehaviour, IFocusable
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
			}else {
				text.text = "Đi chuẩn bị đi?";
			}
		}


		public void OnFocused(GameObject previous)
		{
			Material mat = GetComponent<SpriteRenderer>().material;
			if (mat != null)
			{
				mat.SetColor("_OutlineColor", Color.red);
				mat.SetFloat("_OutlineSize", 4f);
			}
		}

		public void OnDefocused(GameObject next)
		{
			Material mat = GetComponent<SpriteRenderer>().material;
			if (mat != null)
			{
				mat.SetColor("_OutlineColor", Color.yellow);
				mat.SetFloat("_OutlineSize", 2f);
			}
		}
	}
}




