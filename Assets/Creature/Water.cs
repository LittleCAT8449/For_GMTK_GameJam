using UnityEngine;

public class Water : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerSwim>(out var swim))
            swim.EnterWater();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerSwim>(out var swim))
            swim.ExitWater();
    }
}
