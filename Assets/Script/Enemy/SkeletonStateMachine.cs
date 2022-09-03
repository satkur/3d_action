using UnityEngine;
using UnityEngine.Events;

public class SkeletonStateMachine : StateMachineBehaviour {
    public event UnityAction onAttackFade = () => { };
    public event UnityAction onStaggerFade = () => { };

    [SerializeField] string AttackTag = "Attack";
    [SerializeField] string StaggerTag = "Stagger";

    int attackTagHash;
    int StaggerTagHash;

    int prevTagHash;

    public void Init() {
        attackTagHash = Animator.StringToHash(AttackTag);
        StaggerTagHash = Animator.StringToHash(StaggerTag);
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        // state変更の検知
        if (stateInfo.tagHash != prevTagHash) {
            if (prevTagHash == attackTagHash) {
                onAttackFade();
            } else if (prevTagHash == StaggerTagHash) {
                onStaggerFade();
            }
        }

        prevTagHash = stateInfo.tagHash;
    }
}
