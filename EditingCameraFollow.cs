using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditingCameraFollow : MonoBehaviour {
        public float lookSmooth = 0.09f;
        public Vector3 offsetFromTarget = new Vector3(0, 4.8f, -6.5f);
        public float xTilt = -10;
        Vector3 destination = Vector3.zero;
        CharacterEditingScript characterController;
        public Transform[] transformList;

        public Transform target;
        
        float rotateVel = 0;

        private CharacterEditingScript controller;
        private bool isInside = false;

        public Transform testCharacter;

        void Start() {
            // int indexPlaced = 0; // PlayerPrefs.GetInt("CharacterSelected");
            // target = transformList[indexPlaced];
            // // transform.LookAt(target.transform);
            // if (target != null) {
            //     if (target.GetComponent<CharacterEditingScript>() != null) {
            //         characterController = target.GetComponent<CharacterEditingScript>();
            //     } else {
            //         Debug.Log("No Character Script added to Character");
            //     }
            // } else {
            //     Debug.Log("No target to camera");
            // }
            // EDIT : REMOVE THIS EDIT
            target = testCharacter;
            characterController = target.GetComponent<CharacterEditingScript>();
        }

        public void SetTarget(Transform character) {
            Debug.Log("target has been set: "  + character);
            target = character;
            characterController = target.GetComponent<CharacterEditingScript>();
        }

        private void LateUpdate() {
           if (target != null) {
               MoveToTarget();
                LookAtTarget();
           }
        }

        public void ChangeCameraViewpoint(bool insideRoom) {
            Debug.Log("change viewpoint");
            if (insideRoom) {
                isInside = true;
                offsetFromTarget = new Vector3(0, 3.2f, -3f);
            } else {
                isInside = false;
                offsetFromTarget = new Vector3(0, 4.8f, -6.5f);
            }
        }

        public void ToggleViewPoint() {
            if (isInside) {
                Debug.Log("changing offet by toggling");
                offsetFromTarget = new Vector3(0, 4.8f, -6.5f);
                isInside = false;
            } else {
                Debug.Log("making closer");
                offsetFromTarget = new Vector3(0, 3.2f, -3f);
                isInside = true;
            }
        }
        
        void MoveToTarget() {
            destination = characterController.TargetRotation() * offsetFromTarget;
            destination += target.position;
            transform.position = destination;
        }

        void LookAtTarget() {
            float eulerYAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, target.eulerAngles.y, ref rotateVel, lookSmooth);
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, eulerYAngle, 0);
        }
}
