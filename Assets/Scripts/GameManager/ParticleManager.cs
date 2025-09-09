using UnityEngine;
using System.Collections.Generic;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance;

    [System.Serializable]
    public class ParticleType
    {
        public string key;            // tên định danh (vd: "small", "big", "fire")
        public GameObject prefab;     // prefab Particle System
        public int poolSize = 5;      // số lượng khởi tạo trước
    }

    [Header("Explosion Configs")]
    public List<ParticleType> particleTypeTypes;

    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();

    void Awake()
    {
        // Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Khởi tạo pool cho từng loại
        foreach (var type in particleTypeTypes)
        {
            Queue<GameObject> queue = new Queue<GameObject>();
            for (int i = 0; i < type.poolSize; i++)
            {
                GameObject obj = Instantiate(type.prefab);
                obj.SetActive(false);
                obj.transform.SetParent(transform); // cho gọn Hierarchy
                queue.Enqueue(obj);
            }
            pools[type.key] = queue;
        }
    }

    /// <summary>
    /// Gọi hiệu ứng nổ theo key tại vị trí pos
    /// </summary>
    public void PlayParticleSystem(string key, Vector3 pos)
    {
        if (!pools.ContainsKey(key))
        {
            Debug.LogWarning($"Không có explosion với key: {key}");
            return;
        }

        GameObject obj = pools[key].Dequeue();
        obj.transform.position = pos;
        obj.SetActive(true);

        ParticleSystem ps = obj.GetComponent<ParticleSystem>();
        ps.Play();

        // Sau khi phát xong, disable và trả về pool
        StartCoroutine(ReturnToPool(obj, ps.main.duration, key));
    }

    private System.Collections.IEnumerator ReturnToPool(GameObject obj, float delay, string key)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
        pools[key].Enqueue(obj);
    }
}
