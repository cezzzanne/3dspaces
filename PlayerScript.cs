using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Spaces {
    public class PlayerScript : MonoBehaviourPun {
        // Start is called before the first frame update
        void Start() {
            
        }

        void Awake() {
            if (!photonView.IsMine && GetComponent<CharacterScript>() != null) {
                Destroy(GetComponent<CharacterScript>());
            }
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public static void RefreshInstance(ref PlayerScript player, PlayerScript prefab ) {
            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;
            if (player != null) {
                pos = player.transform.position;
                rot = player.transform.rotation;
                PhotonNetwork.Destroy(player.gameObject);
            }
            player = PhotonNetwork.Instantiate(prefab.gameObject.name, pos, rot).GetComponent<PlayerScript>();

        }
    }
}
