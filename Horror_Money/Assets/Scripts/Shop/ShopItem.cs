using UnityEngine;

public class ShopItem : MonoBehaviour, IInteractable
{
    public int cost = 100;
    public enum UpgradeType { MorePhotos, MoreBlueLight }
    public UpgradeType upgradeType;

    private bool purchased = false;

    [SerializeField] private GameObject upgradeEffect; 

    public void Interact()
    {
        if (purchased)
        {
            Debug.Log("Amélioration déjà achetée !");
            return;
        }

        if (GameManager.Instance.SpendCoins(cost))
        {
            ApplyUpgrade();

            if (upgradeType != UpgradeType.MorePhotos && upgradeType != UpgradeType.MoreBlueLight)
                purchased = true;

            Instantiate(upgradeEffect, transform.position, Quaternion.identity);
            Debug.Log("Amélioration achetée : " + upgradeType);

            SoundManager.Instance.PlayRandomGlobalSFX(SoundManager.Instance.pickUp);
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
                    handCamera.IncreasePhoto(10); 
                }
                break;
            case UpgradeType.MoreBlueLight:
                HandCamera handCameraBlueLight = FindObjectOfType<HandCamera>();
                if (handCameraBlueLight != null)
                {
                    handCameraBlueLight.IncreaseBlueLight(5); 
                }
                break;
        }
    }
}
