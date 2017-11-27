using UnityEngine;
using UnityEngine.UI;

#if UNITY_IOS
using UnityEngine.iOS;
using UnityEngine.Apple.ReplayKit;
#endif

public class ReplayKitHelper : MonoBehaviour {

    public Button button;
    public Text text;

    void Start () {
#if UNITY_IOS
        button.interactable = ReplayKit.APIAvailable;
        if (!button.interactable)
            text.text = "ReplayKit unavailable.";
#else
        button.interactable = false;
        text.text = "ReplayKit unavailable: not an iOS device.";
#endif
    }
	
	void OnBroadcastButtonPressed () {
#if UNITY_IOS
        button.interactable = ReplayKit.APIAvailable;
        if (!button.interactable)
            text.text = "ReplayKit unavailable.";
        else{
		    if (ReplayKit.isBroadcasting) {
                ReplayKit.StopBroadcasting();
                text.text = "Start broadcasting";
            } else {
                ReplayKit.StartBroadcasting((bool success, string error) => Debug.Log(string.Format("Start : {0}, error : `{1}`", success, error)));
                text.text = "Stop broadcasting";
            }
        }
#endif
    }
}


