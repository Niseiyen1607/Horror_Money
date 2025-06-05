using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopToGame : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(LoadGameMainScene(1f));
        }
    }

    private IEnumerator LoadGameMainScene(float delay)
    {
        LoadingManager.Instance.ShowLoadingScreen();
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("MainScene");
    }
}
