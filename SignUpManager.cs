using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;

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

    private float t = 0.0f;
    float speed = 4.0F;

    public GameObject usernameLogo, passwordLogo, emailLogo, errorButton;


    // camera start x pos - 368.21 ; y - 5.54 ; z - 434.57; xrot - 15; yrot - 90
    // camera avatar creation = xpos - 368.21; y- 4.2; z - 434.57; xrot - 15; yrot - 0.366

    void Awake() {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://spaces-d9a3c.firebaseio.com/");
        string currentCharacter = PlayerPrefs.GetString("CurrentSkin");
        if (currentCharacter != "") {
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
        nextButton.SetActive(true);
        nextAvatarButton.SetActive(false);
        prevAvatarButton.SetActive(false);
        LoadingIndicator.SetActive(false);
    }

    public IEnumerator RotateCam() {
        t += Time.deltaTime;
        mainCamera.transform.position = new Vector3(368.21f, 4.2f, 434.57f);
        Quaternion desiredRot = Quaternion.Euler(15, 0.366f, 0);
        Quaternion tempRot = mainCamera.transform.rotation;
        Quaternion desiredPlace = Quaternion.Euler(mainCamera.transform.rotation.x + 15, mainCamera.transform.rotation.y + 0.366f, mainCamera.transform.rotation.z); // Quaternion.Slerp(mainCamera.transform.rotation, desiredRot);
        while (mainCamera.transform.rotation.x < desiredPlace.x) {
            yield return new WaitForSeconds(0.01f);
            Quaternion to = Quaternion.Lerp(mainCamera.transform.rotation, desiredPlace, t / 1);
            mainCamera.transform.rotation = to;
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
        // if (username.Length > 2 && password.Length > 5) {
        //     nextButton.SetActive(true);
        // }
    }

    void AnimateFields() {
        if (currentInputField == 0) {
            usernameField.transform.position = new Vector3(usernameField.transform.position.x, usernameInitialY+0.2f*Mathf.Sin(1*Time.time), usernameField.transform.position.z);
        } else if (currentInputField == 1) {
            emailField.transform.position = new Vector3(emailField.transform.position.x, emailInitialY+0.2f*Mathf.Sin(1*Time.time), emailField.transform.position.z);
        } else {
            passwordField.transform.position = new Vector3(passwordField.transform.position.x, passwordInitialY+0.2f*Mathf.Sin(1*Time.time), passwordField.transform.position.z);
        }
   }

    public void SetInput(int type) {
        currentInputField = type;
    }

    public void FillForm(string input) {
        if (currentInputField == 0) {
            username = input;
        } else if (currentInputField == 1) {
            email = input;
        } else {
            password = input;
           
        }
    }

    IEnumerator PhaseOutField(GameObject field, GameObject nextField) {
        GameObject leaving = (currentInputField == 1) ? usernameLogo : emailLogo;
        GameObject entering = (currentInputField == 1)? emailLogo : passwordLogo;
        t += Time.deltaTime;
        entering.transform.position = new Vector3(entering.transform.position.x, entering.transform.position.y - 10, entering.transform.position.z);
        nextField.transform.position = new Vector3(nextField.transform.position.x, nextField.transform.position.y, nextField.transform.position.z + 10);
        nextField.SetActive(true);
        entering.SetActive(true);
        float target = field.transform.position.z + 10;
        while (field.transform.position.z < target) {
            yield return new WaitForSeconds(0.01f);
            float res = Mathf.Lerp(field.transform.position.z, field.transform.position.z + 25, t / speed);
            float res2 = Mathf.Lerp(nextField.transform.position.z, nextField.transform.position.z - 25, t / speed);
            float logoLeaving = Mathf.Lerp(leaving.transform.position.y, leaving.transform.position.y + 25, t/ speed);
            float logoEntering = Mathf.Lerp(entering.transform.position.y, entering.transform.position.y + 25, t/ speed);
            field.transform.position = new Vector3(field.transform.position.x, field.transform.position.y, res);
            nextField.transform.position = new Vector3(nextField.transform.position.x, nextField.transform.position.y, res2);
            entering.transform.position = new Vector3(entering.transform.position.x, logoEntering, entering.transform.position.z);
            leaving.transform.position = new Vector3(leaving.transform.position.x, logoLeaving, leaving.transform.position.z);
        }
    }

    public void GoToCharacterSelection() {
        if (currentInputField == 0) {
            currentInputField = 1;
            StartCoroutine(PhaseOutField(usernameField, emailField));
        } else if (currentInputField == 1) {
            currentInputField = 2;
            StartCoroutine(PhaseOutField(emailField, passwordField));
        } else if (currentInputField == 2) {
            Debug.Log("finished form");
            if (inCharacterSelectionScene) {
                string characterSelected = AvatarSelection.GetComponent<AvatarCreation>().SelectedCharacter();
                Debug.Log("selected character :  " + characterSelected);
                PlayerPrefs.SetString("CurrentSkin", characterSelected);
                PlayerPrefs.SetString("myWorldType", "MainGame");
                SceneManager.LoadScene("MainGame");
            } else {
                Debug.Log("finished it");
                nextButton.transform.GetChild(0).gameObject.SetActive(false);
                LoadingIndicator.SetActive(true);
                nextButton.GetComponent<Button>().interactable = false;
                StartCoroutine(ShowIndicator());
                StartCoroutine(MakeRequest("create-account", username.ToLower(), password, NextSceneCallBack, LoadingIndicator, errorButton));
            }
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
        StartCoroutine(RotateCam());
        LoadingIndicator.SetActive(false);
        nextButton.GetComponent<Button>().interactable = true;
        nextButton.transform.GetChild(0).gameObject.SetActive(true);
        prevAvatarButton.SetActive(true);
        nextAvatarButton.SetActive(true);
        inCharacterSelectionScene = true;
        SetFirebaseProfile();
    }


    public void SetFirebaseProfile() {
        Dictionary<string, object> requests = new Dictionary<string, object>(){};
        Dictionary<string, object> coinsInfo = new Dictionary<string, object>(){
            {"LastRequest", "none"},
            {"ConsecutiveDays", "0"}
        };

        Dictionary<string, object> user = new Dictionary<string, object>
        {
                { "id", PlayerPrefs.GetString("myRoomID") },
                { "requests", requests},
                {"coins",  "0"},
                {"coinsInfo", coinsInfo}
        };
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        reference.Child("users").Child(username).SetValueAsync(user);
    }

    public delegate void GoToNextScene();

    static IEnumerator MakeRequest(string url, string username, string password, GoToNextScene callback, GameObject loadingIndicator, GameObject errorButton) {
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
                errorButton.SetActive(true);
            } else {
                loadingIndicator.GetComponent<Slider>().value = 1;
                OneSignal.SetExternalUserId(data.userID);
                PlayerPrefs.SetString("myRoomID", data.userID);
                PlayerPrefs.SetString("currentWorldType", "MainGame");
                PlayerPrefs.SetString("currentRoomID", data.userID);
                PlayerPrefs.SetString("username", username);
                callback();
            }
        
        }
      }
}
