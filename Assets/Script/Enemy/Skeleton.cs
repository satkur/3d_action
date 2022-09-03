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

        // 巡回状態へ移行する
        StartPatrol();

        // カスタム重力初期化
        rb.useGravity = false;
        gravityVector = Physics.gravity;
        gravityScale = baseGravityScale;
    }

    public void DoUpdate() {
        navAgent.nextPosition = transform.position;

        playerDetr.DoUpdate();

        switch (state) {
            case State.WAIT:
                // プレイヤー(敵)を検知
                if (playerDetr.Detected) {
                    navAgent.isStopped = true;
                    navAgent.ResetPath();
                    state = State.CHASE;
                }
                break;

            case State.PATROL:
                // プレイヤー(敵)を検知
                if (playerDetr.Detected) {
                    navAgent.isStopped = true;
                    navAgent.ResetPath();
                    state = State.CHASE;
                }
                // 到着を検知
                else if (navAgent.hasPath && navAgent.remainingDistance < 0.4f) {
                    // 立ち止まった後に次の地点へ巡回を再開する
                    StartCoroutine(RestartPatrolCoroutine(5));
                } else if (!navAgent.hasPath) {
                    // 経路を探知できない場合は待機状態へ移行する
                    StartWaiting();
                }
                break;

            case State.CHASE:
                // 敵を検知中
                if (playerDetr.Detected) {
                    // プレイヤー(敵)の座標を更新
                    attackTargetPos = playerDetr.TargetObject.transform.position;

                    if (Vector3.Distance(transform.position, attackTargetPos) <= attackDistance) {
                        FireAttack1();
                    }
                }
                // 敵が見えない
                else if ((transform.position - attackTargetPos).sqrMagnitude < 0.1f) {
                    attackTargetPos = Vector3.zero;

                    // NavMeshAgentのシミュレーション位置を強制的に現在位置にする
                    navAgent.Warp(transform.position);
                    // 立ち止まった後に次の地点へ巡回を再開する
                    StartCoroutine(RestartPatrolCoroutine(4));
                }
                break;

            case State.ATTACK:
                if (playerDetr.Detected) {
                    // プレイヤー(敵)の座標を更新
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

        // カスタム重力
        rb.AddForce(gravityVector * gravityScale, ForceMode.Acceleration);
    }

    // 被ダメージ処理
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

        // 怯みモーション
        state = State.STAGGER;
        anim.applyRootMotion = true;
        anim.CrossFadeInFixedTime("Stagger", 0.2f);
    }

    void Waiting() {
        rb.velocity = Vector3.Scale(rb.velocity, Vector3.up);
    }

    void PatrolLocomotion() {
        // 最も近い進行先の座標を取得
        foreach (var pos in navAgent.path.corners) {
            if (Vector3.Distance(transform.position, pos) >= 0.1f) {
                // 進行先を向くクォータニオン
                Quaternion lookRotaion = Quaternion.LookRotation(pos - transform.position, Vector3.up);

                // 進行方向の適用
                rb.MoveRotation(Quaternion.Lerp(transform.rotation, lookRotaion, 0.1f));
                break;
            }
        }

        // 移動処理
        float fowardSpeed = walkSpeed;

        Moving(fowardSpeed);


        var velXZ = Vector3.Scale(rb.velocity, new Vector3(1, 0, 1));

        // モーション制御
        // アニメメーター変数に値を渡す
        anim.SetFloat("SpeedBlend", fowardSpeed / runningMaxSpeed);
        anim.SetFloat("SpeedMult", velXZ.magnitude / runningMaxSpeed);
    }

    void ChaseLocomotion() {
        // プレイヤーの位置を取得する
        if (playerDetr.Detected) {
            attackTargetPos = playerDetr.TargetObject.transform.position;
        }

        // プレイヤーの方向を向くクォータニオン
        Quaternion lookRotaion = Quaternion.LookRotation(attackTargetPos - transform.position, Vector3.up);

        // 進行方向の適用
        rb.MoveRotation(Quaternion.Lerp(transform.rotation, lookRotaion, 0.1f));

        // 移動処理
        float fowardSpeed = runningMaxSpeed;
        Moving(fowardSpeed);

        var velXZ = Vector3.Scale(rb.velocity, new Vector3(1, 0, 1));

        // モーション制御
        // アニメメーター変数に値を渡す
        anim.SetFloat("SpeedBlend", fowardSpeed / runningMaxSpeed);
        anim.SetFloat("SpeedMult", velXZ.magnitude / runningMaxSpeed);
    }

    void AttackLocomotion() {
        // 攻撃対象の位置を更新する
        if (playerDetr.Detected) {
            attackTargetPos = playerDetr.TargetObject.transform.position;
        }

        // 攻撃対象の方向を向くクォータニオン
        Quaternion lookRotaion = Quaternion.LookRotation(attackTargetPos - transform.position, Vector3.up);
        rb.MoveRotation(lookRotaion);
    }

    // 移動処理(rigidBodyに対する変更)
    void Moving(float fowardSpeed) {
        Vector3 velocity = rb.velocity;

        // 進行
        if (fowardSpeed > Mathf.Epsilon) {
            Vector3 fowardVector = transform.forward * fowardSpeed;

            // 坂道の場合
            if (groundDetr.IsSlope) {
                // 斜面に沿ったベクトルを計算
                Vector3 onPlane = Vector3.ProjectOnPlane(fowardVector, groundDetr.GroundHit.normal);

                // 進行方向と坂道法線の内積
                float slopeDot = Vector3.Dot(fowardVector, groundDetr.GroundHit.normal);

                if (Mathf.Approximately(slopeDot, 0f))
                    slopeDot = 0f;

                // 下り坂の場合
                if (0f < slopeDot) {
                    fowardVector = onPlane.normalized * fowardSpeed;
                }
                // 登り坂の場合
                else if (slopeDot < 0f) {
                    fowardVector = onPlane;
                }
            }

            // 空中、下降中の場合はyマイナス方向の速度を残す
            if (!groundDetr.IsLanding || velocity.y < Mathf.Epsilon) {
                fowardVector.y += velocity.y;
            }

            // 進行速度の適用
            velocity = fowardVector;

            // 細かい段差を登る
            float deltafowardDistance = fowardSpeed * Time.fixedDeltaTime;
            if (groundDetr.DetectStep(deltafowardDistance)) {
                rb.position += new Vector3(0f, climbStepSmooth, 0f);
            }
        }
        // 静止
        else {
            // 滑り落ち防止のため斜面で静止中は重力を0にする
            if (groundDetr.IsSlope) {
                gravityScale = 0f;
                velocity = Vector3.zero;
            } else {
                // 重力の影響を残す
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

            // NavMeshAgent:経路を描写
            foreach (var pos in navAgent.path.corners) {
                Gizmos.DrawLine(prefPos, pos);
                prefPos = pos;
            }
        }
    }
    #endregion
}
