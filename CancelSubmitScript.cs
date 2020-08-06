using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CancelSubmitScript : MonoBehaviour {
    // Start is called before the first frame update
    public void ChangeText(string val) {
        Debug.Log(val);
        if (val == "") {
            transform.GetComponentInChildren<Text>().text = "cancel";
        } else {
            transform.GetComponentInChildren<Text>().text = "add";
        }
    }
}
