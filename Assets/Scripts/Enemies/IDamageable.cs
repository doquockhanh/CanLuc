using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int damage, GameObject damageSource = null);
    bool IsAlive { get; }
    int CurrentHealth { get; }
    int MaxHealth { get; }
    System.Action<int, GameObject> OnDamageTaken { get; set; }
    System.Action<GameObject> OnDestroyed { get; set; }
}

