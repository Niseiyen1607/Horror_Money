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
            if(SceneManager.GetActiveScene().name == "Shop")
            {
                StartCoroutine(LoadGameMainScene(1f)); 
            }
            if(SceneManager.GetActiveScene().name == "MainScene")
            {
                StartCoroutine(LoadGameShopScene(0f)); 
            }
        }
    }

    private IEnumerator LoadGameMainScene(float delay)
    {
        LoadingManager.Instance.ShowLoadingScreen();
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("MainScene");
    }

    private IEnumerator LoadGameShopScene(float delay)
    {
        LoadingManager.Instance.ShowLoadingScreen();
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("Shop");
    }
}
