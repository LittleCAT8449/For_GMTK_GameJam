using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Image[] slots;

    private void Start()
    {
        if (Inventory.instance != null)
        {
            Inventory.instance.OnItemsChanged += Refresh;
            Refresh();
        }
        else
        {
            Debug.LogError("InventoryUI: Inventory.instance 为空！检查 Inventory 是否已挂载并激活");
        }
    }

    private void OnDestroy()
    {
        if (Inventory.instance != null)
            Inventory.instance.OnItemsChanged -= Refresh;
    }

    private void Refresh()
    {
        var items = Inventory.instance.items;
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < items.Count && items[i] != null && items[i].itemIcon != null)
            {
                slots[i].sprite = items[i].itemIcon;
                slots[i].enabled = true;
            }
            else
                slots[i].enabled = false;
        }
    }
}
