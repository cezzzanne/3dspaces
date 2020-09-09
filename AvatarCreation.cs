using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AvatarCreation : MonoBehaviour {
    // Start is called before the first frame update
     private int index = 0;

    public Material[] Skins;

    public GameObject character;

    private SkinnedMeshRenderer skinnedRenderer;

    private Material[] materialAssigner;

    public GameObject characterName;
    Text characterText;

    // 1) finsih log in (should be pretty fast)
    // 2) figure out character initial world (make space smaller, maybe even inside a spaceship?)

    void Start() {
        skinnedRenderer = character.GetComponent<SkinnedMeshRenderer>();
        // need to assign array of material
        materialAssigner = new Material[1];
        materialAssigner[0] = Skins[index];
        skinnedRenderer.materials = materialAssigner;
        characterText = characterName.GetComponent<Text>();
        characterText.text = Skins[index].name;
        transform.rotation = Quaternion.Euler(0, -90, 0);
    }

    public void NextSkin() {
        if (index == (Skins.Length - 1)) {
            index = 0;
        } else {
            index += 1;
        }
        transform.rotation = Quaternion.Euler(0, -90, 0);
        materialAssigner[0] = Skins[index];
        skinnedRenderer.materials = materialAssigner;
        characterText.text = Skins[index].name;
    }

    public void PreviousSkin() {
        if (index == 0) {
            index =  Skins.Length - 1;
        } else {
            index -= 1;
        }
        transform.rotation = Quaternion.Euler(0, -90, 0);
        materialAssigner[0] = Skins[index];
        skinnedRenderer.materials = materialAssigner;
        characterText.text = Skins[index].name;
    }

    public string SelectedCharacter() {
        return Skins[index].name;
    }

    void Update() {
        transform.RotateAround(transform.position, new Vector3(0 ,1, 0), 0.2f);
    }
}
