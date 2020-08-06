using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Voice.PUN;
using UnityEngine.Networking;
using UnityEngine.UI;


[System.Serializable]
public struct JsonResponse {
    public string success, userID, message;
    
}


namespace Spaces {
    public class NetworkCreationManager : MonoBehaviourPunCallbacks {
        // Start is called before the first frame update

        private bool connectToRoom = false;

        public GameObject usernameInput;
        public GameObject passwordInput;
        public AvatarCreation Avatar;
        public GameObject avatarGameObject;

        void Awake() {
            string myRoomID = PlayerPrefs.GetString("myRoomID");
            Avatar = avatarGameObject.GetComponent<AvatarCreation>();
            // if (myRoomID != "") {
            //     Debug.Log(myRoomID);
            //     Debug.Log("going to load main game");
            //     SceneManager.LoadScene("MainGame");
            // }
        }


        public void OnClickConnectRoom() {
            string currentSkin = Avatar.SelectedCharacter();
            PlayerPrefs.SetString("CurrentSkin", currentSkin);
            // following lines are test (uncomment)
            SceneManager.LoadScene("MainGame");
            // string username = usernameInput.GetComponent<Text>().text.Trim();
            // string password = passwordInput.GetComponent<Text>().text.Trim();
            // StartCoroutine(MakeRequest("create-account", username, password));
        }



        // static IEnumerator MakeRequest(string url, string username, string password) {
        // WWWForm form = new WWWForm();
        // form.AddField("username", username);
        // form.AddField("password", password);
        // UnityWebRequest www = UnityWebRequest.Post("https://circles-parellano.herokuapp.com/api/" + url, form);
        // yield return www.SendWebRequest();
        // if(www.isNetworkError || www.isHttpError) {
        //     Debug.Log(www.error);
        // }
        // else {
        //     string response = www.downloadHandler.text;
        //     yield return response;
        //     Debug.Log(response);
        //     JsonResponse data = JsonUtility.FromJson<JsonResponse>(response);
        //     if (data.success == "false") {
        //         Debug.Log(data.message);
        //     } else {
        //         string location = "365.285:0.499218:438.6511"; 
        //         PlayerPrefs.SetString("positionInWorld", location);
        //         PlayerPrefs.SetString("myRoomID", data.userID);
        //         PlayerPrefs.SetString("currentRoomID", data.userID);
        //         PlayerPrefs.SetString("username", username);
        //         SceneManager.LoadScene("MainGame");
        //     }
        // }
    // }
    }
}
