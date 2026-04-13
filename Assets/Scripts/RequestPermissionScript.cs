using UnityEngine;
using UnityEngine.Android;

public class RequestPermissionScript : MonoBehaviour
{
    internal void PermissionCallbacks_PermissionGranted(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionCallbacks_PermissionGranted");
    }

    internal void PermissionCallbacks_PermissionDenied(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionCallbacks_PermissionDenied");
        
        // Check if we should show rationale (user denied but can be asked again)
        // If ShouldShowRequestPermissionRationale returns false, it means "Don't Ask Again" was selected
        bool shouldShowRationale = Permission.ShouldShowRequestPermissionRationale(permissionName);
        
        if (!shouldShowRationale)
        {
            Debug.Log($"{permissionName} PermissionDeniedAndDontAskAgain");
        }
    }

    void Start()
    {
        if (Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN"))
        {
            // The user authorized use of the microphone.
        }
        else
        {
            bool useCallbacks = false;
            if (!useCallbacks)
            {
                // We do not have permission to use the microphone.
                // Ask for permission or proceed without the functionality enabled.
                Permission.RequestUserPermission("android.permission.BLUETOOTH_SCAN");
            }
            else
            {
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
                callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
                Permission.RequestUserPermission("android.permission.BLUETOOTH_SCAN", callbacks);
            }
        }
    }
}
