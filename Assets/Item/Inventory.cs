using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    public List<ItemData> items = new List<ItemData>();
    public int maxCapacity = 2;

    public event Action OnItemsChanged;

    void Awake()
    {
        instance = this;
    }

    public void AddItem(ItemData data)
    {
        if (items.Count >= maxCapacity) return;
        items.Add(data);
        OnItemsChanged?.Invoke();
    }

    public ItemData ConsumeFirstItem()
    {
        if (items.Count == 0) return null;
        ItemData item = items[0];
        items.RemoveAt(0);
        OnItemsChanged?.Invoke();
        return item;
    }
}
