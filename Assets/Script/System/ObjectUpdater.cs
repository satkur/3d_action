using Framework;
using UnityEngine;

public class ObjectUpdater : MonoBehaviour {
    [SerializeField] Player player;
    [SerializeField] Sandbag sandbag;
    [SerializeField] Skeleton[] skeletons;

    [SerializeField] UIController uiCtrl;
    [SerializeField] StagePropsController stageCtrl;

    [SerializeField] TPCamera playerCamera;

    PlayerUseCase playerUseCase;

    public void Init(GameModel gameModel) {
        // Playerèâä˙âª
        var playerModel = new PlayerModel(500);
        var playerInventoryModel = new PlayerInventoryModel();
        player.Init(playerModel, playerInventoryModel, playerCamera.transform);

        // PlayerUseCaseê∂ê¨
        playerUseCase = new PlayerUseCase(player, playerModel, playerInventoryModel);

        // Init
        sandbag.Init();
        for (int i = 0; i < skeletons.Length; i++) {
            skeletons[i].Init();
        }

        uiCtrl.Init(gameModel, playerUseCase);
        stageCtrl.Init(playerUseCase);

        playerCamera.Init();
    }

    void Update() {
        player.DoUpdate();
        for (int i = 0; i < skeletons.Length; i++) {
            skeletons[i].DoUpdate();
        }

        uiCtrl.DoUpdate();
        playerCamera.DoUpdate();
    }

    void FixedUpdate() {
        player.DoFixedUpdate();
        for (int i = 0; i < skeletons.Length; i++) {
            skeletons[i].DoFixedUpdate();
        }
    }

    void LateUpdate() {
        playerCamera.DoLateUpdate();
    }
}
