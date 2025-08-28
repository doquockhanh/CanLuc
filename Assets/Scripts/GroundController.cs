using UnityEngine;
using System.Collections.Generic;

public class GroundController : MonoBehaviour
{
    [Header("Ground Settings")]
    [SerializeField] private GameObject groundPrefab;
    [SerializeField] private float groundSpeed = 5f;
    [SerializeField] private float groundWidth = 10f;
    [SerializeField] private int initialGroundCount = 3;
    [SerializeField] private float destroyOffset = 2f; // Distance off-screen before destroying
    
    [Header("Spawn Settings")]
    [SerializeField] private Transform groundParent;
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;
    
    private List<GameObject> activeGroundPieces = new List<GameObject>();
    private float screenLeftEdge;
    private Camera mainCamera;
    
    void Start()
    {
        InitializeGroundSystem();
    }
    
    void Update()
    {
        MoveGround();
        CheckAndSpawnGround();
        CleanupOffScreenGround();
    }
    
    private void InitializeGroundSystem()
    {
        // Get main camera for screen edge calculations
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found! Ground system may not work correctly.");
            return;
        }
        
        // Calculate screen left edge in world coordinates
        screenLeftEdge = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
        
        // Create ground parent if not assigned
        if (groundParent == null)
        {
            GameObject parent = new GameObject("GroundParent");
            groundParent = parent.transform;
        }
        
        // Load ground prefab if not assigned
        if (groundPrefab == null)
        {
            groundPrefab = Resources.Load<GameObject>("Prefabs/Square");
            if (groundPrefab == null)
            {
                Debug.LogError("Ground prefab not found! Please assign a ground prefab or ensure Square.prefab exists in Resources/Prefabs/");
                return;
            }
        }
        
        // Spawn initial ground pieces
        SpawnInitialGround();
    }
    
    private void SpawnInitialGround()
    {
        for (int i = 0; i < initialGroundCount; i++)
        {
            Vector3 spawnPos = spawnPosition + Vector3.right * (groundWidth * i);
            SpawnGroundPiece(spawnPos);
        }
    }
    
    private void SpawnGroundPiece(Vector3 position)
    {
        GameObject groundPiece = Instantiate(groundPrefab, position, Quaternion.identity, groundParent);
        activeGroundPieces.Add(groundPiece);
    }
    
    private void MoveGround()
    {
        foreach (GameObject groundPiece in activeGroundPieces)
        {
            if (groundPiece != null)
            {
                groundPiece.transform.Translate(Vector3.left * groundSpeed * Time.deltaTime);
            }
        }
    }
    
    private void CheckAndSpawnGround()
    {
        if (activeGroundPieces.Count == 0) return;
        
        // Find the rightmost ground piece
        GameObject rightmostGround = null;
        float rightmostX = float.MinValue;
        
        foreach (GameObject groundPiece in activeGroundPieces)
        {
            if (groundPiece != null && groundPiece.transform.position.x > rightmostX)
            {
                rightmostX = groundPiece.transform.position.x;
                rightmostGround = groundPiece;
            }
        }
        
        // If the rightmost piece is within spawn distance, spawn a new piece
        if (rightmostGround != null)
        {
            float spawnThreshold = rightmostGround.transform.position.x + groundWidth * 0.5f;
            float cameraRightEdge = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
            
            if (spawnThreshold < cameraRightEdge + groundWidth)
            {
                Vector3 newSpawnPos = rightmostGround.transform.position + Vector3.right * groundWidth;
                SpawnGroundPiece(newSpawnPos);
            }
        }
    }
    
    private void CleanupOffScreenGround()
    {
        float destroyX = screenLeftEdge - destroyOffset;
        
        for (int i = activeGroundPieces.Count - 1; i >= 0; i--)
        {
            if (activeGroundPieces[i] != null)
            {
                if (activeGroundPieces[i].transform.position.x < destroyX)
                {
                    Destroy(activeGroundPieces[i]);
                    activeGroundPieces.RemoveAt(i);
                }
            }
            else
            {
                // Remove null references
                activeGroundPieces.RemoveAt(i);
            }
        }
    }
    
    // Public methods for external control
    public void SetGroundSpeed(float speed)
    {
        groundSpeed = speed;
    }
    
    public float GetGroundSpeed()
    {
        return groundSpeed;
    }
    
    public void PauseGround()
    {
        groundSpeed = 0f;
    }
    
    public void ResumeGround()
    {
        groundSpeed = 5f; // Default speed
    }
    
    // Debug method to visualize ground pieces
    void OnDrawGizmosSelected()
    {
        if (mainCamera != null)
        {
            Gizmos.color = Color.red;
            Vector3 leftEdge = new Vector3(screenLeftEdge - destroyOffset, 0, 0);
            Gizmos.DrawLine(leftEdge, leftEdge + Vector3.up * 10f);
            
            Gizmos.color = Color.green;
            foreach (GameObject groundPiece in activeGroundPieces)
            {
                if (groundPiece != null)
                {
                    Gizmos.DrawWireCube(groundPiece.transform.position, Vector3.one * groundWidth);
                }
            }
        }
    }
}