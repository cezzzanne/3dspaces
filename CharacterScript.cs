using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Voice.Unity;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.EventSystems;

namespace Spaces {
    public class CharacterScript : MonoBehaviourPun, IPunObservable, IPunInstantiateMagicCallback {
        Joystick joystick;
        Animator animator;
        public float inputDelay = 0.1f;
        public float forwardVel = 12;
        public float rotateVel = 100;
        Quaternion targetRotation;
        Rigidbody rBody;
        float forwardInput, turnInput = 0;

        private bool otherPlayer = false;

        GameObject mainCam;

        public int id;

        CharacterController characterController;

        Vector3 testPosition;

        Quaternion testRotation;

        public Material[] allSkins;

        private double lastUpdatetime = 0.0f;
        Vector3 prevPos;
        Quaternion prevRot;
        PhotonView PV;

        public GameObject glassedPrefab;
        private int inRoomState = -1;
        private int inPublicRoom;

        private GameObject ChatButton;

        private GameObject toolbar;

        public TouchScreenKeyboard keyboard;

        Transform inputTransform;
        
        private bool keyboardActive = false;
        
        public GameObject marker;
 
        string[] markerColors = new string[] {"153:0:0", "153:76:0", "153:153:0", "76:153:0", "0:153:153", "0:76:153", "76:0:153", "64:64:64"};

        MapPlayerScript mapPlayerScript;
        public Quaternion TargetRotation() {
            return transform.rotation;
        }
        void Awake() {
            inPublicRoom = PlayerPrefs.GetInt("isInPublicWorld");
            if (!photonView.IsMine) {
                otherPlayer = true;
                if (inPublicRoom == 0) {
                    int colorIndex = (PhotonNetwork.PlayerList.Length - 2) % 7; // 2 beause 1 is my player and the 2 is the new player
                    marker = Instantiate(marker);
                    string[] colorArray = markerColors[colorIndex].Split(':');
                    Debug.Log("888: " + colorIndex);
                    marker.GetComponent<Image>().color = new Color(float.Parse(colorArray[0]), float.Parse(colorArray[1]), float.Parse(colorArray[2]));
                    marker.transform.SetParent(GameObject.FindGameObjectWithTag("PlayerMap").transform);
                    marker.transform.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
                    marker.transform.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
                    mapPlayerScript = marker.GetComponent<MapPlayerScript>();
                }
            } else {
                mainCam = Resources.Load("Main Camera") as GameObject;
                mainCam = Instantiate(mainCam);
                PlayerFollow cameraScript = mainCam.GetComponent<PlayerFollow>();
                cameraScript.SetCameraTarget(transform);
                GameObject itemControllerObject = GameObject.FindGameObjectWithTag("ItemPlacementController") as GameObject;
                id = PhotonNetwork.PlayerList.Length - 1;
                // StartCoroutine(ChangeSkin());
                PV = transform.GetComponent<PhotonView>();
                PV.RPC("RPC_ChangeCharacterName", RpcTarget.AllBuffered, PlayerPrefs.GetString("username"), PV.ViewID);
                if (inPublicRoom == 1) {
                    GameObject.FindGameObjectWithTag("Canvas").GetComponent<InputHandler>().SetTarget(this);
                } else {
                    marker = Instantiate(marker);
                    marker.GetComponent<Image>().color = new Color(0, 153, 0);
                    marker.transform.SetParent(GameObject.FindGameObjectWithTag("PlayerMap").transform);
                    marker.transform.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
                    marker.transform.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
                    mapPlayerScript = marker.GetComponent<MapPlayerScript>();
                }

            }
        }

        public void SendNewMessage(string message) {
            PV.RPC("RPC_ReceiveMessage", RpcTarget.All, message, PV.ViewID);
        }

        void OpenKeyboard() {
            // need to instantiate input field by resource load
            joystick.gameObject.SetActive(false);
            keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false);   
            // Testing on unity editor    
            toolbar.SetActive(true);
            toolbar.transform.position = new Vector3(toolbar.transform.position.x, 15, toolbar.transform.position.z);
            inputTransform = toolbar.transform.GetChild(0);
            keyboardActive = true;
            EventSystem.current.SetSelectedGameObject(inputTransform.gameObject, null);
        }

        public bool OtherPlayer() {
            return otherPlayer;
        }

        public void DestroyMarker() {
            Destroy(marker);
        }

        void Start() {
            joystick = FindObjectOfType<Joystick>();
            targetRotation = transform.rotation;
            //rBody = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            forwardInput = turnInput = 0;
            characterController = transform.GetComponent<CharacterController>();
        }


        void GetInput() {
            if (otherPlayer) {
                // do something with photon
            } else {
                if (joystick == null) {
                    return;
                }
                forwardInput =  joystick.Vertical;
                turnInput = joystick.Horizontal; 
            }
        }

        void Update() {
            if (otherPlayer) {
                return;
            }
            GetInput();
            Turn();
        }


        void FixedUpdate() {
            if (otherPlayer) {
                return;
            }
            Run();
        }

