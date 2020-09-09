using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;

[System.Serializable]
public struct JSONItem {
    public string name;
    
    public float x_pos, y_pos, z_pos, rotation_y;
}

[System.Serializable]
public struct TerrainData {
        public JSONItem[] items;
        public float player_x, player_y, player_z, rotation_y;
};

[System.Serializable]
public  struct SpacesDataJson {
    public string success, wUserID;

    public TerrainData terrain_data;
    
}
public class SaveSystem: MonoBehaviour {




    public void SaveSpace(GameObject terrain, Transform player, int id) {
        // BinaryFormatter formatter = new BinaryFormatter();
        // string path = Application.persistentDataPath + "/space.oasis";
        // FileStream stream = new FileStream(path, FileMode.Create);
        SpaceData data = new SpaceData(terrain, player, id);
        StartCoroutine(MakeRequestSaveData("https://circles-parellano.herokuapp.com/api/save-world", data));
    } 


    static IEnumerator MakeRequestSaveData(string url, SpaceData data) {
        WWWForm form = new WWWForm();
        string world = JsonUtility.ToJson(data.world);
        form.AddField("userID", PlayerPrefs.GetString("myRoomID"));
        form.AddField("data", world);
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();
        if(www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
        } else {
            string response = www.downloadHandler.text;
            yield return response;
        }
    }

     static IEnumerator MakeRequestLoadData(string url, string roomID, GameObject modifiedTerrain, Action HandleNewObj) {
        WWWForm form = new WWWForm();
        Debug.Log("GOING INTO REQUEST");
        form.AddField("userID", roomID);
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();
        if(www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
        }
        else {
            string response = www.downloadHandler.text;
            yield return response;
            Debug.Log("Form upload complete! Text: " + response);
            SpacesDataJson SpacesJson = JsonUtility.FromJson<SpacesDataJson>(response);
            TerrainData terrainData = SpacesJson.terrain_data;
            foreach(JSONItem item in terrainData.items) {
                string name = item.name.Substring(0, item.name.Length - 7);
                GameObject prefab = Resources.Load<GameObject>("TownPrefabs/" + name);
                GameObject currentItem = Instantiate(prefab) as GameObject;
                // Rigidbody rBody = currentItem.AddComponent<Rigidbody>();
                currentItem.transform.position = new Vector3(item.x_pos, item.y_pos, item.z_pos);
                currentItem.transform.Rotate(currentItem.transform.rotation.x, item.rotation_y, currentItem.transform.rotation.z);
                currentItem.transform.SetParent(modifiedTerrain.transform);
            }
            Debug.Log("loaded all");
            if (HandleNewObj != null) {
                HandleNewObj();
            }
            yield return SpacesJson;
        }
    }

    public void LoadSpace(string roomID, GameObject modifiedTerrain, Action HandleNewObj) {
        StartCoroutine(MakeRequestLoadData("https://circles-parellano.herokuapp.com/api/get-world", roomID, modifiedTerrain, HandleNewObj));
        
        // string path = Application.persistentDataPath + "/space.oasis";
        // if (File.Exists(path)) {
        //     BinaryFormatter formatter = new BinaryFormatter();
        //     FileStream stream = new FileStream(path, FileMode.Open);
        //     SpaceData data = formatter.Deserialize(stream) as SpaceData;
        //     stream.Close();
        //     return data;
        // } else {
        //     Debug.Log("NO FILE FOUND IN " + path);
        //     return null;
        // }
    }
}
