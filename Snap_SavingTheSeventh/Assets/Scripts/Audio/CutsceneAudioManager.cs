using UnityEngine;

public class CutsceneAudioManager : MonoBehaviour
{
    public static CutsceneAudioManager Instance;
    
    [Header("Audio Source")]
    public AudioSource musicSource;
    
    [Header("Cutscene Music")]
    public AudioClip cutsceneMusic;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;

    void Awake()
    {
        // Cek apakah sudah ada instance
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Ini yang bikin gak restart
            
            // Play musik cutscene
            if (cutsceneMusic != null && musicSource != null)
            {
                musicSource.clip = cutsceneMusic;
                musicSource.loop = true;
                musicSource.volume = musicVolume;
                musicSource.Play();
            }
        }
        else
        {
            // Kalau sudah ada, destroy yang baru (biar gak dobel)
            Destroy(gameObject);
        }
    }

    // Destroy audio manager (panggil pas cutscene selesai)
    public void StopAndDestroy()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
        Destroy(gameObject);
        Instance = null;
    }
    
    // Set volume
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }
}