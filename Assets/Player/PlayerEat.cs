using UnityEngine;

public class PlayerEat : MonoBehaviour
{
    [SerializeField] private float holdDuration = 0.5f;
    [SerializeField] private KeyCode eatKey = KeyCode.E;
    [SerializeField] private SatietyUI satietyUI;

    private float holdTimer;

    void Update()
    {
        if (Input.GetKey(eatKey))
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= holdDuration)
            {
                holdTimer = 0f;
                Inventory inv = Inventory.instance;
                if (inv != null)
                {
                    ItemData food = inv.ConsumeFirstItem();
                    if (food != null)
                    {
                        Debug.Log($"吃了: {food.itemName}");
                        satietyUI?.OnEat();
                    }
                }
            }
        }
        else
            holdTimer = 0f;
    }
}
