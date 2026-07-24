using UnityEngine;

public class FoodPickup : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Destroy(gameObject);
        }
    }
}
