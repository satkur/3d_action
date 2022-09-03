using Framework;
using UnityEngine;

public class GroundDetector : MonoBehaviour {
    public CapsuleCollider Col { get; private set; }
    public RaycastHit GroundHit { get { return groundHit; } }
    public bool IsLanding { get; private set; }
    public bool IsSlope { get; private set; }

    [SerializeField] Transform groundRayOrigin;
    [SerializeField] Transform shinRayOrigin;
    [SerializeField] float groundRayMaxDistance = 0.45f;
    [SerializeField] float groundThreshold = 0.25f;
    [SerializeField] float slopeMinAngle = 1f;
    [SerializeField] float slopeMaxAngle = 60f;

    LayerMask groundMask;
    RaycastHit groundHit;

    public void Init() {
        Col = GetComponent<CapsuleCollider>();

        groundMask = LayerMask.GetMask(ConstLayer.Ground);
    }

    public void DoUpdate() {
        IsLanding = DetectGround();
        IsSlope = JudgeSlope();
    }

    public bool DetectGround() {
        // for debug
        Debug.DrawRay(groundRayOrigin.position, Vector3.down * groundRayMaxDistance, Color.green, 0.2f, false);

        if (Physics.Raycast(groundRayOrigin.position, Vector3.down, out groundHit, groundRayMaxDistance, groundMask)) {
            return Mathf.Abs(groundHit.point.y - groundRayOrigin.position.y) <= groundThreshold;
        }

        return false;
    }

    public bool JudgeSlope() {
        float angle = Vector3.Angle(Vector3.up, groundHit.normal);

        return IsLanding && slopeMinAngle <= angle && angle <= slopeMaxAngle;
    }

    public bool DetectStep(float deltaMoveDistance) {
        float rayDistance = deltaMoveDistance + Col.radius;
        Vector3 forward = groundRayOrigin.TransformDirection(Vector3.forward);

        // for debug
        Debug.DrawRay(groundRayOrigin.position, forward * rayDistance, Color.green, 0.2f, false);

        RaycastHit lowerHit;
        if (Physics.Raycast(groundRayOrigin.position, forward, out lowerHit, rayDistance, groundMask)) {
            Debug.DrawRay(shinRayOrigin.position, forward * rayDistance, Color.green, 0.2f, false);

            RaycastHit upperHit;
            if (lowerHit.normal.y == 0f &&
                !Physics.Raycast(shinRayOrigin.position, forward, out upperHit, rayDistance, groundMask)) {
                return true;
            }
        }

        return false;
    }
}
