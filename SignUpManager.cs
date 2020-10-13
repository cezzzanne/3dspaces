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
    public GameObject nextButton;
    private string username = "";
    private string email = "";
    private string password = "";
    public GameObject nextAvatarButton;
    public GameObject prevAvatarButton;
    private bool inCharacterSelectionScene = false;
    public GameObject AvatarSelection;

    public GameObject usernameInput, emailInput, passwordInput, errorDisplay, loadingIndicator, inputBackdrop;

    public GameObject background, chooseAvatarSign, chooseAvatarBackdrop;

    private int currentInput = 0;

    private int usernameConfirmed = -1; //  -1 is stand by , 0 is error 1 is success
    private bool loading = false;



    void Awake() {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://spaces-d9a3c.firebaseio.com/");
        string currentCharacter = PlayerPrefs.GetString("CurrentSkin");
        if (currentCharacter != "") {
            SceneManager.LoadScene("MainGame");
        }
    }

    void Start() {
    }

    public void UpdateInput(string input) {
        if (currentInput == 0) {
            username = input;
        } else if (currentInput == 1) {
            email = input;
        } else {
            password = input;
        }
    }

    public void NextInput() {
        if (inCharacterSelectionScene) {
            string characterSelected = AvatarSelection.GetComponent<AvatarCreation>().SelectedCharacter();
            PlayerPrefs.SetString("CurrentSkin", characterSelected);
            PlayerPrefs.SetString("myWorldType", "MainGame");
            nextAvatarButton.SetActive(false);
            prevAvatarButton.SetActive(false);
            nextButton.SetActive(false);
            chooseAvatarBackdrop.SetActive(false);
            chooseAvatarSign.SetActive(false);
            StartCoroutine(mainCamera.GetComponent<CameraTour>().MoveCameraToWorld(characterSelected));
            return;
        }
        if (currentInput == 0) {
            StartCoroutine(VerifyUsername());
            CheckUsername();
            errorDisplay.SetActive(false);
            loading = true;
        } else if (currentInput == 1) {
            emailInput.SetActive(false);
            passwordInput.SetActive(true);
            currentInput = 2;
        } else {
            Debug.Log(email);
            Debug.Log(password);
            loading = true;
            StartCoroutine(MakeRequest("create-account", username.ToLower(), password, NextSceneCallBack, errorDisplay));

        }
    }

    public void CheckUsername() {
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        reference.Child("usernameList").GetValueAsync().ContinueWith(task => {
            DataSnapshot snapshot = task.Result;
            Dictionary<string, object> usernames = snapshot.Value as Dictionary<string, object>;
            foreach(KeyValuePair<string, object> user in usernames) {
                if (username == user.Key) {
                    Debug.Log("duplicate");
                    usernameConfirmed = 0;
                    return;
                }
                usernameConfirmed = 1;
            }
            
        });
    }

    public IEnumerator VerifyUsername() {
        // stop loading indicator
        while (usernameConfirmed == -1) {
            yield return null;
        }
        if (usernameConfirmed == 0) {
            //error
            errorDisplay.SetActive(true);
            usernameConfirmed = -1;
        } else {
            // success
            usernameInput.SetActive(false);
            emailInput.SetActive(true);
            currentInput = 1;
        }
        loading = false;
    }


    void Update() {
        if (loading) {
            loadingIndicator.transform.RotateAround(loadingIndicator.transform.position, new Vector3(0 , 0, 1), 2f);
        } else {
            loadingIndicator.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

    }

    public void NextSceneCallBack() {
        loading = false;
        prevAvatarButton.SetActive(true);
        nextAvatarButton.SetActive(true);
        background.SetActive(false);
        passwordInput.SetActive(false);
        inputBackdrop.SetActive(false);
        chooseAvatarSign.SetActive(true);
        chooseAvatarBackdrop.SetActive(true);
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
                { "id", PlayerPrefs.GetString("myRoomID").ToLower() },
                { "requests", requests},
                {"coins",  "0"},
                {"coinsInfo", coinsInfo}
        };
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        reference.Child("users").Child(username.ToLower()).SetValueAsync(user);
        OSPermissionSubscriptionState status = OneSignal.GetPermissionSubscriptionState();
        string oneSignalID = (status.subscriptionStatus.userId != null) ? status.subscriptionStatus.userId : "1";
        Dictionary<string, object> usernameList = new Dictionary<string, object> {
            {username.ToLower(), oneSignalID}
        };
        reference.Child("usernameList").UpdateChildrenAsync(usernameList);
    }

    public delegate void GoToNextScene();

    static IEnumerator MakeRequest(string url, string username, string password, GoToNextScene callback, GameObject errorButton) {
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
                OneSignal.SetExternalUserId(data.userID.ToString());
                PlayerPrefs.SetString("myRoomID", data.userID);
                PlayerPrefs.SetString("currentWorldType", "MainGame");
                PlayerPrefs.SetString("currentRoomID", data.userID);
                PlayerPrefs.SetString("username", username.ToLower());
                callback();
            }
        
        }
      }
}
