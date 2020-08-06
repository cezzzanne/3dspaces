using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Voice.Unity;
using UnityEngine.UI;
using UnityEditor;
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



        public Quaternion TargetRotation() {
            return transform.rotation;
        }
        void Awake() {
            if (!photonView.IsMine) {
                otherPlayer = true;
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
                // transform.GetComponentInChildren<Canvas>().GetComponentInChildren<Text>().transform.Rotate(new Vector3(40, 180, 0), Space.Self);
            }
        }

        // IEnumerator ChangeSkin() {
            // yield return new WaitForSeconds(1);
            // GameObject glasses = Instantiate(glassedPrefab) as GameObject;
            // glasses.transform.parent = transform;
            // glasses.transform.localPosition = glasses.transform.position;
            // glasses.transform.localScale = glasses.transform.lossyScale;
            // GameObject[] characters = Resources.LoadAll<GameObject>("Characters");
            // for (int i = 0; i < 3; i++) {
            //     GameObject newText = Instantiate(usernameText);
            //     newText.transform.SetParent(characters[i].transform);
            //     PrefabUtility.SaveAsPrefabAsset(characters[i], "Assets/Resources/Characters/" + characters[i].name +  ".prefab");
            // }
            // string assetPath =  "Assets/Resources/characterMedium.prefab";
            // GameObject contentsRoot = PrefabUtility.LoadPrefabContents(assetPath);
            // gameObject.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().material = mate;
            // PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, "Assets/Resources/newPrefab.prefab", InteractionMode.UserAction);
            // PrefabUtility.UnloadPrefabContents(game);
        // }

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
              if (PlayerPrefs.HasKey("editingPosition")) {
                string[] positions = PlayerPrefs.GetString("editingPosition").Split(':');
                pos = new Vector3(float.Parse(positions[0]), float.Parse(positions[1]), float.Parse(positions[2]));
              } else {
                  pos = new Vector3(365.3f, 0.0f, 438.7f);
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
            TMPro.TextMeshProUGUI playerName = PhotonView.Find(pvID).transform.GetChild(3).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
            if (photonView.IsMine) {
                Destroy(playerName);
                return;
            }
            playerName.text = "@" + name;
        }
    }
}
