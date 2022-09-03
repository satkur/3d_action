using Framework;
using UnityEngine;

public class ItemExchanger : MonoBehaviour {
    [SerializeField] ItemContainerAsset ItemContainerAsset;

    public static ItemExchanger Instance { get; private set; }

    void Awake() {
        if (Instance != null) {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public ItemModel GetItemModel(int id) {
        var itemCont = ItemContainerAsset.itemList[id];

        if (itemCont == null) {
            return null;
        }

        return new ItemModel(id, itemCont.DispName);
    }

    public ItemContainer GetItem(int id) {
        return ItemContainerAsset.itemList[id];
    }

    public ItemContainer GetItem(ItemModel itemModel) {
        return ItemContainerAsset.itemList[itemModel.ID];
    }
}
