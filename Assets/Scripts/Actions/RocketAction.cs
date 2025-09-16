using UnityEngine;


public class RocketAction : ActionBase, IForceAction
{
	[SerializeField] private float forwardMultiplier = 1.0f;
	[SerializeField] private float upwardMultiplier = 0.2f;
	[SerializeField] private float rotationSpeed = 10f;

	[Header("Destruction Settings")]
	[SerializeField] private float maxLifetime = 10f; // Thời gian tối đa tồn tại (giây)
	[SerializeField] private float lowVelocityThreshold = 2f; // Ngưỡng tốc độ thấp
	[SerializeField] private float lowVelocityDuration = 2f; // Thời gian duy trì tốc độ thấp để phá hủy (giây)

	[Header("Combat Settings")]
	[SerializeField] private int damage = 1; // Damage gây ra cho enemy

	[Header("Audio Settings")]
	[SerializeField] private AudioSource audioSource;
	[SerializeField] private AudioClip fireSound;
	[SerializeField] private AudioClip movingSound;

	private bool applyRotateToVelocity = false;
	private bool isExecuted = false; // Đánh dấu đã thực thi Execute

	private Rigidbody2D rb;

	// Biến theo dõi thời gian và tốc độ
	private float lifetime = 0f;
	private float lowVelocityTimer = 0f;

	protected override void Awake()
	{
		base.Awake();
		rb = GetComponent<Rigidbody2D>();

		// Tự động thêm FocusableInfo nếu chưa có
		if (GetComponent<FocusableInfo>() == null)
		{
			var info = gameObject.AddComponent<FocusableInfo>();
			// Có thể set default values ở đây nếu cần
		}

		audioSource.clip = movingSound;
		audioSource.loop = true;
	}

	public override void OnBattlePhaseStarted()
	{
		base.OnBattlePhaseStarted();
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
		cachedRenderer.material.color = Color.red * 2;
	}

	protected override void Update()
	{
		base.Update();
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
			DestroyRocket();
			return;
		}

		// Kiểm tra tốc độ thấp
		float currentVelocity = rb.linearVelocity.magnitude;
		if (currentVelocity <= lowVelocityThreshold)
		{
			lowVelocityTimer += Time.deltaTime;
			if (lowVelocityTimer >= lowVelocityDuration)
			{
				DestroyRocket();
				return;
			}
		}
		else
		{
			// Reset timer nếu tốc độ không còn thấp
			lowVelocityTimer = 0f;
		}
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		if (other.collider.CompareTag("Enemy"))
		{
			// Gây damage cho enemy
			DealDamageToEnemy(other.gameObject);
			DestroyRocket();
		}
	}

	private void DealDamageToEnemy(GameObject enemy)
	{
		// Thử lấy IDamageable component
		if (enemy.TryGetComponent<IDamageable>(out var damageable))
		{
			damageable.TakeDamage(damage, gameObject);
		}
	}

	private void DestroyRocket()
	{
		ParticleManager.Instance.PlayParticleSystem("basicExplosion", transform.position);
		Destroy(gameObject);
	}
}


