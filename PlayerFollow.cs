﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spaces {
    public class PlayerFollow : MonoBehaviour {
        Transform target;
        public float lookSmooth = 0.09f;
        public Vector3 offsetFromTarget = new Vector3(0, 4.8f, -6f);
        public float xTilt = 10;

        Vector3 destination = Vector3.zero;

        CharacterScript characterController;
        public Transform[] targetList;
        
        float rotateVel = 0;

        private CharacterScript controller;

        private bool selectingItem = false;



        public void SetCameraTarget(Transform t) {
            target = t;
            transform.LookAt(target);
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.Rotate(new Vector3(xTilt, 0, 0), Space.Self);
            if (target != null) {
                if (target.GetComponent<CharacterScript>() != null) {
                    characterController = target.GetComponent<CharacterScript>();
                } else {
                    Debug.Log("No Character Script added to Character");
                }
            } else {
                Debug.Log("No target to camera");
            }
        }

        private void LateUpdate() {
           if (target && !selectingItem) {
            MoveToTarget();
            LookAtTarget();
           }
        }

        public void ToggleItemLoader() {
            selectingItem = !selectingItem;
            if (selectingItem) {
                transform.position = new Vector3(15, 52, 13);
                transform.rotation = Quaternion.Euler(0, 0, 0);
            } else {
                transform.rotation = Quaternion.Euler(0, 0, 0);
                transform.Rotate(new Vector3(xTilt, 0, 0), Space.Self);
            }
        }
        
        void MoveToTarget() {
            destination = characterController.TargetRotation() * offsetFromTarget;
            destination += target.position;
            transform.position = destination;
        }
         public void ChangeCameraViewpoint(bool insideRoom) {
            Debug.Log("change viewpoint");
            if (insideRoom) {
                offsetFromTarget = new Vector3(0, 3.2f, -3f);
            } else {
                offsetFromTarget = new Vector3(0, 4.8f, -6.5f);
            }
        }

        void LookAtTarget() {
            float eulerYAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, target.eulerAngles.y, ref rotateVel, lookSmooth);
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, eulerYAngle, 0);
        }
        
    }
}
