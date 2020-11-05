using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;

public class PublicWorldWelcomeScript : MonoBehaviour {
    // Start is called before the first frame update
    public GameObject WelcomePanel;

    public string worldName;

    //test

    public GameObject MatterMostConscript;

    public int matterMostGreen = -1;

    void Start() {
        StartCoroutine(LateStart());
        CronscriptyMattermost();
        StartCoroutine(WithMatter());
    }

    IEnumerator LateStart() {
        yield return new WaitForSeconds(5);
        int worldsVisited = PlayerPrefs.GetInt("firstVisit");
        if (worldName == "Moon") {
            if (worldsVisited == 1 || worldsVisited == 3) {
                WelcomePanel.SetActive(true);
                worldsVisited += 3;
                PlayerPrefs.SetInt("firstVisit", worldsVisited);
            }
        } else {
            if (worldsVisited == 1 || worldsVisited == 4) {
                WelcomePanel.SetActive(true);
                worldsVisited += 2; //idk why i came up with this way of differentiating visiting world but avoid stashing more into local cache/playerprefs
                PlayerPrefs.SetInt("firstVisit", worldsVisited);
            }
        }

    }
    public void ClosePanel() {
        WelcomePanel.SetActive(false);
    }

    public void CronscriptyMattermost() {
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        reference.Child("withMatter").GetValueAsync().ContinueWith(task => {
            DataSnapshot snapshot = task.Result;
            string val = snapshot.Value.ToString();
            Debug.Log("zzzval " + val);
            matterMostGreen = (val == "yes") ? 1 : 0;
        });
    }

    IEnumerator WithMatter() {
        while (matterMostGreen == -1) {
            yield return null;
        }
        if (matterMostGreen == 1) {
            MatterMostConscript.SetActive(true);
        } else {
            MatterMostConscript.SetActive(false);
        }
    }
}
