﻿using System.Collections;
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

        public GameObject mainCam;


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

        private GameObject itemLoader;

        private bool myRoom;
        private bool wardrobeOpen = false;
        MapPlayerScript mapPlayerScript;

        private string username, roomID, myRoomID, accessories;

        public List<GameObject> previousAccesories;

        public Dictionary<int, List<GameObject>> friendsPreviousAccessories;
        public Quaternion TargetRotation() {
            return transform.rotation;
        }
        void Awake() {
            inPublicRoom = PlayerPrefs.GetInt("isInPublicWorld");
            friendsPreviousAccessories = new Dictionary<int, List<GameObject>>();
            if (!photonView.IsMine) {
                otherPlayer = true;
            } else {
                mainCam = Resources.Load("Main Camera") as GameObject;
                mainCam = Instantiate(mainCam);
                PlayerFollow cameraScript = mainCam.GetComponent<PlayerFollow>();
                cameraScript.SetCameraTarget(transform, inPublicRoom);
                GameObject itemControllerObject = GameObject.FindGameObjectWithTag("ItemPlacementController") as GameObject;
                PV = transform.GetComponent<PhotonView>();
                username = PlayerPrefs.GetString("username");
                accessories = PlayerPrefs.GetString("Accessories");
                string tats = PlayerPrefs.GetString("Asdfad");
                previousAccesories = new List<GameObject>();
                // SetCharacterAccessories(accessories, previousAccesories, transform);
                PV.RPC("RPC_ChangeCharacterName", RpcTarget.AllBuffered, username, PV.ViewID);
                PV.RPC("RPC_SetCharacterAccessories", RpcTarget.AllBuffered, accessories, PV.ViewID);
                itemLoader = GameObject.FindGameObjectWithTag("ItemLoader");
                if (inPublicRoom == 1) {
                    GameObject.FindGameObjectWithTag("Canvas").GetComponent<InputHandler>().SetTarget(this);
                    itemLoader.GetComponent<ItemLoaderStore>().SetCamera(mainCam.GetComponent<PlayerFollow>());
                } else {
                    roomID = PlayerPrefs.GetString("currentRoomID");
                    myRoomID = PlayerPrefs.GetString("myRoomID");
                    myRoom = roomID == myRoomID;
                    if (myRoom) {
                        GameObject itemController = GameObject.FindGameObjectWithTag("ItemPlacementController");
                        itemController.GetComponent<ItemPlacementController>().SetTarget(transform);
                        itemLoader.GetComponent<ItemLoader>().SetCamera(mainCam.GetComponent<PlayerFollow>());
                        CharacterChange charChange = GameObject.FindGameObjectWithTag("CharacterChange").GetComponent<CharacterChange>();
                        charChange.SetTargetCharacter(this);
                        charChange.UpdateAccessories(accessories);
                        GameObject notificationManager = GameObject.FindGameObjectWithTag("NotificationManager");
                        notificationManager.GetComponent<InnerNotifManagerScript>().SetCharacterTarget(transform, username, myRoomID);
                    }
                }
            }
        }

        void SetCharacterAccessories(string accss, List<GameObject> prevAccss, Transform character) {
            string accessories = accss;
            // using same names as local variables - ignore that
            if (!accessories.Contains("$")) {
                if (accessories != "") {
                    // one accessory
                    string[] accessoryAttribute = accessories.Split('-');
                    string location = accessoryAttribute[3];
                    string bodyLocation = accessoryAttribute[0];
                    GameObject acc = Resources.Load<GameObject>("Characters/Accessories/" + location);
                    acc = Instantiate(acc);
                    prevAccss.Add(acc);
                    Transform parent = character.Find(bodyLocation);
                    AllocateAccessory(acc.transform, parent);
                } else {
                    // no accessories
                    return;
                }
            } else {
                // multiple accessories
                string[] allAccessories = accessories.Split('$');
                foreach(string accessory in allAccessories) {
                    string[] accessoryAttribute = accessory.Split('-');
                    string location = accessoryAttribute[3];
                    string bodyLocation = accessoryAttribute[0];
                    GameObject acc = Resources.Load<GameObject>("Characters/Accessories/" + location);
                    acc = Instantiate(acc);
                    prevAccss.Add(acc);
                    Transform parent = character.Find(bodyLocation);
                    AllocateAccessory(acc.transform, parent);
                }
            }
            // Move store to first tent
        }

        public void UpdateMyAccessories(string newAccessories) {
            PV.RPC("RPC_UpdateCharacterAccessories", RpcTarget.AllBuffered, newAccessories, PV.ViewID);
        }

        public void UpdateCharacterAccessories(string newAccessories, List<GameObject> prevAcc, Transform character) {
            DestroyPreviousAccessories(prevAcc);
            if (!newAccessories.Contains("$")) {
                if (newAccessories != "") {
                    // one accessory
                    // 0 is the Roots/... -- 1 is the Name -- 2 is the Type (Cap) -- 3 is the GameObject name (astroBackpack)
                    string[] accessoryAttribute = newAccessories.Split('-');
                    string location = accessoryAttribute[3];
                    string bodyLocation = accessoryAttribute[0];
                    GameObject acc = Resources.Load<GameObject>("Characters/Accessories/" + location);
                    acc = Instantiate(acc);
                    prevAcc.Add(acc);
                    Transform parent = character.Find(bodyLocation);
                    AllocateAccessory(acc.transform, parent);
                } else {
                    return;
                }
            } else {
                string[] allAccessories = newAccessories.Split('$');
                foreach(string accessory in allAccessories) {
                    string[] accessoryAttribute = accessory.Split('-');
                    string location = accessoryAttribute[3];
                    string bodyLocation = accessoryAttribute[0];
                    GameObject acc = Resources.Load<GameObject>("Characters/Accessories/" + location);
                    acc = Instantiate(acc);
                    prevAcc.Add(acc);
                    Transform parent = character.Find(bodyLocation);
                    AllocateAccessory(acc.transform, parent);
                }
            }
        }

        void DestroyPreviousAccessories(List<GameObject> prevAccess) {
            foreach(GameObject accessory in prevAccess) {
                RemoveAccessory(accessory);
            }
        }

        public void RemoveAccessory(GameObject accessory) {
            Destroy(accessory);
        }

        public void AllocateAccessory(Transform child, Transform parent) {
            Vector3 pos = child.position;
            Quaternion rot = child.rotation;
            Vector3 scale = child.localScale;
            child.parent = parent;
            child.localPosition = pos;
            child.localRotation = rot;
            child.localScale = scale;
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
                  GameObject moonWorld = GameObject.FindGameObjectWithTag("MoonWorld");
                  if (moonWorld == null) {
                    pos = new Vector3(447.5852f, 0, 335.4253f);
                  } else {
                    pos = new Vector3(2, 0, 4);
                  }
              } else {
                pos = new Vector3(2, 0, 2);
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
            if (inRoomState == -1 && other.gameObject.name == "door") {
                mainCam.GetComponent<PlayerFollow>().ChangeCameraViewpoint(true);
                inRoomState = 1;
            } else if (inRoomState == 1 && other.gameObject.name == "door") {
                mainCam.GetComponent<PlayerFollow>().ChangeCameraViewpoint(false);
                inRoomState = -1;
            } else if (other.gameObject.name == "store") {
                itemLoader.GetComponent<ItemLoaderStore>().ActivateStore(true);
            } else if (other.gameObject.name == "Wardrobe" && myRoom) {
                itemLoader.GetComponent<ItemLoader>().ToggleWardrobe(true);
            } else if (other.gameObject.name == "toggleCameraFloor") {
                mainCam.GetComponent<PlayerFollow>().ZoomInPlayer();
            }
        }


        // JUST LOAD APPROPRIATE ARTIFACTS FOR THIS WORLD TO SELL AND CHANGE THE ITEM TERRAIN TO A MOON-TYPE TERRAIN



        public void ChangeSkin(Material newMat) {
            transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().material = newMat;
            PV.RPC("RPC_SkinChange", RpcTarget.AllBuffered, newMat.name, PV.ViewID);
        }

        private void OnTriggerExit(Collider other) {
            if (otherPlayer) {
                return;
            }
            if (other.gameObject.name == "store") {
                itemLoader.GetComponent<ItemLoaderStore>().ActivateStore(false);
            } else if (other.gameObject.name == "Wardrobe" && myRoom) {
                itemLoader.GetComponent<ItemLoader>().ToggleWardrobe(false);
            } else if (other.gameObject.name == "toggleCameraFloor") {
                mainCam.GetComponent<PlayerFollow>().ZoomOutPlayer();
            }
        }

        public void DestroyCamera() {
            GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
            Destroy(camera);
        }

        public void SetMainCamEditing(Transform placeableObject, bool isPlacingItem) {
            mainCam.GetComponent<PlayerFollow>().NowFollowing(placeableObject, isPlacingItem);
        }

        [PunRPC]
        void RPC_ChangeCharacterName(string name, int pvID) {
            // 0 = private; 1 = public
            inPublicRoom = PlayerPrefs.GetInt("isInPublicWorld");
            GameObject nameCanvas;
            if (inPublicRoom == 0) {
                nameCanvas = PhotonView.Find(pvID).transform.GetChild(3).gameObject;
            } else {
                nameCanvas = PhotonView.Find(pvID).transform.GetChild(4).gameObject;
            }
            nameCanvas.SetActive(true);
            TMPro.TextMeshProUGUI playerName = nameCanvas.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
            if (photonView.IsMine && (inPublicRoom == 0)) {
                Destroy(nameCanvas);
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
            TMPro.TextMeshProUGUI chatCanvas = PhotonView.Find(pvID).transform.GetChild(4).GetChild(1).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
            chatCanvas.text = message;
        }

        [PunRPC]
        void RPC_SkinChange(string skinName, int pvID) {
            Material material = Resources.Load<Material>("Characters/Materials/" + skinName) as Material;
            PhotonView.Find(pvID).transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().material = material;
        }


        [PunRPC]
        void RPC_SetCharacterAccessories(string baseAcc, int pvID) {
            string accessories = baseAcc;
            List<GameObject> previousAccesories = new List<GameObject>();
            friendsPreviousAccessories.Add(pvID, previousAccesories);
            SetCharacterAccessories(baseAcc, previousAccesories, PhotonView.Find(pvID).transform);
        }

        [PunRPC]
        public void RPC_UpdateCharacterAccessories(string newAccessories, int pvID) {
            List<GameObject> prevAcc = friendsPreviousAccessories[pvID];
            UpdateCharacterAccessories(newAccessories, prevAcc, PhotonView.Find(pvID).transform);
        }
    }
}
