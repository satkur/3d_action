using UnityEngine;

[CreateAssetMenu(fileName = "ItemContainer", menuName = "ScriptableObject/ItemContainer")]
public class ItemContainer : ScriptableObject {
    [SerializeField] string dispName;
    [SerializeField] GameObject prefab;

    public string DispName { get { return dispName; } }
    public GameObject Prefab { get { return prefab; } }
}
