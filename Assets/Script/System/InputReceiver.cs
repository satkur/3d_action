using UnityEngine;

public class InputReceiver : MonoBehaviour {
    public static InputReceiver Instance { get; private set; }

    public float Horizontal { get; private set; }
    public float Vertical { get; private set; }
    public Vector3 LStick { get { return new Vector3(Horizontal, 0f, Vertical); } }
    public float LStickTilt { get { return Mathf.Clamp(LStick.magnitude, 0f, 1f); } }

    public float HorizontalR { get; private set; }
    public float VerticalR { get; private set; }
    public Vector3 RStick { get { return new Vector3(VerticalR, HorizontalR, 0f); } }
    public bool R3Button { get; private set; }

    public bool Jump { get; private set; }
    public bool Attack { get; private set; }
    public bool A_Button { get; private set; }
    public bool B_Button { get; private set; }
    public bool Start_Button { get; private set; }

    const string HOR = "Horizontal";
    const string VER = "Vertical";
    const string HORR = "HorizontalR";
    const string VERR = "VerticalR";
    const string R3 = "R3";
    const string JUMP = "Jump";
    const string FIRE1 = "Fire1";
    const string A = "A";
    const string B = "B";
    const string Start = "Start";

    void Awake() {
        if (Instance != null) {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    void Update() {
        Horizontal = Input.GetAxisRaw(HOR);
        Vertical = Input.GetAxisRaw(VER);
        HorizontalR = Input.GetAxisRaw(HORR);
        VerticalR = Input.GetAxisRaw(VERR);
        R3Button = Input.GetButtonDown(R3);
        Jump = Input.GetButton(JUMP);
        Attack = Input.GetButtonDown(FIRE1);
        A_Button = Input.GetButtonDown(A);
        B_Button = Input.GetButtonDown(B);
        Start_Button = Input.GetButtonDown(Start);
    }
}
