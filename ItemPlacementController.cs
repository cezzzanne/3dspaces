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
                MoveCurrentPlaceableObject();
            }
        }

        public void ReleaseIfClicked() {
            foreach(Material m in currentPlaceableObject.GetComponentInChildren<MeshRenderer>().materials) {
                    m.shader = Shader.Find("Standard (Specular setup)");
            }
            currentPlaceableObject.GetComponent<Rigidbody>().isKinematic = true;
            currentPlaceableObject.GetComponent<Rigidbody>().freezeRotation = true;
            currentPlaceableObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
            currentPlaceableObject.transform.SetParent(terrain.transform);
            if (currentPlaceableObject.gameObject.name == "000ModernHouse(Clone)") {
                Debug.Log("is clone");
                currentPlaceableObject.transform.position = new Vector3(currentPlaceableObject.transform.position.x, 0, currentPlaceableObject.transform.position.z);
            }
            currentPlaceableObject = null;
            // startEditingButton.SetActive(true);
            // clearButton.SetActive(false);
            // placeButton.SetActive(false);
            // goBackButton.SetActive(true);
            // rotateButton.SetActive(false);
            // editAvatarButton.SetActive(true);
            // PlayerPrefs.DeleteKey("CurrentItem");
            uiManagerScript.PlacedItem();
            target.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            saveSystem.SaveSpace(terrain, target, int.Parse(myRoomID));
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
                    if (colliders[i].gameObject.name != "MainGame-Terrain(Clone)" && colliders[i].gameObject != currentPlaceableObject.gameObject && colliders[i].gameObject.name != "Small-House(Clone)") {
                        contactWithCollider = true;
                        if (collider != null) {
                            Collider temp = colliders[i];
                            Debug.Log("changing collider ----------");
                            collider = (collider.bounds.center.y + collider.bounds.extents.y + collider.transform.position.y) >= (temp.bounds.center.y + temp.bounds.extents.y + temp.transform.position.y) ? collider : temp;
                        } else {
                            collider = colliders[i];
                        }
                    }
                }
                if (contactWithCollider && !collider.isTrigger) {
                    Debug.Log("COLLIDING WITH : " + collider.gameObject.name);
                    Vector3 pos = currentPlaceableObject.transform.position;
                    pos = target.position + (target.forward * ((maxWidthObject * 1.2f)  + characterWidth + 0.5f));
                    // pos.y = (collider.bounds.center.y + collider.bounds.extents.y + collider.transform.position.y - 0.0001f);
                    float length = collider.transform.localScale.x * ((BoxCollider)collider).size.x;
                    float width = collider.transform.localScale.z * ((BoxCollider)collider).size.z;
                    float height = collider.transform.localScale.y * ((BoxCollider)collider).size.y;
                    Vector3 dimensions = new Vector3(length, height, width);

                    //now to know the world position of top most level of the wall:
                    float topMost = collider.transform.position.y + dimensions.y ;
                    pos.y = topMost;
                    currentPlaceableObject.transform.position = pos;  
                } else {
                    Debug.Log("Hitting floor");
                    currentPlaceableObject.transform.position = target.position + (target.forward * ((maxWidthObject * 1.2f)  + characterWidth + 0.5f));
                }  
            } else {
                currentPlaceableObject.transform.position = target.position + (target.forward * ((maxWidthObject * 1.2f)  + characterWidth + 0.5f));
            }
        }

        public void HandleNewObjectHotKey(GameObject prefab) {
            // when a button presses I can hardcode what number each item is (find more elegant way) 
            if (currentPlaceableObject == null) {
                    // startEditingButton.SetActive(false);
                    // goBackButton.SetActive(false);
                    // clearButton.SetActive(true);
                    // placeButton.SetActive(true);
                    // rotateButton.SetActive(true);
                    // editAvatarButton.SetActive(false);
                    // hideListOfObjects();
                    currentPlaceableObject = Instantiate(prefab) as GameObject;
                    Rigidbody rBody = currentPlaceableObject.GetComponent<Rigidbody>();
                    // rBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                    // rBody.mass = 10;
                    // rBody.drag = 10;
                    // rBody.angularDrag = 0.99f;
                    rBody.useGravity = false;
                    rBody.isKinematic = true;
                    // currentPlaceableObject.transform.localScale = currentPlaceableObject.transform.localScale * 3;
                    BoxCollider bCollider = currentPlaceableObject.GetComponent<BoxCollider>();
                    MeshRenderer meshRenderer = currentPlaceableObject.GetComponentInChildren<MeshRenderer>();
                    Transform tr = currentPlaceableObject.transform;
                    // maxWidthObject =  (tr.localScale.x ) >= (tr.localScale.z )  ? (tr.localScale.x ) : (tr.localScale.z);
                    maxWidthObject = bCollider.bounds.size.x >= bCollider.bounds.size.z ? bCollider.bounds.size.x : bCollider.bounds.size.z;
                    // Debug.Log(currentPlaceableObject.name.Substring(0, stringLength));
                    characterWidth = target.GetComponent<CapsuleCollider>().bounds.size.x;
                    // target.localScale = new Vector3(0, 0.5f, 0);
                    Vector3 tempPos = target.position;
                    tempPos.y = 0;
                    currentPlaceableObject.transform.position = target.position + (target.forward * ((maxWidthObject * 1.2f)  + characterWidth + 0.5f));
                    foreach(Material m in meshRenderer.materials) {
                        m.shader = Shader.Find("Unlit/Transparent Cutout");
                    }
                } else {
            }
        }

        public void RemoveCurrentPlaceableObject() {
            Destroy(currentPlaceableObject);
            uiManagerScript.PlacedItem();
            // PlayerPrefs.DeleteKey("CurrentItem");
            // ShowListOfObjects();
        }

        public void SetTerrain(GameObject ter) {
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
