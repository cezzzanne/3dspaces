using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;

namespace Spaces {

    [System.Serializable]
    public  struct AddFriendResponseData {
        public string success, friendID, world_type;

        
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
        public string world_type;
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
        
        public GameObject UIManager;
        UIManagerScript uiManagerScript;

        public GameObject itemLoaderGO;

        ItemLoader itemLoader;

        private string username;

        public GameObject requestPanelPrefab;

        public GameObject RequestListContent;

        public List<GameObject> RequestPanels;

        public Dictionary<string, object> requestKeys;

        private string roomID;

        private string sendRequestExtID = "null";


        void Start() {
            username = PlayerPrefs.GetString("username");
            Debug.Log("USSERNAME : " + username);
            roomID = PlayerPrefs.GetString("myRoomID");
            managerScript = GameManager.GetComponent<GameManagerScript>();
            StartCoroutine(GetFriends("https://circles-parellano.herokuapp.com/api/get-world-friends", PrefabFriendButton, FriendCallback, FriendListContent));
            uiManagerScript = UIManager.GetComponent<UIManagerScript>();
            itemLoader = itemLoaderGO.GetComponent<ItemLoader>();
            SetUpRequests();
        }

        void OnApplicationQuit() {
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
            reference.Child("users").Child(username).Child("requests").ValueChanged -= HandleRequestsUpdate;
        }

