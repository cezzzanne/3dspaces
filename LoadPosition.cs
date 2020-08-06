using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadPosition : MonoBehaviour {
    // Start is called before the first frame update
    private bool inProgress = false;
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        if (!inProgress) {
            StartCoroutine(SavePosition());
        }
    }

    IEnumerator SavePosition() {
        inProgress = true;
        yield return new WaitForSeconds(8);
        PlayerPrefs.SetString("editingPosition", transform.position.x + ":" + transform.position.y + ":" + transform.position.z);
        inProgress = false;
    }
}
