﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;
using SA.iOS.Contacts;

namespace Spaces {


    // CREATE GROUP RESPONSE
    [System.Serializable]
    public  struct CreateGroupResponse {
        public string success, code;
    }


    // FRIENDS JSON
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

    /// GROUPS JSON
     [System.Serializable]
    public  struct GroupsData {
        public GroupData[] data;
    }

    [System.Serializable]
    public  struct GroupData {
        public string code, name;
        public GroupMember[] members;
    }


    [System.Serializable]
    public  struct GroupMember {
        public FriendJson world_user;
    }

    [System.Serializable]
    public  struct FriendsPhones {
        public List<string> numbers;
    }


    public class FriendManagerScript : MonoBehaviour {
        // Start is called before the first frame update

        public GameObject Panel;

        public GameObject EditingButton;
        public GameObject AddFriendButton;
        public GameObject CancelFormButton;
        public GameObject HomeButton;
        public GameObject GroupFriendButton;

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
        public GameObject JoingGroupInputField;

        public GameObject GroupButtonPrefab;

        public GameObject OpenGroupPanel;

        public GameObject InviteFriendInputField;
        private string currentGroupCode = "";
        public GameObject InviteFriendGroupCodeText;

        void Start() {
            username = PlayerPrefs.GetString("username");
            Debug.Log("USSERNAME : " + username);
            roomID = PlayerPrefs.GetString("myRoomID");
            managerScript = GameManager.GetComponent<GameManagerScript>();
            // need to get groups here
            StartCoroutine(GetGroups("https://circles-parellano.herokuapp.com/api/get-world-groups", GroupButtonPrefab, GroupCallback, FriendListContent));
            uiManagerScript = UIManager.GetComponent<UIManagerScript>();
            itemLoader = itemLoaderGO.GetComponent<ItemLoader>();
        }

        void GetContact(string name, string number, string email) {
            Debug.Log("Zzzz the number is " + number);
            InviteFriendInputField.transform.parent.GetComponent<InputField>().text = number;
        }

