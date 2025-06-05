using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameToShop : MonoBehaviour
{
    private bool hasTriggered = false;
    [SerializeField] private GameObject enemy;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(LoadShopScene(1f));
        }
    }

    private IEnumerator LoadShopScene(float delay)
    {
        Destroy(enemy);
        LoadingManager.Instance.ShowLoadingScreen();
        HandCamera handCamera = FindObjectOfType<HandCamera>();
        if (handCamera != null)
        {
            handCamera.ShowEndPhotos();
            yield return new WaitForSeconds(3f + handCamera.capturedPhotos.Count * 1.5f);
        }

        LoadingManager.Instance.ShowLoadingScreen();
        yield return new WaitForSeconds(delay);

        SceneManager.LoadScene("Shop");
    }
}
