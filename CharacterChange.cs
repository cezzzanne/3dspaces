using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Spaces {
    public class CharacterChange : MonoBehaviour {
        // Start is called before the first frame update

        public GameObject character;

        public string currentSkin;

        private List<Material> skins;
        
        private int index = 0;

        private CharacterScript characterScript;

        private Dictionary<string, List<StoreItem>> accessories;
        private string currentBrowsingType = "";

        private Dictionary<string, string> currentAccessories;  // first is name, second is location;
        StoreItem defaultItem;

        StoreItem currentStoreItem;

        private GameObject currentAccessory;

        private string characterSkin;

        public GameObject currentAccessoryName;
        
        string playerPrefAccessories;

        void Start() {
            skins = new List<Material>();
            defaultItem = new StoreItem();
            defaultItem.name = "None";
            accessories = new Dictionary<string, List<StoreItem>>() {
                {"Arm", new List<StoreItem>() {defaultItem}},
                {"Shoulder", new List<StoreItem>(){defaultItem}},
                {"Hands", new List<StoreItem>() {defaultItem}},
                {"Backpack", new List<StoreItem>() {defaultItem}},
                {"Holster", new List<StoreItem>() {defaultItem}},
                {"Extra", new List<StoreItem>() {defaultItem}},
                {"Hair", new List<StoreItem>() {defaultItem}},
                {"Cap", new List<StoreItem>() {defaultItem}},
            };
            currentAccessories = new Dictionary<string, string>();
            characterSkin = PlayerPrefs.GetString("CurrentSkin");
            currentSkin = characterSkin;
            Material material = Resources.Load<Material>("Characters/Materials/" + characterSkin) as Material;
            skins.Add(material);
            SetSkin();
        }

        void SetUpAccessories() {
            string accessories = playerPrefAccessories;
            if (!accessories.Contains("$")) {
                if (accessories != "") {
                    // one accessory
                    string[] accessoryAttribute = accessories.Split('-');
                    string type = accessoryAttribute[2];
                    string location = accessoryAttribute[3];
                    string bodyLocation = accessoryAttribute[0];
                    currentAccessories[type] = accessories;
                    GameObject acc = Resources.Load<GameObject>("Characters/Accessories/" + location);
                    acc = Instantiate(acc);
                    Transform parent = character.transform.Find(bodyLocation);
                    AllocateAccessory(acc.transform, parent);
                } else {
                    // no accessories
                    return;
                }
            } else {
                // multiple accessories
                string[] allAccessories = accessories.Split('$');
                foreach(string accessory in allAccessories) {
                    string[] accessoryAttribute = accessory.Split('-');
                    string type = accessoryAttribute[2];
                    string location = accessoryAttribute[3];
                    string bodyLocation = accessoryAttribute[0];
                    currentAccessories[type] = accessory;
                    GameObject acc = Resources.Load<GameObject>("Characters/Accessories/" + location);
                    acc = Instantiate(acc);
                    Transform parent = character.transform.Find(bodyLocation);
                    AllocateAccessory(acc.transform, parent);
                }
            }
        }

        public void AddToCharacterObjects(bool isSkin, StoreItem item) {
            if (isSkin) {
                Material material = Resources.Load<Material>(item.location) as Material;
                skins.Add(material);
            } else {
                string accessoryType = item.name.Split('-')[2];
                accessories[accessoryType].Add(item);
            }
        }

        public void SetSkin() {
            character.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().material = skins[index];
            currentSkin = skins[index].name;
        }

        void NextAccessory() {
            List<StoreItem> browseAcc = accessories[currentBrowsingType];
            string name;
            int newIndex = index + 1;
            if (newIndex > browseAcc.Count - 1) {
                newIndex = 0;
            }
            index = newIndex;
            if (currentAccessory != null) {
                RemoveAccessory(currentAccessory);
                currentAccessories.Remove(currentBrowsingType);
            }
            if (browseAcc[index].name != defaultItem.name) {
                StoreItem newItem = browseAcc[newIndex];
                currentAccessories.Add(currentBrowsingType, newItem.name + "-" + newItem.location);
                string location = newItem.name.Split('-')[0];
                name = newItem.name.Split('-')[1];
                currentStoreItem = newItem;
                currentAccessory = Resources.Load<GameObject>("Characters/Accessories/" + newItem.location);
                currentAccessory = Instantiate(currentAccessory);
                Transform parent = character.transform.Find(location);
                AllocateAccessory(currentAccessory.transform, parent);
            } else {
                name = "No item";
            }
            currentAccessoryName.GetComponent<TMPro.TextMeshProUGUI>().text = name;
        }


        void PreviousAccessory() {
            List<StoreItem> browseAcc = accessories[currentBrowsingType];
            string name;
            int newIndex = index - 1;
            if (newIndex < 0) {
                newIndex = browseAcc.Count - 1;
            }
            index = newIndex;
            if (currentAccessory != null) {
                RemoveAccessory(currentAccessory);
                currentAccessories.Remove(currentBrowsingType);
            }
            if (browseAcc[index].name != defaultItem.name) {
                StoreItem newItem = browseAcc[newIndex];
                currentAccessories.Add(currentBrowsingType, newItem.name + "-" + newItem.location); // i need location to be able to instantiate later
                currentStoreItem = newItem;
                string location = newItem.name.Split('-')[0];
                name = newItem.name.Split('-')[1];
                currentAccessory = Resources.Load<GameObject>("Characters/Accessories/" + newItem.location);
                currentAccessory = Instantiate(currentAccessory);
                Transform parent = character.transform.Find(location);
                AllocateAccessory(currentAccessory.transform, parent);
            } else {
                name = "No item";
            }
            currentAccessoryName.GetComponent<TMPro.TextMeshProUGUI>().text = name;
        }

        public void NextItem() {
            if (currentBrowsingType == "Skin") {
                NextSkin();
            } else {
                NextAccessory();
            }
        }


        public void PreviousItem() {
            if (currentBrowsingType == "Skin") {
                PreviousSkin();
            } else {
                PreviousAccessory();
            }
        }

        public void SetBrowsingType(string type) {
            currentAccessory = null;
            currentBrowsingType = type;
            if (currentAccessories.ContainsKey(type)) {
                string location = currentAccessories[type].Split('-')[0];
                string childLocation = location + "/" +  currentAccessories[type].Split('-')[3] + "(Clone)";
                Transform item = character.transform.Find(childLocation);
                currentAccessories.Remove(type);
                Debug.Log("zzz child loc : " + childLocation);
                RemoveAccessory(item.gameObject);
            }
            index = 0;
            currentAccessoryName.GetComponent<TMPro.TextMeshProUGUI>().text = "No item";
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


        public void ConfirmCharacterChanges() {
            string fullAccessoryString = "";
            foreach(KeyValuePair<string, string> item in currentAccessories) {
                fullAccessoryString = (fullAccessoryString != "") ? fullAccessoryString + "$" + item.Value : item.Value;
            }
            PlayerPrefs.SetString("Accessories", fullAccessoryString);
            if (characterSkin != currentSkin) {
                PlayerPrefs.SetString("CurrentSkin", skins[index].name);
                characterScript.ChangeSkin(skins[index]);
            }
            characterScript.UpdateCharacterAccessories(fullAccessoryString);
        }

        public void RemoveAccessory(GameObject accessory) {
            Destroy(accessory);
        }

        public void AllocateAccessory(Transform child, Transform parent) {
            Vector3 pos = child.position;
            Quaternion rot = child.rotation;
            Vector3 scale = child.localScale;
            child.parent = parent;
            child.localPosition = pos;
            child.localRotation = rot;
            child.localScale = scale;
        }

        public void UpdateAccessories(string accessories) {
            playerPrefAccessories = accessories;
            SetUpAccessories();
        }
    }
}
