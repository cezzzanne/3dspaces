using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Spaces {
    public class SendMessageButton : MonoBehaviour {
        string currentText = "";

        public GameObject buttonText;

        public GameObject inputField;

        ChatManager ChatManager;

        InputHandler inputHandler;

        void Start() {
            GameObject canvas = GameObject.FindGameObjectWithTag("Canvas");
            inputHandler = canvas.GetComponent<InputHandler>();
        }

        public void ModifyText(string newText) {
            if (newText == "") {
                buttonText.GetComponent<Text>().text = "cancel";
                currentText = newText;
            } else {
                buttonText.GetComponent<Text>().text = "send";
                currentText = newText;
            }
        }

        public void SendMessage() {
            if (currentText == "") {
                StopTyping();
                return;
            }
            string finalText = currentText.Trim();
            if (finalText == "") {
                return;
            }
            inputField.GetComponent<InputField>().SetTextWithoutNotify("");
            ModifyText("");
            EventSystem.current.SetSelectedGameObject(inputField, null);
            inputHandler.SendNewMessage(finalText);
        }

        public void StopTyping() {
            inputField.GetComponent<InputField>().SetTextWithoutNotify("");
            inputHandler.CloseTyping();
        }

    }
}
