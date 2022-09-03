using Framework;
using System.Collections;
using UnityEngine;

public class Sandbag : MonoBehaviour, IHurtbox {
    [SerializeField] int HP = 50;

    TextMesh textmesh;
    Renderer render;

    public void Init() {
        textmesh = GetComponentInChildren<TextMesh>();
        render = GetComponent<Renderer>();

        textmesh.text = string.Empty;
        render.material.color = Color.white;
    }

    public void TakeDamage(float damage) {
        HP -= Mathf.FloorToInt(damage);

        if (HP <= 0) {
            Destroy(gameObject);
            return;
        }

        StartCoroutine(ShowDamage(damage));
    }

    IEnumerator ShowDamage(float damage) {
        textmesh.text = damage.ToString();

        render.material.color = Color.red;

        yield return new WaitForSeconds(1f);

        textmesh.text = string.Empty;

        render.material.color = Color.white;
    }
}
