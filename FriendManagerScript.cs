using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

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

        void Start() {
            Debug.Log("USERNAMEEE : " + PlayerPrefs.GetString("username"));
            managerScript = GameManager.GetComponent<GameManagerScript>();
            StartCoroutine(GetFriends("https://circles-parellano.herokuapp.com/api/get-world-friends", PrefabFriendButton, FriendCallback, FriendListContent));
            uiManagerScript = UIManager.GetComponent<UIManagerScript>();
            itemLoader = itemLoaderGO.GetComponent<ItemLoader>();
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
                    newButton.GetComponent<Button>().onClick.AddListener(()=> {friendFunc(friend.id, friend.user.username, friend.world_type);});
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
        public delegate void FriendButtonClickCallback(string friendID, string username, string worldType);

        public void FriendCallback(string friendID, string username, string worldType) {
            uiManagerScript.FriendCallback();
            managerScript.GoToRoom(friendID, username, worldType); 
        }

        public void GoBackHome() {
            uiManagerScript.GoHomeCallback();
            string roomID = PlayerPrefs.GetString("myRoomID");
            StartCoroutine(itemLoader.LoadPurchasedItems(roomID));
            managerScript.GoToRoom(roomID, "", PlayerPrefs.GetString("myWorldType"));
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
                    newButton.GetComponent<Button>().onClick.AddListener(()=> {friendFunc(friendData.friendID, friendUsername, friendData.world_type);});
                    newButton.transform.SetParent(panel.transform);   
                    newButton.transform.localScale = new Vector3(1, 1, 1);  
                } 
                yield return response;
            }
        }
    }
}
