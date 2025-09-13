using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Quản lý âm thanh kill cho enemy
/// Hỗ trợ 12 kill sounds tăng dần theo số kill
/// </summary>
public class KillSoundManager : MonoBehaviour
{
    public static KillSoundManager Instance { get; private set; }

    [Header("Kill Sound Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] killSounds = new AudioClip[12]; // 12 kill sounds
    [SerializeField] private bool enableKillSoundLogging = true;

    [Header("Special Victory Settings")]
    [SerializeField] private AudioClip victoryMusic; // Nhạc nền khi tất cả enemy bị tiêu diệt
    [SerializeField] private AudioClip victoryEffect; // Hiệu ứng ăn mừng
    [SerializeField] private float victoryMusicVolume = 0.7f;
    [SerializeField] private float victoryEffectVolume = 1f;

    // Events
    public System.Action<int> OnKillSoundPlayed; // Số kill hiện tại
    public System.Action OnVictorySoundPlayed; // Khi phát âm thanh chiến thắng

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
        // Đảm bảo có AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    /// <summary>
    /// Phát kill sound dựa trên số kill hiện tại
    /// </summary>
    /// <param name="killCount">Số kill hiện tại (1-12+)</param>
    public void PlayKillSound(int killCount)
    {
        if (audioSource == null || killSounds == null || killSounds.Length == 0)
        {
            Debug.LogWarning("[KillSoundManager] AudioSource hoặc killSounds chưa được thiết lập!");
            return;
        }

        // Xác định index của sound cần phát
        int soundIndex = GetKillSoundIndex(killCount);
        
        if (soundIndex < 0 || soundIndex >= killSounds.Length || killSounds[soundIndex] == null)
        {
            Debug.LogWarning($"[KillSoundManager] Kill sound tại index {soundIndex} không hợp lệ!");
            return;
        }

        // Phát sound
        audioSource.PlayOneShot(killSounds[soundIndex]);

        if (enableKillSoundLogging)
        {
            Debug.Log($"[KillSoundManager] Phát kill sound {soundIndex + 1} cho kill count {killCount}");
        }

        // Trigger event
        OnKillSoundPlayed?.Invoke(killCount);
    }

    /// <summary>
    /// Phát âm thanh chiến thắng khi tất cả enemy bị tiêu diệt
    /// </summary>
    public void PlayVictorySounds()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("[KillSoundManager] AudioSource chưa được thiết lập!");
            return;
        }

        // Phát hiệu ứng ăn mừng trước
        if (victoryEffect != null)
        {
            audioSource.PlayOneShot(victoryEffect, victoryEffectVolume);
        }

        // Phát nhạc nền chiến thắng
        if (victoryMusic != null)
        {
            audioSource.clip = victoryMusic;
            audioSource.volume = victoryMusicVolume;
            audioSource.loop = true;
            audioSource.Play();
        }

        if (enableKillSoundLogging)
        {
            Debug.Log("[KillSoundManager] Phát âm thanh chiến thắng!");
        }

        // Trigger event
        OnVictorySoundPlayed?.Invoke();
    }

    /// <summary>
    /// Dừng nhạc nền chiến thắng
    /// </summary>
    public void StopVictoryMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    /// <summary>
    /// Xác định index của kill sound dựa trên số kill
    /// </summary>
    /// <param name="killCount">Số kill hiện tại</param>
    /// <returns>Index của sound (0-11)</returns>
    private int GetKillSoundIndex(int killCount)
    {
        // Nếu kill count > 12, sử dụng sound cuối cùng (index 11)
        if (killCount > 12)
        {
            return 11;
        }

        // Nếu kill count < 1, sử dụng sound đầu tiên (index 0)
        if (killCount < 1)
        {
            return 0;
        }

        // Trả về index (killCount - 1) vì array bắt đầu từ 0
        return killCount - 1;
    }

    /// <summary>
    /// Kiểm tra xem có đang phát nhạc nền chiến thắng không
    /// </summary>
    public bool IsPlayingVictoryMusic()
    {
        return audioSource != null && audioSource.isPlaying && audioSource.clip == victoryMusic;
    }

    #region Debug & Testing

    [ContextMenu("Test Kill Sound 1")]
    private void DebugTestKillSound1()
    {
        PlayKillSound(1);
    }

    [ContextMenu("Test Kill Sound 12")]
    private void DebugTestKillSound12()
    {
        PlayKillSound(12);
    }

    [ContextMenu("Test Kill Sound 15")]
    private void DebugTestKillSound15()
    {
        PlayKillSound(15);
    }

    [ContextMenu("Test Victory Sounds")]
    private void DebugTestVictorySounds()
    {
        PlayVictorySounds();
    }

    [ContextMenu("Stop Victory Music")]
    private void DebugStopVictoryMusic()
    {
        StopVictoryMusic();
    }

    #endregion
}
