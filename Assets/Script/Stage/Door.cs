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

        // MonoBehaviour.Reset���\�b�h
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

            // �E�J���A���J���̂ǂ��炩�ɍ��킹�ĊJ�p�x�𒲐�
            float initialAngle_ = knobSide == KnobSide.RIGHT ?
                initialAngleY * -1f : initialAngleY;
            float openAngle_ = knobSide == KnobSide.RIGHT ?
                openAngleY * -1f : openAngleY;

            // ���݂̊p�x�ɉ����ĊJ���̉�]�p�x���Z�o����
            closedRot = Quaternion.Euler(0f, rb.rotation.eulerAngles.y - initialAngle_, 0f);
            openRot = Quaternion.Euler(0f, closedRot.eulerAngles.y + openAngle_, 0f);

            // �J�̏�Ԃ𔻒肷��
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
                // �o�ߎ��Ԃ̎w�蓮�쎞�Ԃɑ΂��銄��
                rate = elapsedTime * targetTimeInv;
                Quaternion q = Quaternion.Lerp(preRot, targetRot, CubicSplineInterpolation(rate));

                if (rate > 1f - Mathf.Epsilon) {
                    // �Ō�܂ŉ�]���؂��ďI��
                    rb.MoveRotation(targetRot);
                    break;
                }

                // ��������]����
                rb.MoveRotation(q);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            yield break;
        }

        // �ߎ�����
        bool ApproximatelyForQuaternion(Quaternion q1, Quaternion q2) {
            return 1f - Mathf.Abs(Quaternion.Dot(q1, q2)) < 0.000244140625;
        }

        // 3�����
        float CubicSplineInterpolation(float t) {
            // t��0<=t<=1
            // v(t)=a*t^3+b*t^2+c*t+d���猴�_��ʂ�At=0�ŌX����0�Ƃ����(�܂�v'(0)=0)
            // c=d=0�ƂȂ�A
            // v(1)=1�Ƃ����1=a+b�At=1�ŌX��0�Ƃ����v'(1)=0�܂�0=3a+2b
            // �����a=-2,b=3�Ȃ̂�
            // ���_��(1,1)��ʂ�At=0��t=1�̂Ƃ������W����0�ƂȂ�3���֐���
            // v(t)=-2t^3+3t^2=t^2*(3-2t) �ƂȂ�B
            return t * t * (3 - 2 * t);
        }
    }
}
