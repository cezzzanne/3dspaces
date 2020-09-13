using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Spaces {
    public class CharacterChange : MonoBehaviour {
        // Start is called before the first frame update

        public GameObject character;

        public GameObject currentSkin;

        private List<Material> skins;
        
        private int index = 0;

        private CharacterScript characterScript;


        void Start() {
            skins = new List<Material>();
            string currentSkin = PlayerPrefs.GetString("CurrentSkin");
            Material material = Resources.Load<Material>("Characters/Materials/" + currentSkin) as Material;
            skins.Add(material);
            SetSkin();
        }

        public void AddToAvailableSkins(StoreItem item) {
            Debug.Log("xxx skin added " + item.location);
            Material material = Resources.Load<Material>(item.location) as Material;
            skins.Add(material);
        }

        public void SetSkin() {
            character.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().material = skins[index];
        }

        public void NextSkin() {
            int newIndex = index + 1;
            if (newIndex > skins.Count - 1) {
                newIndex = 0;
            }
            index = newIndex;
            SetSkin();
        }

        public void PreviousSkin() {
            int newIndex = index - 1;
            if (newIndex < 0) {
                newIndex = skins.Count - 1;
            }
            index = newIndex;
            SetSkin();
        }

        public void SetTargetCharacter(CharacterScript charScript) {
            characterScript = charScript;
        }


        // have to send it to all players in room through RPC Func, have to update my character, have to set it in playerprefs
        public void ConfirmSkin() {
            PlayerPrefs.SetString("CurrentSkin", skins[index].name);
            characterScript.ChangeSkin(skins[index]);
        }

    }
}
