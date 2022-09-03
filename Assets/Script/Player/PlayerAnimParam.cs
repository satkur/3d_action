using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAnimParam", menuName = "ScriptableObject/PlayerAnimParam")]
public class PlayerAnimParam : ScriptableObject {
    public int Speed = Animator.StringToHash("Speed");
    public int SpeedMotionMultiplier = Animator.StringToHash("SpeedMotionMultiplier");
    public int Terminate = Animator.StringToHash("Terminate");
    public int Attack = Animator.StringToHash("Attack");
    public int AttackMotionType = Animator.StringToHash("AttackMotionType");
    public int Fall = Animator.StringToHash("Fall");
    public int IsGrounded = Animator.StringToHash("IsGrounded");

    public int DodgeRollState = Animator.StringToHash("DodgeRoll");
    public int StaggerState = Animator.StringToHash("Stagger");
    public int DeathState = Animator.StringToHash("Death");
}
