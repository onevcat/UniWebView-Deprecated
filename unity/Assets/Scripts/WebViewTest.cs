using UnityEngine;
using System.Collections;

public class WebViewTest : MonoBehaviour {

	public GUISkin guiSkin;
	public GameObject redBoxPrefab;
	public GameObject blueBoxPrefab;

	public string Url;
	// private WebViewObject webViewObject;

	private string note;

	// Use this for initialization
	void Start()
	{
		// webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
		WebViewObject.Instance.Init((msg)=>{
			Debug.Log(string.Format("CallFromJS[{0}]", msg));
		});
	}
	
	void Update() {
	    if (WebViewObject.Instance.visibility) {
	        ProcessMessages();
	    } else if (Input.GetButtonDown("Fire1") && Input.mousePosition.y < Screen.height / 2) {
	        ActivateWebView();
	    }
	}

	private void ActivateWebView() {
		WebViewObject.Instance.LoadURL(Url);
		WebViewObject.Instance.SetVisibleWithMargins(12, Screen.height / 2 + 12, 12, 12);
	}

	private void DeactivateWebView() {
    	WebViewObject.Instance.SetInvisible();
    	// Clear the state of the web view (by loading a blank page).
    	WebViewObject.Instance.LoadURL("about:blank");
	}

	private void ProcessMessages() {
	    while (true) {
	        // Poll a message or break.
	        WebViewObjectMessage message = WebViewObject.Instance.PollMessage();
	        if (message == null) break;

	        if (message.path == "/spawn") {
	            // "spawn" message.
	            GameObject prefab = null;
	            if (message.args.ContainsKey("color")) {
	            	string receiveColor = message.args["color"] as string;
	                prefab = (receiveColor == "red") ? redBoxPrefab : blueBoxPrefab;
	            } else {
	                prefab = Random.value < 0.5 ? redBoxPrefab : blueBoxPrefab;
	            }
	            GameObject box = Instantiate(prefab, redBoxPrefab.transform.position, Random.rotation) as GameObject; 
	            if (message.args.ContainsKey("scale")) {
	                box.transform.localScale = Vector3.one * float.Parse(message.args["scale"] as string);
	            }
	        } else if (message.path == "/note") {
	            // "note" message.
	            note = message.args["text"] as string;
	        } else if (message.path == "/print") {
	            // "print" message.
	            string text = message.args["line1"] as string;
	            if (message.args.ContainsKey("line2")) {
	                text += "\n" + message.args["line2"] as string;
	            }
	            Debug.Log(text);
	            Debug.Log("(" + text.Length + " chars)");
	        } else if (message.path == "/close") {
	            // "close" message.
	            DeactivateWebView();
	        }
	    }
	}


	void OnGUI() {
    	float sw = Screen.width;
    	float sh = Screen.height;
    	GUI.skin = guiSkin;
    	if (note != null) {
    		GUI.Label(new Rect(0f, 0f, sw, 0.5f * sh), note);	
    	}
    
    	GUI.Label(new Rect(0f, 0.5f * sh, sw, 0.5f * sh), "TAP HERE", "center");
	}
}
