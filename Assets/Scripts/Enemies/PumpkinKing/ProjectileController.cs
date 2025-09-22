using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private LayerMask groundLayerMask = 1;
    [SerializeField] private float lifetime = 5f; // Thời gian sống tối đa
    
    private Vector3 direction;
    private System.Action<Vector3> onHitCallback;
    private System.Action onDestroyCallback; // Callback khi projectile bị hủy
    private bool isInitialized = false;
    
    private void Update()
    {
        if (!isInitialized) return;
        
        // Di chuyển projectile
        transform.position += direction * speed * Time.deltaTime;
        
        // Tự hủy sau thời gian sống
        lifetime -= Time.deltaTime;
        if (lifetime <= 0)
        {
            DestroyProjectile();
        }
    }
    
    /// <summary>
    /// Khởi tạo projectile với hướng và tốc độ
    /// </summary>
    public void Initialize(Vector3 moveDirection, float projectileSpeed, System.Action<Vector3> hitCallback, System.Action destroyCallback = null)
    {
        direction = moveDirection.normalized;
        speed = projectileSpeed;
        onHitCallback = hitCallback;
        onDestroyCallback = destroyCallback;
        isInitialized = true;
    }
    
    private void OnHitGround(Vector3 hitPosition)
    { 
        // Gọi callback để thông báo va chạm
        onHitCallback?.Invoke(hitPosition);
        
        // Hủy projectile
        DestroyProjectile();
    }
    
    private void DestroyProjectile()
    {
        // Gọi callback khi projectile bị hủy
        onDestroyCallback?.Invoke();
        
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (IsInLayerMask(other.gameObject.layer, groundLayerMask))
        {
            OnHitGround(transform.position);
        }
    }
    
    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }
}
