using Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuController : MonoBehaviour {
    public bool Active { get; private set; }

    System.Action<int> equipRHandWeapon;

    List<InvItemView> itemViewList = new List<InvItemView>(99);
    PlayerUseCase playerUseCase;
    PlayerInventoryModel playerInvModel;
    Canvas canvas;

    public void Init(PlayerUseCase playerUseCase) {
        canvas = GetComponent<Canvas>();

        this.playerUseCase = playerUseCase;
        equipRHandWeapon = playerUseCase.EquipRHandWeapon;
        playerInvModel = playerUseCase.GetPlayerInventoryModel();

        playerInvModel.onAddedItem += AddItemView; // Modelイベント講読

        // xxxxxxxx
        var itemViewArray = GetComponentsInChildren<InvItemView>();
        if (itemViewArray != null || itemViewArray.Length != 0) {
            itemViewList.AddRange(itemViewArray);
        }

        // xxxxxxxxxxxx
        for (int i = 0; i < itemViewList.Count; i++) {
            var itemView = itemViewList[i];
            itemView.Init();
            itemView.onClickEvent += equipRHandWeapon; // Viewイベント講読

            if (i < playerInvModel.ItemCount) {
                var item = playerInvModel.ItemList[i];
                itemView.SetValue(item.ID, item.DispName);
            }
        }

        CloseMenu();
    }

    public void OpenMenu() {
        Active = true;
        canvas.enabled = true;
    }

    public void CloseMenu() {
        // フォーカスの解除
        EventSystem.current.SetSelectedGameObject(null);

        Active = false;
        canvas.enabled = false;
    }

    void AddItemView(ItemModel item) {
        // xxxxxxxx
        if (playerInvModel.ItemCount <= itemViewList.Count) {
            for (int i = 0; i < itemViewList.Count; i++) {
                var itemView = itemViewList[i];

                if (!itemView.HasValue) {
                    itemView.SetValue(item.ID, item.DispName);
                    return;
                }
            }
        }
    }
}
