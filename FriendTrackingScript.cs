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

        void OnApplicationQuit() {
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
            reference.Child("users").Child(username).ValueChanged -= HandleLocationUpdate;
        }
        void OnDestroy() {
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
            reference.Child("users").Child(username).ValueChanged -= HandleLocationUpdate;
        }

        public void TriggerStart() {
            username = transform.GetChild(0).GetComponent<Text>().text.Split('@')[1];
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
            reference.Child("users").Child(username).ValueChanged += HandleLocationUpdate;
            notificaiton = GameObject.FindGameObjectWithTag("NotificationManager").GetComponent<InnerNotifManagerScript>();
        }

        private void HandleLocationUpdate(object sender, ValueChangedEventArgs arg) {
            if (this == null) {
                DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
                reference.Child("users").Child(username).ValueChanged -= HandleLocationUpdate;
                return;  
            }
            Dictionary<string, object> data = arg.Snapshot.Value as Dictionary<string, object>;
            string locationString = FormatString(data);
            transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = locationString;
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
                notificaiton.SendNotification("@" + username + " is at " + place);
            }
            initialSetup = false;
            return when + " " + place;
        }


        void Update() {
            
        }
    }
}
