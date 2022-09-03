using Framework;
using UnityEngine;

public class PlayerDetector : MonoBehaviour {
    [SerializeField] Transform origin;

    public bool Detected { get; private set; }
    public GameObject TargetObject { get; private set; }

    Collider searchAreaCol;
    LayerMask mask;
    int playerLayer;

    bool inSearchArea;

    public void Init() {
        searchAreaCol = GetComponent<Collider>();
        mask = LayerMask.GetMask(ConstLayer.Ground, ConstLayer.Player);
        playerLayer = LayerMask.NameToLayer(ConstLayer.Player);
    }

    public void DoUpdate() {
        if (TargetObject == null) {
            return;
        }

        Detected = DetectPlayer(TargetObject);

        if (!Detected && !inSearchArea) {
            TargetObject = null;
        }
    }

    #region OnTrigger
    void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == playerLayer) {
            inSearchArea = true;
            TargetObject = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.gameObject.layer == playerLayer) {
            inSearchArea = false;
        }
    }
    #endregion

    public bool DetectPlayer(GameObject _targetObject) {
        Vector3 targetPos = new Vector3(_targetObject.transform.position.x, _targetObject.transform.position.y + 1.5f, _targetObject.transform.position.z);

        // xxxxxxxxxxxxx
        // box castÇ÷â¸èCó\íË
        bool res = Physics.Linecast(origin.position, targetPos, out RaycastHit hitInfo, mask);

        //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //sphere.GetComponent<Collider>().enabled = false;
        //sphere.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        //sphere.transform.position = hitInfo.point;

        return res && hitInfo.collider.CompareTag(ConstTag.Player);
    }

    public void Disable() {
        searchAreaCol.enabled = false;
        Detected = false;
        TargetObject = null;
    }

    #region Gizmo
    void OnDrawGizmos() {
        if (TargetObject != null) {
            Vector3 targetPos = new Vector3(TargetObject.transform.position.x, TargetObject.transform.position.y + 1.5f, TargetObject.transform.position.z);

            Gizmos.color = Detected ? Color.red : Color.blue;
            Gizmos.DrawLine(origin.position, targetPos);
        }
    }
    #endregion
}
