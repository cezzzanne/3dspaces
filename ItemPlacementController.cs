using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System;

namespace Spaces {
    public class ItemPlacementController : MonoBehaviourPun {
        // Start is called before the first frame update
        [SerializeField]
        private List<GameObject> placeableList;

        private GameObject currentPlaceableObject;

        [SerializeField]
        private KeyCode newObjectHotKey = KeyCode.Z;

        private float currRotation = 0;

        Transform target;
        public Transform[] transformList;

        [SerializeField]
        private Button itemButton;

        [SerializeField]
        private GameObject buttonList;

        public GameObject fullItemList;

        public GameObject clearButton;

        public GameObject placeButton;

        public GameObject startEditingButton;

        private float maxWidthObject;
        private float characterWidth;

        public Button testButton;

        public GameObject terrain;


        private int indexPlaced;

        public SaveSystem saveSystem;

        private string myRoomID;

        public GameObject modifiedTerrain;

        private bool align = false;

        public GameObject characterList;
        public GameObject rotateButton;
        public GameObject goBackButton;
        public GameObject editAvatarButton;

        public GameObject uiManager;

        private UIManagerScript uiManagerScript;

        private Vector3 pastPostion;

        private float forwardInput, rightInput = 0;
        private float sideInput;

        private bool beingPressed, isForward, isReversed = false;

        public float speed = 1f;

        public GameObject SpeedIndicator;
        

        void Start() {
            myRoomID = PlayerPrefs.GetString("myRoomID");
            uiManagerScript = uiManager.GetComponent<UIManagerScript>();
            // int indexPlaced = 0;//PlayerPrefs.GetInt("CharacterSelected");
            // target = transformList[indexPlaced];
            // // string itemString = PlayerPrefs.GetString("CurrentItem");
            // // saveSystem.LoadSpace(myRoomID, modifiedTerrain, HandleNewObj);
        }

        public void SetTarget(Transform character) {
            target = character;
        }

        public void HandleNewObj(string item) {
            // Debug.Log("handling new object");
            // string itemString = PlayerPrefs.GetString("CurrentItem");
            // characterList.GetComponent<CharacterSelection>().SetPosition();
            // if (itemString != "") {
            //     GameObject prefab = Resources.Load<GameObject>("TownPrefabs/" + itemString);
            //     HandleNewObjectHotKey(prefab);
            // }
            GameObject prefab = Resources.Load<GameObject>(item);
            HandleNewObjectHotKey(prefab);
            
        }


        void Update() {
            // HandleNewObjectHotKey();
            if (currentPlaceableObject != null) {
                // MoveCurrentPlaceableObject();
                RotateOnKey();
                // ReleaseIfClicked();
            }
        }

        void FixedUpdate() {
            if (currentPlaceableObject != null) {
                forwardInput = 0;
                rightInput = 0;
                if (beingPressed) {
                    if (isForward) {
                        forwardInput = speed;
                        if (isReversed) {
                            forwardInput = -speed;
                        }
                    } else {
                        rightInput = speed;
                        if (isReversed) {
                            rightInput = -speed;
                        }
                    }
                }
                MoveCurrentPlaceableObject();
            }
        }

        public void SetSpeed() {
            if (speed == 1f) {
                speed = 2f;
                SpeedIndicator.transform.GetChild(0).gameObject.SetActive(false);
                SpeedIndicator.transform.GetChild(1).gameObject.SetActive(true);
            } else if (speed == 2f) {
                speed = 2.5f;
                SpeedIndicator.transform.GetChild(1).gameObject.SetActive(false);
                SpeedIndicator.transform.GetChild(2).gameObject.SetActive(true);
            } else {
                speed = 1f;
                SpeedIndicator.transform.GetChild(2).gameObject.SetActive(false);
                SpeedIndicator.transform.GetChild(0).gameObject.SetActive(true);

            }
        }

