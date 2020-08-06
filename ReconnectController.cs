using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;

public class ReconnectController : MonoBehaviourPunCallbacks {

    // private bool isPaused = false;
    // private void OnApplicationPause(bool isPaused) {
    //     if (isPaused) {
    //         isPaused = true;
    //         PhotonNetwork.Disconnect();
    //     }
    // }
    // private void Update() {
    //     if (!PhotonNetwork.IsConnected && isPaused)
    //     {
    //         if (!PhotonNetwork.ReconnectAndRejoin())
    //         {
    //             Debug.Log("Failed reconnecting and joining!!: 1111", this);
    //         }
    //         else {
    //             Debug.Log("Successful reconnected and joined!: 1111", this);
    //             isPaused = false;
    //         }
    //     }
    // }
}
