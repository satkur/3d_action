using UnityEngine;

public class HumanIK : MonoBehaviour {
    [SerializeField] Transform IKGoalRightHand;
    [SerializeField] Transform IKGoalLeftHand;

    public bool IsActive { get; private set; } = false;

    Animator anim;

    public void Init(Animator anim) {
        this.anim = anim;
    }

    void OnAnimatorIK() {
        if (IsActive) {
            // âEéË
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            anim.SetIKPosition(AvatarIKGoal.RightHand, IKGoalRightHand.position);
            anim.SetIKRotation(AvatarIKGoal.RightHand, IKGoalRightHand.rotation);

            // ç∂éË
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
            anim.SetIKPosition(AvatarIKGoal.LeftHand, IKGoalLeftHand.position);
            anim.SetIKRotation(AvatarIKGoal.LeftHand, IKGoalLeftHand.rotation);
        }
    }
}
