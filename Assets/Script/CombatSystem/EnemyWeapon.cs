using UnityEngine;

namespace Framework {
    public class EnemyWeapon : MonoBehaviour, IHitbox {
        [SerializeField] protected float damage;

        public bool ColEnabled { get { return col.enabled; } }

        Collider col;
        IHurtbox[] hurtboxArr = new IHurtbox[5];

        public void Init() {
            col = GetComponent<Collider>();

            DisableHit();
        }

        public void EnableHit() {
            col.enabled = true;
        }

        public void DisableHit() {
            col.enabled = false;

            for (int i = 0; i < hurtboxArr.Length; i++) {
                hurtboxArr[i] = null;
            }
        }

        void OnTriggerEnter(Collider other) {
            // Enemy‚Ìê‡‚ÍƒXƒ‹[
            if (other.CompareTag(ConstTag.Enemy)) {
                return;
            }

            var hurtbox = other.gameObject.GetComponent<IHurtbox>();

            if (hurtbox == null) {
                return;
            }

            int emptyInd = 0;
            for (int i = 0; i < hurtboxArr.Length; i++) {
                if (hurtboxArr[i] == hurtbox) {
                    return;
                }

                if (hurtboxArr[i] == null) {
                    emptyInd = i;
                }
            }

            hurtboxArr[emptyInd] = hurtbox;

            hurtbox.TakeDamage(damage);
        }
    }
}
