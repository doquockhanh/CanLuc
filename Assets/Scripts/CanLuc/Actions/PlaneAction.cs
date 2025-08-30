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
		[SerializeField] private float rotationSpeed = 10f;
		[SerializeField] private Color focusColor = Color.yellow;
		[SerializeField] private Color normalColor = Color.white;
		private bool applyRotateToVelocity = false;

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
			applyRotateToVelocity = true;
			Vector2 forward = transform.right * (force * forwardMultiplier);
			Vector2 upward = transform.up * (force * upwardMultiplier);
			rb.AddForce(forward + upward, ForceMode2D.Impulse);
		}

		void Update()
		{
			if (!applyRotateToVelocity) return;
			if (rb.linearVelocity.sqrMagnitude > 0.01f)
			{
				// Góc mục tiêu dựa theo vector velocity
				float targetAngle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
				Quaternion targetRot = Quaternion.AngleAxis(targetAngle, Vector3.forward);

				// Xoay dần (mượt) về hướng đó
				transform.rotation = Quaternion.RotateTowards(
					transform.rotation,
					targetRot,
					rotationSpeed * Time.deltaTime
				);
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


