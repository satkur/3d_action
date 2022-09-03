using Framework;
using UnityEngine;

public class StagePropsController : MonoBehaviour {
    [SerializeField] DeathZone[] deathZoneArr;
    [SerializeField] Door[] doorArr;

    PlayerUseCase playerUse;

    public void Init(PlayerUseCase playerUse) {
        this.playerUse = playerUse;

        foreach (var deathZone in deathZoneArr) {
            deathZone.OnEntered += playerUse.KillPlayer;
        }

        foreach (var door in doorArr) {
            door.Init();
        }
    }
}
