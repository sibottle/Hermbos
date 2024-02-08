using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NameText : MonoBehaviour
{
    Transform ba;
    TMP_Text text;
    void Start()
    {
        ba = Camera.main.transform;
        text = gameObject.GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        text.enabled = !(Physics.Raycast(transform.position,ba.position) & Vector3.Distance(transform.position, ba.position) < 500);
        transform.rotation = ba.rotation;
    }
}
