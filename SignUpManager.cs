using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
public class SignUpManager : MonoBehaviour {
    public GameObject mainCamera;
    public GameObject usernameField;
    public GameObject passwordField;
    public GameObject emailField;
    public GameObject nextButton;
    private int currentStep; // 0 is sign up, 1 is character selection
    private float usernameInitialY;
    private float emailInitialY;
    private float passwordInitialY;
    private int currentInputField; // 0 is username , 1 is email and 2 is password
    private string username = "";
    private string email = "";
    private string password = "";
    GameObject nextField;

    public GameObject nextAvatarButton;
    public GameObject prevAvatarButton;
    public GameObject LoadingIndicator;
    private bool inCharacterSelectionScene = false;
    public GameObject AvatarSelection;


    // camera start x pos - 368.21 ; y - 5.54 ; z - 434.57; xrot - 15; yrot - 90
    // camera avatar creation = xpos - 368.21; y- 4.2; z - 434.57; xrot - 15; yrot - 0.366

    void Awake() {
        string currentCharacter = PlayerPrefs.GetString("CurrentSkin");
        Debug.Log("current skin : " + currentCharacter);
        if (currentCharacter != "") {
            Debug.Log("character" + currentCharacter);
            SceneManager.LoadScene("MainGame");
        }
    }

    void Start() {
        currentStep = 0;
        currentInputField = -1;
        usernameInitialY = usernameField.transform.position.y;
        passwordInitialY = passwordField.transform.position.y;
        emailInitialY = emailField.transform.position.y;
        SetCaretsInputs();
        nextField = usernameField;
        mainCamera.transform.position = new Vector3(368.21f, 5.54f, 434.57f);
        mainCamera.transform.rotation = Quaternion.Euler(15, 90, 0);
        nextButton.SetActive(false);
        nextAvatarButton.SetActive(false);
        prevAvatarButton.SetActive(false);
        LoadingIndicator.SetActive(false);
    }

    public void RotateCamera() {
        mainCamera.transform.position = new Vector3(368.21f, 4.2f, 434.57f);
        Quaternion desiredRot = Quaternion.Euler(15, 0.366f, 0);
        for(float t = 0f; t < 1; t += Time.deltaTime/5) {
             mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, desiredRot, t);
         }
    }

    void SetCaretsInputs() {
        usernameField.GetComponent<TMPro.TMP_InputField>().caretWidth = 0;
        usernameField.GetComponent<TMPro.TMP_InputField>().caretPosition = 0;

        emailField.GetComponent<TMPro.TMP_InputField>().caretWidth = 0;
        emailField.GetComponent<TMPro.TMP_InputField>().caretPosition = 0;

        passwordField.GetComponent<TMPro.TMP_InputField>().caretWidth = 0;
        passwordField.GetComponent<TMPro.TMP_InputField>().caretPosition = 0;
    }

    void Update() {
        if (currentStep == 0) {
            AnimateFields();
        }
        if (username.Length > 2 && password.Length > 5) {
            nextButton.SetActive(true);
        }
    }

    void AnimateFields() {
        if (currentInputField == 0) {
            usernameField.transform.position = new Vector3(usernameField.transform.position.x, usernameInitialY+0.2f*Mathf.Sin(1*Time.time), usernameField.transform.position.z);
        } else if (currentInputField == 1) {
            emailField.transform.position = new Vector3(emailField.transform.position.x, emailInitialY+0.2f*Mathf.Sin(1*Time.time), emailField.transform.position.z);
        } else if (currentInputField == 2) {
            passwordField.transform.position = new Vector3(passwordField.transform.position.x, passwordInitialY+0.2f*Mathf.Sin(1*Time.time), passwordField.transform.position.z);
        }
    }

    public void SetInput(int type) {
        currentInputField = type;
    }

    public void FillForm(string input) {
        if (currentInputField == 0) {
            Debug.Log("username : " + input);
            username = input;
        } else if (currentInputField == 1) {
            Debug.Log("email : " + input);
            email = input;
        } else {
            Debug.Log("password : " + input);
            password = input;
           
        }
    }

    public void GoToCharacterSelection() {
        if (inCharacterSelectionScene) {
            string characterSelected = AvatarSelection.GetComponent<AvatarCreation>().SelectedCharacter();
            Debug.Log("selected character :  " + characterSelected);
            PlayerPrefs.SetString("CurrentSkin", characterSelected);
            SceneManager.LoadScene("MainGame");
        } else {
            Debug.Log("finished it");
            nextButton.transform.GetChild(0).gameObject.SetActive(false);
            LoadingIndicator.SetActive(true);
            nextButton.GetComponent<Button>().interactable = false;
            StartCoroutine(ShowIndicator());
            StartCoroutine(MakeRequest("create-account", username.ToLower(), password, NextSceneCallBack, LoadingIndicator));
        }
    }

    IEnumerator ShowIndicator() {
        yield return new WaitForSeconds(2);
        LoadingIndicator.GetComponent<Slider>().value = 0.2f;
        yield return new WaitForSeconds(1);
        LoadingIndicator.GetComponent<Slider>().value = 0.6f;
        yield return new WaitForSeconds(2);
        LoadingIndicator.GetComponent<Slider>().value = 0.9f;
    }

    public void NextSceneCallBack() {
        RotateCamera();
        LoadingIndicator.SetActive(false);
        nextButton.GetComponent<Button>().interactable = true;
        nextButton.transform.GetChild(0).gameObject.SetActive(true);
        prevAvatarButton.SetActive(true);
        nextAvatarButton.SetActive(true);
        inCharacterSelectionScene = true;
    }

    public delegate void GoToNextScene();

    static IEnumerator MakeRequest(string url, string username, string password, GoToNextScene callback, GameObject loadingIndicator) {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);
        UnityWebRequest www = UnityWebRequest.Post("https://circles-parellano.herokuapp.com/api/" + url, form);
        yield return www.SendWebRequest();
        if(www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
        }
        else {
            string response = www.downloadHandler.text;
            yield return response;
            Debug.Log(response);
            JsonResponse data = JsonUtility.FromJson<JsonResponse>(response);
            if (data.success == "false") {
                Debug.Log(data.message);
            } else {
                Debug.Log(response);
                loadingIndicator.GetComponent<Slider>().value = 1;
                string location = "365.285:0.499218:438.6511"; 
                PlayerPrefs.SetString("positionInWorld", location);
                PlayerPrefs.SetString("myRoomID", data.userID);
                PlayerPrefs.SetString("currentRoomID", data.userID);
                PlayerPrefs.SetString("username", username);
                callback();

            }
        
        }
      }
}
