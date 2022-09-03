using Framework;
using UnityEngine;
using UnityEngine.Events;

public class DeathZone : MonoBehaviour {
    public event UnityAction OnEntered;

    void OnTriggerEnter(Collider other) {
        DealWithInvader(other);
    }

    void OnTriggerStay(Collider other) {
        DealWithInvader(other);
    }

    void DealWithInvader(Collider other) {
        if (other.CompareTag(ConstTag.Player)) {
            OnEntered?.Invoke();

            GetComponent<Collider>().enabled = false;
        }
    }
}
