using UnityEngine;

namespace Gameplay.Focus
{
	/// <summary>
	/// Example plane behavior: move forward/up using consumed force.
	/// Requires a Rigidbody.
	/// </summary>
	[RequireComponent(typeof(Rigidbody2D))]
	public class RocketAction : MonoBehaviour, IForceAction, IFocusable
	{
		[SerializeField] private float forwardMultiplier = 1.0f;
		[SerializeField] private float upwardMultiplier = 0.2f;
		[SerializeField] private float rotationSpeed = 10f;
		[SerializeField] private Color focusColor = Color.yellow;
		[SerializeField] private Color normalColor = Color.white;

		[Header("Destruction Settings")]
		[SerializeField] private float maxLifetime = 10f; // Thời gian tối đa tồn tại (giây)
		[SerializeField] private float lowVelocityThreshold = 2f; // Ngưỡng tốc độ thấp
		[SerializeField] private float lowVelocityDuration = 2f; // Thời gian duy trì tốc độ thấp để phá hủy (giây)

		[Header("Audio Settings")]
		[SerializeField] private AudioSource audioSource;
		[SerializeField] private AudioClip fireSound;
		[SerializeField] private AudioClip movingSound;

		private bool applyRotateToVelocity = false;
		private bool isExecuted = false; // Đánh dấu đã thực thi Execute

		private Rigidbody2D rb;
		private Renderer cachedRenderer;
		
		// Biến theo dõi thời gian và tốc độ
		private float lifetime = 0f;
		private float lowVelocityTimer = 0f;

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

			audioSource.clip = movingSound;
			audioSource.loop = true;
		}

		public void Execute(float force)
		{
			if (force <= 0f) return;
			audioSource.PlayOneShot(fireSound);
			applyRotateToVelocity = true;
			isExecuted = true; // Đánh dấu đã thực thi
			Vector2 forward = transform.right * (force * forwardMultiplier);
			Vector2 upward = transform.up * (force * upwardMultiplier);
			rb.AddForce(forward + upward, ForceMode2D.Impulse);
			audioSource.Play();
		}

		void Update()
		{
			// Chỉ kiểm tra sau khi đã thực thi Execute
			if (isExecuted)
			{
				CheckDestructionConditions();
			}

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

		private void CheckDestructionConditions()
		{
			// Tăng thời gian tồn tại
			lifetime += Time.deltaTime;

			// Kiểm tra thời gian tồn tại
			if (lifetime >= maxLifetime)
			{
				DestroyRocket("Thời gian tồn tại vượt quá giới hạn");
				return;
			}

			// Kiểm tra tốc độ thấp
			float currentVelocity = rb.linearVelocity.magnitude;
			if (currentVelocity <= lowVelocityThreshold)
			{
				lowVelocityTimer += Time.deltaTime;
				if (lowVelocityTimer >= lowVelocityDuration)
				{
					DestroyRocket("Duy trì tốc độ thấp quá lâu");
					return;
				}
			}
			else
			{
				// Reset timer nếu tốc độ không còn thấp
				lowVelocityTimer = 0f;
			}
		}

		private void DestroyRocket(string reason)
		{
			Debug.Log($"Rocket bị phá hủy: {reason} - Lifetime: {lifetime:F2}s, Low velocity time: {lowVelocityTimer:F2}s");
			Destroy(gameObject);
		}

		public void OnFocused(GameObject previous)
		{
			if (cachedRenderer != null)
			{
				cachedRenderer.material.color = focusColor;
				Material mat = GetComponent<SpriteRenderer>().material;
				if (mat != null)
				{
					mat.SetColor("_OutlineColor", Color.red);
					mat.SetFloat("_OutlineSize", 4f);
				}
			}
		}

		public void OnDefocused(GameObject next)
		{
			if (cachedRenderer != null)
			{
				cachedRenderer.material.color = normalColor;
				Material mat = GetComponent<SpriteRenderer>().material;
				if (mat != null)
				{
					mat.SetColor("_OutlineColor", Color.yellow);
					mat.SetFloat("_OutlineSize", 2f);
				}
			}
		}
	}
}


