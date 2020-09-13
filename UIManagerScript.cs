using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public GameObject confirmItemB, nextItemB, prevItemB, placeItemB, clearItemB, quitSelectorB, rotateB;

    private bool isEditing = false;
    public GameObject joystick;

    public GameObject PurchasedItemsB, RegularItemsB;

    private bool browsingPurchased = false;

    public GameObject NextSkinB, PreviousSkinB, CancelSkinB, ConfirmSkinB, WardrobeB;

    private bool changingCharacter = false;

    void Start() {
        inMyRoom = PlayerPrefs.GetString("currentRoomID") == PlayerPrefs.GetString("myRoomID");
        SetInitialState();
    }

    public void SetInitialState() {
        panel.SetActive(false);
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
            closePanelButton.SetActive(true);
            SpeakingPanel.SetActive(false);
            OpenTabsToggle.SetActive(false);
            SpeakingPanel.SetActive(false);
            EditOrGoHomePanel.SetActive(false);
            openPanelButton.SetActive(false);
        } else {
            panel.SetActive(false);
            closePanelButton.SetActive(false);
            OpenTabsToggle.SetActive(true);
        }
    }

    public void FriendCallback() {
        inMyRoom = false;
        SetInitialState();
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
        placeItemB.SetActive(true);
        clearItemB.SetActive(true);
        joystick.SetActive(true);
        rotateB.SetActive(true);
        confirmItemB.SetActive(false);
        nextItemB.SetActive(false);
        prevItemB.SetActive(false);
        quitSelectorB.SetActive(false);
        PurchasedItemsB.SetActive(false);
        RegularItemsB.SetActive(false);
    }

    public void PlacedItem() {
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
        PreviousSkinB.SetActive(!changingCharacter);
        NextSkinB.SetActive(!changingCharacter);
        if (changingCharacter) {
            SetInitialState();
        }
        changingCharacter = !changingCharacter;
    }

}
