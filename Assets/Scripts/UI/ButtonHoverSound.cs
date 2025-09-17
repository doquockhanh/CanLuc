using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    private AudioSource audioSource; // Kéo AudioSource vào Inspector
    public AudioClip hoverSound;    // Kéo file âm thanh vào Inspector
    public AudioClip clickSound;
    void Start()
    {
        audioSource = GameObject.Find("SoundEffectManager").GetComponent<AudioSource>();
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (audioSource != null && hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
