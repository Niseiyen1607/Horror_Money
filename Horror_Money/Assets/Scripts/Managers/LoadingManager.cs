using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    [Header("References")]
    public Animator loadingAnimator;
    public GameObject loadingCanvas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        HideLoadingScreen();
        GameObject playerSpawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (playerSpawnPoint != null && player != null)
        {
            player.transform.position = playerSpawnPoint.transform.position;
            player.transform.rotation = playerSpawnPoint.transform.rotation;
        }
        else
        {
            Debug.LogWarning("Player or PlayerSpawnPoint not found in the scene.");
        }
    }

    public void ShowLoadingScreen()
    {
        loadingCanvas.SetActive(true);
        loadingAnimator.SetTrigger("StartLoading");
    }

    public void HideLoadingScreen()
    {
        loadingAnimator.SetTrigger("EndLoading");
        StartCoroutine(DisableCanvasAfterDelay(1f)); 
    }

    private IEnumerator DisableCanvasAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        loadingCanvas.SetActive(false);
    }
}
