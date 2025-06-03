using UnityEngine;

public class PickUpItem : MonoBehaviour, IInteractable
{
    public string itemName = "Cube";

    [Header("Effets")]
    public GameObject pickupParticlesPrefab;  

    public void Interact()
    {
        Debug.Log("Ramassé: " + itemName);

        if (pickupParticlesPrefab != null)
        {
            Instantiate(pickupParticlesPrefab, transform.position, Quaternion.identity);
        }

        SoundManager.Instance.PlayRandomGlobalSFX(SoundManager.Instance.pickUp);

        Destroy(gameObject);
    }
}
