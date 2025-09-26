using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonEffectController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Effects Settings")]
    public bool enableHoverEffect = true;
    public bool enableClickEffect = true;
    public bool enableAppearEffect = true;
    public bool enableClickSound = true;
    
    [Header("Hover Settings")]
    public float hoverScale = 1.1f;
    public float hoverDuration = 0.2f;
    public Ease hoverEase = Ease.OutQuad;
    
    [Header("Click Settings")]
    public float clickScale = 0.95f;
    public float clickDuration = 0.1f;
    public Ease clickEase = Ease.OutQuad;
    
    [Header("Appear Settings")]
    public float appearScale = 1.2f;
    public float appearDuration = 0.3f;
    public Ease appearEase = Ease.OutBack;
    
    [Header("Audio")]
    public AudioClip clickSound;
    public AudioClip hoverSound;
    
    private Vector3 originalScale;
    private Button button;
    private AudioSource audioSource;
    private Tween currentTween;
    
    void Start()
    {
        originalScale = transform.localScale;
        button = GetComponent<Button>();
        
        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Appear effect khi start
        if (enableAppearEffect)
        {
            PlayAppearEffect();
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!enableHoverEffect || !button.interactable) return;
        
        PlayHoverEffect();
        PlayHoverSound();
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!enableHoverEffect || !button.interactable) return;
        
        PlayExitEffect();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!enableClickEffect || !button.interactable) return;
        
        PlayClickEffect();
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!enableClickEffect || !button.interactable) return;
        
        PlayClickUpEffect();
        PlayClickSound();
    }
    
    private void PlayAppearEffect()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(originalScale * appearScale, appearDuration)
            .SetEase(appearEase)
            .SetUpdate(true)
            .OnComplete(() => {
                transform.DOScale(originalScale, appearDuration * 0.5f)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true);
            });
    }
    
    private void PlayHoverEffect()
    {
        KillCurrentTween();
        currentTween = transform.DOScale(originalScale * hoverScale, hoverDuration)
            .SetEase(hoverEase)
            .SetUpdate(true);
    }
    
    private void PlayExitEffect()
    {
        KillCurrentTween();
        currentTween = transform.DOScale(originalScale, hoverDuration)
            .SetEase(hoverEase)
            .SetUpdate(true);
    }
    
    private void PlayClickEffect()
    {
        KillCurrentTween();
        currentTween = transform.DOScale(originalScale * clickScale, clickDuration)
            .SetEase(clickEase)
            .SetUpdate(true);
    }
    
    private void PlayClickUpEffect()
    {
        KillCurrentTween();
        currentTween = transform.DOScale(originalScale, clickDuration)
            .SetEase(clickEase)
            .SetUpdate(true);
    }
    
    private void PlayClickSound()
    {
        if (enableClickSound && clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
    
    private void PlayHoverSound()
    {
        if (hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }
    
    private void KillCurrentTween()
    {
        if (currentTween != null)
        {
            currentTween.Kill();
            currentTween = null;
        }
    }
    
    void OnDestroy()
    {
        KillCurrentTween();
    }
}