        void LateUpdate() {
            if ((inPublicRoom == 0)) {
                mapPlayerScript.MoveMarker(transform.position.x / 1000f, transform.position.z / 1000);
            }
        }

        void Run() {
            Vector3 newPos = transform.position + (transform.forward * forwardInput * forwardVel * Time.deltaTime * 55);
            // transform.position = newPos; //Vector3.Lerp(transform.position, newPos, Time.deltaTime * 10f);
            characterController.Move(transform.forward * forwardInput * forwardVel);
            animator.SetFloat("Speed", forwardInput);
        }

        void Turn() {
            targetRotation *= Quaternion.AngleAxis(rotateVel * turnInput * Time.deltaTime, Vector3.up);
            animator.SetFloat("TurnDirection", turnInput);
            transform.rotation = targetRotation;//Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            if (transform.position.y > 0.3) {
                Vector3 pos = transform.position;
                pos.y -= 0.3f;
                transform.position = pos;
            }
        }


          public static void RefreshInstance(ref CharacterScript player, CharacterScript prefab ) {
              Vector3 pos;
              int publicWorld = PlayerPrefs.GetInt("isInPublicWorld");
              if (publicWorld == 1) {
                  pos = new Vector3(447.5852f, 0, 335.4253f);
              } else {
                if (PlayerPrefs.HasKey("editingPosition")) {
                    string[] positions = PlayerPrefs.GetString("editingPosition").Split(':');
                    pos = new Vector3(float.Parse(positions[0]), float.Parse(positions[1]) + 2, float.Parse(positions[2]));
                } else {
                    pos = new Vector3(365.3f, 0.0f, 438.7f);
                }
              }
            Quaternion rot = Quaternion.identity;
            if (player != null) {
                pos = player.transform.position;
                rot = player.transform.rotation;
                PhotonNetwork.Destroy(player.gameObject);
            }
            player = PhotonNetwork.Instantiate("Characters/" + prefab.gameObject.name, pos, rot).GetComponent<CharacterScript>();
        }
      


        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            if (stream.IsWriting) {
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
                stream.SendNext(forwardInput);
                stream.SendNext(turnInput);
            } else if (stream.IsReading) {
                Vector3 pos = (Vector3) stream.ReceiveNext();
                Quaternion rot = (Quaternion) stream.ReceiveNext();
                float forwardInp = (float) stream.ReceiveNext();
                float turnInp = (float) stream.ReceiveNext();
                if (prevPos == null) {
                    transform.position = pos;
                    transform.rotation = rot;
                    prevPos = pos;
                    prevRot = rot;
                } else {
                    transform.position = Vector3.Lerp(prevPos, pos, Time.deltaTime * (float)lastUpdatetime);
                    transform.rotation = Quaternion.Lerp(prevRot, rot, Time.deltaTime * (float)lastUpdatetime);
                    lastUpdatetime = info.SentServerTime;
                }
                if (animator) {
                    animator.SetFloat("Speed", forwardInp);
                    animator.SetFloat("TurnDirection", turnInp);
                }
            }
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info) {
            info.Sender.TagObject = transform.gameObject;
        }
        private void OnTriggerEnter(Collider other) {
            // -1 is not assigned ; 0 is no and ; 1 is in room
            if (otherPlayer) {
                return;
            }
            if (inRoomState == -1) {
                if (other.gameObject.name == "floorFull") {
                    mainCam.GetComponent<PlayerFollow>().ChangeCameraViewpoint(true);
                    inRoomState = 1;
                }
            } else if (inRoomState == 0) {
                if (other.gameObject.name == "floorFull") {
                    mainCam.GetComponent<PlayerFollow>().ChangeCameraViewpoint(true);
                    inRoomState = 1;
                }
            } else {
                if (other.gameObject.name == "door") {
                    mainCam.GetComponent<PlayerFollow>().ChangeCameraViewpoint(false);
                    inRoomState = 0;
                }
            }
        }

        public void DestroyCamera() {
            GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
            Destroy(camera);
        }

        [PunRPC]
        void RPC_ChangeCharacterName(string name, int pvID) {
            // 0 = private; 1 = public
            inPublicRoom = PlayerPrefs.GetInt("isInPublicWorld");
            Debug.Log("in public room: s" + inPublicRoom);
            GameObject nameCanvas;
            if (inPublicRoom == 0) {
                nameCanvas = PhotonView.Find(pvID).transform.GetChild(3).gameObject;
            } else {
                nameCanvas = PhotonView.Find(pvID).transform.GetChild(4).gameObject;
            }
            nameCanvas.SetActive(true);
            TMPro.TextMeshProUGUI playerName = nameCanvas.transform.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
            if (photonView.IsMine && (inPublicRoom == 0)) {
                Destroy(playerName);
                return;
            }

            playerName.text = "@" + name;
        }

        [PunRPC]
        void RPC_ReceiveMessage(string message, int pvID) {
            // 0 = private; 1 = public
            if (inPublicRoom == 0) {
                return;
            }
            TMPro.TextMeshProUGUI chatCanvas = PhotonView.Find(pvID).transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
            chatCanvas.text = message;
        }
    }
}
