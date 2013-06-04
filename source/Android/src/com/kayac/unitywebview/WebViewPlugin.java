/*
 * Copyright (C) 2011 Keijiro Takahashi
 * Copyright (C) 2012 GREE, Inc.
 * 
 * @onevcat Make it compatible with Kayac's webview version 2013.01.15
 *
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

package com.kayac.unitywebview;

import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

import android.app.Activity;
import android.os.Bundle;
import android.content.res.Resources;
import android.os.SystemClock;
import android.util.Log;
import android.view.Gravity;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup.LayoutParams;
import android.webkit.WebChromeClient;
import android.webkit.WebSettings;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.FrameLayout;
import android.widget.ProgressBar;
import java.util.concurrent.SynchronousQueue;
import android.graphics.drawable.ClipDrawable;
import android.graphics.drawable.Drawable;
import android.graphics.drawable.ShapeDrawable;
import android.graphics.drawable.shapes.RoundRectShape;
import android.graphics.PorterDuff.Mode;
import android.graphics.Color;
import android.graphics.LightingColorFilter;

class WebViewPluginInterface
{
	private String mGameObject;

	public WebViewPluginInterface(final String gameObject)
	{
		mGameObject = gameObject;
	}

	public void call(String message)
	{
		UnityPlayer.UnitySendMessage(mGameObject, "CallFromJS", message);
	}
}

public class WebViewPlugin
{
	// JavaScript interface class for embedded WebView.
    private class JSInterface {
        public SynchronousQueue<String> mMessageQueue;

        JSInterface() {
            mMessageQueue = new SynchronousQueue<String>();
        }

        public void pushMessage(String message) {
            Log.d("WebView", message);
            try {
                mMessageQueue.put(message);
            } catch (java.lang.InterruptedException e) {
                Log.d("WebView", "Queueing error - " + e.getMessage());
            }
        }
    }

    private JSInterface mJSInterface;   // JavaScript interface (message receiver)

	private static FrameLayout layout = null;
	private WebView mWebView;
	private ProgressBar mProgress;      // Progress bar

	private long mDownTime;

	// public WebViewPlugin()
	// {
	// }

	private Resources res = null;
	private String name = null;

	public void Init(final String gameObject)
	{
		final Activity a = UnityPlayer.currentActivity;
		
		a.runOnUiThread(new Runnable() {public void run() {

			CreateWebView(a);
			AddWebView(a);
			SetWebView();
			
			// Create a JavaScript interface and bind the WebView to it.
        	mJSInterface = new JSInterface();
        	mWebView.addJavascriptInterface(mJSInterface, "UnityInterface");

        	// AddProgressBar(a);
        	// SetProgressLayout(a);

        	mWebView.setWebChromeClient(new WebChromeClient());
        	// mWebView.setWebChromeClient(new WebChromeClient() {
         //    	public void onProgressChanged(WebView view, int progress) {
         //        	if (progress < 100) {
         //           		mProgress.setVisibility(View.VISIBLE);
         //            	mProgress.setProgress(progress);
         //        	} else {
         //            	mProgress.setProgress(0);
         //        	}
         //    	}
        	// }
        	// );
        
		}});
	}

	private void CreateWebView (Activity a) {
		mWebView = new WebView(a);
		mWebView.setVisibility(View.GONE);
		mWebView.setFocusable(true);
		mWebView.setFocusableInTouchMode(true);
	}

	private void AddWebView (Activity a) {
		if (layout == null) {
			layout = new FrameLayout(a);
			a.addContentView(layout, new LayoutParams(LayoutParams.FILL_PARENT, 
				LayoutParams.FILL_PARENT));
			layout.setFocusable(true);
			layout.setFocusableInTouchMode(true);
		}

		layout.addView(mWebView, new FrameLayout.LayoutParams(
			LayoutParams.FILL_PARENT, LayoutParams.FILL_PARENT,
			Gravity.NO_GRAVITY));
	}

	private void SetWebView() {
		// Set a dummy WebViewClient (which enables loading a new page in own WebView).
		mWebView.setWebViewClient(new WebViewClient());

		// Basic settings of WebView.
		WebSettings webSettings = mWebView.getSettings();
		webSettings.setSupportZoom(false);
		webSettings.setJavaScriptEnabled(true);
		webSettings.setPluginsEnabled(true);
		webSettings.setDatabaseEnabled(true);
    	webSettings.setDomStorageEnabled(true);
    	mWebView.setScrollBarStyle(WebView.SCROLLBARS_INSIDE_OVERLAY);
        mWebView.setVerticalScrollbarOverlay(true);
	}

	private void AddProgressBar(Activity a) {
		mProgress = new ProgressBar(a, null, android.R.attr.progressBarStyleHorizontal);
		if (layout == null) {
			layout = new FrameLayout(a);
			a.addContentView(layout, new LayoutParams(LayoutParams.FILL_PARENT, 
				LayoutParams.FILL_PARENT));
			layout.setFocusable(true);
			layout.setFocusableInTouchMode(true);
		}
        layout.addView(mProgress, new FrameLayout.LayoutParams(LayoutParams.FILL_PARENT, 5));
        mProgress.setMax(100);
        mProgress.setVisibility(View.GONE);
	}

	private void SetProgressLayout(Activity a) {
		SetLayoutParams(a);
		SetProgressDetail(a);
	}

	private void SetLayoutParams(Activity a) {
		// String name = getPackageName();
		// FrameLayout.LayoutParams params = new FrameLayout.LayoutParams(LayoutParams.MATCH_PARENT, 50, Gravity.NO_GRAVITY);
		FrameLayout.LayoutParams params = new FrameLayout.LayoutParams(LayoutParams.FILL_PARENT, 5, Gravity.NO_GRAVITY);
        params.leftMargin = 0;
        params.rightMargin = 0;
        
        String scompare="Bottom".toString();
        if("Top".equals(scompare)) {
        	Log.d("WebView", "Top");
        	params.topMargin= 0;
        	params.gravity = Gravity.LEFT | Gravity.TOP;
        } else {
        	// params.bottomMargin= res.getDimensionPixelOffset(res.getIdentifier("TopBottomMargin", "dimen", name));
        	Log.d("WebView", "Bottom");
        	params.bottomMargin= 0;
        	params.gravity = Gravity.LEFT | Gravity.BOTTOM;
        }
        mProgress.setLayoutParams(params);
	}

	private void SetProgressDetail (Activity a) {
		// Resources res = getResources();
		// String name = getPackageName();
		// Define a shape with rounded corners
        final float[] roundedCorners = new float[] { 5, 5, 5, 5, 5, 5, 5, 5 };
        ShapeDrawable pgDrawable = new ShapeDrawable(new RoundRectShape(roundedCorners, null, null));

        // Sets the progressBar color
        String myColor = "#00C0FF";
        pgDrawable.getPaint().setColor(Color.parseColor(myColor));

        // Adds the drawable to your progressBar
        ClipDrawable progressD = new ClipDrawable(pgDrawable, Gravity.LEFT, ClipDrawable.HORIZONTAL);
        mProgress.setProgressDrawable(progressD);

        // Sets a background to have the 3D effect
        // mProgress.setBackgroundDrawable(res.getDrawable(android.R.drawable.progress_horizontal));
        // mProgress.getBackground().setColorFilter( 0x87888800, Mode.MULTIPLY);
        Drawable drawable = mProgress.getProgressDrawable();
		drawable.setColorFilter(new LightingColorFilter(0xFF000000, 0xFF5ea618));
	}

	public void Destroy()
	{
		Activity a = UnityPlayer.currentActivity;
		a.runOnUiThread(new Runnable() {public void run() {

			if (mWebView != null) {
				layout.removeView(mWebView);
				mWebView = null;
			}

		}});
	}

	public void LoadURL(final String url)
	{
		final Activity a = UnityPlayer.currentActivity;
		a.runOnUiThread(new Runnable() {public void run() {

			mWebView.loadUrl(url);

		}});
	}

	public void EvaluateJS(final String js)
	{
		final Activity a = UnityPlayer.currentActivity;
		a.runOnUiThread(new Runnable() {public void run() {

			mWebView.loadUrl("javascript:" + js);

		}});
	}

	public void SetMargins(int left, int top, int right, int bottom)
	{
		final FrameLayout.LayoutParams params = new FrameLayout.LayoutParams(
			LayoutParams.FILL_PARENT, LayoutParams.FILL_PARENT,
				Gravity.NO_GRAVITY);
		params.setMargins(left, top, right, bottom);

		Activity a = UnityPlayer.currentActivity;
		a.runOnUiThread(new Runnable() {public void run() {

			mWebView.setLayoutParams(params);

		}});
	}

	public void SetVisibility(final boolean visibility)
	{
		Activity a = UnityPlayer.currentActivity;
		a.runOnUiThread(new Runnable() {public void run() {

			if (visibility) {
				mWebView.setVisibility(View.VISIBLE);
				layout.requestFocus();
				mWebView.requestFocus();
			} else {
				mWebView.setVisibility(View.GONE);
			}

		}});
	}

	public String pollWebViewMessage() {
        return mJSInterface.mMessageQueue.poll();
    }
}
