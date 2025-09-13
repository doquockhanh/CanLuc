using UnityEngine;

public class FinishObstacle : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // Gây sát thương chí mạng để kích hoạt OnDestroyed
                damageable.TakeDamage(int.MaxValue, gameObject);
            }
            else
            {
                Destroy(other.gameObject);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            var damageable = other.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(int.MaxValue, gameObject);
            }
            else
            {
                Destroy(other.gameObject);
            }
        }
    }
}