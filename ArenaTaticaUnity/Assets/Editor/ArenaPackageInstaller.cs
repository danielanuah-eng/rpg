using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
public static class ArenaPackageInstaller
{
    static AddAndRemoveRequest request;
    static double deadline;
    public static void Install()
    {
        request = Client.AddAndRemove(new[] { "com.unity.cloud.gltfast" });
        deadline = EditorApplication.timeSinceStartup + 600;
        EditorApplication.update += Poll;
    }
    static void Poll()
    {
        if (!request.IsCompleted && EditorApplication.timeSinceStartup < deadline) return;
        EditorApplication.update -= Poll;
        bool ok = request.IsCompleted && request.Status == StatusCode.Success;
        if (!ok) Debug.LogError("Arena package install: " + request.Error?.message);
        else Debug.Log("ARENA_PACKAGES_OK");
        EditorApplication.Exit(ok ? 0 : 1);
    }
}
