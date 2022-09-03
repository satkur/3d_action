using UnityEngine;

namespace Framework {
    public class PlayerInventory {
        public PlayerInventoryModel InventoryModel { get; private set; }

        public WeaponComp RHandWeapon { get; private set; }

        Transform rightHand;

        public PlayerInventory(PlayerInventoryModel invModel, Transform rightHand) {
            InventoryModel = invModel;
            this.rightHand = rightHand;
        }

        public void Init() {
            var defaultWeapon = ItemExchanger.Instance.GetItemModel(0);
            EquipRHandWeapon(defaultWeapon);
        }

        public void AddItems(params int[] itemsId) {
            foreach (int id in itemsId) {
                var item = ItemExchanger.Instance.GetItemModel(id);
                InventoryModel.AddItem(item);
            }
        }

        public void EquipRHandWeapon(ItemModel item) {
            EquipWeapon(item, rightHand);

            InventoryModel.SetRHandEquip(item);
        }

        public void EquipWeapon(ItemModel item, Transform location) {
            var weaponContainer = ItemExchanger.Instance.GetItem(item);

            // 現在の装備を削除
            foreach (Transform child in location) {
                Object.Destroy(child.gameObject);
            }

            // prefabをGripの子としてインスタンス化
            var weaponObj = Object.Instantiate(weaponContainer.Prefab, location);

            // Weaponコンポーネントを初期化
            RHandWeapon = weaponObj.GetComponent<WeaponComp>();
            RHandWeapon.Init();
        }
    }
}