        public void ReleaseIfClicked() {
            // foreach(Material m in currentPlaceableObject.GetComponentInChildren<MeshRenderer>().materials) {
            //         m.shader = Shader.Find("Standard (Specular setup)");
            // }
            currentPlaceableObject.GetComponent<Rigidbody>().isKinematic = true;
            currentPlaceableObject.GetComponent<Rigidbody>().freezeRotation = true;
            currentPlaceableObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
            currentPlaceableObject.transform.SetParent(terrain.transform);
            if (currentPlaceableObject.gameObject.name == "000ModernHouse(Clone)") {
                currentPlaceableObject.transform.position = new Vector3(currentPlaceableObject.transform.position.x, 0, currentPlaceableObject.transform.position.z);
            }
            float xPos = currentPlaceableObject.transform.position.x;
            float yPos = currentPlaceableObject.transform.position.y;
            float zPos= currentPlaceableObject.transform.position.z;
            float yRot = currentPlaceableObject.transform.eulerAngles.y;
            Dictionary<string, object> objectData = new Dictionary<string, object> {
                {"xPos", xPos},
                {"yPos", yPos},
                {"zPos", zPos},
                {"yRot", yRot},
                {"name", currentPlaceableObject.name}
            };
            target.gameObject.SetActive(true);
            target.GetComponent<CharacterScript>().SetMainCamEditing(target, false);
            currentPlaceableObject = null;
            uiManagerScript.PlacedItem();
            saveSystem.SaveSpace(objectData, target, int.Parse(myRoomID));
        }

        private void RotateOnKey() {
            // if (Input.GetKeyDown(KeyCode.C)) {
            //     currRotation = 2;
            //     currentPlaceableObject.transform.Rotate(Vector3.up, currRotation * 2f);
            // }
        }

        public void Rotate() {
            Debug.Log("rotating");
            currentPlaceableObject.transform.rotation = Quaternion.Euler(currentPlaceableObject.transform.eulerAngles.x, currentPlaceableObject.transform.eulerAngles.y + 10, currentPlaceableObject.transform.eulerAngles.z);
        }

        private void MoveCurrentPlaceableObject() {
            Collider[] colliders = Physics.OverlapBox(currentPlaceableObject.transform.position, currentPlaceableObject.transform.localScale / 2);
            bool contactWithCollider = false;
            if (colliders.Length > 0) {
                Collider collider = null;
                for (int i = 0; i < colliders.Length; i++) {
                    if (colliders[i].gameObject.name != "MainGame-Terrain(Clone)" && colliders[i].gameObject != currentPlaceableObject.gameObject) {
                        contactWithCollider = true;
                        if (collider != null) {
                            Collider temp = colliders[i];
                            collider = (collider.bounds.center.y + collider.bounds.extents.y + collider.transform.position.y) >= (temp.bounds.center.y + temp.bounds.extents.y + temp.transform.position.y) ? collider : temp;
                        } else {
                            collider = colliders[i];
                        }
                    }
                }
                if (contactWithCollider && !collider.isTrigger) {
                    Vector3 pos = currentPlaceableObject.transform.position;
                    // pos.y = (collider.bounds.center.y + collider.bounds.extents.y + collider.transform.position.y - 0.0001f);
                    float length = collider.transform.localScale.x * ((BoxCollider)collider).size.x;
                    float width = collider.transform.localScale.z * ((BoxCollider)collider).size.z;
                    float height = collider.transform.localScale.y * ((BoxCollider)collider).size.y;
                    Vector3 dimensions = new Vector3(length, height, width);

                    //now to know the world position of top most level of the wall:
                    float topMost = collider.transform.position.y + dimensions.y ;
                    pos.y = topMost;
                    currentPlaceableObject.transform.position = pos;  
                    currentPlaceableObject.transform.position = currentPlaceableObject.transform.position + (currentPlaceableObject.transform.forward * 0.1f * forwardInput);
                    currentPlaceableObject.transform.position = currentPlaceableObject.transform.position + (currentPlaceableObject.transform.right * 0.1f * rightInput);
                } else {
                    // currentPlaceableObject.transform.position = target.position + (target.forward* ((maxWidthObject * 1.2f)  + characterWidth + 0.5f));
                    Vector3 pos = currentPlaceableObject.transform.position;
                    pos.y = 0;
                    currentPlaceableObject.transform.position = pos;
                    currentPlaceableObject.transform.position = currentPlaceableObject.transform.position + (currentPlaceableObject.transform.forward * 0.1f * forwardInput);
                    currentPlaceableObject.transform.position = currentPlaceableObject.transform.position + (currentPlaceableObject.transform.right * 0.1f * rightInput);
                }  
            } else {
                Vector3 pos = currentPlaceableObject.transform.position;
                pos.y = 0;
                currentPlaceableObject.transform.position = pos;
                currentPlaceableObject.transform.position = currentPlaceableObject.transform.position + (currentPlaceableObject.transform.forward * 0.1f * forwardInput);
                currentPlaceableObject.transform.position = currentPlaceableObject.transform.position + (currentPlaceableObject.transform.right * 0.1f * rightInput);

            }
        }

