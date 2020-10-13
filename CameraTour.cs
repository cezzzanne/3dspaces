using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraTour : MonoBehaviour {
    // Start is called before the first frame update

    public GameObject intro, editWorld, editCharacter, finalText;
    public GameObject character;

    private string characterSkin;
    bool firstTimeInCameraToWorld = true;

    public IEnumerator RotateCam() {
        // yield return null;
        // t += Time.deltaTime;
        // mainCamera.transform.position = new Vector3(368.21f, 4.2f, 434.57f);
        // Quaternion desiredRot = Quaternion.Euler(15, 0.366f, 0);
        // Quaternion tempRot = mainCamera.transform.rotation;
        // Quaternion desiredPlace = Quaternion.Euler(mainCamera.transform.rotation.x + 15, mainCamera.transform.rotation.y + 0.366f, mainCamera.transform.rotation.z); // Quaternion.Slerp(mainCamera.transform.rotation, desiredRot);
        Quaternion des = Quaternion.Euler(15, 0, 0);
        while (Quaternion.Angle(transform.rotation, des) > 2) {
            yield return new WaitForSeconds(0.01f);
            Quaternion to = Quaternion.Lerp(transform.rotation, des, Time.deltaTime);
            transform.rotation = to;
        }
        Debug.Log("done rotating cam");
        intro.SetActive(true);
        yield return new WaitForSeconds(10);
        intro.SetActive(false);
        editWorld.SetActive(true);
        yield return new WaitForSeconds(10);
        editWorld.SetActive(false);
        StartCoroutine(MoveCameraToCloset());

    }

    IEnumerator MoveCameraToCloset() {
        Vector3 des = new Vector3(24.8344f, 308.1046f, 24.33046f);
        while ((transform.position - des).magnitude > 0.2) {
            yield return new WaitForSeconds(0.01f);
            transform.position = Vector3.Lerp(transform.position, new Vector3(24.8344f, 308.1046f, 24.33046f), Time.deltaTime);
        }
        Debug.Log("done moving to closet");
        editCharacter.SetActive(true);
        yield return new WaitForSeconds(10);
        editCharacter.SetActive(false);
        firstTimeInCameraToWorld = false;
        StartCoroutine(MoveCameraToWorld(characterSkin)); // pass it again because I need to not because I want to
    }

    public IEnumerator MoveCameraToWorld(string characterSelected) {
        characterSkin = characterSelected;
        character.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().material = Resources.Load("Characters/Materials/" + characterSkin) as Material;
        Vector3 des = new Vector3(16, 304, -3);
        while ((transform.position - des).magnitude > 0.2) {
            yield return new WaitForSeconds(0.004f);
            transform.position = Vector3.Lerp(transform.position, new Vector3(16, 304, -3), Time.deltaTime);
        }
        if (firstTimeInCameraToWorld) {
            StartCoroutine(RotateCam());
        } else {
            finalText.SetActive(true);
        }
    }

    public void ReadyPlayerOne() {
        Debug.Log("is finished and load scene");
        SceneManager.LoadScene("MainGame");
    }
}
