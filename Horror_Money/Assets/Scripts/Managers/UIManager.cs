using UnityEngine;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject bloodOverlay;
    public GameObject gameOverText;
    public GameObject dustOverlay;

    private CanvasGroup dustGroup;

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
}
