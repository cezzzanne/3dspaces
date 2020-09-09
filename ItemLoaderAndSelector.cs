using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class ItemLoaderAndSelector : MonoBehaviour {
    // Start is called before the first frame update
    private List<GameObject> prefabList;

    private List<string> itemNames;
    int currIndex = 0;

    public GameObject mainCamera;

    public GameObject RotateStick;

    public GameObject objectTitle;


    void Start() {
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
            instPrefab.transform.SetParent(transform);
            instPrefab.transform.localPosition = new Vector3(0, 1, 0);
            float width = instPrefab.GetComponent<BoxCollider>().size.x * Screen.width/ Screen.height; // basically height * screen aspect ratio
            instPrefab.transform.localScale = Vector3.one * width / 2f;
            instPrefab.transform.localScale = instPrefab.transform.localScale * (1f / instPrefab.GetComponent<BoxCollider>().size.x);
            instPrefab.transform.Rotate(new Vector3(-20, 0, 0), Space.Self);
        }
        mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y + 1, mainCamera.transform.position.z);
        mainCamera.transform.Rotate(new Vector3(3, 0, 0), Space.Self);
        prefabList[currIndex].SetActive(true);
        objectTitle.GetComponent<Text>().text = itemNames[currIndex];
        FitCamera();
    }

    void Update() {
        transform.RotateAround(transform.position, new Vector3(0 ,1, 0), 0.1f);
    }

    public void shiftLeft() {
        int newIndex = currIndex - 1;
        if (newIndex < 0) {
            newIndex = prefabList.Count - 1;
        }
        prefabList[currIndex].SetActive(false);
        prefabList[newIndex].SetActive(true);
        currIndex = newIndex;
        FitCamera();
    }

    public void shiftRight() {
        int newIndex = currIndex + 1;
        if (newIndex >  prefabList.Count - 1) {
            newIndex = 0;
        }
        prefabList[currIndex].SetActive(false);
        prefabList[newIndex].SetActive(true);
        currIndex = newIndex;
        FitCamera();
      
    }

    public void ConfirmItem() {
        int stringLength = prefabList[currIndex].name.Length -7;
        Debug.Log("NAME: " +  prefabList[currIndex].name);
        PlayerPrefs.SetString("CurrentItem", prefabList[currIndex].name.Substring(0, stringLength));
        SceneManager.LoadScene("WorldEdit");
    }

    public void Cancel() {
        PlayerPrefs.DeleteKey("CurrentItem");
        SceneManager.LoadScene("WorldEdit");
    }

    void FitCamera() {
        objectTitle.GetComponent<Text>().text = itemNames[currIndex];
        transform.rotation = Quaternion.Euler(0, 0, 0);
        Bounds itemBounds = prefabList[currIndex].GetComponent<BoxCollider>().bounds;
        float cameraDistance = 6.0f; // Constant factor
        Vector3 objectSizes = itemBounds.max - itemBounds.min;
        float objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);
        float cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * mainCamera.GetComponent<Camera>().fieldOfView); // Visible height 1 meter in front
        float distance = cameraDistance * objectSize / cameraView; // Combined wanted distance from the object
        distance += 0.5f * objectSize; // Estimated offset from the center to the outside of the object
        mainCamera.transform.position = itemBounds.center - distance * mainCamera.transform.forward;
    }


}
