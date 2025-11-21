using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private IInteractable interactableInRange = null; //closest interatable
    public GameObject interactionIcon;
    void Start()
    {
        interactionIcon.SetActive(false);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        interactableInRange?.Interact();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IInteractable>(out IInteractable interactable) && interactable.CanInteract())
        {
            interactableInRange = interactable; 
            interactionIcon.SetActive(true);
        } //checks if the item has a interactable script
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IInteractable>(out IInteractable interactable) && interactable == interactableInRange)
        {
            interactableInRange = null; //Moved away from interactable
            interactionIcon.SetActive(false);
        }
    }
}
