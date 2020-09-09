using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spaces {
    public class DoorOpen : MonoBehaviour {
        // Start is called before the first frame update
        void Start() {
            
        }

        // Update is called once per frame
        void Update() {
            
        }

        void OnTriggerEnter(Collider other) {
            if (other.transform.GetComponent<CharacterScript>() != null) {
                transform.localRotation = Quaternion.Euler(0, 90, 0);
                transform.localScale = new Vector3(1, 1, 4);
                StartCoroutine(CloseDoor());
            }
        }

        IEnumerator CloseDoor() {
            yield return new WaitForSeconds(2);
            transform.localRotation = Quaternion.Euler(0, 0, 0);
            transform.localScale = new Vector3(1, 1, 1);
        }
    }
}
