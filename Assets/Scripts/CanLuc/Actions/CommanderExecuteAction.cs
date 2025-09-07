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
		[Header("Visual Settings")]
		[SerializeField] private Color focusColor = Color.yellow;
		[SerializeField] private Color normalColor = Color.white;
		private Renderer cachedRenderer;

		void Awake()
		{
			cachedRenderer = GetComponentInChildren<Renderer>();
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


		public void OnFocused(GameObject previous)
		{
			if (cachedRenderer != null)
			{
				cachedRenderer.material.color = focusColor;
			}
		}

		public void OnDefocused(GameObject next)
		{
			if (cachedRenderer != null)
			{
				cachedRenderer.material.color = normalColor;
			}
		}
	}
}




