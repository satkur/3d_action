using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameOverView : MonoBehaviour {
    [SerializeField] AnimationClip gameOverClip;

    public event UnityAction OnFinishShow;

    UIAnimationPlayer animPlayer;
    Image faderImg;
    Text text;

    public void Init() {
        animPlayer = GetComponent<UIAnimationPlayer>();
        animPlayer.Init();
        animPlayer.OnFinishAnimation += OnFinishShow;

        faderImg = GetComponentInChildren<Image>();
        text = GetComponentInChildren<Text>();

        faderImg.enabled = false;
        text.enabled = false;
    }

    public void ShowGameOver() {
        faderImg.enabled = true;
        text.enabled = true;

        animPlayer.Play(gameOverClip);
    }
}
