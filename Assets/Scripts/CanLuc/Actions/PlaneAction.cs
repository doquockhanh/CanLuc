using UnityEngine;

namespace Gameplay.Focus
{
	/// <summary>
	/// Example plane behavior: move forward/up using consumed force.
	/// Requires a Rigidbody.
	/// </summary>
	[RequireComponent(typeof(Rigidbody2D))]
	public class PlaneAction : MonoBehaviour, IForceAction, IFocusable
	{
		[SerializeField] private float forwardMultiplier = 1.0f;
		[SerializeField] private float upwardMultiplier = 0.2f;
		[SerializeField] private Color focusColor = Color.yellow;
		[SerializeField] private Color normalColor = Color.white;

		private Rigidbody2D rb;
		private Renderer cachedRenderer;

		void Awake()
		{
			rb = GetComponent<Rigidbody2D>();
			cachedRenderer = GetComponentInChildren<Renderer>();
			
			// Tự động thêm FocusableInfo nếu chưa có
			if (GetComponent<FocusableInfo>() == null)
			{
				var info = gameObject.AddComponent<FocusableInfo>();
				// Có thể set default values ở đây nếu cần
			}
		}

		public void Execute(float force)
		{
			if (force <= 0f) return;
			Vector2 forward = transform.right * (force * forwardMultiplier);
			Vector2 upward = transform.up * (force * upwardMultiplier);
			rb.AddForce(forward + upward, ForceMode2D.Impulse);
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


