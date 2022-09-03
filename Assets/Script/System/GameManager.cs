using Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    [SerializeField] ObjectUpdater objUpder;

    GameModel gameModel;

    void Awake() {
        if (SceneManager.GetActiveScene().name == ConstSceneName.Main) {
            InitMainScene();
        }
    }

    public void LoadMainScene() {
        SceneManager.LoadScene(ConstSceneName.Main, LoadSceneMode.Single);
    }

    void InitMainScene() {
        gameModel = new GameModel(GameModes.MAIN);

        objUpder.Init(gameModel);

        gameModel.OnSetGameOver += LoadMainScene;
    }
}
