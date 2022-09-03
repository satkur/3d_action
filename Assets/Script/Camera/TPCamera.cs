using Framework;
using System;
using UnityEngine;

public class TPCamera : MonoBehaviour {
    [SerializeField, Header("追尾対象")] Transform followTran;
    [SerializeField, Header("初期のカメラアングル")] Vector3 initialAngle = new Vector3(15f, 0f, 0f);
    [SerializeField] float cameraDistance = 2.5f;
    [SerializeField] float rotationSpeed = 100f;
    [SerializeField] float autoRotationDeltaAngle = 0.140625f;
    [SerializeField] float followDelay = 0.2f;
    [SerializeField] float lockOnDistance = 10f;
    [SerializeField] float lockOnTargetFollowDelay = 0.5f;
    [SerializeField] Vector3 lockOnPointAdjuster = new Vector3(0f, 0.04f, 0f);

    Vector3 lastFollowPosition; // 追尾対象のnフレーム前の位置
    Vector3 lookPoint; // 実際にカメラを向ける視点
    Vector3 followVelBuff;
    Transform lockOnTargetTran;
    Vector3 lockOnPoint;
    Vector3 lockOnFollowVelBuff;
    RaycastHit[] hitBuffArr = new RaycastHit[20];
    RaycastHit hit;
    LayerMask lockOnTargetLayerMask;
    LayerMask groundLayerMask;

    InputReceiver inp;
    float localAngleX;
    bool enableLockOn;

    const float extraDistFromWall = 0.1f;

    public void Init() {
        lastFollowPosition = followTran.position;
        lookPoint = followTran.position;

        inp = InputReceiver.Instance;

        lockOnTargetLayerMask = LayerMask.GetMask(ConstLayer.Enemy);
        groundLayerMask = LayerMask.GetMask(ConstLayer.Ground);

        ResetPosRot();
    }

    public void DoUpdate() {
        if (inp.R3Button) {
            if (enableLockOn) {
                // ロックオン解除
                DisableLockOnMode();
                return;
            }

            // ロックオン試行
            enableLockOn = TryLockOn();

            if (!enableLockOn) {
                // ロックオンできない場合はカメラリセット
                ResetPosRot();
            }
        }
    }

    public void DoLateUpdate() {
        // 視点の座標更新
        UpdateLookPoint();

        // カメラの回転
        if (enableLockOn) {
            UpdateLockOnPoint();
            LookAtLockOnTarget();
        } else {
            InputRotation(inp.HorizontalR, inp.VerticalR);
        }

        // カメラ座標の移動
        UpdateCameraPosition();

        // 追尾回転
        FollowAutoRotation();
    }

    // 視点の座標更新
    void UpdateLookPoint() {
        if (Vector3.SqrMagnitude(followTran.position - lookPoint) < Mathf.Epsilon) {
            followVelBuff = Vector3.zero;
            lookPoint = followTran.position;
            return;
        }

        lookPoint = Vector3.SmoothDamp(lookPoint, followTran.position, ref followVelBuff, followDelay);
    }

    // カメラ位置の移動
    void UpdateCameraPosition() {
        float distance = cameraDistance;

        // 壁・地形判定
        if (Physics.Raycast(followTran.position, -transform.forward, out hit, cameraDistance, groundLayerMask)) {
            distance = hit.distance - extraDistFromWall;
        }

        Vector3 offsetVec = -transform.forward * distance;

        // 視点を基準にカメラを移動
        transform.position = lookPoint + offsetVec;
    }

    // 追尾対象の左右移動に伴うカメラ回転
    void FollowAutoRotation() {
        if (Vector3.SqrMagnitude(followTran.position - lastFollowPosition) > 0.000244140625f) {
            Vector3 fowardXZ = transform.forward;
            fowardXZ.y = 0f;
            Vector3 targetXZ = followTran.position - transform.position;
            targetXZ.y = 0f;

            float ang = Vector3.SignedAngle(fowardXZ, targetXZ, Vector3.up);
            if (-10f < ang && ang < 10f) {
                ang = 0f;
            }

            transform.rotation = Quaternion.AngleAxis(Math.Sign(ang) * autoRotationDeltaAngle, Vector3.up) * transform.rotation;
        }

        // xxxx毎フレーム更新しないようにする
        lastFollowPosition = followTran.position;
    }

    // コントローラーの入力に応じたカメラ回転
    void InputRotation(float hor, float ver) {
        float deltaAngleX = rotationSpeed * ver * Time.deltaTime;
        float deltaAngleY = rotationSpeed * hor * Time.deltaTime;

        // 回転制限
        // memo : 90度(-90度)に限りなく近い状態から90度(-90度)にできない=微妙にカクつくため不完全
        if (localAngleX + deltaAngleX > 89f || localAngleX + deltaAngleX < -89f) {
            deltaAngleX = 0f;
        }

        localAngleX += deltaAngleX;

        Quaternion xRot = Quaternion.AngleAxis(deltaAngleX, transform.right);
        Quaternion yRot = Quaternion.AngleAxis(deltaAngleY, Vector3.up);

        // 回転
        transform.rotation = yRot * xRot * transform.rotation;
    }

    // カメラ位置・方向のリセット
    void ResetPosRot() {
        transform.rotation = followTran.rotation * Quaternion.Euler(initialAngle);
        localAngleX = transform.rotation.eulerAngles.x;

        Vector3 offset = -transform.forward * cameraDistance;
        transform.position = lookPoint + offset;
    }

    // ロックオン開始
    bool TryLockOn() {
        var target = CaptureClosestTarget();

        if (target == null) {
            return false;
        }

        lockOnTargetTran = target.transform;
        lockOnPoint = lockOnTargetTran.position;

        return true;
    }

    // ロックオン対象捕捉
    GameObject CaptureClosestTarget() {
        int count = Physics.SphereCastNonAlloc(
                followTran.position,
                lockOnDistance,
                followTran.forward,
                hitBuffArr,
                0.01f,
                lockOnTargetLayerMask,
                QueryTriggerInteraction.Ignore
            );

        float minDist = float.MaxValue;
        GameObject lockOnTarget = null;
        for (int i = 0; i < count; i++) {
            float dist = Vector3.SqrMagnitude(followTran.position - hitBuffArr[i].transform.position);

            if (dist < minDist) {
                minDist = dist;
                lockOnTarget = hitBuffArr[i].transform.gameObject;
            }
        }

        return lockOnTarget;
    }

    // ロックオン捕捉対象の視点位置を更新
    void UpdateLockOnPoint() {
        if (Vector3.SqrMagnitude(lockOnTargetTran.position - lockOnPoint) < Mathf.Epsilon) {
            lockOnFollowVelBuff = Vector3.zero;
            lockOnPoint = lockOnTargetTran.position;
            return;
        }

        lockOnPoint = Vector3.SmoothDamp(lockOnPoint, lockOnTargetTran.position, ref lockOnFollowVelBuff, lockOnTargetFollowDelay);
    }

    // ロックオン対象を捕捉するようにカメラを回転
    void LookAtLockOnTarget() {
        Vector3 dist = lockOnPoint + lockOnPointAdjuster - transform.position;
        Quaternion lookq = Quaternion.LookRotation(dist);

        transform.rotation = lookq;
    }

    // ロックオン解除
    void DisableLockOnMode() {
        enableLockOn = false;
        localAngleX = transform.rotation.eulerAngles.x;
    }
}
