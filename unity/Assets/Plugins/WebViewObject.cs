/*
 * @onevcat 
 * This software is provided 'as-is', without any express or implied
 * warranty.  In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Callback = System.Action<string>;

public class WebViewObjectMessage {
    public string path;      // Message path
    public Hashtable args;   // Argument table

    public WebViewObjectMessage(string rawMessage) {
        // Retrieve a path.
        string[] split = rawMessage.Split("?"[0]);
        path = split[0];
        // Parse arguments.
        args = new Hashtable();
        if (split.Length > 1) {
            foreach (string pair in split[1].Split("&"[0])) {
                string[] elems = pair.Split("="[0]);
                args[elems[0]] = WWW.UnEscapeURL(elems[1]);
            }
        }
    }
}

#if UNITY_EDITOR
public class UnitySendMessageDispatcher {
	public static void Dispatch(string name, string method, string message)
	{
		GameObject obj = GameObject.Find(name);
		if (obj != null)
			obj.SendMessage(method, message);
	}
}

public class WebViewObject : MonoBehaviour {
	Callback callback;
	IntPtr webView;
	public bool visibility;
	Rect rect;
	Texture2D texture;
	string inputString;
	private static WebViewObject _instance = null;
	public static WebViewObject Instance
	{
		get 
		{ 
			if (_instance == null) {
				GameObject go = new GameObject("WebViewObject");
				DontDestroyOnLoad(go);
				_instance = go.AddComponent<WebViewObject>();
			}
			return _instance; 
		}
	}

	[DllImport("WebView")]
	private static extern IntPtr _WebViewPlugin_Init(
		string gameObject, int width, int height, bool ineditor);
	[DllImport("WebView")]
	private static extern int _WebViewPlugin_Destroy(IntPtr instance);
	[DllImport("WebView")]
	private static extern void _WebViewPlugin_SetRect(
		IntPtr instance, int width, int height);
	[DllImport("WebView")]
	private static extern void _WebViewPlugin_SetVisibility(
		IntPtr instance, bool visibility);
	[DllImport("WebView")]
	private static extern void _WebViewPlugin_LoadURL(
		IntPtr instance, string url);
	[DllImport("WebView")]
	private static extern void _WebViewPlugin_EvaluateJS(
		IntPtr instance, string url);
	[DllImport("WebView")]
	private static extern void _WebViewPlugin_Update(IntPtr instance,
		int x, int y, float deltaY, bool down, bool press, bool release,
		bool keyPress, short keyCode, string keyChars, int textureId);
	[DllImport("WebView")]
	private static extern string _WebViewPluginPollMessage();

	private void CreateTexture(int x, int y, int width, int height) {
		if (Application.platform != RuntimePlatform.OSXEditor) {
			return;
		}
		int w = 1;
		int h = 1;
		while (w < width)
			w <<= 1;
		while (h < height)
			h <<= 1;
		rect = new Rect(x, y, width, height);
		texture = new Texture2D(w, h, TextureFormat.ARGB32, false);
	}

	public void Init(Callback cb = null) {
		if (Application.platform != RuntimePlatform.OSXEditor) {
			return;
		}
		callback = cb;
		CreateTexture(0, 0, Screen.width, Screen.height);
		webView = _WebViewPlugin_Init(name, Screen.width, Screen.height,
			Application.platform == RuntimePlatform.OSXEditor);
	}

	public WebViewObjectMessage PollMessage() {
		if (Application.platform != RuntimePlatform.OSXEditor) {
			return null;
		}
    	string message =  _WebViewPluginPollMessage();
    	if (message != null) {Debug.Log(message);}
    	return (message != null) ? new WebViewObjectMessage(message) : null;
	}

	void OnDestroy() {
		if (Application.platform != RuntimePlatform.OSXEditor) {
			return;
		}
		if (webView == IntPtr.Zero)
			return;
		_instance = null;
		_WebViewPlugin_Destroy(webView);
	}

	public void SetVisibleWithMargins(int left, int top, int right, int bottom)
	{
		//Must set margins before setting the visibility.
		//If first visibility and then margin, Unity will crash in GUI.DrawTexture(rect, texture);
		SetMargins(left, top, right, bottom);
		SetVisibility(true);
	}

	public void SetInvisible()
	{
		SetVisibility(false);
	}

	private void SetMargins(int left, int top, int right, int bottom) {
		if (Application.platform != RuntimePlatform.OSXEditor) {
			return;
		}
		if (webView == IntPtr.Zero)
			return;
		int width = Screen.width - (left + right);
		int height = Screen.height - (bottom + top);
		CreateTexture(left, bottom, width, height);
		_WebViewPlugin_SetRect(webView, width, height);
	}

	private void SetVisibility(bool v) {
		if (Application.platform != RuntimePlatform.OSXEditor) {
			return;
		}
		if (webView == IntPtr.Zero)
			return;
		visibility = v;
		_WebViewPlugin_SetVisibility(webView, v);
	}

	public void LoadURL(string url) {
		if (Application.platform != RuntimePlatform.OSXEditor) {
			return;
		}
		if (webView == IntPtr.Zero)
			return;
		_WebViewPlugin_LoadURL(webView, url);
	}

	public void EvaluateJS(string js)
	{
		if (Application.platform != RuntimePlatform.OSXEditor) {
			return;
		}
		if (webView == IntPtr.Zero)
			return;
		_WebViewPlugin_EvaluateJS(webView, js);
	}

	public void CallFromJS(string message)
	{
		if (Application.platform != RuntimePlatform.OSXEditor) {
			return;
		}
		if (callback != null)
			callback(message);
	}

	void Update()
	{
		if (Application.platform != RuntimePlatform.OSXEditor) {
			return;
		}
		inputString += Input.inputString;
	}

	void OnGUI()
	{

		if (Application.platform != RuntimePlatform.OSXEditor) {
			return;
		}
		if (webView == IntPtr.Zero || !visibility)
			return;

		Vector3 pos = Input.mousePosition;
		bool down = Input.GetButton("Fire1");
		bool press = Input.GetButtonDown("Fire1");
		bool release = Input.GetButtonUp("Fire1");
		float deltaY = Input.GetAxis("Mouse ScrollWheel");
		bool keyPress = false;
		string keyChars = "";
		short keyCode = 0;
		if (inputString.Length > 0) {
			keyPress = true;
			keyChars = inputString.Substring(0, 1);
			keyCode = (short)inputString[0];
			inputString = inputString.Substring(1);
		}

		_WebViewPlugin_Update(webView,
			(int)(pos.x - rect.x), (int)(pos.y - rect.y), deltaY,
			down, press, release, keyPress, keyCode, keyChars,
			texture.GetNativeTextureID());
		GL.IssuePluginEvent((int)webView);
		Matrix4x4 m = GUI.matrix;
		GUI.matrix = Matrix4x4.TRS(new Vector3(0, Screen.height, 0),
			Quaternion.identity, new Vector3(1, -1, 1));
		GUI.DrawTexture(rect, texture);
		GUI.matrix = m;
	}
}
#elif UNITY_IPHONE
public class WebViewObject : MonoBehaviour {
	Callback callback;
	IntPtr webView;
	public bool visibility;

	[DllImport("__Internal")]
	private static extern IntPtr _WebViewPlugin_Init(string gameObject);
	[DllImport("__Internal")]
	private static extern int _WebViewPlugin_Destroy(IntPtr instance);
	[DllImport("__Internal")]
	private static extern void _WebViewPlugin_SetMargins(
		IntPtr instance, int left, int top, int right, int bottom);
	[DllImport("__Internal")]
	private static extern void _WebViewPlugin_SetVisibility(
		IntPtr instance, bool visibility);
	[DllImport("__Internal")]
	private static extern void _WebViewPlugin_LoadURL(
		IntPtr instance, string url);
	[DllImport("__Internal")]
	private static extern void _WebViewPlugin_EvaluateJS(
		IntPtr instance, string url);
	[DllImport("__Internal")]
	private static extern string _WebViewPluginPollMessage();

	private static WebViewObject _instance = null;
	public static WebViewObject Instance
	{
		get 
		{ 
			if (_instance == null) {
				GameObject go = new GameObject("WebViewObject");
				DontDestroyOnLoad(go);
				_instance = go.AddComponent<WebViewObject>();
			}
			return _instance; 
		}
	}

	public void Init(Callback cb = null)
	{
		callback = cb;
		webView = _WebViewPlugin_Init(name);
	}

	public WebViewObjectMessage PollMessage() {
    	string message =  _WebViewPluginPollMessage();
    	if (message != null) {Debug.Log(message);}
    	return (message != null) ? new WebViewObjectMessage(message) : null;
	}

	void OnDestroy() {
		if (webView == IntPtr.Zero)
			return;
		_WebViewPlugin_Destroy(webView);
	}

	public void SetVisibleWithMargins(int left, int top, int right, int bottom)
	{
		SetMargins(left, top, right, bottom);
		SetVisibility(true);
	}

	public void SetInvisible()
	{
		SetVisibility(false);
	}

	private void SetMargins(int left, int top, int right, int bottom) {
		if (webView == IntPtr.Zero)
			return;
		_WebViewPlugin_SetMargins(webView, left, top, right, bottom);
	}

	private void SetVisibility(bool v) {
		if (webView == IntPtr.Zero)
			return;
		visibility = v;
		_WebViewPlugin_SetVisibility(webView, v);
	}

	public void LoadURL(string url) {
		if (webView == IntPtr.Zero)
			return;
		_WebViewPlugin_LoadURL(webView, url);
	}

	public void EvaluateJS(string js) {
		if (webView == IntPtr.Zero)
			return;
		_WebViewPlugin_EvaluateJS(webView, js);
	}

	public void CallFromJS(string message) {
		if (callback != null)
			callback(message);
	}
}
#elif UNITY_ANDROID
public class WebViewObject : MonoBehaviour {

	private static WebViewObject _instance = null;
	public static WebViewObject Instance
	{
		get 
		{ 
			if (_instance == null) {
				GameObject go = new GameObject("WebViewObject");
				DontDestroyOnLoad(go);
				_instance = go.AddComponent<WebViewObject>();
			}
			return _instance; 
		}
	}

	Callback callback;
	AndroidJavaObject webView;
	// Vector2 offset;
	public bool visibility;

	public void Init(Callback cb = null) {
		callback = cb;
		// offset = new Vector2(0, 0);
		webView = new AndroidJavaObject("net.gree.unitywebview.WebViewPlugin");
		webView.Call("Init", name);
	}

	public WebViewObjectMessage PollMessage() {
		if (webView == null)
			return null;
		string message = webView.Call<String>("pollWebViewMessage");
		return (message != null) ?  new WebViewObjectMessage(message) : null;
	}

	void OnDestroy() {
		if (webView == null)
			return;
		webView.Call("Destroy");
	}

	public void SetVisibleWithMargins(int left, int top, int right, int bottom)
	{
		SetMargins(left, top, right, bottom);
		SetVisibility(true);
	}

	public void SetInvisible()
	{
		SetVisibility(false);
	}

	private void SetMargins(int left, int top, int right, int bottom) {
		if (webView == null)
			return;
		// offset = new Vector2(left, top);
		webView.Call("SetMargins", left, top, right, bottom);
	}

	private void SetVisibility(bool v) {
		if (webView == null)
			return;
		visibility = v;
		webView.Call("SetVisibility", v);
	}

	public void LoadURL(string url) {
		if (webView == null)
			return;
		webView.Call("LoadURL", url);
	}

	public void EvaluateJS(string js) {
		if (webView == null)
			return;
		webView.Call("LoadURL", "javascript:" + js);
	}

	public void CallFromJS(string message) {
		if (callback != null)
			callback(message);
	}
}
#endif
