using UnityEngine;

public class FoodPickup : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Food food = GetComponent<Food>();
            if (food != null && food.itemData != null)
                Inventory.instance?.AddItem(food.itemData);

            OneTimeTip.FindByTipId("food")?.Show();
            AudioManager.Instance?.PlayFoodPickup();

            Destroy(gameObject);
        }
    }
}
