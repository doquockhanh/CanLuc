using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [SerializeField] protected EnemyType type;

    public EnemyType GetEnemyType() {
        return type;
    }
}


public enum EnemyType
{
    A7,
    B52,
    Balloon,
    F4
}
