using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;
using System;

namespace Spaces {
    public class FriendTrackingScript : MonoBehaviour {

        string username;

        InnerNotifManagerScript notificaiton;

        private bool initialSetup = true;
        private string locationString = "";

        void Start() {
            username = transform.GetChild(1).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text.Split('@')[1];
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
            StartCoroutine(UpdateStatus());
            reference.Child("users").Child(username).GetValueAsync().ContinueWith(task => {
                DataSnapshot snapshot = task.Result;
                Debug.Log("zz snapshot " + snapshot.Value);
                Dictionary<string, object> data = snapshot.Value as Dictionary<string, object>;
                locationString = FormatString(data);
            });
            // notificaiton = GameObject.FindGameObjectWithTag("NotificationManager").GetComponent<InnerNotifManagerScript>();
        }

        IEnumerator UpdateStatus() {
            while (locationString == "") {
                yield return null;
            }
            transform.GetChild(1).GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = locationString;
        }

        string FormatString(Dictionary<string, object> data) {
            string place = " ";
            string when = " ";
            foreach (KeyValuePair<string, object> pair in data) {
                if (pair.Key == "LastSeen") {
                    when =  pair.Value.ToString();
                    if (when == "0") {
                        when = "sleeping in";
                    } else if (when == "1") {
                        when = "currently in";
                    } else {
                        DateTime dateTime = Convert.ToDateTime(when);
                        TimeSpan ts = DateTime.Now - dateTime;
                        string hoursOrMinutes;
                        if (ts.Hours < 1 && ts.Days < 1) {
                            hoursOrMinutes = ts.Minutes.ToString() + " minutes ago in";
                        } else if (ts.Days > 0) {
                            hoursOrMinutes = ts.Days.ToString() + " days ago in";
                        } else {
                            hoursOrMinutes = ts.Hours.ToString() + " hours ago in";
                        }
                        when = "seen " + hoursOrMinutes;
                    }
                } else if (pair.Key == "Place") {
                    place = pair.Value.ToString();
                }
            }
            if (when == "currently in" && !initialSetup) {
                // notificaiton.SendNotification("@" + username + " is at " + place);
            }
            initialSetup = false;
            return when + " " + place;
        }
    }
}
