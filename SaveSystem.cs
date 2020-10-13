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

    private string myUserID = "";

    public void SaveSpace(Dictionary<string, object> data, Transform player, int id) {
        // SpaceData data = new SpaceData(terrain, player, id);
        StartCoroutine(MakeRequestSaveData("https://circles-parellano.herokuapp.com/api/save-world", data, id));
    } 


    static IEnumerator MakeRequestSaveData(string url, Dictionary<string, object> data, int id) {
        WWWForm form = new WWWForm();
        string world = JsonUtility.ToJson(data);
        form.AddField("userID", id.ToString());
        form.AddField("name", data["name"] as string);
        form.AddField("xPos", data["xPos"].ToString());
        form.AddField("yPos", data["yPos"].ToString());
        form.AddField("zPos", data["zPos"].ToString());
        form.AddField("yRot", data["yRot"].ToString());
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();
        if(www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
            Debug.Log("zzzz error ");
        } else {
            string response = www.downloadHandler.text;
            Debug.Log("zzzz response" + response);
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
                GameObject prefab;
                GameObject currentItem;
                try {
                    prefab = Resources.Load<GameObject>("TownPrefabs/" + name);
                    currentItem = Instantiate(prefab) as GameObject;
                } catch {
                    prefab = Resources.Load<GameObject>("StoreItems/" + name);
                    currentItem = Instantiate(prefab) as GameObject;
                }
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
    }
}
