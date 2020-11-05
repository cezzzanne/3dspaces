using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spaces {
    public class UIManagerScript : MonoBehaviour {
        public GameObject panel, closePanelButton;

        public GameObject editWorld,goBackHome, openPanelButton;
        public GameObject OpenTabsToggle;

        public GameObject SpeakingPanel;

        public GameObject EditOrGoHomePanel;

        string currentRoomID;

        private bool mapActivated;
        private bool inMyRoom;

        private bool tabsSpread;

        private bool mapIsToggled;

        public GameObject confirmItemB, nextItemB, prevItemB, placeItemB, clearItemB, quitSelectorB, rotateB, ItemNameB;

        private bool isEditing = false;
        public GameObject joystick;

        public GameObject PurchasedItemsB, RegularItemsB;

        private bool browsingPurchased = false;

        public GameObject NextSkinB, PreviousSkinB, CancelSkinB, ConfirmSkinB, WardrobeB;

        private bool changingCharacter = false;

        public GameObject TopPanel, FriendsPanel, RequestsPanel; 

        private bool friendsPanelOpen = true;

        public GameObject AddFriendB, AddFriendForm, LoadingFriendReqB, SuccessFriendReqB, ErrorFriendReqB;

        public GameObject InviteFriendB, InviteFriendForm, SuccessFriendInviteB, ErrorFriendInviteB, LoadingFriendInvite;

        public GameObject JoinGroupB, JoinGroupForm, SuccessJoinGroup, ErrorJoinGroup, LoadingJoinGroup;

        public GameObject GroupFriendsPanel;

        public GameObject InGroupsTopTab, InRequestsTopTab, BackToGroupsTopTab;

        public GameObject MovePanel, PlacePanel, ShadowMove, ShadowPlace;

        public GameObject MainMenuCharacterSelect, MainMenuCharacterBack, BackToMainMenuCharacterSelect, ArmPanel, TorsoPanel, FacePanel;

        public Dictionary<string, GameObject> CharacterPanels;

        public GameObject CharacterChange;

        public GameObject AccessoryName, AccessoryNameShadow;

        public Dictionary<string, GameObject> menuSelectors;

        public GameObject ArmS, ShoulderS, HandsS, BackpackS, HolsterS, ExtraS, HariS, CapS;

        public GameObject NoAccesoriesButton;

        string specificType;

        public GameObject RotateCharacterB;

        void Start() {
            inMyRoom = PlayerPrefs.GetString("currentRoomID") == PlayerPrefs.GetString("myRoomID");
            SetInitialState();
            CharacterPanels = new Dictionary<string, GameObject>() {
            {"Arm", ArmPanel},
            {"Torso", TorsoPanel},
            {"Face", FacePanel},
            };
            menuSelectors = new Dictionary<string, GameObject>() {
                {"Arm", ArmS},
                {"Shoulder", ShoulderS},
                {"Hands", HandsS},
                {"Backpack", BackpackS},
                {"Holster", HolsterS},
                {"Extra", ExtraS},
                {"Hair", HariS},
                {"Cap", CapS},
                {"Skin", ArmS}
            };
        }

        public void SetInitialState() {
            panel.SetActive(false);
            TopPanel.SetActive(false);
            FriendsPanel.SetActive(true);
            RequestsPanel.SetActive(false);
            tabsSpread = false;
            mapIsToggled = false;
            openPanelButton.SetActive(false);
            closePanelButton.SetActive(false);
            SpeakingPanel.SetActive(false);
            OpenTabsToggle.SetActive(true);
            EditOrGoHomePanel.SetActive(false);
            if (inMyRoom) {
                goBackHome.SetActive(false);
            } else {
                editWorld.SetActive(false);
                goBackHome.SetActive(true);
            }        
        }

        public void ActivateEditing() {
            editWorld.SetActive(true);
        }

        public void TogglePanel(bool open) {
            if (open) {
                panel.SetActive(true);
                TopPanel.SetActive(true);
                closePanelButton.SetActive(true);
                SpeakingPanel.SetActive(false);
                OpenTabsToggle.SetActive(false);
                SpeakingPanel.SetActive(false);
                EditOrGoHomePanel.SetActive(false);
                openPanelButton.SetActive(false);
            } else {
                panel.SetActive(false);
                TopPanel.SetActive(false);
                closePanelButton.SetActive(false);
                OpenTabsToggle.SetActive(true);
            }
        }

        public void GoToFriendsRoom() {
            inMyRoom = false;
            SetInitialState();
            BackToGroups();
        }

        public void GoHomeCallback() {
            inMyRoom = true;
            SetInitialState();
            editWorld.SetActive(true);
        }

        public void SpreadTabs() {
            if (tabsSpread) {
                SpeakingPanel.SetActive(false);
                EditOrGoHomePanel.SetActive(false);
                openPanelButton.SetActive(false);
                tabsSpread = false;
            } else {
                SpeakingPanel.SetActive(true);
                EditOrGoHomePanel.SetActive(true);
                openPanelButton.SetActive(true);
                tabsSpread = true;
            }
        }

        public void ToggleBrowsing(bool isbrowsingPurchased) {
            PurchasedItemsB.SetActive(isbrowsingPurchased);
            RegularItemsB.SetActive(!isbrowsingPurchased);
            browsingPurchased = isbrowsingPurchased;
        }

        public void ToggleEditing() {
            if (isEditing) {
                OpenTabsToggle.SetActive(true);
                confirmItemB.SetActive(false);
                ItemNameB.SetActive(false);
                rotateB.SetActive(false);
                nextItemB.SetActive(false);
                prevItemB.SetActive(false);
                PurchasedItemsB.SetActive(false);
                RegularItemsB.SetActive(false);
                quitSelectorB.SetActive(false);
                joystick.SetActive(true);
            } else {
                OpenTabsToggle.SetActive(false);
                if (tabsSpread) {
                    SpreadTabs();
                }
                confirmItemB.SetActive(true);
                ItemNameB.SetActive(true);
                nextItemB.SetActive(true);
                prevItemB.SetActive(true);
                ToggleBrowsing(browsingPurchased);
                quitSelectorB.SetActive(true);
                joystick.SetActive(false);
            }
            isEditing = !isEditing;
        }

        public void IsPlacingItem() {
            //
            // placeItemB.SetActive(true);
            // clearItemB.SetActive(true);
            // joystick.SetActive(true);
            // rotateB.SetActive(true);
            joystick.SetActive(false);
            MovePanel.SetActive(true);
            ShadowMove.SetActive(true);
            PlacePanel.SetActive(true);
            ShadowPlace.SetActive(true);
            PlacePanel.SetActive(true);
            confirmItemB.SetActive(false);
            ItemNameB.SetActive(false);
            nextItemB.SetActive(false);
            prevItemB.SetActive(false);
            quitSelectorB.SetActive(false);
            PurchasedItemsB.SetActive(false);
            RegularItemsB.SetActive(false);
        }

        public void PlacedItem() {
            MovePanel.SetActive(false);
            PlacePanel.SetActive(false);
            ShadowMove.SetActive(false);
            ShadowPlace.SetActive(false);
            placeItemB.SetActive(false);
            clearItemB.SetActive(false);
            rotateB.SetActive(false);
            ToggleEditing();
        }

        public void FinishedEditing() {
            //
        }

        public void ToggleWardrobe(bool open) {
            WardrobeB.SetActive(open);
        }

        public void ToggleCharacterChange() {
            joystick.SetActive(changingCharacter);
            OpenTabsToggle.SetActive(changingCharacter);
            WardrobeB.SetActive(changingCharacter);
            if (tabsSpread && !changingCharacter) {
                    SpreadTabs();
            }
            ConfirmSkinB.SetActive(!changingCharacter);
            CancelSkinB.SetActive(!changingCharacter);
            MainMenuCharacterSelect.SetActive(!changingCharacter);
            RotateCharacterB.SetActive(!changingCharacter);
            MainMenuCharacterBack.SetActive(!changingCharacter);
            PreviousSkinB.SetActive(false);
            NextSkinB.SetActive(false);
            if (changingCharacter) {
                SetInitialState();
            }
            changingCharacter = !changingCharacter;
        }

        public void ToggleRequestAndFriendsPanel() {
            RequestsPanel.SetActive(friendsPanelOpen);
            FriendsPanel.SetActive(!friendsPanelOpen);
            friendsPanelOpen = !friendsPanelOpen;
        }

        public void AddGroupUsername() {
            AddFriendB.SetActive(false);
            AddFriendForm.SetActive(true);
        }

        public void JoinGroup() {
            JoinGroupB.SetActive(false);
            JoinGroupForm.SetActive(true);
        }

        public void HideJoinGroupForm() {
            JoinGroupB.SetActive(true);
            JoinGroupForm.SetActive(false);
        }

        public void HideInviteFriendForm() {
            InviteFriendB.SetActive(true);
            InviteFriendForm.SetActive(false);
        }

        public void LoadingInviteFriend() {
            InviteFriendB.SetActive(false);
            LoadingFriendInvite.SetActive(true);
        }

        public void ResultFriendInvite(bool success, string message) {
            InviteFriendB.SetActive(false);
            LoadingFriendInvite.SetActive(false);
            if (success) {
                SuccessFriendInviteB.SetActive(true);
            } else {
                ErrorFriendInviteB.transform.GetChild(1).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = message;
                ErrorFriendInviteB.SetActive(true);
            }
            StartCoroutine(RevertInvite(success));
        }
        public void LoadingGroupCreation() {
            AddFriendForm.SetActive(false);
            LoadingFriendReqB.SetActive(true);
        }

        public void ResultGroupCreation(bool success) {
            LoadingFriendReqB.SetActive(false);
            if (success) {
                SuccessFriendReqB.SetActive(true);
            } else {
                ErrorFriendReqB.SetActive(true);
            }
            StartCoroutine(RevertGroupCreation(success));
        }

        public void LoadingJoin() {
            JoinGroupForm.SetActive(false);
            LoadingJoinGroup.SetActive(true);
        }

        // refactor and make more abstract all this code


        public void ResultGroupJoin(bool success) {
            LoadingJoinGroup.SetActive(false);
            if (success) {
                SuccessJoinGroup.SetActive(true);
            } else {
                ErrorJoinGroup.SetActive(true);
            }
            StartCoroutine(RevertGroupJoin(success));
        }

        IEnumerator RevertGroupCreation(bool success) {
            yield return new WaitForSeconds(3);
            SuccessFriendReqB.SetActive(false);
            ErrorFriendReqB.SetActive(false);
            AddFriendB.SetActive(true);
            if (success) {
                GoToGroups();
            }
        }

        IEnumerator RevertGroupJoin(bool success) {
            yield return new WaitForSeconds(3);
            SuccessJoinGroup.SetActive(false);
            ErrorJoinGroup.SetActive(false);
            JoinGroupB.SetActive(true);
            if (success) {
                GoToGroups();
            }   
        }

        IEnumerator RevertInvite(bool success) {
            yield return new WaitForSeconds(3);
            SuccessFriendInviteB.SetActive(false);
            ErrorFriendInviteB.SetActive(false);
            InviteFriendB.SetActive(true); 
        }

        public void OpenGroup() {
            GroupFriendsPanel.SetActive(true);
            FriendsPanel.SetActive(false);
            RequestsPanel.SetActive(false); // only for precaution
            BackToGroupsTopTab.SetActive(true);
            InGroupsTopTab.SetActive(false);
        }

        public void GoToRequests() {
            InGroupsTopTab.SetActive(false);
            InRequestsTopTab.SetActive(true);
            RequestsPanel.SetActive(true);
            FriendsPanel.SetActive(false);
        }

        public void GoToGroups() {
            FriendsPanel.SetActive(true);
            RequestsPanel.SetActive(false);
            InRequestsTopTab.SetActive(false);
            InGroupsTopTab.SetActive(true);
        }

        public void BackToGroups() {
            // add invite to group button at top of group
            BackToGroupsTopTab.SetActive(false);
            InGroupsTopTab.SetActive(true);
            GroupFriendsPanel.SetActive(false);
            FriendsPanel.SetActive(true);
        }

        public void BackToCharacterselect() {
            NextSkinB.SetActive(false);
            PreviousSkinB.SetActive(false);
            ArmPanel.SetActive(false);
            TorsoPanel.SetActive(false);
            FacePanel.SetActive(false);
            MainMenuCharacterSelect.SetActive(true);
            MainMenuCharacterBack.SetActive(true);
            RotateCharacterB.SetActive(true);
            BackToMainMenuCharacterSelect.SetActive(false);
            CancelSkinB.SetActive(true);
            ConfirmSkinB.SetActive(true);
            AccessoryName.SetActive(false);
            AccessoryNameShadow.SetActive(false);
            NoAccesoriesButton.SetActive(false);
            menuSelectors[specificType].SetActive(false);
        }

        public void BrowseTypeOfAccessory(string type) {
            AccessoryName.SetActive(true);
            AccessoryNameShadow.SetActive(true);
            string mainType = type.Split('-')[0];
            specificType = type.Split('-')[1];
            BackToMainMenuCharacterSelect.SetActive(true);
            MainMenuCharacterSelect.SetActive(false);
            CancelSkinB.SetActive(false);
            ConfirmSkinB.SetActive(false);
            if (mainType != "Skin") {
                CharacterPanels[mainType].SetActive(true);
            } else {
                AccessoryName.SetActive(false);
                AccessoryNameShadow.SetActive(false);
                MainMenuCharacterBack.SetActive(false);
            }
            BrowseSpecificAccessory(specificType);
        }

        public void BrowseSpecificAccessory(string type) {
            menuSelectors[specificType].SetActive(false);
            menuSelectors[type].SetActive(true);
            NoAccesoriesButton.SetActive(false);
            specificType = type;
            NextSkinB.SetActive(true);
            PreviousSkinB.SetActive(true);
            CharacterChange.GetComponent<CharacterChange>().SetBrowsingType(type);
        }
    }
}
