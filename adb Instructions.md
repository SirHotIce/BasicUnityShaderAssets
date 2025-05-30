
```bash
adb shell am broadcast -a com.yourcompany.DISABLE_XR_CAMERA

```

…it will disable a GameObject called  `camera`  in your Unity scene.

----------

## Step-by-Step Breakdown

----------

###  1.  **Unity Script**

Attach this script to any GameObject (e.g. an empty GameObject named  `Receiver`):

```csharp
using UnityEngine;

public class XRCommandReceiver : MonoBehaviour
{
    public GameObject camera; // Assign in Inspector

    // Called from Java via UnityPlayer.UnitySendMessage
    public void OnDisableXRCamera(string message)
    {
        Debug.Log("Disabling XR Camera from ADB");
        if (camera != null)
        {
            camera.SetActive(false);
        }
    }
}

```

>  Make sure the GameObject this script is attached to is named  **Receiver**  (case-sensitive) or change the name in the Java call accordingly.

----------

### 2.  **Java Plugin (BroadcastReceiver)**

Create a Java class called  `XRCommandReceiver.java`:

```java
package com.yourcompany.xrplugin;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.util.Log;
import com.unity3d.player.UnityPlayer;

public class XRCommandReceiver extends BroadcastReceiver {
    @Override
    public void onReceive(Context context, Intent intent) {
        if ("com.yourcompany.DISABLE_XR_CAMERA".equals(intent.getAction())) {
            Log.d("XRCommandReceiver", "Disabling XR motion camera via Unity");
            UnityPlayer.UnitySendMessage("Receiver", "OnDisableXRCamera", "triggered");
        }
    }
}

```

----------

###  3.  **Register the Receiver in  `AndroidManifest.xml`**

Add this inside the  `<application>`  tag:

```xml
<receiver android:name="com.yourcompany.xrplugin.XRCommandReceiver">
    <intent-filter>
        <action android:name="com.yourcompany.DISABLE_XR_CAMERA" />
    </intent-filter>
</receiver>

```

----------

### 4.  **Package Plugin as  `.aar`**

If you need help packaging the plugin:

-   Use Android Studio →  `File > New > New Module > Android Library`
    
-   Copy the  `.aar`  to  `Assets/Plugins/Android/`  in Unity
    

Let me know if you want a starter  `.aar`  structure.

----------

### 5.  **Test from ADB**

Once your Unity app is running on a device or emulator:

```bash
adb shell am broadcast -a com.yourcompany.DISABLE_XR_CAMERA

```

This should:

-   Trigger your Java receiver
    
-   Call  `UnitySendMessage("Receiver", "OnDisableXRCamera", "triggered")`
    
-   Disable the assigned  `camera`  GameObject
    

----------
