namespace Framework {
    public enum GameModes {
        MAIN,
        GAMEOVER
    }

    public class GameModel {
        public GameModes Mode { get; private set; }

        public event System.Action OnSetGameOver;

        public GameModel(GameModes mode) {
            Mode = mode;
        }

        public void SetGameOver() {
            Mode = GameModes.GAMEOVER;
            OnSetGameOver?.Invoke();
        }
    }
}
