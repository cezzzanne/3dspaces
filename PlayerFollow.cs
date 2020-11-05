using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spaces {
    public class PlayerFollow : MonoBehaviour {
        Transform target;
        public float lookSmooth = 0.09f;
        private Vector3 offsetFromTarget = new Vector3(0, 2f, -3f);
        public float xTilt = 10;

        Vector3 destination = Vector3.zero;

        CharacterScript characterController;
        public Transform[] targetList;
        
        float rotateVel = 0;

        private CharacterScript controller;

        private bool selectingItem = false;

        private Touch touch;

        private bool rotating = false;

        float eulerX;

        private bool isPlacingItem = false;

        private int cameraDistance = 4;

        private Vector3 prevPos;


        public void SetCameraTarget(Transform t, int inPublicRoom) {
            target = t;
            offsetFromTarget = (inPublicRoom == 1) ? new Vector3(0, 4.8f, -6.5f): new Vector3(0, 2f, -3f);
            transform.LookAt(target);
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.Rotate(new Vector3(xTilt, 0, 0), Space.Self);
            eulerX = transform.eulerAngles.x;
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

        void Update() {
            // if (Input.touchCount > 0) {
            //     touch = Input.GetTouch(0);
            //     if (touch.phase == TouchPhase.Moved) {
            //         float deltaX = touch.deltaPosition.x;
            //         float deltaY = touch.deltaPosition.y;
            //         transform.RotateAround(target.position, new Vector3(deltaX, deltaY, 0), Time.deltaTime * 55);
            //         rotating = true;
            //     }
            // } else {
            //     rotating = false;
            // }
            // float rotX = Input.GetAxis("Vertical");
            // float rotY = Input.GetAxis("Horizontal");
            // if (rotX != 0 || rotY != 0) {
            //     transform.RotateAround(target.position, new Vector3(rotX, rotY, 0), 85 * Time.deltaTime);
            //     transform.LookAt(target);
            //     rotating = true;
            // } else {
            //     rotating = false;
            // }
        }

        private void LateUpdate() {
           if (target && !selectingItem && !rotating) {
               if (isPlacingItem) {
                   FitCamera();
                    LookAtTarget();
               } else {
                MoveToTarget();
                LookAtTarget();
               }
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

        public void ToggleCharacterChange() {
            selectingItem = !selectingItem;
            if (selectingItem) {
                transform.position = new Vector3(23.7f, 91, 19);
                transform.rotation = Quaternion.Euler(0, 0, 0);
            } else {
                transform.rotation = Quaternion.Euler(0, 0, 0);
                transform.Rotate(new Vector3(xTilt, 0, 0), Space.Self);
            }
        }
        
        void MoveToTarget() {
            destination = target.rotation * offsetFromTarget; // characterController.TargetRotation() * offsetFromTarget;
            destination += target.position;
            transform.position = destination;
        }
         public void ChangeCameraViewpoint(bool insideRoom) {
            // if (insideRoom) {
            //     offsetFromTarget = new Vector3(0, 3.2f, -3.5f);
            // } else {
            //     offsetFromTarget = new Vector3(0, 4.8f, -6.5f);
            // }
        }

        public void ZoomInPlayer() {
            offsetFromTarget = new Vector3(0, 2f, -3f);
        }

        public void ZoomOutPlayer() {
            offsetFromTarget = new Vector3(0, 4.8f, -6.5f);
        }

        void LookAtTarget() {
            float eulerYAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, target.eulerAngles.y, ref rotateVel, lookSmooth);
            transform.rotation = Quaternion.Euler(eulerX, eulerYAngle, 0);
        }

        public void NowFollowing(Transform toFollow, bool placingItem) {
            target = toFollow;
            isPlacingItem = placingItem;
        }

        public void SetCameraDistance(int zoomIn) {
            cameraDistance += zoomIn;
        }

        void FitCamera() {
            Bounds itemBounds = target.GetComponent<BoxCollider>().bounds;
            Vector3 objectSizes = itemBounds.max - itemBounds.min;
            float objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);
            float cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * transform.GetComponent<Camera>().fieldOfView); // Visible height 1 meter in front
            // float distance = cameraDistance * objectSize / cameraView; // Combined wanted distance from the object
            // distance += 0.1f * objectSize; // Estimated offset from the center to the outside of the object
            transform.position = itemBounds.center - cameraDistance * transform.forward;
        } 
    }
}
