using UnityEngine;

public class PickUpItem : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Ramassé");
        Destroy(gameObject);
    }
}
