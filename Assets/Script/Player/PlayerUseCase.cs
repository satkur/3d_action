namespace Framework {
    public class PlayerUseCase {
        Player player;
        PlayerModel playerObj;
        PlayerInventoryModel playerInventoryModel;

        public PlayerUseCase(Player player, PlayerModel playerObj, PlayerInventoryModel playerInventoryModel) {
            this.player = player;
            this.playerObj = playerObj;
            this.playerInventoryModel = playerInventoryModel;
        }

        // xxxxUIにObjectを参照させてもよいのか(例えばHPなどを操作されてもよいのか)
        public PlayerModel GetPlayerObject() {
            return playerObj;
        }

        public PlayerInventoryModel GetPlayerInventoryModel() {
            return playerInventoryModel;
        }

        public void KillPlayer() {
            player.TakeDamage(playerObj.MaxHP);
        }

        public void EquipRHandWeapon(int id) {
            player.EquipRHandWeapon(ItemExchanger.Instance.GetItemModel(id));
        }
    }
}
