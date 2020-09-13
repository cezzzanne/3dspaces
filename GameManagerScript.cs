﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using System;

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
        public GameObject LoadingScreen;
        string currentUsername;

        public GameObject CurrentRoomUsername;
        private bool initialConnection = true;
        private bool reconnect = false;

        private GameObject currentTerrain;
        private GameObject currentPrebuiltTerrain;
        private string currentWorldType;
        private string previousWorldType;
        public GameObject ItemController;
        // world type is based on the type of world the user posseses (3 kinds) // roomID is to join the same photon room based on player id


        void Awake() {
            int inPublic = PlayerPrefs.GetInt("isInPublicWorld", -1);
            if (inPublic == -1) {
                PlayerPrefs.SetInt("isInPublicWorld", 0);
                inPublic = 0;
            } 
            if (inPublic == 1) {
                PlayerPrefs.SetInt("isInPublicWorld", 0);
            }
        }

        void OnApplicationFocus(bool focus) {
            if (focus && !initialConnection) {
                StartCoroutine(CheckIfDisconnected());
            }
        }

        IEnumerator CheckIfDisconnected() {
            yield return new WaitForSeconds(1);
            Debug.Log("111: application is focused now");
            Debug.Log("111 is connected and ready: " + PhotonNetwork.IsConnectedAndReady);
            Debug.Log("111: network state: " + PhotonNetwork.NetworkClientState);
            if (!PhotonNetwork.IsConnectedAndReady) {
                LoadingScreen.SetActive(true);
                PhotonNetwork.ConnectUsingSettings();
                reconnect = true;
            }
        }

        void Start() {
            LoadingScreen.SetActive(true);
            currentWorldType =  PlayerPrefs.GetString("currentWorldType");
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
            initialConnection = false;
            if (reconnect) {
                Debug.Log("111: reconnecting active");
                reconnect = false;
                LoadingScreen.SetActive(false);
                return;
            }
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

        public void GoToPublicWorld(string worldName) {
            StartCoroutine(DisconnectToPublicWorld(worldName));
        }

        IEnumerator DisconnectToPublicWorld(string worldName) {
            PhotonVoiceNetwork.Instance.Disconnect();
            PhotonNetwork.Disconnect();
            while (PhotonNetwork.IsConnected) {
                Debug.Log("In room");
                yield return null;
            }
            PlayerPrefs.SetInt("isInPublicWorld", 1);
            SceneManager.LoadScene("RacingWorld");
        }

        
        public override void OnPlayerEnteredRoom(Photon.Realtime.Player player) {
            base.OnPlayerEnteredRoom(player);
            Debug.Log("player has tag object "  + player.TagObject);
            // ChatManager manager = ChatManager.GetComponent<ChatManager>();
            // maybe don't need to refresh instance?
        }

        public void GoToRoom(string newRoomID, string username, string worldType) {
            // change currentWorldType to parameter of the next world type and previous world type to current before
            // worldType = "TestWorld";
            previousWorldType = currentWorldType;
            currentWorldType = worldType;
            LoadingScreen.SetActive(true);
            if (roomIDToJoin == newRoomID) {
                return;
            }
            PlayerPrefs.SetString("currentRoomID", newRoomID);
            PlayerPrefs.SetString("currentRoomUsername", username);
            PlayerPrefs.SetString("currentWorldType", worldType);
            PlayerPrefab.DestroyCamera();
            roomIDToJoin = newRoomID;
            currentUsername = username;
            // PhotonVoiceNetwork.Instance.Disconnect();
            PhotonNetwork.LeaveRoom();
        }

        public void SetUpNewTerrain() {
            string myRoom = PlayerPrefs.GetString("myRoomID");
            // check if same terrain type as previous (first check its not null // actually maybe itll just default to false if null)
            if (currentWorldType != previousWorldType) {
                if (currentTerrain != null && currentPrebuiltTerrain != null) {
                    Destroy(currentPrebuiltTerrain);
                    Destroy(currentTerrain);
                }
                GameObject terrain = Resources.Load<GameObject>("Worlds/" + currentWorldType + "-Terrain");
                GameObject prebuiltTerrain = Resources.Load<GameObject>("Worlds/" + currentWorldType + "-PrebuiltTerrain");
                currentTerrain = Instantiate(terrain);
                ItemController.GetComponent<ItemPlacementController>().SetTerrain(currentTerrain);
                currentPrebuiltTerrain = Instantiate(prebuiltTerrain);
            }
            if (myRoom == roomIDToJoin) {
                CurrentRoomUsername.SetActive(false);
            } else {
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
