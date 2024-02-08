using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HitTextFallScript : MonoBehaviour
{
    public TMP_Text txt;
    float yVel;
    Vector2 vel;
    public Transform camToFollow;
    // Start is called before the first frame update
    void Awake()
    {
        txt = GetComponent<TMP_Text>();
        yVel = Random.Range(1,3);
        vel = new Vector2(Random.Range(-0.5f,0.5f),Random.Range(-0.5f,0.5f));
        Destroy(this.gameObject, 2);
    }

    // Update is called once per frame
    void Update()
    {
      transform.position += transform.up * yVel * Time.deltaTime + transform.right * vel.x * Time.deltaTime + transform.forward * vel.y * Time.deltaTime;
      yVel -= Time.deltaTime * 3f;
      transform.rotation = camToFollow.rotation;
    }
}
