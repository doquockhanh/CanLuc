using UnityEngine;

public class Shield : MonoBehaviour, IGamePhaseAware
{
    public int maxPhaseLive = 3;
    private int phaseCount = 0;

    void Start()
    {
        GameManager.Instance.RegisterGamePhaseAwareComponent(this);
    }
    public void OnPreparePhaseStarted()
    {

    }

    public void OnBattlePhaseStarted()
    {
    }

    public void OnPhaseChanged(GamePhase newPhase)
    {
        phaseCount++;
        if (phaseCount >= maxPhaseLive)
        {
            Destroy(gameObject, Random.Range(5f, 10f));
        }
    }

    void OnDestroy()
    {
        GameManager.Instance.UnregisterGamePhaseAwareComponent(this);
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            Destroy(other.gameObject);
        }
    }
}