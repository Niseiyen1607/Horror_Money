using UnityEngine;

public class PickUpItem : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Ramass�");
        Destroy(gameObject);
    }
}
