using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AvatarCreation : MonoBehaviour {
    // Start is called before the first frame update
     private int index = 0;

    public Material[] SkinList;

    public GameObject character;

    private SkinnedMeshRenderer skinnedRenderer;

    private Material[] materialAssigner;

    public GameObject characterName;

    public GameObject redWomanHair, brownWomanHair;
    Text characterText;

    // 1) finsih log in (should be pretty fast)
    // 2) figure out character initial world (make space smaller, maybe even inside a spaceship?)

    void Start() {
        skinnedRenderer = character.GetComponent<SkinnedMeshRenderer>();
        // need to assign array of material
        materialAssigner = new Material[1];
        materialAssigner[0] = SkinList[index];
        skinnedRenderer.materials = materialAssigner;
        characterText = characterName.GetComponent<Text>();
        characterText.text = SkinList[index].name;
        transform.rotation = Quaternion.Euler(0, -90, 0);
    }

    public void NextSkin() {
        if (index == (SkinList.Length - 1)) {
            index = 0;
        } else {
            index += 1;
        }
        transform.rotation = Quaternion.Euler(0, -90, 0);
        string name = SkinList[index].name;
        if ("casualFemaleB" == name) {
            character.transform.parent.GetChild(1).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(1).GetChild(2).gameObject.SetActive(true);
            character.transform.parent.GetChild(1).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(1).GetChild(1).gameObject.SetActive(false);
        } else if ("casualFemaleA" == name) {
            character.transform.parent.GetChild(1).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(1).GetChild(1).gameObject.SetActive(true);
            character.transform.parent.GetChild(1).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(1).GetChild(2).gameObject.SetActive(false);
        } else {
            character.transform.parent.GetChild(1).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(1).GetChild(1).gameObject.SetActive(false);
            character.transform.parent.GetChild(1).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(1).GetChild(2).gameObject.SetActive(false);

        }
        materialAssigner[0] = SkinList[index];
        skinnedRenderer.materials = materialAssigner;
        characterText.text = SkinList[index].name;
    }

    public void PreviousSkin() {
        if (index == 0) {
            index =  SkinList.Length - 1;
        } else {
            index -= 1;
        }
        string name = SkinList[index].name;
        if ("casualFemaleB" == name) {
            character.transform.parent.GetChild(1).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(1).GetChild(2).gameObject.SetActive(true);
            character.transform.parent.GetChild(1).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(1).GetChild(1).gameObject.SetActive(false);
        } else if ("casualFemaleA" == name) {
            character.transform.parent.GetChild(1).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(1).GetChild(1).gameObject.SetActive(true);
            character.transform.parent.GetChild(1).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(1).GetChild(2).gameObject.SetActive(false);
        } else {
            character.transform.parent.GetChild(1).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(1).GetChild(1).gameObject.SetActive(false);
            character.transform.parent.GetChild(1).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(1).GetChild(2).gameObject.SetActive(false);

        }
        transform.rotation = Quaternion.Euler(0, -90, 0);
        materialAssigner[0] = SkinList[index];
        skinnedRenderer.materials = materialAssigner;
        characterText.text = SkinList[index].name;
    }

    public string SelectedCharacter() {
        return SkinList[index].name;
    }

    void Update() {
        transform.RotateAround(transform.position, new Vector3(0 ,1, 0), 0.2f);
    }
}
