using UnityEngine;
using UnityEngine.UI;

public class InvItemView : MonoBehaviour {
    public bool HasValue { get; private set; } = false;

    public event System.Action<int> onClickEvent;

    int id;
    string dispName;

    Text text;
    Button button;
    Image[] images;

    public void Init() {
        text = GetComponentInChildren<Text>();
        button = GetComponentInChildren<Button>();
        images = GetComponentsInChildren<Image>();

        SetEnabled(false);
    }

    public void SetValue(int id, string itemName) {
        this.id = id;
        dispName = itemName;
        text.text = itemName;
        HasValue = true;

        SetEnabled(true);
    }

    public void OnClicked() {
        if (HasValue) {
            onClickEvent(id);
        }
    }

    public void SetEnabled(bool enabled) {
        text.enabled = enabled;
        button.enabled = enabled;
        foreach (var image in images) {
            image.enabled = enabled;
        }
    }
}
