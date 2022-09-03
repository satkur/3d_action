using UnityEngine;
using UnityEngine.Events;

namespace Framework {
    public class PlayerModel {
        public event UnityAction<int> OnChangeHP;
        public event UnityAction OnDead;

        public event UnityAction OnStateEnterLocomotion;
        public event UnityAction OnStateEnterAttack1;
        public event UnityAction OnStateEnterDodgeroll;
        public event UnityAction OnStateEnterStagger;
        public event UnityAction OnStateEnterDead;

        public event UnityAction OnStateExitAttack1;

        public int HP { get; private set; }
        public int MaxHP { get; private set; }

        public bool IsDead { get; private set; }

        public PlayerModel(int hp) {
            MaxHP = hp;
            HP = hp;
            IsDead = false;
        }

        public void DecreaseHp(float damage) {
            HP -= Mathf.FloorToInt(damage);
            OnChangeHP?.Invoke(HP);

            if (HP <= 0) {
                // Dead
                IsDead = true;
                OnDead?.Invoke();
            }
        }
    }
}
