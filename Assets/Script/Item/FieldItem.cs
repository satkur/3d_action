using UnityEngine;

public class FieldItem : MonoBehaviour {
    [SerializeField] int[] itemDataID;

    public int[] ItemDataID { get { return itemDataID; } }
}
