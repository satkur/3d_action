using UnityEngine;

namespace Framework {

    [RequireComponent(typeof(Rigidbody))]
    public class CharacterMotor : MonoBehaviour {
        [SerializeField] float climbStepSmooth = 0.1f;
        [SerializeField] float dragInAirXZ = 2.125f;

        Rigidbody rb;
        GroundDetector gd;
        GravityParameter gravity;

        bool ignoreGravityOnce = false;
        Vector3 climbStepAddPos;

        public void Init(Rigidbody rigidbody, GroundDetector groundDetr, GravityParameter gravity) {
            rb = rigidbody;
            gd = groundDetr;
            this.gravity = gravity;

            climbStepAddPos = new Vector3(0, climbStepSmooth, 0);
        }

        // 移動処理
        public void Move(Vector3 velocity, bool enableClimbStep = true) {
            float speed = velocity.magnitude;

            if (!gd.IsLanding) {
                // 空中での移動制御
                MoveInAir();
            } else if (!Mathf.Approximately(speed, 0f)) {
                // 移動中
                MoveOnGround(velocity);

                // 段差処理
                // 細かい段差を登る
                if (enableClimbStep && gd.DetectStep(speed * Time.fixedDeltaTime)) {
                    rb.position += climbStepAddPos;
                }
            } else {
                // 静止中
                Stand();
            }

            if (ignoreGravityOnce) {
                // 1度だけ重力を無視する
                ignoreGravityOnce = false;
                return;
            }
            // 重力の加算
            rb.velocity += gravity.GetScaledGravityAccel() * Time.deltaTime;
        }

        // 前進処理(xz平面)
        public void MoveForwardXZ(float speed, bool enableClimbStep = true) {
            Vector3 forward = transform.forward;
            forward.y = 0f;

            Move(forward * speed, enableClimbStep);
        }

        // 地上移動
        private void MoveOnGround(Vector3 velocity) {
            // 坂道での移動速度計算
            if (gd.IsSlope) {
                // 進行方向と坂道の法線の内積
                float slopeDot = Vector3.Dot(velocity, gd.GroundHit.normal);

                // 進行方向と坂道の法線がほぼ直角の場合、何もしない
                if (!Mathf.Approximately(slopeDot, 0f)) {

                    // 斜面に沿ったベクトルを計算
                    velocity = Vector3.ProjectOnPlane(velocity, gd.GroundHit.normal);
                }
            }

            if (rb.velocity.y < 0f) {
                // 重力の影響を残すため、y成分をそのまま設定
                velocity.y = rb.velocity.y;
            } else {
                // 地上での移動を表現したいため、y成分には0を設定し、キャラクタが跳ねるのを防止する
                velocity.y = 0f;
            }

            rb.velocity = velocity;
        }

        // 空中移動
        private void MoveInAir() {
            var velocity = rb.velocity;

            // xz方向の空気抵抗を計算する
            velocity.y = 0f;
            // v(t+dt)=v(t)(1−drag*dt)
            velocity = velocity * (1f - dragInAirXZ * Time.deltaTime);
            
            velocity.y = rb.velocity.y;

            rb.velocity = velocity;
        }

        // 静止挙動
        private void Stand() {
            if (gd.IsSlope) {
                // 滑り落ち防止のため斜面で静止中は重力を0にする
                ignoreGravityOnce = false;
                rb.velocity = Vector3.zero;
            }

            // x, zを0にする
            rb.velocity = Vector3.Scale(rb.velocity, Vector3.up);
        }

        // 特定方向への回転処理: xz平面
        public void TurnInDirectionXZ(Vector3 target, float smooth) {
            target.y = 0f;

            rb.MoveRotation(
                Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.LookRotation(target, Vector3.up),
                    smooth));
        }
    }
}
