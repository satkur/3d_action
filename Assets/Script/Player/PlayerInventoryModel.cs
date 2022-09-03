using System.Collections.Generic;

namespace Framework {
    public class PlayerInventoryModel {
        public List<ItemModel> ItemList { get; private set; }
        public int ItemCount { get; private set; }

        public ItemModel RHandEquip { get; private set; }

        // UI通知用
        public event System.Action<ItemModel> onAddedItem;
        public event System.Action<string> onChangeRHandEquipName;

        public PlayerInventoryModel() {
            ItemList = new List<ItemModel>(99); // キャパシティを指定する
        }

        public void AddItem(ItemModel item) {
            ItemList.Add(item);
            ItemCount++;
            onAddedItem?.Invoke(item);
        }

        public void SetRHandEquip(ItemModel equip) {
            RHandEquip = equip;
            onChangeRHandEquipName?.Invoke(equip.DispName);
        }
    }
}
