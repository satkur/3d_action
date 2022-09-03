using UnityEngine;
using UnityEngine.UI;

public class RHandEquipView : MonoBehaviour {
    Text iconText;

    public void Init(string dispName) {
        iconText = GetComponentInChildren<Text>();
        SetEquip(dispName);
    }

    public void SetEquip(string dispName) {
        iconText.text = dispName;
    }
}
