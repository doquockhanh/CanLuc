using UnityEngine;

namespace Gameplay.Focus
{
	public class BombAction : FocusableBase, IForceAction
	{
		[SerializeField] private float dropHeightMeters = 10f;
		[SerializeField] private float explodeAfterSecondsPerForce = 0.02f;
		[SerializeField] private GameObject explosionPrefab;

		private Rigidbody2D rb;
		private bool armed;

		protected override void Awake()
		{
			base.Awake();
			rb = GetComponent<Rigidbody2D>();
			rb.bodyType = RigidbodyType2D.Kinematic;

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
	}
}


