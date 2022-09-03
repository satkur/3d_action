using UnityEngine;

namespace Framework {
    public class WeaponComp : MonoBehaviour, IHitbox {
        [SerializeField] protected string dispName;
        [SerializeField] protected float damage;
        [SerializeField] int attackMotionType;

        [SerializeField] ParticleSystem trailFX; // 軌跡エフェクト

        public bool ColEnabled { get { return col.enabled; } }
        public int AttackMotionType { get; private set; }
        public string DispName { get { return dispName; } }

        Collider col;
        IHurtbox[] hurtboxArr = new IHurtbox[5];
        bool safety = true;

        public void Init() {
            col = GetComponent<Collider>();
            AttackMotionType = attackMotionType;

            DisableHit();
        }

        public void SetSafety(bool enabled) {
            safety = enabled;
        }

        /// <summary>
        /// 攻撃判定はsafetyがfalseの状態でしか発生しない
        /// </summary>
        public void EnableHit() {
            if (safety) {
                return;
            }

            col.enabled = true;

            // 軌跡エフェクト
            if (trailFX != null) {
                trailFX.Play();
            }
        }

        public void DisableHit() {
            col.enabled = false;

            for (int i = 0; i < hurtboxArr.Length; i++) {
                hurtboxArr[i] = null;
            }

            // 軌跡エフェクト
            if (trailFX != null) {
                trailFX.Stop();
            }
        }

        // 衝突検知
        void OnTriggerEnter(Collider other) {
            // 自分と同じタグだったらスルー
            if (other.CompareTag(gameObject.tag))
                return;

            var hurtbox = other.gameObject.GetComponent<IHurtbox>();

            if (hurtbox == null)
                return;

            Hit(hurtbox);
        }

        // 与ダメージ処理
        void Hit(IHurtbox hurtbox) {
            int empty = 0;
            for (int i = 0; i < hurtboxArr.Length; i++) {
                if (hurtboxArr[i] == hurtbox)
                    return;

                if (hurtboxArr[i] == null) {
                    empty = i;
                }
            }

            hurtboxArr[empty] = hurtbox;

            hurtbox.TakeDamage(damage);
        }
    }
}
