using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Spaces {
    public class ItemLoader : MonoBehaviour {
        // Start is called before the first frame update
        PlayerFollow mainCam;

        List<GameObject> prefabList;
        List<string> itemNames;

        private Transform ObjectHolder;

        private int currIndex = 0;

        public GameObject nextItemButton;
        public GameObject confirmItemButton;
        private bool inEditing = false;
        public GameObject itemController;

        public GameObject uiManager;

        private UIManagerScript uiManagerScript;

        public GameObject EditWorldButton;

        private List<StoreItem> storeDataObjects;
        private List<StoreItem> storeDataSkins;
        private bool browsingPurchased = false;

        private int purchasedItemsIndex = 0;

        private GameObject currentItem;

        public GameObject CharacterChange;

        private CharacterChange CharacterChangeScript;

        void Start() {
            ObjectHolder = transform.GetChild(0);
            uiManagerScript = uiManager.GetComponent<UIManagerScript>();
            CharacterChangeScript = CharacterChange.GetComponent<CharacterChange>();
            string roomID = PlayerPrefs.GetString("myRoomID");
            if (PlayerPrefs.GetString("currentRoomID") == roomID) {
                StartCoroutine(LoadPurchasedItems(roomID));
            }
        }

        public void BrowsePurchasedItems() {
            if (storeDataObjects.Count > 0) {
                browsingPurchased = true;
                uiManagerScript.ToggleBrowsing(browsingPurchased);
                prefabList[currIndex].SetActive(false);
                LoadItem();
            }
        }

        public void BrowseRegularItems() {
            browsingPurchased = false;
            uiManagerScript.ToggleBrowsing(browsingPurchased);
            Destroy(currentItem);
            prefabList[currIndex].SetActive(true);
            currentItem = prefabList[currIndex];
            FitCamera();
        }


        public IEnumerator LoadPurchasedItems(string roomID) {
            if (storeDataObjects == null) {
                WWWForm form = new WWWForm();
                Debug.Log("GOING INTO REQUEST");
                form.AddField("userID", roomID);
                form.AddField("storeType", 0);
                UnityWebRequest www = UnityWebRequest.Post("https://circles-parellano.herokuapp.com/api/get-purchased-items", form);
                yield return www.SendWebRequest();
                if(www.isNetworkError || www.isHttpError) {
                    Debug.Log(www.error);
                }
                else {
                    string response = www.downloadHandler.text;
                    yield return response;
                    Debug.Log("purchasedItems: " + response);
                    storeDataObjects = new List<StoreItem>();
                    storeDataSkins = new List<StoreItem>();
                    StoreResponse fullData = JsonUtility.FromJson<StoreResponse>(response);
                    foreach(StoreItem item in fullData.data) {
                        if (item.type == "skin") {
                            CharacterChangeScript.AddToAvailableSkins(item);
                        } else {
                            Debug.Log("7878: adding object : " +  item);
                            storeDataObjects.Add(item);
                        }
                    }
                    uiManagerScript.ActivateEditing();
                }
            }
        }


    public void LoadItem() {
        Debug.Log("indexx: " + purchasedItemsIndex);
        StoreItem item = storeDataObjects[purchasedItemsIndex];
        GameObject currentAsset = Resources.Load<GameObject>("StoreItems/" + item.location) as GameObject;
        GameObject instPrefab = Instantiate(currentAsset);
        instPrefab.transform.SetParent(ObjectHolder);
        instPrefab.transform.localPosition = new Vector3(0, 0, 0);
        float width = instPrefab.GetComponent<BoxCollider>().size.x * Screen.width/ Screen.height; // basically height * screen aspect ratio
        instPrefab.transform.localScale = Vector3.one * width / 4f;
        instPrefab.transform.localScale = instPrefab.transform.localScale * (1f / instPrefab.GetComponent<BoxCollider>().size.x);
        instPrefab.transform.Rotate(new Vector3(-20, 0, 0), Space.Self);
        instPrefab.SetActive(true);
        currentItem = instPrefab;
        FitCamera();
    }


        public void GoToItemSelection() {
            if (mainCam == null) {
                return;
            }
            mainCam.ToggleItemLoader();
            if (prefabList == null) {
                LoadItems();
            } else {
                if (!inEditing) {
                    Debug.Log("111: fitting camera");
                    FitCamera();
                }
            }
            inEditing = !inEditing;
            uiManagerScript.ToggleEditing();
        }

        public void SetCamera(PlayerFollow cam) {
            mainCam = cam;
        }

        public void ToggleWardrobe(bool open) {
            uiManagerScript.ToggleWardrobe(open);
        }

        public void ToggleCharacterChange() {
            if (mainCam == null) {
                return;
            }
            mainCam.ToggleCharacterChange();
            inEditing = !inEditing;
            uiManagerScript.ToggleCharacterChange();
        }


        void LoadItems() {
            prefabList = new List<GameObject>();
            itemNames = new List<string>();
            GameObject[] assetsList = Resources.LoadAll<GameObject>("TownPrefabs");
            int maxObjects = assetsList.Length;
            for(int i = 0; i < maxObjects; i++) {
                GameObject currentAsset = assetsList[i] as GameObject;
                GameObject instPrefab = Instantiate(currentAsset);
                int x = i;
                prefabList.Insert(x, instPrefab);
                itemNames.Insert(x, instPrefab.name.Substring(0, instPrefab.name.Length - 7));
                instPrefab.SetActive(false);
                instPrefab.transform.SetParent(ObjectHolder);
                instPrefab.transform.localPosition = new Vector3(0, 0, 0);
                float width = instPrefab.GetComponent<BoxCollider>().size.x * Screen.width/ Screen.height; // basically height * screen aspect ratio
                instPrefab.transform.localScale = Vector3.one * width / 4f;
                instPrefab.transform.localScale = instPrefab.transform.localScale * (1f / instPrefab.GetComponent<BoxCollider>().size.x);
                instPrefab.transform.Rotate(new Vector3(-20, 0, 0), Space.Self);
            }
            prefabList[currIndex].SetActive(true);
            currentItem = prefabList[currIndex];
            // objectTitle.GetComponent<Text>().text = itemNames[currIndex];
            FitCamera();
        }

        void FitCamera() {
            // objectTitle.GetComponent<Text>().text = itemNames[currIndex];
            transform.rotation = Quaternion.Euler(0, 0, 0);
            Bounds itemBounds = currentItem.GetComponent<BoxCollider>().bounds;
            float cameraDistance = 4.0f; // Constant factor
            Vector3 objectSizes = itemBounds.max - itemBounds.min;
            float objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);
            float cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * mainCam.gameObject.GetComponent<Camera>().fieldOfView); // Visible height 1 meter in front
            float distance = cameraDistance * objectSize / cameraView; // Combined wanted distance from the object
            distance += 0.5f * objectSize; // Estimated offset from the center to the outside of the object
            if (browsingPurchased) {
                mainCam.gameObject.transform.position = currentItem.transform.position - (distance * 0.5f) * mainCam.transform.forward;
            } else {
                mainCam.gameObject.transform.position = itemBounds.center - distance * mainCam.transform.forward;
            }
        }

        public void NextItem() {
            int count;
            int ind;
            if (browsingPurchased) {
                count = storeDataObjects.Count;
                ind = purchasedItemsIndex;
            } else {
                count = prefabList.Count;
                ind = currIndex;
            }
            int newIndex = ind + 1;
            if (newIndex >  count - 1) {
                newIndex = 0;
            }
            if (browsingPurchased) {
                Destroy(currentItem);
                purchasedItemsIndex = newIndex;
                LoadItem();
            } else {
                prefabList[currIndex].SetActive(false);
                prefabList[newIndex].SetActive(true);
                currIndex = newIndex;
                currentItem = prefabList[currIndex];
                FitCamera();
            }
        }

        public void PrevItem() {
            int count;
            int ind;
            if (browsingPurchased) {
                count = storeDataObjects.Count;
                ind = purchasedItemsIndex;
            } else {
                count = prefabList.Count;
                ind = currIndex;
            }
            int newIndex = ind - 1;
            if (newIndex < 0) {
                newIndex = count - 1;
            }
            if (browsingPurchased) {
                Destroy(currentItem);
                purchasedItemsIndex = newIndex;
                LoadItem();
            } else {
                prefabList[currIndex].SetActive(false);
                prefabList[newIndex].SetActive(true);
                currIndex = newIndex;
                currentItem = prefabList[currIndex];
                FitCamera();
            }
        }

        public void ConfirmItem() {
            inEditing = false;
            mainCam.ToggleItemLoader();
            uiManagerScript.IsPlacingItem();
            string itemLoc = (!browsingPurchased) ? "TownPrefabs/" + itemNames[currIndex] : "StoreItems/" + storeDataObjects[purchasedItemsIndex].location;
            itemController.GetComponent<ItemPlacementController>().HandleNewObj(itemLoc);
        }

        public void CancelEditing() {
            inEditing = false;
            uiManagerScript.ToggleEditing();
            mainCam.ToggleItemLoader();
        }
    }
}
