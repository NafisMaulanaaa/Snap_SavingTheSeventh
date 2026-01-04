using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance; // Singleton biar bisa diakses dari mana aja
    
    [Header("Audio Source")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    
    [Header("Scene Music")]
    public AudioClip sceneMusic;
    public bool playOnStart = true;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 0.5f; // Default volume musik
    [Range(0f, 1f)]
    public float sfxVolume = 1f; // Default volume SFX

    void Awake()
    {
        // Singleton - biar cuma ada 1 AudioManager
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // Load volume dari PlayerPrefs (save data)
        LoadVolumeSettings();
    }

    void Start()
    {
        // Apply volume yang udah di-load
        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
        
        // Play musik saat scene dimulai
        if (playOnStart && sceneMusic != null && musicSource != null)
        {
            musicSource.clip = sceneMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    // Set volume musik
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
        SaveVolumeSettings();
    }

    // Set volume SFX
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
        SaveVolumeSettings();
    }

    // Play sound effect
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // Save volume settings
    void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    // Load volume settings
    void LoadVolumeSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f); // Default 0.5
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f); // Default 1
    }
}