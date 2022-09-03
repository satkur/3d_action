using UnityEngine;
using UnityEngine.Events;

public class PlayerStateMachine : StateMachineBehaviour {
    public event UnityAction onAttackMotionFade = () => { };
    public event UnityAction onDodgeMotionFade = () => { };
    public event UnityAction onStaggerMotionFade = () => { };

    [SerializeField] string AttackTag = "Attack";
    [SerializeField] string DodgeTag = "DodgeRoll";
    [SerializeField] string StaggerTag = "Stagger";

    int prevTagHash;

    int attackTagHash;
    int dodgeTagHash;
    int StaggerTagHash;

    public void Init() {
        attackTagHash = Animator.StringToHash(AttackTag);
        dodgeTagHash = Animator.StringToHash(DodgeTag);
        StaggerTagHash = Animator.StringToHash(StaggerTag);
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        // memo:ハッシュ値をキーにDictionaryでメソッド呼び分けしたい

        if (stateInfo.tagHash != prevTagHash) {
            // OnMotionFadeイベント
            if (prevTagHash == attackTagHash) {
                onAttackMotionFade();
            } else if (prevTagHash == dodgeTagHash) {
                onDodgeMotionFade();
            } else if (prevTagHash == StaggerTagHash) {
                onStaggerMotionFade();
            }
        }

        prevTagHash = stateInfo.tagHash;
    }
}
