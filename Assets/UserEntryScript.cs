using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UserEntryScript : MonoBehaviour
{
    public TMP_Text infoText;
    public PlayerScript ps;

    // Update is called once per frame
    void Update()
    {
        infoText.text = "<SIZE=25>"+ps.name+"</SIZE>\n<SIZE=13>Points: "+ps.points+"\nDeaths: "+ps.deaths;
    }
}
