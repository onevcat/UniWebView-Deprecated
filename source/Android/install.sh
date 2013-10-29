#!/bin/sh
UNITYLIBS="/Applications/Unity4.2.2/Unity.app/Contents/PlaybackEngines/AndroidPlayer/bin/classes.jar"
DSTDIR="../../build/Android"
ANDROID_PATH="/Users/onevcat/AndriodDev/sdk/tools"
export ANT_OPTS=-Dfile.encoding=UTF8

# Change the path to your 'android' command
$ANDROID_PATH/android update project -p . --target 1

mkdir -p libs
cp $UNITYLIBS libs
ant release
mkdir -p $DSTDIR
cp -a bin/classes.jar $DSTDIR/WebViewPlugin.jar
ant clean
rm -rf libs res proguard-project.txt
