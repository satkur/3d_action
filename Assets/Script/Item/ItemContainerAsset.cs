using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemContainerAsset", menuName = "ScriptableObject/ItemContainerAsset")]
public class ItemContainerAsset : ScriptableObject {
    public List<ItemContainer> itemList = new List<ItemContainer>();
}
