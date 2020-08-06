using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Pun;
using System;
using UnityEngine.UI;
public class ChatManager : MonoBehaviour, IChatClientListener {

    ChatClient chatClient;

    public List<GameObject> playerList;


    string currentMessage = "";

    public void DebugReturn(DebugLevel level, string message) {
        if (level == ExitGames.Client.Photon.DebugLevel.ERROR)
		{
			Debug.LogError(message);
		}
		else if (level == ExitGames.Client.Photon.DebugLevel.WARNING)
		{
			Debug.LogWarning(message);
		}
		else
		{
			Debug.Log(message);
		}
    }

    public void OnChatStateChange(ChatState state) {
        Debug.Log(state.ToString());
    }

    public void OnConnected() {
        Debug.Log("CONNECTED TO CHAT");
        string[] channels = new string[1];
        channels[0] = "Pablos chat";
        chatClient.Subscribe(channels);
    }

    public void SendToChannel (string message) {
        currentMessage = message;
        SendMessage();
    }

    public void SendMessage() {
        chatClient.PublishMessage("Pablos chat", id.ToString() + ":" + currentMessage);
    }

    public void OnDisconnected()
    {
        throw new System.NotImplementedException();
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages) {
        foreach(object message in messages) {
            char[] divider = ":".ToCharArray();
            Debug.Log("DIVIDER: " + divider[0]);
            string[] contents = message.ToString().Split(divider[0]);
            Debug.Log(contents[0]);
            int userID = int.Parse(contents[0]);
            string newMessage = contents[1];
            Debug.Log("GOT MESSAGE:  " + newMessage + " FROM: " + userID);
            GameObject player = PhotonNetwork.PlayerList[userID].TagObject as GameObject;
            Debug.Log("player: " + player);
            Debug.Log("canvas : " + player.GetComponentInChildren<Canvas>());
            Text playerText = player.GetComponentInChildren<Canvas>().GetComponentInChildren<Text>();
            playerText.text = newMessage;
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        throw new System.NotImplementedException();
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        throw new System.NotImplementedException();
    }

    public void OnSubscribed(string[] channels, bool[] results) {
        foreach (string channel in channels) {
			Debug.Log("joined " + channel);
		}
    }

    public void OnUnsubscribed(string[] channels)
    {
        throw new System.NotImplementedException();
    }

    public void OnUserSubscribed(string channel, string user)
    {
        throw new System.NotImplementedException();
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        throw new System.NotImplementedException();
    }

    [SerializeField]
    public int id;

    void Start() {
        id = PhotonNetwork.PlayerList.Length - 1;
        Debug.Log("players currently here: " + id);
        Application.runInBackground = true;
        chatClient = new ChatClient(this);
        chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, PhotonNetwork.AppVersion, new AuthenticationValues(id.ToString()));
        // playerList = new List<GameObject>();
    }

    public void ChangeID() {
        int debugid = id;
        id--;
        Debug.Log("CHANGING ID FROM:  " + debugid + " TO : " + id);
    }

    void Update() {
        if (chatClient != null) {
            chatClient.Service();
        }
    }

    // public void AddPlayer(GameObject player) {
    //     Debug.Log("added player to chat manaager : " + player.name);
    //     playerList.Add(player);
    // }
}
