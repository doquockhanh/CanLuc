using UnityEngine;
using System.Collections;

namespace Gameplay.Focus
{
	/// <summary>
	/// GameObject có khả năng thực hiện hành động trực tiếp:
	/// - Click để focus
	/// - Hold Space để xoay child object theo ping-pong
	/// - Nhấn A/D để di chuyển trái phải
	/// - Hiển thị focusable info
	/// </summary>
	[RequireComponent(typeof(Rigidbody2D))]
	public class DirectActionObject : MonoBehaviour, IFocusable
	{
		[Header("Movement Settings")]
		[SerializeField] private float moveSpeed = 5f;

		[Header("Rotation Settings")]
		[SerializeField] private Transform childToRotate;
		[SerializeField] private float rotationSpeed = 60f; // degrees per second
		[SerializeField] private float maxAngle = 60f; // maximum angle range

		[Header("Visual Settings")]
		[SerializeField] private Color focusColor = Color.yellow;
		[SerializeField] private Color normalColor = Color.white;

		[Header("Input Settings")]
		[SerializeField] private KeyCode rotateKey = KeyCode.Space;
		[SerializeField] private KeyCode leftKey = KeyCode.A;
		[SerializeField] private KeyCode rightKey = KeyCode.D;
		[SerializeField] private FocusManager focusManager;

		private Rigidbody2D rb;
		private Renderer cachedRenderer;
		private bool isFocused = false;
		private float currentAngle = 0f;
		private int direction = 1; // 1 = rotate up, -1 = rotate down


		void Awake()
		{
			rb = GetComponent<Rigidbody2D>();
			cachedRenderer = GetComponentInChildren<Renderer>();
		}

		void Update()
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

		#region IFocusable Implementation

		public void OnFocused(GameObject previous)
		{
			isFocused = true;

			// Thay đổi màu sắc khi được focus
			if (cachedRenderer != null)
			{
				cachedRenderer.material.color = focusColor;
			}

			// Khóa di chuyển camera khi focus vào DirectActionObject
			if (focusManager != null)
			{
				focusManager.LockCameraMovement();
			}
		}

		public void OnDefocused(GameObject next)
		{
			isFocused = false;

			// Trở về màu sắc bình thường
			if (cachedRenderer != null)
			{
				cachedRenderer.material.color = normalColor;
			}

			// Mở khóa di chuyển camera khi unfocus
			if (focusManager != null)
			{
				focusManager.UnlockCameraMovement();
			}
		}

		#endregion
	}
}
