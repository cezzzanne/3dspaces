using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SA.iOS.Contacts;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Spaces {
    public class IntroCreateGroup : MonoBehaviour {
        public GameObject FirstStep, SecondStep, ThirdStep, MainPanel;
        public GameObject LoadingInvites, ErrorInvites, SuccessInvites, ContactSelectButton;
        public GameObject ErrorInvitesText, GroupNameText, GroupButtonPrefab;
        public GameObject FriendManager;
        string groupName = "";
        private int currentStep = 0;
        void Start() {
            StartCoroutine(LateStart());
        }

        IEnumerator LateStart() {
            yield return new WaitForSeconds(4);
            int firstVisit = PlayerPrefs.GetInt("firstVisit", -1);
            if (firstVisit == -1) {
                MainPanel.SetActive(true);
                PlayerPrefs.SetInt("firstVisit", 1);
            }
        }

        public void CancelGroupCreate() {
            MainPanel.SetActive(false);
        }

        public void GoToNextStep() {
            if (currentStep == 0) {
                FirstStep.SetActive(false);
                SecondStep.SetActive(true);
                currentStep++;
            } else if (currentStep == 1) {
                groupName = GroupNameText.GetComponent<TMPro.TextMeshProUGUI>().text.Trim();
                SecondStep.SetActive(false);
                ThirdStep.SetActive(true);
                currentStep++;
            }
        }

        public void CheckContacts() {
            var status = ISN_CNContactStore.GetAuthorizationStatus(ISN_CNEntityType.Contacts);
            if(status == ISN_CNAuthorizationStatus.Authorized) {
                OpenContacts();
            } else {
                ISN_CNContactStore.RequestAccess(ISN_CNEntityType.Contacts, (result) => {
                    if (result.IsSucceeded) {
                        OpenContacts();
                    } else {
                            ContactSelectButton.SetActive(false);
                            ErrorInvites.SetActive(true);
                        ErrorInvitesText.GetComponent<TMPro.TextMeshProUGUI>().text = "Contact permission denied. You can go to settings and give spaces access to your contacts so you can send them invitations!";
                        StartCoroutine(CloseModal(6));
                    }
                });
            }
        }

        // clear cache in untiy and start over (see why Im getting stuck in loading window in create group)

        void OpenContacts() {
            FriendsPhones friendsPhones = new FriendsPhones(){numbers = new List<string>()};
            ISN_CNContactStore.ShowContactsPickerUI((result) => {
                ContactSelectButton.SetActive(false);
                LoadingInvites.SetActive(true);
                if (result.IsSucceeded) {
                    foreach (var contact in result.Contacts) {
                        string fullNumber = contact.Phones[0].FullNumber;
                        friendsPhones.numbers.Add(fullNumber);
                    }
                    FriendManagerScript FMScript = FriendManager.GetComponent<FriendManagerScript>();
                    StartCoroutine(FMScript.SendGroupRequest(groupName, true, friendsPhones, ()=> {GroupSuccessCreation();}, ()=> {GroupFailCreation();} ));
                } else {
                    LoadingInvites.SetActive(false);
                    ErrorInvites.SetActive(true);
                    ErrorInvitesText.GetComponent<TMPro.TextMeshProUGUI>().text = "Hmmmm... server error! Try again from the main menu!";
                    StartCoroutine(CloseModal(3));
                }
            });
        }

        void GroupSuccessCreation() {
            LoadingInvites.SetActive(false);
            SuccessInvites.SetActive(true);
            StartCoroutine(CloseModal(3));
        }

        void GroupFailCreation() {
            LoadingInvites.SetActive(false);
            ErrorInvites.SetActive(true);
            ErrorInvitesText.GetComponent<TMPro.TextMeshProUGUI>().text = "There was an error with the numbers you sent the invites to, try again from the main menu!";
            StartCoroutine(CloseModal(6));
        }

        IEnumerator CloseModal(int seconds) {
            yield return new WaitForSeconds(seconds);
            MainPanel.SetActive(false);
        }
    }
}
