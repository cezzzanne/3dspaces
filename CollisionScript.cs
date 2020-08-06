using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionScript : MonoBehaviour
{
    // Start is called before the first frame update

    private bool isInCollision;
    Vector3 pointOfCollision;

    void OnCollisionEnter(Collision other) {
        if (other.gameObject.name != "Terrain") {
            isInCollision = true;
            pointOfCollision = other.gameObject.transform.position;
        }
    }

    void OnCollisionExit(Collision collisionInfo) {
        isInCollision = false;
    }

    public bool isColliding() {
       return isInCollision;
    }
}
