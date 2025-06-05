using UnityEngine;

public class ShopItem : MonoBehaviour, IInteractable
{
    public int cost = 100;
    public enum UpgradeType { MorePhotos, MoreBlueLight }
    public UpgradeType upgradeType;

    private bool purchased = false;

    public void Interact()
    {
        if (purchased)
        {
            Debug.Log("Amélioration déjà achetée !");
            return;
        }

        if (GameManager.Instance.totalItemValue >= cost)
        {
            GameManager.Instance.AddItemValue(-cost);

            ApplyUpgrade();

            purchased = true;

            Debug.Log("Amélioration achetée : " + upgradeType);
        }
        else
        {
            Debug.Log("Pas assez d'argent pour acheter cette amélioration !");
        }
    }

    private void ApplyUpgrade()
    {
        switch (upgradeType)
        {
            case UpgradeType.MorePhotos:
                HandCamera handCamera = FindObjectOfType<HandCamera>();
                if (handCamera != null)
                {
                    handCamera.IncreasePhoto(1); 
                }
                break;
            case UpgradeType.MoreBlueLight:
                HandCamera handCameraBlueLight = FindObjectOfType<HandCamera>();
                if (handCameraBlueLight != null)
                {
                    handCameraBlueLight.IncreaseBlueLight(1); 
                }
                break;
        }
    }
}
