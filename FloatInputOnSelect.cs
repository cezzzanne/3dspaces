using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class FloatInputOnSelect : MonoBehaviour, IPointerClickHandler{
    public GameObject SignUpManager;

    public void OnPointerClick(PointerEventData eventData) {
        SignUpManager manager = SignUpManager.GetComponent<SignUpManager>();
        string name = gameObject.name;
        if (name == "username") {
            manager.SetInput(0);
        } else if (name == "email") {
            manager.SetInput(1);
        } else {
            manager.SetInput(2);

        }
    }

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }
}
