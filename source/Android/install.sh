#!/bin/sh
UNITYLIBS="/Applications/Unity/Unity.app/Contents/PlaybackEngines/AndroidPlayer/bin/classes.jar"
DSTDIR="../../build/Android"
export ANT_OPTS=-Dfile.encoding=UTF8

# Change the path to your 'android' command
/Users/onevcat/AndroidDev/sdk/tools/android update project -p .

mkdir -p libs
cp $UNITYLIBS libs
ant release
mkdir -p $DSTDIR
cp -a bin/classes.jar $DSTDIR/WebViewPlugin.jar
ant clean
rm -rf libs res proguard-project.txt
