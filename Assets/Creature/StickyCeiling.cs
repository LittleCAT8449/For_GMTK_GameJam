using UnityEngine;

public class StickyCeiling : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerInteraction>(out var interaction))
            interaction.AttachCeiling();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerInteraction>(out var interaction))
            interaction.DetachCeiling();
    }
}
