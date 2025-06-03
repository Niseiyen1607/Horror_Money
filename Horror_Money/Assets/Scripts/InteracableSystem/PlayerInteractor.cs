using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public float range = 5f;
    public Transform rayOrigin;
    public KeyCode interactionKey = KeyCode.E;

    private GameObject lastTarget;

    void Update()
    {
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range))
        {
            GameObject target = hit.collider.gameObject;

            if (target.CompareTag("Interactable"))
            {
                if (lastTarget != target)
                {
                    ClearLastTarget();

                    OutlineController outline = target.GetComponent<OutlineController>();
                    if (outline != null)
                    {
                        outline.EnableOutline();
                        lastTarget = target;
                    }
                }

                if (Input.GetKeyDown(interactionKey))
                {
                    IInteractable interactable = target.GetComponent<IInteractable>();
                    interactable?.Interact();
                }

                return;
            }
        }

        ClearLastTarget();
    }

    void ClearLastTarget()
    {
        if (lastTarget != null)
        {
            OutlineController outline = lastTarget.GetComponent<OutlineController>();
            if (outline != null)
                outline.DisableOutline();

            lastTarget = null;
        }
    }
}
