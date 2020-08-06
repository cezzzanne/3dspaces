using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spaces {
    public class CharacterSelection : MonoBehaviour {

        public GameObject Character;
        public GameObject Camera;
        public GameObject LoadingScreen;

        void Start() {
            LoadingScreen.SetActive(true);
            string currentSkin = PlayerPrefs.GetString("CurrentSkin");
            Debug.Log("current skin: " + currentSkin);
            string path = "Characters/Materials/" + currentSkin;
            Debug.Log("path: " + path);
            Material currentMaterial = Resources.Load<Material>(path) as Material;
            Debug.Log("the material : " + currentMaterial);
            Character.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().material =  currentMaterial;
        }

        public void SetPosition() {
            string editingPos = PlayerPrefs.GetString("editingPosition");
            if (editingPos != "") {
                Debug.Log("has position");
                Debug.Log(editingPos);
                string[] pos = editingPos.Split(':');
                Debug.Log(pos[0]);
                Debug.Log("Y POS: " + pos[1]);
                float yPos = float.Parse(pos[1]);
                if (yPos < 3.0f) {
                    yPos = 0;
                }
                transform.position = new Vector3(float.Parse(pos[0]), yPos, float.Parse(pos[2]));
            }
             if (Character) {
                Character.SetActive(true);
                Character.transform.position = new Vector3(0, 0, 0);
            }
            Camera.GetComponent<EditingCameraFollow>().SetTarget(Character.transform);
            LoadingScreen.SetActive(false);
        }


    void Update() {
    }

    }
}