        IEnumerator GetGroups(string url, GameObject buttonPrefab, GroupButtonCallback callback, GameObject panel) {
            WWWForm form = new WWWForm();
            form.AddField("userID", roomID);
            UnityWebRequest www = UnityWebRequest.Post(url, form);
            yield return www.SendWebRequest();
            if(www.isNetworkError || www.isHttpError) {
                Debug.Log(www.error);
            } else {
                string response = www.downloadHandler.text;
                Debug.Log("zz data : " + response);
                GroupsData groupsData = JsonUtility.FromJson<GroupsData>(response);
                foreach (GroupData group in groupsData.data) {
                    GameObject newButton = Instantiate(buttonPrefab) as GameObject;
                    newButton.transform.GetChild(1).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = group.name;
                    newButton.GetComponent<Button>().onClick.AddListener(()=> {callback(group.name, group.code, group);});
                    // add this trigger start to friend buttons though
                    // newButton.GetComponent<FriendTrackingScript>().TriggerStart(); // do this because once in parent it will not run
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

        public void CreateGroup() {
            uiManagerScript.AddGroupUsername();
        }

        public void JoinGroupForm() {
            uiManagerScript.JoinGroup();
        }

        public void SubmitForm() {
            string text = AddFriendInputField.GetComponent<Text>().text;
            if (text.Trim() == "") {
                AddFriendForm.SetActive(false);
                AddFriendButton.SetActive(true);
                return;
            }
            FriendsPhones friendsPhones = new FriendsPhones(){numbers = new List<string>()};
            uiManagerScript.LoadingGroupCreation();
            StartCoroutine(SendGroupRequest(text, false, friendsPhones, ()=> {uiManagerScript.ResultGroupCreation(true);}, ()=> {uiManagerScript.ResultGroupCreation(false);} ));
        }

        public void JoinGroup() {
            string code = JoingGroupInputField.GetComponent<Text>().text;
            if (code.Trim() == "") {
                uiManagerScript.HideJoinGroupForm();
                return;
            }
            uiManagerScript.LoadingJoin();
            StartCoroutine(JoinGroupRequest(code, FriendListContent, GroupButtonPrefab, GroupCallback));
        }

        public void InviteFriend() {
            var status = ISN_CNContactStore.GetAuthorizationStatus(ISN_CNEntityType.Contacts);
            if(status == ISN_CNAuthorizationStatus.Authorized) {
                AddContacts();
            } else {
                ISN_CNContactStore.RequestAccess(ISN_CNEntityType.Contacts, (result) => {
                    if (result.IsSucceeded) {
                        AddContacts();
                    } else {
                        uiManagerScript.ResultFriendInvite(false, "Contact permission denied. You can go to settings and give spaces access to your contacts so you can send them invitations!");
                    }
                });
            }
        }

        public void AddContacts() {
            FriendsPhones friendsPhones = new FriendsPhones(){numbers = new List<string>()};
            ISN_CNContactStore.ShowContactsPickerUI((result) => {
                uiManagerScript.LoadingInviteFriend();
                if (result.IsSucceeded) {
                    foreach (var contact in result.Contacts) {
                        if (contact.Phones.Count > 0) {
                            string fullNumber = contact.Phones[0].FullNumber;
                            friendsPhones.numbers.Add(fullNumber);
                        }
                    }
                    StartCoroutine(SendInvites(friendsPhones));
                } else {
                    uiManagerScript.ResultFriendInvite(false, "Hmmmm... Server error, try again!");
                }
            });
        }

        IEnumerator SendInvites(FriendsPhones numbers) {
            WWWForm form = new WWWForm();
            form.AddField("userID", roomID);
            form.AddField("numbers", JsonUtility.ToJson(numbers));
            form.AddField("code", currentGroupCode);
            UnityWebRequest www = UnityWebRequest.Post("https://circles-parellano.herokuapp.com/api/invite-friends", form);
            yield return www.SendWebRequest();
            if(www.isNetworkError || www.isHttpError) {
                Debug.Log(www.error);
                uiManagerScript.ResultFriendInvite(false, "Hmmmm... Server error, try again!");
            } else {
                string response = www.downloadHandler.text;
                uiManagerScript.ResultFriendInvite(true, "nn");
                yield return response;
            }
        }


        public delegate void GroupButtonCallback(string groupName, string code, GroupData members);
        public delegate void FriendButtonClickCallback(string id, string username, string worldType);

        public void GroupCallback(string groupName, string code, GroupData group) {
            uiManagerScript.OpenGroup();
            currentGroupCode = code;
            InviteFriendGroupCodeText.GetComponent<TMPro.TextMeshProUGUI>().text = "group code: " + code;
            foreach(GroupMember member in group.members) {
                if (username != member.world_user.user.username) {
                    GameObject newButton = Instantiate(GroupFriendButton) as GameObject;
                    newButton.transform.GetChild(1).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "@" + member.world_user.user.username; 
                    newButton.GetComponent<Button>().onClick.AddListener(()=> {GoToFriendsRoom(member.world_user.id, member.world_user.user.username, member.world_user.world_type);});
                    newButton.transform.SetParent(OpenGroupPanel.transform);     
                    newButton.transform.localScale = new Vector3(1, 1, 1);
                    newButton.SetActive(true);
                }
            }
        }


        public void GoToFriendsRoom(string friendID, string username, string worldType) {
            GoBackToGroups();
            uiManagerScript.GoToFriendsRoom();
            managerScript.GoToRoom(friendID, username, worldType); 
        }

        public void GoBackHome() {
            GoBackToGroups();
            uiManagerScript.GoHomeCallback();
            StartCoroutine(itemLoader.LoadPurchasedItems(roomID));
            managerScript.GoToRoom(roomID, "", PlayerPrefs.GetString("myWorldType"));
        }


     

        public void SendNotification(string externalPlayerID, string message) {
            Dictionary<string, object> notification = new Dictionary<string, object>();
            notification["headings"] = new Dictionary<string, string>() { {"en", "@" + username.ToLower() + " " +  message} };
            notification["contents"] = new Dictionary<string, string>() { {"en",  "Check out their world!"} };
            notification["include_external_user_ids"] = new List<string>() { externalPlayerID };
            OneSignal.PostNotification(notification);
        }


        public IEnumerator SendGroupRequest(string groupName, bool withPhoneNumbers, FriendsPhones numbers, System.Action callbackSuccess, System.Action callbackFailure) {
            WWWForm form = new WWWForm();
            form.AddField("userID", roomID);
            form.AddField("name", groupName);
            form.AddField("numbers", JsonUtility.ToJson(numbers));
            UnityWebRequest www = UnityWebRequest.Post("https://circles-parellano.herokuapp.com/api/create-group-world", form);
            yield return www.SendWebRequest();
            if(www.isNetworkError || www.isHttpError) {
                Debug.Log(www.error);
                callbackFailure();
            } else {
                string response = www.downloadHandler.text;
                GroupData group = JsonUtility.FromJson<GroupData>(response);
                GameObject newButton = Instantiate(GroupButtonPrefab) as GameObject;
                newButton.transform.GetChild(1).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = group.name;
                newButton.GetComponent<Button>().onClick.AddListener(()=> {GroupCallback(group.name, group.code, group);});
                newButton.transform.SetParent(FriendListContent.transform);     
                newButton.transform.localScale = new Vector3(1, 1, 1);  
                callbackSuccess();
                yield return null;
            }
        }


        IEnumerator JoinGroupRequest(string groupCode, GameObject panel, GameObject buttonPrefab, GroupButtonCallback callback) {
            WWWForm form = new WWWForm();
            form.AddField("userID", roomID);
            form.AddField("code", groupCode);
            UnityWebRequest www = UnityWebRequest.Post("https://circles-parellano.herokuapp.com/api/join-group-world", form);
            yield return www.SendWebRequest();
            if(www.isNetworkError || www.isHttpError) {
                Debug.Log(www.error);
                uiManagerScript.ResultGroupJoin(false);
            } else {
                string response = www.downloadHandler.text;
                Debug.Log("zz resp " + response);
                GroupData group = JsonUtility.FromJson<GroupData>(response);
                if (group.name == null) {
                    Debug.Log("zz error ");
                    uiManagerScript.ResultGroupJoin(false);
                } else {
                    GameObject newButton = Instantiate(buttonPrefab) as GameObject;
                    newButton.transform.GetChild(1).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = group.name;
                    newButton.GetComponent<Button>().onClick.AddListener(()=> {callback(group.name, group.code, group);});
                    newButton.transform.SetParent(panel.transform);     
                    newButton.transform.localScale = new Vector3(1, 1, 1);  
                    uiManagerScript.ResultGroupJoin(true);
                    foreach(GroupMember member in group.members) {
                        if (username != member.world_user.user.username) {
                            SendNotificationAndCoins(member.world_user.user.username);
                        }
                    }
                }
                yield return response;
            }
        }

        public void SendNotificationAndCoins(string friendUsername) {
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
            reference.Child("usernameList").Child(friendUsername).GetValueAsync().ContinueWith(task => {
                DataSnapshot snapshot = task.Result;
                Dictionary<string, object> notification = new Dictionary<string, object>();
                notification["headings"] = new Dictionary<string, string>() { {"en", "🚨🚨 We have a new member in your group 🚨🚨" } };
                notification["contents"] = new Dictionary<string, string>() { {"en", "@" + username.ToLower() + " has joined your group! Come say hi 👋" } };
                notification["include_player_ids"] = new List<string>() { snapshot.Value.ToString().Trim() };
                OneSignal.PostNotification(notification);
                reference.Child("users").Child(friendUsername).Child("coins").GetValueAsync().ContinueWith(task2 => {
                    DataSnapshot snapshot2 = task2.Result;
                    int totalCoins = int.Parse(snapshot2.Value.ToString()) + 30;
                    Dictionary<string, object> coinsData = new Dictionary<string, object>() {
                        {"coins", totalCoins},
                    };
                    reference.Child("users").Child(friendUsername).UpdateChildrenAsync(coinsData);
                });
            });
        }

        public void GoBackToGroups() {
            uiManagerScript.BackToGroups();
            foreach(Transform child in OpenGroupPanel.transform) {
                if (child.gameObject.name.ToCharArray()[0] != '1')
                Destroy(child.gameObject);
            }
        }

    }
}
