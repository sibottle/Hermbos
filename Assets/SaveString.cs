using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SaveString : MonoBehaviour
{
  public string namea;
  public TMP_Text ba;
  public void Save() {
    PlayerPrefs.SetString(namea, ba.text);
  }
}
