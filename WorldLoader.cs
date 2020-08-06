using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WorldLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {
        StartCoroutine(PostRequest("http://127.0.0.1:8000/api/test-unity"));
    }


    IEnumerator PostRequest(string url) {
        WWWForm form = new WWWForm();
        form.AddField("myField", "myData");
 
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();
 
        if(www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
        }
        else {
            string response = www.downloadHandler.text;
            Debug.Log("Form upload complete! Text: " + response);
            yield return response;

        }
    }

    // Update is called once per frame
    void Update() {
        
    }
}
