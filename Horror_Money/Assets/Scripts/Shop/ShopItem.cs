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
            Debug.Log("Am�lioration d�j� achet�e !");
            return;
        }

        if (GameManager.Instance.SpendCoins(cost))
        {
            ApplyUpgrade();

            if (upgradeType != UpgradeType.MorePhotos && upgradeType != UpgradeType.MoreBlueLight)
                purchased = true;

            Instantiate(upgradeEffect, transform.position, Quaternion.identity);
            Debug.Log("Am�lioration achet�e : " + upgradeType);

            SoundManager.Instance.PlayRandomGlobalSFX(SoundManager.Instance.pickUp);
        }
        else
        {
            Debug.Log("Pas assez d'argent pour acheter cette am�lioration !");
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
