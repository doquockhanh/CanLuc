using System.Collections;
using TMPro;
using UnityEngine;

public class CutsceneCamera : MonoBehaviour
{
    [Header("Camera Path")]
    public Transform[] points;          // các điểm camera sẽ di chuyển qua
    public float moveSpeed = 2f;        // tốc độ di chuyển

    [Header("UI")]
    public TextMeshProUGUI dialogueText;           // tham chiếu tới UI Text
    public GameObject uiPanel;          // panel hiển thị text
    public float typingSpeed = 0.05f;   // tốc độ gõ chữ

    [Header("Audio")]
    public AudioSource audioSource;     // gắn AudioSource của camera
    public AudioClip[] dialogues;       // voice/nhạc nền từng đoạn

    [TextArea(2, 5)]
    public string[] texts;              // đoạn thoại tương ứng
    public AudioClip typingSound;

    private bool skipCutscene = false;  // skip toàn bộ cutscene

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // lần nhấn Space tiếp theo → skip toàn bộ cutscene
            skipCutscene = true;
        }
    }

    private void Start()
    {
        StartCoroutine(PlayCutscene());
    }

    IEnumerator PlayCutscene()
    {
        uiPanel.SetActive(true);

        for (int i = 0; i < points.Length; i++)
        {
            if (skipCutscene) break;

            // 1. Di chuyển camera
            yield return StartCoroutine(MoveToPoint(points[i]));

            // 2. Hiển thị text theo typing effect
            yield return StartCoroutine(TypeText(texts[i]));

            // 3. Phát audio nếu có
            if (i < dialogues.Length && dialogues[i] != null)
            {
                audioSource.clip = dialogues[i];
                audioSource.Play();

                float t = 0f;
                while (t < audioSource.clip.length + 1f && !skipCutscene)
                {
                    t += Time.deltaTime;
                    yield return null;
                }
            }

            yield return new WaitForSeconds(1f);
        }

        // Nếu skip toàn bộ → nhảy camera tới point cuối
        if (skipCutscene && points.Length > 0)
        {
            transform.position = points[^1].position;
            audioSource.Stop();
            yield return StartCoroutine(TypeText(texts[points.Length - 1], false));
        }

        // Ẩn UI
        yield return new WaitForSeconds(2f);
        uiPanel.SetActive(false);
    }

    IEnumerator MoveToPoint(Transform target)
    {
        while (Vector3.Distance(transform.position, target.position) > 0.05f && !skipCutscene)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator TypeText(string fullText, bool ignoreSkip = true)
    {
        dialogueText.text = "";
        int index = 0;
        audioSource.clip = typingSound;
        audioSource.Stop();
        audioSource.pitch = 2f;
        audioSource.loop = true;
        audioSource.Play();
        while (index < fullText.Length)
        {
            if (skipCutscene && ignoreSkip) yield break;

            // Random số ký tự gõ cùng lúc (1-3)
            int charCount = Random.Range(1, 4);
            charCount = Mathf.Min(charCount, fullText.Length - index);

            dialogueText.text += fullText.Substring(index, charCount);
            index += charCount;

            yield return new WaitForSeconds(typingSpeed);
        }
        audioSource.Stop();
    }
}
