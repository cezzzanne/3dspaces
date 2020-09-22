using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spaces {
    public class NotificationInitScript : MonoBehaviour {
        void Start () {
            // Uncomment this method to enable OneSignal Debugging log output 
            // OneSignal.SetLogLevel(OneSignal.LOG_LEVEL.INFO, OneSignal.LOG_LEVEL.INFO);
            
            // Replace 'YOUR_ONESIGNAL_APP_ID' with your OneSignal App ID.
            OneSignal.StartInit("73edd87b-7555-4075-b728-5a27c4fb1a9f")
                .HandleNotificationOpened(HandleNotificationOpened)
                .Settings(new Dictionary<string, bool>() {
                { OneSignal.kOSSettingsAutoPrompt, false },
                { OneSignal.kOSSettingsInAppLaunchURL, false } })
                .EndInit();
            OneSignal.inFocusDisplayType = OneSignal.OSInFocusDisplayOption.Notification;
            
            // The promptForPushNotifications function code will show the iOS push notification prompt. We recommend removing the following code and instead using an In-App Message to prompt for notification permission.
            OneSignal.PromptForPushNotificationsWithUserResponse(OneSignal_promptForPushNotificationsResponse);
        }

        private static void HandleNotificationOpened(OSNotificationOpenedResult result) {
        }

        private void OneSignal_promptForPushNotificationsResponse(bool accepted) {
            Debug.Log("OneSignal_promptForPushNotificationsResponse: " + accepted);
        }
    }
}
