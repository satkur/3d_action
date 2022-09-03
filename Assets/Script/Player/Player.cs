using Framework;
using UnityEngine;

public class Player : MonoBehaviour, IHurtbox {
    [SerializeField] float walkSpeed = 5f;
    [SerializeField] float walkAnimSpeedMult = 1.4f;
    [SerializeField] float dodgeRollSpeed = 4.5f;
    [SerializeField] PlayerAnimParam aniParam;
    [SerializeField] GameObject rightHandGrip;

    PlayerModel model;
    PlayerInventory itemInventory;
    HumanIK ik;
    Rigidbody rb;
    Animator anim;
    PlayerStateMachine stateMachine;
    InputReceiver inp;
    GroundDetector groundDetr;
    CharacterMotor charaMotor;
    Transform cameraT;

    // ボタン制御
    bool actionLock = false;

    // 姿勢状態
    bool isFallPosture = false;

    // インタラクション可能なオブジェクトを保持
    FieldItem nearbyItem;
    Door nearbyDoor;

    // アニメーション制御
    bool enableRootMotion = false;
    bool enableRootRotation = false;

    delegate void StateBaseUpdate();
    StateBaseUpdate currentUpdAction;
    StateBaseUpdate normalAction;
    StateBaseUpdate attackAction;
    StateBaseUpdate restrictedAction;

    delegate void StateBaseFixedUpdate();
    StateBaseFixedUpdate currentFixedUpdAction;
    StateBaseFixedUpdate inputLocomotion;
    StateBaseFixedUpdate attackLocomotion;
    StateBaseFixedUpdate dodgeRollLocomotion;

    public enum PlayerState {
        LOCOMOTION,
        ATTACK,
        DODGEROLL,
        STAGGER,
        DEAD
    }

    PlayerState state = PlayerState.LOCOMOTION;

    #region public
    public void Init(PlayerModel model, PlayerInventoryModel inventoryModel, Transform cameraT) {
        // プレイヤーモデル
        this.model = model;

        // プレイヤーアイテム管理
        itemInventory = new PlayerInventory(inventoryModel, rightHandGrip.transform);
        itemInventory.Init();

        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        // StateMachine
        stateMachine = anim.GetBehaviour<PlayerStateMachine>();
        stateMachine.Init();

        // 地面判定
        groundDetr = GetComponent<GroundDetector>();
        groundDetr.Init();

        // 移動
        charaMotor = GetComponent<CharacterMotor>();
        charaMotor.Init(
            rigidbody: rb,
            groundDetr: groundDetr,
            gravity: new GravityParameter(Physics.gravity, 1f));

        // IK
        ik = GetComponent<HumanIK>();
        ik.Init(anim);

        // イベント登録
        CacheStateBaseAction();
        BindStateMachineEvent();

        this.cameraT = cameraT;

        inp = InputReceiver.Instance;
    }

    // 被ダメージ処理
    public void TakeDamage(float damage) {
        // ダメージ無効
        if (state == PlayerState.DODGEROLL || state == PlayerState.DEAD)
            return;

        // 被ダメ計算
        model.DecreaseHp(damage);

        // HPが0の場合
        if (model.IsDead) {
            BeginDead();
            return;
        }

        // 怯み
        BeginStagger();
    }

    public void EquipRHandWeapon(ItemModel item) {
        itemInventory.EquipRHandWeapon(item);
    }

    public void DoUpdate() {
        currentUpdAction?.Invoke();
    }

    public void DoFixedUpdate() {
        groundDetr.DoUpdate();

        currentFixedUpdAction?.Invoke();
    }
    #endregion

    void OnAnimatorMove() {
        // ルートモーションによる回転の反映
        if (enableRootRotation) {
            rb.MoveRotation(anim.rootRotation);
        }
        // ルートモーションによる移動の反映
        if (enableRootMotion) {
            charaMotor.Move(anim.velocity);
        }
    }

    void OnTriggerEnter(Collider other) {
        // アイテムを検知
        if (other.CompareTag(ConstTag.Item)) {
            nearbyItem = other.gameObject.GetComponent<FieldItem>();
        }
        // 扉を検知
        if (other.CompareTag(ConstTag.Door)) {
            nearbyDoor = other.gameObject.GetComponent<Door>();
        }
    }

