using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Voice.PUN;
using Photon.Voice.Unity;

namespace Spaces {
    public class GameManagerScript : MonoBehaviourPunCallbacks {
        // Start is called before the first frame update
        // public GameObject ChatManager;

        public CharacterScript[] PlayerPrefabs;

        private CharacterScript PlayerPrefab;

        [HideInInspector]
        public CharacterScript LocalPlayer;

        private int index;

        GameObject Voice;

        int roomCount = 1;

        string roomIDToJoin;

        public GameObject ModTerrainPrefab;

        public SaveSystem SaveSystem;
        public GameObject EditRoomButton;
        public GameObject GoBackHomeButton;
        public GameObject LoadingScreen;
        string currentUsername;

        public GameObject CurrentRoomUsername;


        void Awake() {
            // if (!PhotonNetwork.IsConnected) {
            //     SceneManager.LoadScene("scene2");
            // }
        }

        void Start() {
            LoadingScreen.SetActive(true);
            roomIDToJoin = PlayerPrefs.GetString("currentRoomID");
            string currentSkin = PlayerPrefs.GetString("CurrentSkin");
            GameObject playerPrefab = Resources.Load<GameObject>("Characters/" + currentSkin);
            Debug.Log("current skin: " + currentSkin);
            Debug.Log("current prefab: " + playerPrefab);
            PlayerPrefab = playerPrefab.GetComponent<CharacterScript>();;
            PhotonNetwork.OfflineMode = false;
            PhotonNetwork.NickName = "Pablo";
            PhotonNetwork.GameVersion = "v1";
            if (PhotonNetwork.IsConnected) {
                OnClickConnectRoom();
            } else {
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        public override void OnConnectedToMaster() {
            base.OnConnectedToMaster();
            Debug.Log("connected to master");
            OnClickConnectRoom();
        }
       public void OnClickConnectRoom() {
            Debug.Log("IN JOIN ROOM NAME : " + roomIDToJoin);
            PhotonNetwork.JoinRoom(roomIDToJoin);
        }

        public override void OnJoinedRoom() {
            Debug.Log("successfully joined a room: IN: " + PhotonNetwork.CurrentRoom.Name);
            base.OnJoinedRoom();
            CharacterScript.RefreshInstance(ref LocalPlayer, PlayerPrefab);
            SetUpNewTerrain();
        }

        public override void OnJoinRoomFailed(short returnCode, string message) {
            Debug.Log("failed to join room");
            base.OnJoinRoomFailed(returnCode, message);
            Debug.Log(message);
            Debug.Log("going to create room now");
            PhotonNetwork.CreateRoom(roomIDToJoin);
            Voice = GameObject.FindGameObjectWithTag("Voice");
        }

        public override void OnCreateRoomFailed(short returnCode, string message) {
            Debug.Log("failed to create room");
            base.OnCreateRoomFailed(returnCode, message);
            Debug.Log("failed to create room");
        }

        public void DisconnectPlayer() {
            StartCoroutine(DisconnectAndLoad());
        }

        IEnumerator DisconnectAndLoad() {
            PhotonVoiceNetwork.Instance.Disconnect();
            PhotonNetwork.Disconnect();
            while (PhotonNetwork.IsConnected) {
                Debug.Log("In room");
                yield return null;
            }
            SceneManager.LoadScene("WorldEdit");
        }

        
        public override void OnPlayerEnteredRoom(Photon.Realtime.Player player) {
            base.OnPlayerEnteredRoom(player);
            Debug.Log("player has tag object "  + player.TagObject);
            // ChatManager manager = ChatManager.GetComponent<ChatManager>();
            // maybe don't need to refresh instance?
            // CharacterScript.RefreshInstance(ref LocalPlayer, PlayerPrefab);
        }

        public void GoToRoom(string newRoomID, string username) {
            LoadingScreen.SetActive(true);
            if (roomIDToJoin == newRoomID) {
                return;
            }
            PlayerPrefs.SetString("currentRoomID", newRoomID);
            PlayerPrefs.SetString("currentRoomUsername", username);
            PlayerPrefab.DestroyCamera();
            roomIDToJoin = newRoomID;
            currentUsername = username;
            // PhotonVoiceNetwork.Instance.Disconnect();
            PhotonNetwork.LeaveRoom();
        }

        public void SetUpNewTerrain() {
            string myRoom = PlayerPrefs.GetString("myRoomID");
            if (myRoom == roomIDToJoin) {
                EditRoomButton.SetActive(true);
                GoBackHomeButton.SetActive(false);
                CurrentRoomUsername.SetActive(false);
            } else {
                EditRoomButton.SetActive(false);
                GoBackHomeButton.SetActive(true);
                CurrentRoomUsername.SetActive(true);
                if (currentUsername == null) {
                    currentUsername = PlayerPrefs.GetString("currentRoomUsername");
                }
                CurrentRoomUsername.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "@" + currentUsername.ToLower();
            }
            GameObject oldTerrain = GameObject.FindGameObjectWithTag("ModifiedTerrain");
            Destroy(oldTerrain);
            GameObject newTerrain = Instantiate(ModTerrainPrefab) as GameObject;
            if (!(transform.childCount > 0)) {
                SaveSystem.LoadSpace(roomIDToJoin, newTerrain, null);
            }
            LoadingScreen.SetActive(false);
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) {
            Debug.Log("player leaving");
            base.OnPlayerLeftRoom(otherPlayer);
            CharacterScript otherScript = (otherPlayer.TagObject as GameObject).GetComponent<CharacterScript>();
            // otherScript.DestroyCamera();
            // ChatManager manager = ChatManager.GetComponent<ChatManager>();
            // if (manager.id > otherScript.id) {
            //     manager.ChangeID();
            // }
            PhotonNetwork.DestroyPlayerObjects(otherPlayer);   
        }

    }
}
