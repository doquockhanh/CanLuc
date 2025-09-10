using UnityEngine;

/// <summary>
/// Follows a target Transform with an offset and applies a vertical bobbing motion.
/// </summary>
public class ArrowIndicatorFollower : MonoBehaviour
{
	[SerializeField] private Transform target;
	[SerializeField] private Vector3 baseOffset = new Vector3(0f, 2f, 0f);
	[SerializeField] private float amplitude = 0.25f;
	[SerializeField] private float speed = 2.0f;

	private Vector3 initialLocalPosition;

	public void Initialize(Transform followTarget, Vector3 offset, float bobAmplitude, float bobSpeed)
	{
		target = followTarget;
		baseOffset = offset;
		amplitude = bobAmplitude;
		speed = bobSpeed;
	}

	private void Start()
	{
		initialLocalPosition = Vector3.zero;
	}

	private void LateUpdate()
	{
		if (target == null)
		{
			Destroy(gameObject);
			return;
		}

		// Bobbing using sine wave
		float bobOffset = Mathf.Sin(Time.time * speed) * amplitude;
		Vector3 worldPos = target.position + baseOffset + new Vector3(0f, bobOffset, 0f);
		transform.position = worldPos;

		// Face camera if available (useful for 3D), otherwise keep as-is
		var cam = Camera.main;
		if (cam != null)
		{
			transform.forward = cam.transform.forward;
		}
	}
}


