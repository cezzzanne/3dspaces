using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace Spaces {

    [System.Serializable]
    public  struct AddFriendResponseData {
        public string success, friendID;

        
    }

     [System.Serializable]
    public  struct FriendsData {
        public FriendJson[] friends;
    }

     [System.Serializable]
    public  struct GetFriendsResponseData {
        public FriendsData data;
    }

    [System.Serializable]
    public  struct FriendJson {
        public string id;
        public FriendJsonUsername user;


        
    }
     [System.Serializable]
    public  struct FriendJsonUsername {
        public string username;

        
    }
    public class FriendManagerScript : MonoBehaviour {
        // Start is called before the first frame update

        public GameObject Panel;

        public GameObject EditingButton;
        public GameObject AddFriendButton;
        public GameObject CancelFormButton;
        public GameObject HomeButton;
        public GameObject PrefabFriendButton;

        public GameObject ContactListButton;
        public GameObject SpeakerButton;

        public GameObject AddFriendForm;

        public GameObject AddFriendInputField;
        public GameObject GameManager;
        GameManagerScript managerScript;

        public GameObject FriendListContent;

        public GameObject ClosePanelButton;
        
        void Start() {
            Debug.Log("USERNAMEEE : " + PlayerPrefs.GetString("username"));
            managerScript = GameManager.GetComponent<GameManagerScript>();
            StartCoroutine(GetFriends("https://circles-parellano.herokuapp.com/api/get-world-friends", PrefabFriendButton, FriendCallback, FriendListContent));
        }

        IEnumerator GetFriends(string url, GameObject buttonPrefab, FriendButtonClickCallback friendFunc, GameObject panel) {
            WWWForm form = new WWWForm();
            form.AddField("userID", PlayerPrefs.GetString("myRoomID"));
            UnityWebRequest www = UnityWebRequest.Post(url, form);
            yield return www.SendWebRequest();
            if(www.isNetworkError || www.isHttpError) {
                Debug.Log(www.error);
            } else {
                string response = www.downloadHandler.text;
                Debug.Log("success");
                GetFriendsResponseData friendData = JsonUtility.FromJson<GetFriendsResponseData>(response);
                foreach (FriendJson friend in friendData.data.friends) {
                    GameObject newButton = Instantiate(buttonPrefab) as GameObject;
                    newButton.transform.GetChild(0).GetComponent<Text>().text = "@"+friend.user.username;
                    newButton.GetComponent<Button>().onClick.AddListener(()=> {friendFunc(friend.id, friend.user.username);});
                    newButton.transform.SetParent(panel.transform);     
                    newButton.transform.localScale = new Vector3(1, 1, 1);          
                }
                yield return response;
            }
        }

        // Update is called once per frame
        void Update() {
            
        }

        void TogglePanel(bool setOpen) {
            if (setOpen) {
                Panel.SetActive(true);
                EditingButton.SetActive(false);
                HomeButton.SetActive(false);
                ContactListButton.SetActive(false);
                ClosePanelButton.SetActive(true);
                SpeakerButton.SetActive(false);
            } else {
                Panel.SetActive(false);
                if (PlayerPrefs.GetString("currentRoomID") == PlayerPrefs.GetString("myRoomID")) {
                    EditingButton.SetActive(true);
                    HomeButton.SetActive(false);
                } else {
                    EditingButton.SetActive(false);
                    HomeButton.SetActive(true);
                }
                ContactListButton.SetActive(true);
                ClosePanelButton.SetActive(false);
                SpeakerButton.SetActive(true);
            }
        }

        public void OpenPanel() {
            TogglePanel(true);
        }

        public void ClosePanel() {
            TogglePanel(false);
        }

        public void AddFriendClick() {
            AddFriendButton.SetActive(false);
            AddFriendForm.SetActive(true);
        }

        public void SubmitForm() {
            string text = AddFriendInputField.GetComponent<Text>().text.ToLower();
            if (text.Trim() == "") {
                AddFriendForm.SetActive(false);
                AddFriendButton.SetActive(true);
                return;
            }
            StartCoroutine(AddFriend(text, "https://circles-parellano.herokuapp.com/api/add-friend", FriendListContent, PrefabFriendButton, FriendCallback));
        }
        public delegate void FriendButtonClickCallback(string friendID, string username);

        public void FriendCallback(string friendID, string username) {
            Panel.SetActive(false);
            HomeButton.SetActive(true);
            EditingButton.SetActive(false);
            ContactListButton.SetActive(true);
            SpeakerButton.SetActive(true);
            ClosePanelButton.SetActive(false);
            managerScript.GoToRoom(friendID, username); 
        }

        public void GoBackHome() {
            Panel.SetActive(false);
            HomeButton.SetActive(false);
            EditingButton.SetActive(true);
            ContactListButton.SetActive(true);
            SpeakerButton.SetActive(true);
            managerScript.GoToRoom(PlayerPrefs.GetString("myRoomID"), "");
        }

        IEnumerator AddFriend(string friendUsername, string url, GameObject panel, GameObject buttonPrefab, FriendButtonClickCallback friendFunc) {
            WWWForm form = new WWWForm();
            form.AddField("userID", PlayerPrefs.GetString("myRoomID"));
            form.AddField("username", friendUsername);
            UnityWebRequest www = UnityWebRequest.Post(url, form);
            yield return www.SendWebRequest();
            if(www.isNetworkError || www.isHttpError) {
                Debug.Log(www.error);
            } else {
                string response = www.downloadHandler.text;
                AddFriendResponseData friendData = JsonUtility.FromJson<AddFriendResponseData>(response);
                Debug.Log(friendData.success);
                if (friendData.success == "true") {
                    GameObject newButton = Instantiate(buttonPrefab) as GameObject;
                    newButton.transform.GetChild(0).GetComponent<Text>().text = friendUsername;
                    newButton.GetComponent<Button>().onClick.AddListener(()=> {friendFunc(friendData.friendID, friendUsername);});
                    newButton.transform.SetParent(panel.transform);   
                    newButton.transform.localScale = new Vector3(1, 1, 1);  
                } 
                yield return response;
            }
        }
    }
}
