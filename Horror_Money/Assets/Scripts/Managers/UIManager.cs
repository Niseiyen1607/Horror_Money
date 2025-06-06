using UnityEngine;
using DG.Tweening;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Elements")]
    public GameObject bloodOverlay;
    public GameObject gameOverText;
    public GameObject dustOverlay;

    [Header("Goal UI")]
    public TextMeshProUGUI goalText;

    private CanvasGroup dustGroup;

    [Header("Item Popup")]
    public GameObject itemValuePopupPrefab;
    public Transform itemPopupContainer;

    [Header("Photo Counters UI")]
    public TextMeshPro normalPhotoText;
    public TextMeshPro violetPhotoText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            dustGroup = dustOverlay.GetComponent<CanvasGroup>();
            if (dustGroup == null)
                Debug.LogError("CanvasGroup is missing on DustOverlay!");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowDust()
    {
        dustOverlay.SetActive(true);
        dustGroup.alpha = 1f;
    }

    public void UpdateGoalText(int currentValue, int goal)
    {
        if (goalText != null)
            goalText.text = $"${currentValue} / ${goal}";
    }

    public void UpdateTotalCoinsText(int total)
    {
        goalText.text = $"Total : {total}$";
    }

    public void UpdatePhotoCounters(int normalLeft, int violetLeft)
    {
        if (normalPhotoText != null)
            normalPhotoText.text = $"Photos : {normalLeft}";
        if (violetPhotoText != null)
            violetPhotoText.text = $"Filtre bleu : {violetLeft}";
    }

    public void FadeOutDust(float duration = 1f)
    {
        if (dustGroup != null)
        {
            dustGroup.DOFade(0f, duration).OnComplete(() =>
            {
                dustOverlay.SetActive(false);
            });
        }
    }

    public void ShowGameOverScreen()
    {
        bloodOverlay.SetActive(true);
        dustOverlay.SetActive(true);
        dustGroup.alpha = 1f; 
        gameOverText.SetActive(true);
    }
    public void HideGameOverScreen()
    {
        bloodOverlay.SetActive(false);
        dustOverlay.SetActive(false);
        gameOverText.SetActive(false);
    }

    public void ShowItemValuePopup(int value)
    {
        GameObject popup = Instantiate(itemValuePopupPrefab, itemPopupContainer);
        popup.SetActive(true);

        TextMeshProUGUI text = popup.GetComponent<TextMeshProUGUI>();
        text.text = $"{value}";

        RectTransform rect = popup.GetComponent<RectTransform>();
        CanvasGroup group = popup.GetComponent<CanvasGroup>();
        if (group == null) group = popup.AddComponent<CanvasGroup>();
        group.alpha = 1f;

        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;

        Sequence s = DOTween.Sequence();

        float randomRot = Random.Range(-10f, 10f);
        rect.localRotation = Quaternion.Euler(0f, 0f, randomRot);

        s.Append(rect.DOShakeScale(0.5f, strength: 0.6f, vibrato: 15, randomness: 90f))
         .Join(rect.DOLocalMoveY(rect.localPosition.y + 30f, 0.5f).SetEase(Ease.OutCubic)) 
         .AppendInterval(0.2f)
         .Append(group.DOFade(0f, 0.5f))
         .OnComplete(() => Destroy(popup));
    }
}
