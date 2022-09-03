using System.Collections;
using UnityEngine;

namespace Framework {

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
    public class Door : MonoBehaviour {
        enum KnobSide {
            RIGHT,
            LEFT,
        }

        [SerializeField] KnobSide knobSide = KnobSide.RIGHT;
        [SerializeField] float initialAngleY = 0f;
        [SerializeField] float openAngleY = 92f;
        [SerializeField] float motionTime = 0.8f;

        Rigidbody rb;
        Quaternion closedRot;
        Quaternion openRot;
        bool isOpen = false;
        bool canInteract = false;

        // MonoBehaviour.Resetメソッド
        void Reset() {
            gameObject.tag = ConstTag.Door;

            rb = GetComponent<Rigidbody>();
            var triggerCol = GetComponent<BoxCollider>();

            rb.angularDrag = 0f;
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;

            triggerCol.isTrigger = true;
            triggerCol.size = new Vector3(2.75f, 3.2f, 1.5f);
        }

        public void Init() {
            rb = GetComponent<Rigidbody>();

            // 右開き、左開きのどちらかに合わせて開閉角度を調整
            float initialAngle_ = knobSide == KnobSide.RIGHT ?
                initialAngleY * -1f : initialAngleY;
            float openAngle_ = knobSide == KnobSide.RIGHT ?
                openAngleY * -1f : openAngleY;

            // 現在の角度に応じて開閉時の回転角度を算出する
            closedRot = Quaternion.Euler(0f, rb.rotation.eulerAngles.y - initialAngle_, 0f);
            openRot = Quaternion.Euler(0f, closedRot.eulerAngles.y + openAngle_, 0f);

            // 開閉の状態を判定する
            isOpen = ApproximatelyForQuaternion(rb.rotation, openRot);
            canInteract = true;
        }

        public void Interact() {
#if DEBUG
            if (rb == null) Init();
#endif
            if (canInteract) {
                if (isOpen) {
                    StartCoroutine(CloseCoroutine());
                } else {
                    StartCoroutine(OpenCoroutine());
                }
            }
        }

        public IEnumerator OpenCoroutine() {
            canInteract = false;

            yield return StartCoroutine(RotationCoroutine(openRot, motionTime));

            isOpen = true;
            canInteract = true;
        }

        public IEnumerator CloseCoroutine() {
            canInteract = false;

            yield return StartCoroutine(RotationCoroutine(closedRot, motionTime));

            isOpen = false;
            canInteract = true;
        }

        IEnumerator RotationCoroutine(Quaternion targetRot, float motionTime) {
            float elapsedTime = Time.deltaTime;
            float targetTimeInv = 1f / motionTime;
            float rate = 0f;

            Quaternion preRot = rb.rotation;
            while (true) {
                // 経過時間の指定動作時間に対する割合
                rate = elapsedTime * targetTimeInv;
                Quaternion q = Quaternion.Lerp(preRot, targetRot, CubicSplineInterpolation(rate));

                if (rate > 1f - Mathf.Epsilon) {
                    // 最後まで回転し切って終了
                    rb.MoveRotation(targetRot);
                    break;
                }

                // 少しずつ回転する
                rb.MoveRotation(q);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            yield break;
        }

        // 近似判定
        bool ApproximatelyForQuaternion(Quaternion q1, Quaternion q2) {
            return 1f - Mathf.Abs(Quaternion.Dot(q1, q2)) < 0.000244140625;
        }

        // 3次補間
        float CubicSplineInterpolation(float t) {
            // tは0<=t<=1
            // v(t)=a*t^3+b*t^2+c*t+dから原点を通り、t=0で傾きが0とすると(つまりv'(0)=0)
            // c=d=0となり、
            // v(1)=1とすると1=a+b、t=1で傾き0とするとv'(1)=0つまり0=3a+2b
            // よってa=-2,b=3なので
            // 原点と(1,1)を通り、t=0とt=1のとき微分係数が0となる3次関数は
            // v(t)=-2t^3+3t^2=t^2*(3-2t) となる。
            return t * t * (3 - 2 * t);
        }
    }
}
