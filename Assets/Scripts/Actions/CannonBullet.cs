using UnityEngine;

public class CannonBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float lifetime = 5f; // Thời gian tồn tại
    [SerializeField] private int damage = 1; // Damage gây ra
    [SerializeField] private LayerMask targetLayers = -1; // Layers có thể bị damage

    private void Start()
    {
        // Tự động destroy sau lifetime
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra layer
        if (targetLayers != (targetLayers | (1 << other.gameObject.layer)))
        {
            return;
        }

        // Gây damage nếu có IDamageable
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage, gameObject);
        }

        // Destroy bullet
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        ParticleManager.Instance.PlayParticleSystem("basicExplosion", transform.position);
    }
}
