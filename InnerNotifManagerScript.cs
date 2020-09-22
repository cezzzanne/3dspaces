using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using UnityEngine.UI;
using Firebase;
using Firebase.Unity.Editor;
using System;


namespace Spaces {
    public class InnerNotifManagerScript : MonoBehaviour {
        public GameObject GoldCoinPrefab;

        public GameObject notification;

        private GameObject goldCoin;
        private Transform character;
        private bool displayingCoin = false;
        private string username, roomID;
        private int coins;
        private string lastRequest, consecutiveDays = "";
        // Start is called before the first frame update
        void Start() {
            FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://spaces-d9a3c.firebaseio.com/");
            goldCoin = Instantiate(GoldCoinPrefab);
            goldCoin.SetActive(false);
        }

        void Update() {
            if (character && displayingCoin) {
                // need to set gold coin active before displaying
                SetCoin();
                TurnCoin();
            }
        }

        void SetCoin() {
            Vector3 pos = character.transform.position;
            pos.y = pos.y + 2.1f;
            goldCoin.transform.position = pos;
        }

        void TurnCoin() {
            goldCoin.transform.RotateAround(goldCoin.transform.position, new Vector3(0 , 1, 0), 2f);
        }

        public void SetCharacterTarget(Transform characterTransform, string pUsername, string pRoomID) {
            character = characterTransform;
            username = pUsername;
            roomID = pRoomID;
            CheckForCoins();
        }

        void CheckForCoins() {
            StartCoroutine(GetAmountOfCoins());
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
            reference.Child("users").Child(username).Child("coinsInfo").GetValueAsync().ContinueWith(task => {
                DataSnapshot snapshot = task.Result;
                if (!snapshot.Exists) {
                    Debug.Log("zz no user with that username");
                    return;
                }
                Dictionary<string, object> coinsData = snapshot.Value as Dictionary<string, object>;
                lastRequest = coinsData["LastRequest"].ToString();
                consecutiveDays = coinsData["ConsecutiveDays"].ToString();
            });
        }

        IEnumerator GetAmountOfCoins() {
            while (consecutiveDays == "") {
                yield return null;
            }
            int amountOfCoins = 0;
            float multiplier = 1 + (0.2f * int.Parse(consecutiveDays));
            if (lastRequest == "none") {
                amountOfCoins = 20;
                lastRequest = DateTime.Now.ToString();
                AddNewCoins(amountOfCoins);
            } else {
                DateTime dt = Convert.ToDateTime(lastRequest);
                TimeSpan ts = DateTime.Now - dt;
                Debug.Log("zz hours " + ts.Hours);
                if (ts.Days > 0 || ts.Hours >= 12) {
                    amountOfCoins = Mathf.CeilToInt(15 * multiplier);
                    AddNewCoins(amountOfCoins);
                } else {
                }
            }
        }

        void AddNewCoins(int coinsAdded) {
            string message = "$" + coinsAdded.ToString() + " new coins added to your account!";
            StartCoroutine(DisplayNotification(message, true));
            AddNewFirebaseCoins(coinsAdded);
        }

        void AddNewFirebaseCoins(int coinsAdded) {
            DateTime dt = Convert.ToDateTime(lastRequest);
            TimeSpan ts = DateTime.Now - dt;
            int newConsecDays = (ts.Days < 1) ?  int.Parse(consecutiveDays) + 1 : 0;
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
            reference.Child("users").Child(username).Child("coins").GetValueAsync().ContinueWith(task => {
                DataSnapshot snapshot = task.Result;
                Debug.Log("zz value : " + snapshot.Value);
                int totalCoins = int.Parse(snapshot.Value.ToString()) + coinsAdded;
                Dictionary<string, object> coinsInfo = new Dictionary<string, object>() {
                    {"LastRequest", DateTime.Now.ToString()},
                    {"ConsecutiveDays", newConsecDays.ToString()}
                };
                Dictionary<string, object> coinsData = new Dictionary<string, object>() {
                    {"coins", totalCoins},
                    {"coinsInfo", coinsInfo}
                };
                reference.Child("users").Child(username).UpdateChildrenAsync(coinsData);
            });
        }

        public IEnumerator DisplayNotification(string message, bool withCoin) {
            yield return new WaitForSeconds(2);
            float width = notification.transform.GetComponent<RectTransform>().position.x;
            RectTransform rect = notification.transform.GetComponent<RectTransform>();
            int distance = 5;
            rect.position = new Vector3(0 - (distance * 50), rect.position.y, rect.position.z);
            notification.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = message;
            notification.SetActive(true);
            while (rect.position.x < -15) {
                yield return new WaitForSeconds(0.01f);
                rect.position = new Vector3(rect.position.x + (distance * 3), rect.position.y, rect.position.z);
            }
            displayingCoin = withCoin ? true : false;
            goldCoin.SetActive(displayingCoin);
            yield return new WaitForSeconds(4);
            while (rect.position.x > -(distance * 120)) {
                yield return new WaitForSeconds(0.01f);
                rect.position = new Vector3(rect.position.x - (distance * 3), rect.position.y, rect.position.z);
            }
            notification.SetActive(false);
            displayingCoin = false;
            goldCoin.SetActive(false);
        }

        public void SendNotification(string message) {
            StartCoroutine(DisplayNotification(message, false));
        }
    }

}
