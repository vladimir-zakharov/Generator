open System.Xml
open System.Xml.Linq
open System.IO
open System.Collections
open System.Diagnostics
open System.Threading

System.Console.WriteLine("insert project folder, please")

let path = System.Console.ReadLine()

System.Console.WriteLine("insert project package, please")

let package = System.Console.ReadLine()

// permissions register
let permissions = new Hashtable()

let writeToFile fn (str : string) =
   let conv = System.Text.Encoding.UTF8
   use f = File.CreateText fn
   f.WriteLine(str)

let activities = new System.Text.StringBuilder()

let manifest = new System.Text.StringBuilder()
ignore(manifest.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>
<manifest xmlns:android=\"http://schemas.android.com/apk/res/android\"
    package=\"com.qrealclouds." + package + "\"
    android:versionCode=\"1\"
    android:versionName=\"1.0\">

    <uses-sdk android:minSdkVersion=\"8\"/>\n"))

let createImplementation form =
    if (form = "main") then
        ignore(activities.Append("        <activity android:name=\".main\"
            android:label=\"@string/app_name\">
            <intent-filter>
                <action android:name=\"android.intent.action.MAIN\"/>
                <category android:name=\"android.intent.category.LAUNCHER\"/>
            </intent-filter>
        </activity>"))
    else
        ignore(activities.Append("\n        <activity android:name=\"." + form + "\"></activity>"))

    // imports register
    let imports = new Hashtable()

    let currentImports = new System.Text.StringBuilder()
    ignore(currentImports.Append("\nimport android.app.Activity;
import android.os.Bundle;
import android.view.View;\n"))

    let onCreate = new System.Text.StringBuilder()
    ignore(onCreate.Append("\n\n    /**
    * Called when the activity is first created.
    */
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout." + form + ");"))
            
    let activity = System.Text.StringBuilder()
    ignore(activity.Append(currentImports.ToString()))
    ignore(activity.Append("\npublic class " + form + " extends Activity {"))

    let reader = XmlReader.Create(path + @"\res\layout\" + form + ".xml")
    while (reader.Read ()) do
        if reader.Name = "Button" then
            let onClickName = reader.GetAttribute("android:onClick")

            if not (imports.ContainsKey("android.content.Intent")) then 
                ignore(activity.Insert(0, "\nimport android.content.Intent;"))
                imports.Add("android.content.Intent", "android.content.Intent")

            ignore(activity.Append("\n\n    public void " + onClickName + "(View v) {"))

            let readerTrans = XmlReader.Create(path + @"\Transition2.xml")
            let id = reader.GetAttribute("android:id")

            while ((readerTrans.Read()) && not (readerTrans.GetAttribute("id") = id )) do
                ignore()

            let nextForm = readerTrans.GetAttribute("name_to")

            ignore(activity.Append("
        Intent intent = new Intent(this, " + nextForm + ".class);
        startActivity(intent);"))
            ignore(activity.Append("\n    }"))
        elif reader.Name = "WebView" then
            if not (permissions.ContainsKey("Internet")) then 
                ignore(manifest.Append("    <uses-permission android:name=\"android.permission.INTERNET\" />\n"))
                permissions.Add("Internet", "Internet")

            if not (imports.ContainsKey("android.webkit.WebView")) then 
                ignore(activity.Insert(0, "\nimport android.webkit.WebView;"))
                imports.Add("android.webkit.WebView", "android.webkit.WebView")

            ignore(onCreate.Append("\n        WebView webView = (WebView) findViewById(R.id.webViewInfo);
        webView.getSettings().setJavaScriptEnabled(true);
        webView.loadUrl(\"http://www.lanit-tercom.ru\");")) // сайт зашит в код

    ignore(onCreate.Append("\n    }"))
    ignore(activity.Append(onCreate.ToString()))

    ignore(activity.Append("\n}"))

    ignore(activity.Insert(0, "package com.qrealclouds." + package + ";\n"))

    writeToFile (path + @"\src\com\qrealclouds\" + package + "\\" + form + ".java") (activity.ToString())

ignore (System.IO.Directory.CreateDirectory(path + @"\src\com\qrealclouds\" + package))

createImplementation "main" 
createImplementation "second"
createImplementation "third"
createImplementation "fourth"
createImplementation "fifth"
createImplementation "sixth"
createImplementation "seventh"
createImplementation "eighth"
createImplementation "ninth"

ignore(manifest.Append("\n    <application android:label=\"@string/app_name\"
        android:theme=\"@android:style/Theme.Light.NoTitleBar\">\n"))
ignore(manifest.Append(activities.ToString()))
ignore(manifest.Append("\n    </application>
</manifest>"))
writeToFile (path + "\AndroidManifest.xml") (manifest.ToString())

let createBuildXml = "android update project --target 1 -p " + path
Thread.Sleep(1000);
let pathToAndroidSdk = @"D:\android-sdk\sdk\tools\" //для работы на другом компе нужно заменть до android-sdk
ignore(System.Diagnostics.Process.Start("cmd.exe", "/C " + pathToAndroidSdk + createBuildXml))
Thread.Sleep(1000);
ignore(System.Diagnostics.Process.Start("cmd.exe", "/C " + "cd /d " + path + " & ant debug"))