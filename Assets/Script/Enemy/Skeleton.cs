using Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Skeleton : MonoBehaviour, IHurtbox {
    [SerializeField] float baseGravityScale = 1f;
    [SerializeField] float climbStepSmooth = 0.1f;
    [SerializeField] float runningMaxSpeed = 2.5f;
    [SerializeField] float walkSpeed = 0.5f;
    [SerializeField] float attackDistance = 1.4f;
    [SerializeField] EnemyWeapon weapon;

    [SerializeField] int HP = 250;

    [SerializeField] Transform[] patrolPoint;

    Rigidbody rb;
    Animator anim;
    NavMeshAgent navAgent;
    GroundDetector groundDetr;
    PlayerDetector playerDetr;

    State state;
    Vector3 patrolTargetPos;
    Vector3 attackTargetPos;

    Vector3 gravityVector;
    float gravityScale;

    enum State {
        WAIT,
        PATROL,
        CHASE,
        ATTACK,
        STAGGER,
        DEAD
    }

    public void Init() {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();

        groundDetr = GetComponent<GroundDetector>();
        groundDetr.Init();

        playerDetr = GetComponentInChildren<PlayerDetector>();
        playerDetr.Init();

        weapon = GetComponentInChildren<EnemyWeapon>();
        weapon.Init();

        var stateMachine = anim.GetBehaviour<SkeletonStateMachine>();
        stateMachine.Init();
        stateMachine.onAttackFade += OnAttackMotionFade;
        stateMachine.onStaggerFade += OnStaggerMotionFade;

        navAgent.updatePosition = false;
        navAgent.updateRotation = false;

        // �����Ԃֈڍs����
        StartPatrol();

        // �J�X�^���d�͏�����
        rb.useGravity = false;
        gravityVector = Physics.gravity;
        gravityScale = baseGravityScale;
    }

    public void DoUpdate() {
        navAgent.nextPosition = transform.position;

        playerDetr.DoUpdate();

        switch (state) {
            case State.WAIT:
                // �v���C���[(�G)�����m
                if (playerDetr.Detected) {
                    navAgent.isStopped = true;
                    navAgent.ResetPath();
                    state = State.CHASE;
                }
                break;

            case State.PATROL:
                // �v���C���[(�G)�����m
                if (playerDetr.Detected) {
                    navAgent.isStopped = true;
                    navAgent.ResetPath();
                    state = State.CHASE;
                }
                // ���������m
                else if (navAgent.hasPath && navAgent.remainingDistance < 0.4f) {
                    // �����~�܂�����Ɏ��̒n�_�֏�����ĊJ����
                    StartCoroutine(RestartPatrolCoroutine(5));
                } else if (!navAgent.hasPath) {
                    // �o�H��T�m�ł��Ȃ��ꍇ�͑ҋ@��Ԃֈڍs����
                    StartWaiting();
                }
                break;

            case State.CHASE:
                // �G�����m��
                if (playerDetr.Detected) {
                    // �v���C���[(�G)�̍��W���X�V
                    attackTargetPos = playerDetr.TargetObject.transform.position;

                    if (Vector3.Distance(transform.position, attackTargetPos) <= attackDistance) {
                        FireAttack1();
                    }
                }
                // �G�������Ȃ�
                else if ((transform.position - attackTargetPos).sqrMagnitude < 0.1f) {
                    attackTargetPos = Vector3.zero;

                    // NavMeshAgent�̃V�~�����[�V�����ʒu�������I�Ɍ��݈ʒu�ɂ���
                    navAgent.Warp(transform.position);
                    // �����~�܂�����Ɏ��̒n�_�֏�����ĊJ����
                    StartCoroutine(RestartPatrolCoroutine(4));
                }
                break;

            case State.ATTACK:
                if (playerDetr.Detected) {
                    // �v���C���[(�G)�̍��W���X�V
                    attackTargetPos = playerDetr.TargetObject.transform.position;
                }
                break;
        }
    }

    public void DoFixedUpdate() {
        gravityScale = baseGravityScale;

        groundDetr.DoUpdate();

        switch (state) {
            case State.WAIT:
                Waiting();
                break;
            case State.PATROL:
                PatrolLocomotion();
                break;
            case State.CHASE:
                ChaseLocomotion();
                break;
            case State.ATTACK:
                AttackLocomotion();
                break;
            case State.DEAD:
                Waiting();
                break;
        }

        // �J�X�^���d��
        rb.AddForce(gravityVector * gravityScale, ForceMode.Acceleration);
    }

    // ��_���[�W����
    public void TakeDamage(float damage) {
        if (state == State.DEAD) {
            return;
        }

        HP -= Mathf.FloorToInt(damage);

        if (HP <= 0) {
            // Dead
            state = State.DEAD;
            anim.applyRootMotion = true;
            anim.CrossFadeInFixedTime("Death", 0.2f);
            playerDetr.Disable();
            return;
        }

        // ���݃��[�V����
        state = State.STAGGER;
        anim.applyRootMotion = true;
        anim.CrossFadeInFixedTime("Stagger", 0.2f);
    }

    void Waiting() {
        rb.velocity = Vector3.Scale(rb.velocity, Vector3.up);
    }

    void PatrolLocomotion() {
        // �ł��߂��i�s��̍��W���擾
        foreach (var pos in navAgent.path.corners) {
            if (Vector3.Distance(transform.position, pos) >= 0.1f) {
                // �i�s��������N�H�[�^�j�I��
                Quaternion lookRotaion = Quaternion.LookRotation(pos - transform.position, Vector3.up);

                // �i�s�����̓K�p
                rb.MoveRotation(Quaternion.Lerp(transform.rotation, lookRotaion, 0.1f));
                break;
            }
        }

        // �ړ�����
        float fowardSpeed = walkSpeed;

        Moving(fowardSpeed);


        var velXZ = Vector3.Scale(rb.velocity, new Vector3(1, 0, 1));

        // ���[�V��������
        // �A�j�����[�^�[�ϐ��ɒl��n��
        anim.SetFloat("SpeedBlend", fowardSpeed / runningMaxSpeed);
        anim.SetFloat("SpeedMult", velXZ.magnitude / runningMaxSpeed);
    }

    void ChaseLocomotion() {
        // �v���C���[�̈ʒu���擾����
        if (playerDetr.Detected) {
            attackTargetPos = playerDetr.TargetObject.transform.position;
        }

        // �v���C���[�̕����������N�H�[�^�j�I��
        Quaternion lookRotaion = Quaternion.LookRotation(attackTargetPos - transform.position, Vector3.up);

        // �i�s�����̓K�p
        rb.MoveRotation(Quaternion.Lerp(transform.rotation, lookRotaion, 0.1f));

        // �ړ�����
        float fowardSpeed = runningMaxSpeed;
        Moving(fowardSpeed);

        var velXZ = Vector3.Scale(rb.velocity, new Vector3(1, 0, 1));

        // ���[�V��������
        // �A�j�����[�^�[�ϐ��ɒl��n��
        anim.SetFloat("SpeedBlend", fowardSpeed / runningMaxSpeed);
        anim.SetFloat("SpeedMult", velXZ.magnitude / runningMaxSpeed);
    }

    void AttackLocomotion() {
        // �U���Ώۂ̈ʒu���X�V����
        if (playerDetr.Detected) {
            attackTargetPos = playerDetr.TargetObject.transform.position;
        }

        // �U���Ώۂ̕����������N�H�[�^�j�I��
        Quaternion lookRotaion = Quaternion.LookRotation(attackTargetPos - transform.position, Vector3.up);
        rb.MoveRotation(lookRotaion);
    }

    // �ړ�����(rigidBody�ɑ΂���ύX)
    void Moving(float fowardSpeed) {
        Vector3 velocity = rb.velocity;

        // �i�s
        if (fowardSpeed > Mathf.Epsilon) {
            Vector3 fowardVector = transform.forward * fowardSpeed;

            // �⓹�̏ꍇ
            if (groundDetr.IsSlope) {
                // �Ζʂɉ������x�N�g�����v�Z
                Vector3 onPlane = Vector3.ProjectOnPlane(fowardVector, groundDetr.GroundHit.normal);

                // �i�s�����ƍ⓹�@���̓���
                float slopeDot = Vector3.Dot(fowardVector, groundDetr.GroundHit.normal);

                if (Mathf.Approximately(slopeDot, 0f))
                    slopeDot = 0f;

                // �����̏ꍇ
                if (0f < slopeDot) {
                    fowardVector = onPlane.normalized * fowardSpeed;
                }
                // �o���̏ꍇ
                else if (slopeDot < 0f) {
                    fowardVector = onPlane;
                }
            }

            // �󒆁A���~���̏ꍇ��y�}�C�i�X�����̑��x���c��
            if (!groundDetr.IsLanding || velocity.y < Mathf.Epsilon) {
                fowardVector.y += velocity.y;
            }

            // �i�s���x�̓K�p
            velocity = fowardVector;

            // �ׂ����i����o��
            float deltafowardDistance = fowardSpeed * Time.fixedDeltaTime;
            if (groundDetr.DetectStep(deltafowardDistance)) {
                rb.position += new Vector3(0f, climbStepSmooth, 0f);
            }
        }
        // �Î~
        else {
            // ���藎���h�~�̂��ߎΖʂŐÎ~���͏d�͂�0�ɂ���
            if (groundDetr.IsSlope) {
                gravityScale = 0f;
                velocity = Vector3.zero;
            } else {
                // �d�͂̉e�����c��
                velocity = Vector3.Scale(rb.velocity, Vector3.up);
            }
        }

        rb.velocity = velocity;
    }

    void StartWaiting() {
        state = State.WAIT;
        navAgent.isStopped = true;

        anim.SetFloat("SpeedBlend", 0f);
        anim.SetFloat("SpeedMult", 0f);
    }

    void StartPatrol() {
        state = State.PATROL;
        navAgent.isStopped = false;

        int next = Random.Range(0, patrolPoint.Length - 1);

        if (patrolPoint.Length >= 0 && patrolPoint[next] != null) {
            patrolTargetPos = patrolPoint[next].position;
        } else {
            patrolTargetPos = transform.position;
        }

        navAgent.SetDestination(patrolTargetPos);
    }

    IEnumerator RestartPatrolCoroutine(int second) {
        StartWaiting();

        yield return new WaitForSeconds(second);

        if (state == State.WAIT) {
            StartPatrol();
        }
    }

    void FireAttack1() {
        state = State.ATTACK;

        anim.applyRootMotion = true;
        anim.SetFloat("SpeedBlend", 0f);
        anim.SetFloat("SpeedMult", 0f);
        anim.SetTrigger("Attack1");
    }

    #region AnimtaionEvent
    void EnableHit() {
        if (state == State.ATTACK) {
            weapon.EnableHit();
        }
    }

    void DisableHit() {
        weapon.DisableHit();
    }
    #endregion

    void OnAttackMotionFade() {
        weapon.DisableHit();

        if (state == State.ATTACK) {
            state = State.CHASE;
            anim.applyRootMotion = false;
        }
    }

    void OnStaggerMotionFade() {
        if (state == State.STAGGER) {
            state = State.CHASE;
            anim.applyRootMotion = false;
        }
    }

    #region Gizmo
    void OnDrawGizmos() {
        if (navAgent != null) {
            Gizmos.color = Color.red;
            var prefPos = transform.position;

            // NavMeshAgent:�o�H��`��
            foreach (var pos in navAgent.path.corners) {
                Gizmos.DrawLine(prefPos, pos);
                prefPos = pos;
            }
        }
    }
    #endregion
}