    void OnTriggerStay(Collider other) {
        // アイテムを検知
        if (nearbyItem == null && other.CompareTag(ConstTag.Item)) {
            nearbyItem = other.gameObject.GetComponent<FieldItem>();
        }
        // 扉を検知
        if (nearbyDoor == null && other.CompareTag(ConstTag.Door)) {
            nearbyDoor = other.gameObject.GetComponent<Door>();
        }
    }

    void OnTriggerExit(Collider other) {
        // インタラクションオブジェクトの検知情報をリセット
        if (other.CompareTag(ConstTag.Item)) {
            nearbyItem = null;
        }
        if (other.CompareTag(ConstTag.Door)) {
            nearbyDoor = null;
        }
    }

    #region Subscribe Event
    void CacheStateBaseAction() {
        // Update、FixedUpdateでステートごとに呼び分ける処理をデリゲートにキャッシュする
        // Update
        normalAction = NormalAction;
        attackAction = AttackAction;
        restrictedAction = RestrictedAction;
        // 初期値
        currentUpdAction = normalAction;

        // FixedUpdate
        inputLocomotion = InputLocomotion;
        attackLocomotion = AttackLocomotion;
        dodgeRollLocomotion = DodgeRollLocomotion;
        // 初期値
        currentFixedUpdAction = inputLocomotion;
    }

    void BindStateMachineEvent() {
        stateMachine.onAttackMotionFade += OnAttackMotionFade;
        stateMachine.onDodgeMotionFade += OnDodgeRollMotionFade;
        stateMachine.onStaggerMotionFade += OnStaggerMotionFade;
    }
    #endregion

    #region Update内ステートベース処理
    void NormalAction() {
        // 落下状態での行動制限
        if (isFallPosture) {
            return;
        }

        // ローリング回避入力
        if (inp.A_Button) {
            // ローリング回避開始
            BeginDodgeRoll();
        }

        // 攻撃1入力
        if (inp.Attack) {
            // 攻撃1開始
            BeginAttack1();
        }

        // 使用ボタン入力
        if (inp.B_Button) {
            if (nearbyItem != null) {
                // アイテムを拾う
                itemInventory.AddItems(nearbyItem.ItemDataID);
            }
            if (nearbyDoor != null) {
                // ドア開閉
                nearbyDoor.Interact();
            }
        }
    }

    void AttackAction() {
        if (inp.Attack) {
            // 連続攻撃の実行
            actionLock = true;
            anim.SetTrigger(aniParam.Attack);
            return;
        }

        if (!actionLock) {
            // 攻撃後動作キャンセル
            if (inp.LStickTilt > 0f || inp.Attack || inp.A_Button) {
                OnAttackMotionFade();
            }
        }
    }

    void RestrictedAction() {
        if (!actionLock) {
            if (inp.LStickTilt > 0f || inp.Attack || inp.A_Button) {
                anim.SetTrigger(aniParam.Terminate);
            }
        }
    }
    #endregion

    #region FixedUpdate内ステートベース処理
    // 通常移動処理
    void InputLocomotion() {
        float inpLStickTilt = inp.LStickTilt;

        // 落下/着地判定
        DetectLanding();

        // スティック入力に応じて旋回
        if (!isFallPosture && inpLStickTilt != 0f) {
            TurnInDirection(cameraT, inp.LStick, 0.6f);
        }

        // 移動処理
        charaMotor.MoveForwardXZ(walkSpeed * inpLStickTilt, 0.1f < inpLStickTilt);

        // モーション制御
        // アニメメーター変数に値を渡す
        anim.SetFloat(aniParam.Speed, inpLStickTilt);
        anim.SetFloat(aniParam.SpeedMotionMultiplier, inpLStickTilt * walkAnimSpeedMult + 0.6f);
    }

    // 攻撃時処理
    void AttackLocomotion() {
        // 落下/着地判定
        DetectLanding();

        // 攻撃方向の再入力処理
        // スティック入力に応じて旋回
        if (inp.LStickTilt != 0f) {
            TurnInDirection(cameraT, inp.LStick, 0.1f);
        }
    }

    // 回避中移動処理
    void DodgeRollLocomotion() {
        charaMotor.MoveForwardXZ(dodgeRollSpeed);
    }
    #endregion

