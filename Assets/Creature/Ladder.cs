using UnityEngine;

public class Ladder : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerInteraction>(out var interaction))
        {
            interaction.EnterLadder();
            OneTimeTip.FindByTipId("ladder")?.Show();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerInteraction>(out var interaction))
            interaction.ExitLadder();
    }
}
