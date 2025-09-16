using UnityEngine;
using System;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int highScore = 0;
    [SerializeField] private bool enableScoreLogging = true;

    [Header("Kill Tracking")]
    [SerializeField] private int killCount = 0;
    [SerializeField] private bool enableKillLogging = true;

    // Events
    public Action<int> OnScoreChanged;
    public Action<int> OnHighScoreChanged;
    public Action<int> OnScoreAdded;
    public Action<int> OnKillCountChanged; // Event khi kill count thay đổi

    public Dictionary<EnemyType, int> killedEnemies = new();
    public Dictionary<EnemyType, int> scoredEachEm = new();
    public int CurrentScore => currentScore;

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

        foreach (EnemyType type in Enum.GetValues(typeof(EnemyType)))
        {
            killedEnemies[type] = 0;
            scoredEachEm[type] = 0;
        }

    }

    private void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (enableScoreLogging)
        {
            Debug.Log($"[ScoreManager] Loaded High Score: {highScore}");
        }
    }

    public void AddKillAndScore(EnemyType enemyType, int points)
    {
        AddKill(enemyType);
        AddScore(points, enemyType);
    }

    public void AddScore(int points, EnemyType enemyType)
    {
        if (points <= 0) return;
        scoredEachEm[enemyType] += points;

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

    private void SaveHighScore()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();
    }

    public void AddKill(EnemyType enemyType)
    {
        killedEnemies[enemyType]++;
        killCount++;

        // Gọi event
        OnKillCountChanged?.Invoke(killCount);

        // Phát kill sound
        if (KillSoundManager.Instance != null)
        {
            KillSoundManager.Instance.PlayKillSound(killCount);
        }
        else
        {
            Debug.LogWarning("[ScoreManager] KillSoundManager không tìm thấy! Không thể phát kill sound.");
        }
    }

    public void ResetKillCount()
    {
        int oldKillCount = killCount;
        killCount = 0;

        if (enableKillLogging)
        {
            Debug.Log($"[ScoreManager] Kill count reset từ {oldKillCount} về 0");
        }

        // Gọi event
        OnKillCountChanged?.Invoke(killCount);
    }
}
