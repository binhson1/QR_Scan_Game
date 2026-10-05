package com.birthday.balloon;

import android.app.Activity;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import java.io.File;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.lang.reflect.Method;

public class GalleryActivity extends Activity {
    static String sGameObject = "App";
    static final int REQ = 9911;

    public static void pick(Activity activity, String gameObject) {
        if (activity == null) return;
        sGameObject = gameObject;
        Intent i = new Intent(activity, GalleryActivity.class);
        activity.startActivity(i);
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        Intent intent = new Intent(Intent.ACTION_GET_CONTENT);
        intent.setType("image/*");
        intent.addCategory(Intent.CATEGORY_OPENABLE);
        startActivityForResult(Intent.createChooser(intent, "Chon anh"), REQ);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        String path = "";
        if (resultCode == RESULT_OK && data != null && data.getData() != null) {
            path = copy(data.getData());
        }
        sendToUnity(path == null ? "" : path);
        finish();
    }

    private static void sendToUnity(String path) {
        try {
            Class<?> unityPlayer = Class.forName("com.unity3d.player.UnityPlayer");
            Method send = unityPlayer.getMethod("UnitySendMessage", String.class, String.class, String.class);
            send.invoke(null, sGameObject, "OnGalleryPicked", path);
        } catch (Exception ignored) {
        }
    }

    private String copy(Uri uri) {
        InputStream in = null;
        FileOutputStream fos = null;
        try {
            File out = new File(getCacheDir(), "pick_" + System.currentTimeMillis() + ".jpg");
            in = getContentResolver().openInputStream(uri);
            fos = new FileOutputStream(out);
            byte[] buf = new byte[8192];
            int n;
            while ((n = in.read(buf)) > 0) {
                fos.write(buf, 0, n);
            }
            return out.getAbsolutePath();
        } catch (Exception e) {
            return "";
        } finally {
            try { if (in != null) in.close(); } catch (Exception ignored) {}
            try { if (fos != null) fos.close(); } catch (Exception ignored) {}
        }
    }
}
