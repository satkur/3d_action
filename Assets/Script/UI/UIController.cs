using Framework;
using UnityEngine;

public class UIController : MonoBehaviour {
    InputReceiver inp;
    GameModel gameModel;
    PlayerUseCase playerUse;
    PlayerModel playerModel;
    PlayerInventoryModel playerInvModel;
    MainMenuController mainMenuCtrl;
    RHandEquipView rEquipView;
    HPBarView hpBarView;
    GameOverView gameOverView;

    public void Init(GameModel gameModel, PlayerUseCase playerUse) {
        inp = InputReceiver.Instance;

        this.gameModel = gameModel;
        this.playerUse = playerUse;

        // スタートメニュー
        mainMenuCtrl = GetComponentInChildren<MainMenuController>();
        mainMenuCtrl.Init(playerUse);

        // PlayerModel取得
        playerModel = playerUse.GetPlayerObject();
        playerInvModel = playerUse.GetPlayerInventoryModel();

        rEquipView = GetComponentInChildren<RHandEquipView>();
        hpBarView = GetComponentInChildren<HPBarView>();
        gameOverView = GetComponentInChildren<GameOverView>();

        // Bind
        playerModel.OnChangeHP += hpBarView.UpdateBar;
        playerModel.OnDead += gameOverView.ShowGameOver;
        playerInvModel.onChangeRHandEquipName += rEquipView.SetEquip;
        gameOverView.OnFinishShow += gameModel.SetGameOver;

        rEquipView.Init(playerInvModel.RHandEquip.DispName);
        hpBarView.Init(playerModel.HP);
        gameOverView.Init();
    }

    public void DoUpdate() {
        if (inp.Start_Button) {
            if (mainMenuCtrl.Active) {
                mainMenuCtrl.CloseMenu();
            } else {
                mainMenuCtrl.OpenMenu();
            }
        }
    }
}
