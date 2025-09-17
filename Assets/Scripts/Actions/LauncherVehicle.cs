using UnityEngine;

public class LaunchVehicle : ActionBase
{
	[Header("Movement Settings")]
	[SerializeField] private float moveSpeed = 5f;

	[Header("Rotation Settings")]
	[SerializeField] private Transform childToRotate;
	[SerializeField] private float rotationSpeed = 60f; // degrees per second
	[SerializeField] private float maxAngle = 60f; // maximum angle range

	[Header("Audio Settings")]
	[SerializeField] private AudioSource audioSource;
	[SerializeField] private AudioClip engineSound;
	[SerializeField] private AudioSource audioSource2;
	[SerializeField] private AudioClip launchSound;

	[Header("Input Settings")]
	[SerializeField] private KeyCode rotateKey = KeyCode.Space;
	[SerializeField] private KeyCode leftKey = KeyCode.A;
	[SerializeField] private KeyCode rightKey = KeyCode.D;

	private Rigidbody2D rb;
	private float currentAngle = 0f;
	private int direction = -1; // 1 = rotate up, -1 = rotate down

	protected override void Awake()
	{
		base.Awake();
		rb = GetComponent<Rigidbody2D>();
		audioSource.clip = engineSound;
		audioSource.loop = true;
		audioSource.Play();
		audioSource2.clip = launchSound;
		audioSource2.loop = true;
	}

	protected override void Update()
	{
		if (!isFocused) return;

		HandleRotationInput();
		HandleMovementInput();
	}

	private void HandleRotationInput()
	{
		// Xử lý xoay khi giữ Space
		if (Input.GetKey(rotateKey))
		{
			HandleRotation();
		}

		if (Input.GetKeyDown(rotateKey))
		{
			audioSource2.Play();
		}

		if (Input.GetKeyUp(rotateKey))
		{
			audioSource2.Stop();
		}
	}

	private void HandleMovementInput()
	{
		Vector2 moveDirection = Vector2.zero;

		if (Input.GetKey(leftKey))
		{
			moveDirection.x = -1f;
		}
		else if (Input.GetKey(rightKey))
		{
			moveDirection.x = 1f;
		}

		if (moveDirection != Vector2.zero)
		{
			// Cập nhật target position
			transform.position += moveSpeed * Time.deltaTime * (Vector3)moveDirection;
			rb.linearVelocity = Vector2.zero;
		}
	}

	private void HandleRotation()
	{
		if (childToRotate == null) return;

		// Increase angle based on direction
		currentAngle += direction * rotationSpeed * Time.deltaTime;

		// Change direction when hitting limits
		if (currentAngle > maxAngle)
		{
			currentAngle = maxAngle;
			direction = -1;
		}
		else if (currentAngle < -maxAngle)
		{
			currentAngle = -maxAngle;
			direction = 1;
		}

		// Apply rotation to child object
		childToRotate.localEulerAngles = new Vector3(0, 0, currentAngle);
	}

	public override void OnFocused(GameObject previous)
	{
		base.OnFocused(previous);
		isFocused = true;
		CameraController.Instance.LockCameraMovement();
	}

	public override void OnDefocused(GameObject next)
	{
		base.OnDefocused(next);
		isFocused = false;
		CameraController.Instance.UnlockCameraMovement();
	}
}

