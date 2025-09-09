using UnityEngine;
using System;


public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int highScore = 0;
    [SerializeField] private bool enableScoreLogging = true;

    // Events
    public Action<int> OnScoreChanged;
    public Action<int> OnHighScoreChanged;
    public Action<int> OnScoreAdded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Load high score từ PlayerPrefs
        LoadHighScore();
    }

    #region Score Management

    /// <summary>
    /// Thêm điểm vào màn chơi
    /// </summary>
    public void AddScore(int points)
    {
        if (points <= 0) return;

        int oldScore = currentScore;
        currentScore += points;

        if (enableScoreLogging)
        {
            Debug.Log($"[ScoreManager] Added {points} points. Total: {currentScore}");
        }

        // Gọi events
        OnScoreAdded?.Invoke(points);
        OnScoreChanged?.Invoke(currentScore);

        // Kiểm tra high score
        CheckHighScore();
    }

    /// <summary>
    /// Thiết lập điểm số
    /// </summary>
    public void SetScore(int newScore)
    {
        if (newScore < 0) newScore = 0;

        int oldScore = currentScore;
        currentScore = newScore;

        if (enableScoreLogging)
        {
            Debug.Log($"[ScoreManager] Score set to {currentScore}");
        }

        // Gọi events
        OnScoreChanged?.Invoke(currentScore);

        // Kiểm tra high score
        CheckHighScore();
    }

    /// <summary>
    /// Reset điểm số về 0
    /// </summary>
    public void ResetScore()
    {
        int oldScore = currentScore;
        currentScore = 0;

        if (enableScoreLogging)
        {
            Debug.Log($"[ScoreManager] Score reset to 0");
        }

        // Gọi events
        OnScoreChanged?.Invoke(currentScore);
    }

    /// <summary>
    /// Kiểm tra và cập nhật high score
    /// </summary>
    private void CheckHighScore()
    {
        if (currentScore > highScore)
        {
            int oldHighScore = highScore;
            highScore = currentScore;

            if (enableScoreLogging)
            {
                Debug.Log($"[ScoreManager] New High Score! {oldHighScore} -> {highScore}");
            }

            // Gọi event
            OnHighScoreChanged?.Invoke(highScore);

            // Lưu high score
            SaveHighScore();
        }
    }

    #endregion

    #region High Score Persistence

    /// <summary>
    /// Lưu high score vào PlayerPrefs
    /// </summary>
    private void SaveHighScore()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load high score từ PlayerPrefs
    /// </summary>
    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (enableScoreLogging)
        {
            Debug.Log($"[ScoreManager] Loaded High Score: {highScore}");
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Lấy điểm số hiện tại
    /// </summary>
    public int GetCurrentScore()
    {
        return currentScore;
    }

    /// <summary>
    /// Lấy high score
    /// </summary>
    public int GetHighScore()
    {
        return highScore;
    }

    /// <summary>
    /// Lấy điểm số dưới dạng string với format
    /// </summary>
    public string GetScoreString()
    {
        return currentScore.ToString("D6"); // 6 chữ số, ví dụ: 000123
    }

    /// <summary>
    /// Lấy high score dưới dạng string với format
    /// </summary>
    public string GetHighScoreString()
    {
        return highScore.ToString("D6"); // 6 chữ số, ví dụ: 000456
    }

    /// <summary>
    /// Kiểm tra xem có phải high score mới không
    /// </summary>
    public bool IsNewHighScore()
    {
        return currentScore >= highScore;
    }

    /// <summary>
    /// Lấy điểm còn lại để đạt high score
    /// </summary>
    public int GetPointsToHighScore()
    {
        return Mathf.Max(0, highScore - currentScore);
    }

    #endregion

    #region Debug & Testing

    [ContextMenu("Add 100 Points")]
    private void DebugAdd100Points()
    {
        AddScore(100);
    }

    [ContextMenu("Add 1000 Points")]
    private void DebugAdd1000Points()
    {
        AddScore(1000);
    }

    [ContextMenu("Reset Score")]
    private void DebugResetScore()
    {
        ResetScore();
    }

    [ContextMenu("Clear High Score")]
    private void DebugClearHighScore()
    {
        highScore = 0;
        SaveHighScore();
        Debug.Log("[ScoreManager] High Score cleared");
    }

    #endregion
}