    // 入力方向への回転処理
    void TurnInDirection(Transform cameraTran, Vector3 stickInput, float smooth) {
        // カメラの向いている方向を前として入力方向から進行方向を決定する(カメラのチルトは0度(水平)で計算)
        var moveDirection = Quaternion.FromToRotation(cameraTran.up, Vector3.up) * cameraTran.rotation * stickInput;

        // 進行方向へ回転
        charaMotor.TurnInDirectionXZ(moveDirection, smooth);
    }

    // 落下姿勢の開始/終了判定
    void DetectLanding() {
        if (!isFallPosture && !groundDetr.IsLanding) {
            // 落下開始
            isFallPosture = true;
            BeginFallAnim();
        } else if (isFallPosture && groundDetr.IsLanding) {
            // 着地
            isFallPosture = false;
            EndFallAnim();
        }
    }

    // 落下モーション開始
    void BeginFallAnim() {
        anim.SetFloat(aniParam.Speed, 0f);
        anim.SetTrigger(aniParam.Fall);
        anim.SetBool(aniParam.IsGrounded, false);
    }

    // 落下モーション終了(着地)
    void EndFallAnim() {
        anim.SetBool(aniParam.IsGrounded, true);
    }


    void BeginLocomotion() {
        state = PlayerState.LOCOMOTION;

        actionLock = false;
        enableRootMotion = false;
        enableRootRotation = false;

        currentUpdAction = normalAction;
        currentFixedUpdAction = inputLocomotion;
    }

    public void BeginAttack1() {
        state = PlayerState.ATTACK;

        actionLock = true;
        enableRootMotion = true;
        enableRootRotation = false;

        itemInventory.RHandWeapon.SetSafety(false);

        anim.SetFloat(aniParam.Speed, 0f);
        anim.SetTrigger(aniParam.Attack);
        anim.SetInteger(aniParam.AttackMotionType, itemInventory.RHandWeapon.AttackMotionType);

        currentUpdAction = attackAction;
        currentFixedUpdAction = attackLocomotion;
    }

    public void EndAttack1() {
        itemInventory.RHandWeapon.DisableHit();
        itemInventory.RHandWeapon.SetSafety(true);

        anim.ResetTrigger(aniParam.Attack);

        if (state == PlayerState.ATTACK) {
            BeginLocomotion();
        }
    }

    public void BeginDodgeRoll() {
        state = PlayerState.DODGEROLL;

        enableRootMotion = false;
        enableRootRotation = false;

        anim.Play(aniParam.DodgeRollState, anim.GetLayerIndex(ConstAnimLayer.Default), 0f);

        currentUpdAction = null;
        currentFixedUpdAction = dodgeRollLocomotion;
    }

    public void EndDodgeRoll() {
        BeginLocomotion();
    }

    public void BeginStagger() {
        state = PlayerState.STAGGER;

        actionLock = true;
        enableRootMotion = true;
        enableRootRotation = true;
        anim.CrossFadeInFixedTime(
            aniParam.StaggerState,
            0.2f,
            anim.GetLayerIndex(ConstAnimLayer.Default),
            0f);

        currentUpdAction = restrictedAction;
        currentFixedUpdAction = null;
    }

    public void EndStagger() {
        BeginLocomotion();
    }

    public void BeginDead() {
        state = PlayerState.DEAD;

        actionLock = true;
        enableRootMotion = true;
        enableRootRotation = true;
        anim.CrossFadeInFixedTime(
            aniParam.DeathState,
            0.1f,
            anim.GetLayerIndex(ConstAnimLayer.Default),
            0f);

        currentUpdAction = null;
        currentFixedUpdAction = null;
    }

    #region StateMachineEventCallback
    void OnAttackMotionFade() {
        EndAttack1();
    }

    void OnDodgeRollMotionFade() {
        EndDodgeRoll();
    }

    void OnStaggerMotionFade() {
        EndStagger();
    }
    #endregion

    #region AnimationEventFunction
    void EnableHit() {
        itemInventory.RHandWeapon.EnableHit();
    }

    void DisableHit() {
        itemInventory.RHandWeapon.DisableHit();
    }
    #endregion
}
