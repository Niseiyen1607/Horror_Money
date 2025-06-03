using DG.Tweening.Core.Easing;
using UnityEngine;

public class PickUpItem : MonoBehaviour, IInteractable
{
    public string itemName = "Cube";

    [Header("Valeur")]
    public int minValue = 0;
    public int maxValue = 100;

    private int actualValue;

    [Header("Effets")]
    public GameObject normalPickupParticles;
    public GameObject emptyPickupParticles;

    public void Interact()
    {
        actualValue = Random.Range(minValue, maxValue + 1);
        Debug.Log($"Ramassé: {itemName} | Valeur: {actualValue}");

        GameManager.Instance.AddItemValue(actualValue);

        if (actualValue > 0 && normalPickupParticles != null)
            Instantiate(normalPickupParticles, transform.position, Quaternion.identity);
        else if (actualValue == 0 && emptyPickupParticles != null)
            Instantiate(emptyPickupParticles, transform.position, Quaternion.identity);

        SoundManager.Instance.PlayRandomGlobalSFX(SoundManager.Instance.pickUp);
        UIManager.Instance.ShowItemValuePopup(actualValue);

        Destroy(gameObject);
    }
}
