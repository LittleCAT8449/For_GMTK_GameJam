using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Gatti/ItemData", order = 1)]
public class ItemData : ScriptableObject
{
   public int itemID;
   public string itemName;
   public Sprite itemIcon;
}
