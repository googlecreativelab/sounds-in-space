package com.google.cl.syd.solo.flic;

import android.content.Context;
import android.content.Intent;
import android.content.BroadcastReceiver;


/**
 * Created by judeosborn on 12/5/18.
 */

public class MyReceiver extends BroadcastReceiver {

    private static MyReceiver instance;

    // text that will be read by Unity
    public static String text = "";

    // Triggered when an Intent is caught
    @Override
    public void onReceive(Context context, Intent intent) {
        text = "clicked";
    }

    // static method to create our receiver object, it'll be Unity that will create ou receiver object (singleton)
    public static void createInstance()
    {
        if (instance ==  null)
        {
            instance = new MyReceiver();
        }
    }

    // static method to create our receiver object, it'll be Unity that will create ou receiver object (singleton)
    public static void clearText()
    {
        text = "";
    }
}