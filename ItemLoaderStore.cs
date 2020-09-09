using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

namespace Spaces {


   [System.Serializable]
    public  struct StoreResponse {
        public string success;
        public List<StoreItem> data;
        public int coins;
    }

    [System.Serializable]
    public  struct StoreItem {
        public string name, type, location;
        public string price;
        
    }


    [System.Serializable]
    public  struct PurchaseResponse {
        public string success;        
    }



    public class ItemLoaderStore : MonoBehaviour {
        // Start is called before the first frame update
        PlayerFollow mainCam;

        List<GameObject> prefabList;
        List<string> itemNames;

        private Transform ObjectHolder;

        private int currIndex = 0;

        private bool inEditing = false;

        public GameObject BuyItemB, ReturnB, NextItemB, PrevItemB, StoreB;

        public GameObject GoHomeB, StartChatB, joystick;
        public GameObject objectTitle;

        private StoreResponse storeData;

        public GameObject characterModel;
        public GameObject currentItem;

        public GameObject price;


        private int currentObjectType; // 0 is skin; 1 is object ; 2 is world

        public GameObject coins;
        public GameObject LoadingPurchase;

        
        void Start() {
            ObjectHolder = transform.GetChild(0);
            StartCoroutine(GetItems());
        }


        IEnumerator GetItems() {
            WWWForm form = new WWWForm();
            Debug.Log("GOING INTO REQUEST");
            form.AddField("userID", PlayerPrefs.GetString("myRoomID"));
            form.AddField("storeType", 0);
            UnityWebRequest www = UnityWebRequest.Post("https://circles-parellano.herokuapp.com/api/get-store", form);
            yield return www.SendWebRequest();
            if(www.isNetworkError || www.isHttpError) {
                Debug.Log(www.error);
            }
            else {
                string response = www.downloadHandler.text;
                yield return response;
                Debug.Log("Form upload complete! Text: " + response);
                storeData = JsonUtility.FromJson<StoreResponse>(response);
                coins.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "$" + storeData.coins;
                coins.SetActive(true);
            }
        }


        public void GoToItemSelection() {
            if (mainCam == null) {
                return;
            }
            mainCam.ToggleItemLoader();
            if (prefabList == null) {
                LoadItem();
            } else {
                if (!inEditing) {
                    Debug.Log("111: fitting camera");
                    FitCamera();
                }
            }
            inEditing = !inEditing;
            ToggleUI();
        }

        void ToggleUI() {
            if (inEditing && (int.Parse(storeData.data[currIndex].price) > 400)) {
                BuyItemB.SetActive(false);
            } else {
                BuyItemB.SetActive(inEditing);
            }
            ReturnB.SetActive(inEditing);
            NextItemB.SetActive(inEditing);
            PrevItemB.SetActive(inEditing);
            objectTitle.SetActive(inEditing);
            price.SetActive(inEditing);
            GoHomeB.SetActive(!inEditing);
            StartChatB.SetActive(!inEditing);
            joystick.SetActive(!inEditing);
            StoreB.SetActive(!inEditing);
        }

        public void SetCamera(PlayerFollow cam) {
            mainCam = cam;
        }

        public void ActivateStore(bool activate) {
            StoreB.SetActive(activate);
        }


        void FitCamera() {
            if (currentObjectType == 0) {
                mainCam.transform.position = new Vector3(15, 52, 13);
                mainCam.transform.rotation = Quaternion.Euler(0, 0, 0);
            } else {
                transform.rotation = Quaternion.Euler(0, 0, 0);
                Bounds itemBounds = currentItem.GetComponent<BoxCollider>().bounds; //prefabList[currIndex].GetComponent<BoxCollider>().bounds;
                float cameraDistance = 1.0f; // Constant factor
                Vector3 objectSizes = itemBounds.max - itemBounds.min;
                float objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);
                float cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * mainCam.gameObject.GetComponent<Camera>().fieldOfView); // Visible height 1 meter in front
                float distance = cameraDistance * objectSize / cameraView; // Combined wanted distance from the object
                distance += 0.5f * objectSize; // Estimated offset from the center to the outside of the object
                mainCam.gameObject.transform.position = currentItem.transform.position - distance * mainCam.transform.forward; //itemBounds.center - distance * mainCam.transform.forward;
            }
            objectTitle.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = storeData.data[currIndex].name;
            price.transform.GetChild(0).GetComponent<Text>().text = "$" + storeData.data[currIndex].price;
        }

        public void NextItem() {
            int newIndex = currIndex + 1;
            if (newIndex >  storeData.data.Count - 1) {
                newIndex = 0;
            }
            currIndex = newIndex;
            characterModel.transform.parent.gameObject.SetActive(false);
            if (currentObjectType == 0) {

            } else {
                Destroy(currentItem);
            }
            LoadItem();
        }

        public void LoadItem() {
            StoreItem item = storeData.data[currIndex];
            if (int.Parse(item.price) > 400) {
                BuyItemB.SetActive(false);
            } else {
                BuyItemB.SetActive(true);
            }
            if (item.type == "skin") {
                Material material = Resources.Load<Material>(item.location) as Material;
                characterModel.GetComponent<SkinnedMeshRenderer>().material = material;
                characterModel.transform.parent.gameObject.SetActive(true);
                currentObjectType = 0;
            } else if (item.type == "object") {
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
                currentObjectType = 1;
            } else {
                Debug.Log("supposed to be world");
            }
            FitCamera();
        }

        public void PrevItem() {
            int newIndex = currIndex - 1;
            if (newIndex < 0) {
                newIndex = storeData.data.Count - 1;
            }
            currIndex = newIndex;
            characterModel.transform.parent.gameObject.SetActive(false);
            if (currentObjectType == 0) {

            } else {
                Destroy(currentItem);
            }
            LoadItem();
        }

        public void ConfirmItem() {
            LoadingPurchase.SetActive(true);
            StartCoroutine(PurchaseItem(storeData.data[currIndex]));
        }

        public IEnumerator PurchaseItem(StoreItem currentItem) {
            WWWForm form = new WWWForm();
            form.AddField("userID", PlayerPrefs.GetString("myRoomID"));
            form.AddField("name", currentItem.name);
            form.AddField("location", currentItem.location);
            form.AddField("price", currentItem.price.ToString());
            form.AddField("type", currentItem.type);
            UnityWebRequest www = UnityWebRequest.Post("https://circles-parellano.herokuapp.com/api/purchase-item", form);
            yield return www.SendWebRequest();
            if(www.isNetworkError || www.isHttpError) {
                Debug.Log(www.error);
            }
            else {
                string response = www.downloadHandler.text;
                yield return response;
                Debug.Log("purchased item " + response);
                PurchaseResponse resp = JsonUtility.FromJson<PurchaseResponse>(response);
                if (resp.success == "true") {
                    LoadingPurchase.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "congratulations! purchase completed!";
                    yield return new WaitForSeconds(4);
                    storeData.coins = storeData.coins - int.Parse(currentItem.price);
                    coins.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "$" + storeData.coins;
                    storeData.data.RemoveAt(currIndex);
                    NextItem();
                    LoadingPurchase.SetActive(false);
                    LoadingPurchase.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "completing purchase...";
                } else {
                    LoadingPurchase.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "oops! something went wrong. try again";
                    yield return new WaitForSeconds(4);
                    LoadingPurchase.SetActive(false);
                    
                }
            }
        }

        public void CancelEditing() {
            inEditing = false;
            if (currentObjectType == 1 ){
                Destroy(currentItem);
            }
            ToggleUI();
            mainCam.ToggleItemLoader();
        }
    }
}