        public void SetForwardInput(bool pressed, bool forward, bool reversed) {
            beingPressed = pressed;
            isForward = forward;
            isReversed = reversed;
        }

        public void HandleNewObjectHotKey(GameObject prefab) {
            // when a button presses I can hardcode what number each item is (find more elegant way) 
            if (currentPlaceableObject == null) {
                    currentPlaceableObject = Instantiate(prefab) as GameObject;
                    Rigidbody rBody = currentPlaceableObject.GetComponent<Rigidbody>();
                    rBody.useGravity = false;
                    rBody.isKinematic = true;
                    BoxCollider bCollider = currentPlaceableObject.GetComponent<BoxCollider>();
                    MeshRenderer meshRenderer = currentPlaceableObject.GetComponentInChildren<MeshRenderer>();
                    Transform tr = currentPlaceableObject.transform;
                    maxWidthObject = bCollider.bounds.size.x >= bCollider.bounds.size.z ? bCollider.bounds.size.x : bCollider.bounds.size.z;
                    characterWidth = target.GetComponent<CapsuleCollider>().bounds.size.x;
                    Vector3 tempPos = target.position;
                    target.gameObject.SetActive(false);
                    target.GetComponent<CharacterScript>().SetMainCamEditing(currentPlaceableObject.transform, true); //  new
                    currentPlaceableObject.transform.position = tempPos; //+ (target.forward * ((maxWidthObject * 1.2f)  + characterWidth + 0.5f)); //target.position + (target.forward * ((maxWidthObject * 1.2f)  + characterWidth + 0.5f));
                    // foreach(Material m in meshRenderer.materials) {
                    //     m.shader = Shader.Find("Unlit/Transparent Cutout");
                    // }
                } else {
            }
        }

        public void RemoveCurrentPlaceableObject() {
            Destroy(currentPlaceableObject);
            uiManagerScript.PlacedItem();
            target.gameObject.SetActive(true);
            target.GetComponent<CharacterScript>().SetMainCamEditing(target, false);
            // PlayerPrefs.DeleteKey("CurrentItem");
            // ShowListOfObjects();
        }

        public void SetTerrain(GameObject ter) {
            Debug.Log("zzzz");
            terrain = ter;
        }

        public void ShowListOfObjects() {
            // saveSystem.SaveSpace(terrain, target, indexPlaced);
            clearButton.SetActive(false);
            placeButton.SetActive(false);
            startEditingButton.SetActive(false);
            PlayerPrefs.SetString("editingPosition", target.position.x + ":" + target.position.y + ":" + target.position.z);
            Debug.Log("set editing pos :  " + PlayerPrefs.GetString("editingPosition"));
            SceneManager.LoadScene("ItemSelection");
        }

        public void hideListOfObjects() {
            fullItemList.SetActive(false);
        }

        public void GoBack() {
            PlayerPrefs.SetString("editingPosition", target.position.x + ":" + target.position.y + ":" + target.position.z);
            SceneManager.LoadScene("MainGame");
        }

        void OnDestroy() {
        }
    }
}
