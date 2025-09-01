using UnityEngine;

namespace Gameplay.Focus
{
	/// <summary>
	/// Example bomb behavior: when executed with a force value representing target height,
	/// detach and enable gravity, then optionally delay detonation based on force.
	/// </summary>
	[RequireComponent(typeof(Rigidbody2D))]
	public class BombAction : MonoBehaviour, IForceAction, IFocusable
	{
		[SerializeField] private float dropHeightMeters = 10f;
		[SerializeField] private float explodeAfterSecondsPerForce = 0.02f;
		[SerializeField] private GameObject explosionPrefab;

		private Rigidbody2D rb;
		private Renderer cachedRenderer;
		private bool armed;

		void Awake()
		{
			rb = GetComponent<Rigidbody2D>();
			rb.bodyType = RigidbodyType2D.Kinematic;
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
			if (armed) return;
			armed = true;
			// Interpret force as desired drop height offset
			dropHeightMeters = Mathf.Max(dropHeightMeters, force);
			StartCoroutine(DropAndExplode(force));
		}

		private System.Collections.IEnumerator DropAndExplode(float force)
		{
			// Drop when above target height
			while (transform.position.y < dropHeightMeters)
			{
				yield return null;
			}
			rb.bodyType = RigidbodyType2D.Dynamic;

			float wait = Mathf.Clamp(force * explodeAfterSecondsPerForce, 0.1f, 5f);
			yield return new WaitForSeconds(wait);

			if (explosionPrefab != null)
			{
				Instantiate(explosionPrefab, transform.position, Quaternion.identity);
			}
			Destroy(gameObject);
		}

		public void OnFocused(GameObject previous)
		{
			Material mat = GetComponent<SpriteRenderer>().material;
			if (mat != null)
			{
				mat.SetColor("_GlowColor", Color.red);
				mat.SetFloat("_GlowSize", 4f);
			}
		}

		public void OnDefocused(GameObject next)
		{
			Material mat = GetComponent<SpriteRenderer>().material;
			if (mat != null)
			{
				mat.SetColor("_GlowColor", Color.yellow);
				mat.SetFloat("_GlowSize", 2f);
			}
		}
	}
}


