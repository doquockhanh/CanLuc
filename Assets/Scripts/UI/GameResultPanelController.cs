using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameResultPanelController : MonoBehaviour
{
    [SerializeField] private RectTransform mainPanel;
    [SerializeField] private RectTransform resultText;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Enemy Score Result")]
    [SerializeField] private RectTransform linesParent; // Container để spawn các dòng kill
    [SerializeField] private GameObject enemyLine;
    private int finalScore = 999;
    private Sequence mySequence;

    void Start()
    {
        mainPanel.anchoredPosition = new Vector2(0, 800);
        resultText.localScale = Vector3.zero;
        scoreText.text = "0";
        finalScore = ScoreManager.Instance.CurrentScore;

        mySequence = DOTween.Sequence();
        mySequence
                .Append(mainPanel.DOAnchorPos(Vector2.zero, 0.8f).SetEase(Ease.OutBounce))
                .Append(resultText.DOScale(1f, 0.6f).SetEase(Ease.OutBack))
                .AppendCallback(() =>
                {
                    if (enemyLine == null || linesParent == null)
                    {
                        Debug.LogWarning("[GameResultPanel] Chưa gán killLinePrefab hoặc linesParent.");
                        return;
                    }

                    Dictionary<EnemyType, int> scoredEachEm = ScoreManager.Instance.scoredEachEm;
                    // Tạo sequence con cho từng enemy chạy tuần tự
                    Sequence enemySeq = DOTween.Sequence();

                    // Tạo các dòng theo thứ tự duyệt dictionary, chỉ hiển thị những loại có kill > 0
                    foreach (var kv in ScoreManager.Instance.killedEnemies)
                    {
                        if (kv.Value <= 0) continue;

                        enemySeq.AppendCallback(() =>
                        {
                            GameObject newIcon = Instantiate(enemyLine, linesParent);

                            // Lấy từng element
                            Image img = newIcon.transform.Find("Image").GetComponent<Image>();
                            TextMeshProUGUI countText = newIcon.transform.Find("Count").GetComponent<TextMeshProUGUI>();
                            TextMeshProUGUI scoreText = newIcon.transform.Find("Score").GetComponent<TextMeshProUGUI>();

                            EnemyType type = kv.Key;
                            img.sprite = Resources.Load<Sprite>($"Enemy/Sprites/{type}");
                            countText.text = " x " + kv.Value.ToString();

                            int tempScore = 0;
                            DOTween.To(() => tempScore, x =>
                            {
                                tempScore = x;
                                scoreText.text = tempScore.ToString();
                            },
                            scoredEachEm[type], 1f);
                        });

                    }
                })
                .AppendInterval(1f)
                .AppendCallback(() =>
                {
                    int tempScore = 0;
                    DOTween.To(() => tempScore, x =>
                    {
                        tempScore = x;
                        scoreText.text = tempScore.ToString();
                    },
                    finalScore, 1.5f);
                });
    }
}