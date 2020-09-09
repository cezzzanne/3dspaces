using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LoadPosition : MonoBehaviourPun {
    // Start is called before the first frame update
    private bool inProgress = false;

    private bool inPublicRoom;
    void Start() {
        // inPublicRoom = PlayerPrefs.GetInt("isInPublicWorld") == 1? true : false;
    }

    // Update is called once per frame
    void Update() {
        // if (!inProgress && photonView.IsMine && !inPublicRoom) {
        //     StartCoroutine(SavePosition());
        // }
    }

    // IEnumerator SavePosition() {
        // inProgress = true;
        // yield return new WaitForSeconds(8);
        // PlayerPrefs.SetString("editingPosition", transform.position.x + ":" + transform.position.y + ":" + transform.position.z);
        // inProgress = false;
    // }
}
