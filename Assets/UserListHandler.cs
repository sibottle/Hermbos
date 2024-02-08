using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UserListHandler : MonoBehaviour
{
    RectTransform tra;
    public GameObject entry;
    public bool isOpen;
    float width;

    void Start() {
        tra = GetComponent<RectTransform>();
        RefreshPlayerList();
    }
    // Update is called once per frame
    void Update()
    {
        isOpen = Input.GetKey(KeyCode.Tab);
        width = Mathf.Lerp(width, isOpen ? 500 : 70, Time.deltaTime * 10);
        tra.sizeDelta = new Vector2(width,Mathf.Min(transform.childCount, 7) * 60 + 10);
        if (Input.GetKeyDown(KeyCode.P))
            RefreshPlayerList();
    }

    void RefreshPlayerList() {
        foreach (Transform tr in transform) {
            Destroy(tr.gameObject);
        }
        foreach (PlayerScript piss in FindObjectsOfType<PlayerScript>()) {
            Instantiate(entry,transform).GetComponent<UserEntryScript>().ps = piss;
        }
    }
}
