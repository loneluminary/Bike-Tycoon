using SceneLoading;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] AudioSource music;

    private void Awake()
    {
        if (music)
        {
            DontDestroyOnLoad(music.gameObject);
            foreach (var audio in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
            {
                if (audio.name == music.name && audio != music) Destroy(music.gameObject);
            }
        }

        if (!PlayerPrefs.HasKey("FirstTimeInstall"))
        {
            GameManager.DeleteAllSaveData();
            PlayerPrefs.SetInt("FirstTimeInstall", 1);
        }
    }

    private void Start() => Play(); // No main menu play directly

    public void Play() => LoadingScreen.Instance.Load(1);
    public void Quit() => Application.Quit();
}