        IEnumerator GetFriends(string url, GameObject buttonPrefab, FriendButtonClickCallback friendFunc, GameObject panel) {
            WWWForm form = new WWWForm();
            form.AddField("userID", roomID);
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
                    newButton.GetComponent<Button>().onClick.AddListener(()=> {friendFunc(friend.id, friend.user.username, friend.world_type);});
                    newButton.GetComponent<FriendTrackingScript>().TriggerStart(); // do this because once in parent it will not run
                    newButton.transform.SetParent(panel.transform);     
                    newButton.transform.localScale = new Vector3(1, 1, 1);          
                }
                yield return response;
            }
        }


        void TogglePanel(bool setOpen) {
            if (setOpen) {
                uiManagerScript.TogglePanel(setOpen);
            } else {
                uiManagerScript.TogglePanel(setOpen);
            }
        }

        public void OpenPanel() {
            TogglePanel(true);
        }

        public void ClosePanel() {
            TogglePanel(false);
        }

        public void AddFriendClick() {
            uiManagerScript.AddFriendUsername();
        }

        public void SubmitForm() {
            string text = AddFriendInputField.GetComponent<Text>().text.ToLower();
            if (text.Trim() == "") {
                AddFriendForm.SetActive(false);
                AddFriendButton.SetActive(true);
                return;
            }
            SendFriendRequest(text);
        }
        public delegate void FriendButtonClickCallback(string friendID, string username, string worldType);

        public void FriendCallback(string friendID, string username, string worldType) {
            uiManagerScript.FriendCallback();
            managerScript.GoToRoom(friendID, username, worldType); 
        }

        public void GoBackHome() {
            uiManagerScript.GoHomeCallback();
            StartCoroutine(itemLoader.LoadPurchasedItems(roomID));
            managerScript.GoToRoom(roomID, "", PlayerPrefs.GetString("myWorldType"));
        }

        IEnumerator AddFriend(string friendUsername, string url, GameObject panel, GameObject buttonPrefab, FriendButtonClickCallback friendFunc) {
            WWWForm form = new WWWForm();
            form.AddField("userID", roomID);
            form.AddField("username", friendUsername);
            UnityWebRequest www = UnityWebRequest.Post(url, form);
            yield return www.SendWebRequest();
            if(www.isNetworkError || www.isHttpError) {
                Debug.Log(www.error);
            } else {
                string response = www.downloadHandler.text;
                AddFriendResponseData friendData = JsonUtility.FromJson<AddFriendResponseData>(response);
                if (friendData.success == "true") {
                    GameObject newButton = Instantiate(buttonPrefab) as GameObject;
                    newButton.transform.GetChild(0).GetComponent<Text>().text = "@" +  friendUsername.ToLower();
                    newButton.GetComponent<Button>().onClick.AddListener(()=> {friendFunc(friendData.friendID, friendUsername, friendData.world_type);});
                    newButton.transform.SetParent(panel.transform);   
                    newButton.transform.localScale = new Vector3(1, 1, 1);  
                } 
                yield return response;
            }
        }

        public void ToggleRequestPanel(GameObject acceptRejectPanel, GameObject friendRequest) {
            acceptRejectPanel.SetActive(true);
            friendRequest.SetActive(false);
        }

        public void SetUpAcceptRejectPanel(GameObject acceptRejectPanel, string requestUsername) {
            acceptRejectPanel.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(()=> {RejectRequest(requestUsername, acceptRejectPanel);});
            acceptRejectPanel.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(()=> {AcceptFriendRequest(requestUsername, acceptRejectPanel);});
        }

        void AcceptFriendRequest(string requestUsername, GameObject panel) {
            object friendID = requestKeys[requestUsername];
            SendNotification(friendID.ToString(), "accepted your request!");
            RemoveRequestFromFirebase(requestUsername);
            panel.transform.parent.gameObject.SetActive(false);
            Destroy(panel.transform.parent);
            StartCoroutine(AddFriend(requestUsername, "https://circles-parellano.herokuapp.com/api/add-friend", FriendListContent, PrefabFriendButton, FriendCallback));
        }

        public void SendNotification(string externalPlayerID, string message) {
            Dictionary<string, object> notification = new Dictionary<string, object>();
            notification["headings"] = new Dictionary<string, string>() { {"en", "@" + username.ToLower() + " " +  message} };
            notification["contents"] = new Dictionary<string, string>() { {"en",  "Check out their world!"} };
            notification["include_external_user_ids"] = new List<string>() { externalPlayerID };
            OneSignal.PostNotification(notification);
        }


        void RejectRequest(string requestUsername, GameObject panel) {
            RemoveRequestFromFirebase(requestUsername);
            panel.transform.parent.gameObject.SetActive(false);
            Destroy(panel.transform.parent);
        }

        void RemoveRequestFromFirebase(string requestUsername) {
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
            requestKeys.Remove(requestUsername);
            Dictionary<string, object> requests = new Dictionary<string, object>
            {
                    { "requests", requestKeys },
            };
            reference.Child("users").Child(username).UpdateChildrenAsync(requests);
        }

        // TODO : add success when the user was able to add friend ; add error when the username was not found

        public void SendFriendRequest(string friendUsername) {
            Dictionary<string, object> requestsFromFriend = new Dictionary<string, object>();
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
            Debug.Log("zz username: " + friendUsername);
            uiManagerScript.LoadingFriendRequest();
            StartCoroutine(SendRequestNotif());
            reference.Child("users").Child(friendUsername).GetValueAsync().ContinueWith(task => {
                DataSnapshot snapshot = task.Result;
                if (!snapshot.Exists) {
                    sendRequestExtID = "failed";
                    Debug.Log("zz no user with that username");
                    return;
                }
                Debug.Log("inside trquqest");
                Dictionary<string, object> data = snapshot.Value as Dictionary<string, object>;
                requestsFromFriend.Add(username, roomID);
                Dictionary<string, object> requests = new Dictionary<string, object>
                {
                        { "requests", requestsFromFriend },
                };
                sendRequestExtID = data["id"].ToString();
                reference.Child("users").Child(friendUsername).UpdateChildrenAsync(requests);
            });
        }

        // need to do it here because of async in firebase and permits in unity
        IEnumerator SendRequestNotif() {
            while(sendRequestExtID == "null") {
                yield return null;
            }
            if (sendRequestExtID != "failed") {
                uiManagerScript.ResultFriendRequest(true);
                SendNotification(sendRequestExtID, "has added you as a friend!");
            } else {
                uiManagerScript.ResultFriendRequest(false);
            }
            sendRequestExtID = "null";
        }

        public void SetUpRequests() {
            RequestPanels = new List<GameObject>();
            requestKeys = new Dictionary<string, object>();
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
            reference.Child("users").Child(username).Child("requests").ValueChanged += HandleRequestsUpdate;
        }

        void HandleRequestsUpdate(object sender, ValueChangedEventArgs arg) {
            if (this == null)  {
                DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
                reference.Child("users").Child(username).Child("requests").ValueChanged -= HandleRequestsUpdate;
                return;
            }
            if (!arg.Snapshot.Exists) {
                return;
            }
            Dictionary<string, object> requests = arg.Snapshot.Value as Dictionary<string, object>;
            foreach(KeyValuePair<string, object> request in requests) {
                if (!requestKeys.ContainsKey(request.Key)) {
                    GameObject requestPanel = Instantiate(requestPanelPrefab) as GameObject;
                    requestPanel.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "@" + request.Key.ToString().ToLower() + " added you as a friend";
                    Button requestPanelbutton = requestPanel.transform.GetChild(0).GetComponent<Button>();
                    requestPanelbutton.onClick.AddListener(()=> {ToggleRequestPanel(requestPanel.transform.GetChild(1).gameObject, requestPanel.transform.GetChild(0).gameObject);});
                    SetUpAcceptRejectPanel(requestPanel.transform.GetChild(1).gameObject, request.Key.ToString());
                    requestPanel.transform.SetParent(RequestListContent.transform);   
                    requestPanel.transform.localScale = new Vector3(1, 1, 1); 
                    RequestPanels.Add(requestPanel);
                    requestKeys.Add(request.Key, request.Value);
                }
            }
        }
    }
}
