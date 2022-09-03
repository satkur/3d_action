using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HPBarView : MonoBehaviour {
    [SerializeField] Image barImg;
    [SerializeField] Image backImg;
    [SerializeField] Image frameImg;
    [SerializeField] float barSmooth = 0.003f;

    int maxHP;
    float invMaxHP;
    float barAmount;
    float targetBarAmount;
    IEnumerator co;

    public void Init(int maxHP) {
        Init(maxHP, maxHP);
    }

    public void Init(int maxHP, int currentHP) {
        this.maxHP = maxHP;
        invMaxHP = 1f / maxHP;

        if (maxHP == currentHP) {
            barAmount = 1f;
        } else {
            targetBarAmount = currentHP * invMaxHP;
            barAmount = targetBarAmount;
            barImg.fillAmount = barAmount;
        }
    }

    public void UpdateBar(int hp) {
        if (co != null) {
            StopCoroutine(co);
        }
        co = UpdateBarCoroutine(hp);
        StartCoroutine(co);
    }

    IEnumerator UpdateBarCoroutine(int hp) {
        targetBarAmount = hp * invMaxHP;

        while (Mathf.Abs(targetBarAmount - barAmount) > barSmooth) {
            barAmount += Mathf.Sign(targetBarAmount - barAmount) * barSmooth;
            barImg.fillAmount = barAmount;
            yield return null;
        }
        barAmount = targetBarAmount;
        barImg.fillAmount = barAmount;
    }
}
