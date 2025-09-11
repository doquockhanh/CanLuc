using UnityEngine;


/// <summary>
/// Action với 2 thanh lực:
/// - Thanh 1: lực phóng về phía trước (impulse)
/// - Thanh 2: quy đổi ra thời gian chờ để kích hoạt một GameObject được gắn vào action
/// Mỗi lần nhấn/nhả Space sẽ chốt một thanh, tối đa 2 thanh.
/// </summary>
public class BombaPlantAction : FocusableBase, IMultiForceAction
{
    [Header("Movement Settings")]
    [SerializeField] private float forwardMultiplier = 1.0f;

    [Header("Activation Settings")]
    [SerializeField] private float secondsPerForceBar2 = 0.02f;
    [SerializeField] private float minActivateDelay = 0.0f;
    [SerializeField] private float maxActivateDelay = 10.0f;
    [SerializeField] private GameObject Bom;

    private Rigidbody2D rb;

    public int ForceBarCount => 2;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
    }

    public void Execute(float[] forces)
    {
        if (forces == null || forces.Length < 2) return;

        float forwardForce = Mathf.Max(0f, forces[0]);
        float delayForce = Mathf.Max(0f, forces[1]);

        if (forwardForce > 0f && rb != null)
        {
            Vector2 impulse = (Vector2)transform.right * (forwardForce * forwardMultiplier);
            rb.AddForce(impulse, ForceMode2D.Impulse);
        }

        float wait = Mathf.Clamp(delayForce * secondsPerForceBar2, minActivateDelay, maxActivateDelay);
        if (Bom != null)
        {
            StartCoroutine(DelayedActivate(wait));
        }
    }

    private System.Collections.IEnumerator DelayedActivate(float seconds)
    {
        if (seconds > 0f)
        {
            yield return new WaitForSeconds(seconds);
        }
        Instantiate(Bom, transform.position, transform.rotation);
    }
}


