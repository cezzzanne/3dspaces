using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEditingScript : MonoBehaviour {
        // Start is called before the first frame update
        Joystick joystick;
        Animator animator;
        public float inputDelay = 0.1f;
        public float forwardVel = 0.1f;
        public float rotateVel = 100;
        Quaternion targetRotation;
        Rigidbody rBody;
        float forwardInput, turnInput = 0;

        private bool otherPlayer = false;

        public GameObject mainCam;

        public int id;

        CharacterController characterController;
        public Material otherMaterial;
        private int inRoomState = -1;

        public Quaternion TargetRotation() {
            return targetRotation;
        }
       
        void Start() {
            joystick = FindObjectOfType<Joystick>();
            targetRotation = transform.rotation;
            characterController = transform.GetComponent<CharacterController>();
            rBody = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            forwardInput = turnInput = 0;
        }


        void GetInput() {
            forwardInput = joystick.Vertical;
            turnInput = joystick.Horizontal; 
        }

        void Update() {
            GetInput();
            Turn();
        }

        void FixedUpdate() {
            Run();
        }

        void Run() {
            characterController.Move(transform.forward * forwardInput * forwardVel * Time.deltaTime * 0.4f);
            // rBody.velocity = transform.forward * forwardInput * forwardVel;
            animator.SetFloat("Speed", forwardInput);
        }

        void OnControllerColliderHit(ControllerColliderHit col) {
            //Debug.Log("HIT WITH CONTROLLER : " + col.gameObject.name);
        }

        void Turn() {
            targetRotation *= Quaternion.AngleAxis(rotateVel * turnInput * Time.deltaTime, Vector3.up);
            animator.SetFloat("TurnDirection", turnInput);
            transform.rotation = targetRotation;
            if (transform.position.y > 0.3f) {
                Vector3 pos = transform.position;
                pos.y -= 0.3f;
                transform.position = pos;
            }
        }

        public void ChangeSkin() {
            transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().material = otherMaterial;
            
        }  
        private void OnTriggerEnter(Collider other) {
            // -1 is not assigned ; 0 is no and ; 1 is in room
            Debug.Log("444: entered trigger: " + other.gameObject.name);
            if (inRoomState == -1) {
                if (other.gameObject.name == "Road") {
                    mainCam.GetComponent<EditingCameraFollow>().ChangeCameraViewpoint(true);
                    inRoomState = 1;
                }
            } else if (inRoomState == 0) {
                if (other.gameObject.name == "Road") {
                    mainCam.GetComponent<EditingCameraFollow>().ChangeCameraViewpoint(true);
                    inRoomState = 1;
                }
            } else {
                if (other.gameObject.name == "door") {
                    mainCam.GetComponent<EditingCameraFollow>().ChangeCameraViewpoint(false);
                    inRoomState = 0;
                }
            }
        }
}
