using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip mergeSound;      // x2 и x/2 объединение
    [SerializeField] private AudioClip deathSound;      // deth объединение
    [SerializeField] private AudioClip freezeSound;    // freeze объединение
    [SerializeField] private AudioClip vortexSound;    // vortex объединение
    [SerializeField] private AudioClip growShrinkSound; // Grow и Shrink
    [SerializeField] private AudioClip wowSound;        // Новый рекорд
    [SerializeField] private AudioClip fooSound;        // Рекорд не побит

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f; // По умолчанию включен
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize audio source if not assigned
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        // Load volume settings
        LoadVolumeSettings();
        ApplyVolumeSettings();
    }

    private void Start()
    {
    }

    #region Volume Control
    public void LoadVolumeSettings()
    {
        // Не загружаем из PlayerPrefs - звук всегда включен по умолчанию
        masterVolume = 1f;
        sfxVolume = 1f;
        ApplyVolumeSettings();
    }

    private void ApplyVolumeSettings()
    {
        sfxSource.volume = masterVolume * sfxVolume;
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyVolumeSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        ApplyVolumeSettings();
    }

    public void ToggleMute()
    {
        if (masterVolume > 0f)
        {
            SetMasterVolume(0f);
        }
        else
        {
            SetMasterVolume(1f);
        }
    }
    #endregion

    #region Sound Effects
    public void PlayMergeSound()
    {
        if (mergeSound != null)
        {
            sfxSource.PlayOneShot(mergeSound);
        }
        else
        {
        }
    }

    public void PlayDeathSound()
    {
        if (deathSound != null)
        {
            sfxSource.PlayOneShot(deathSound);
        }
        else
        {
        }
    }

    public void PlayFreezeSound()
    {
        if (freezeSound != null)
        {
            sfxSource.PlayOneShot(freezeSound);
        }
        else
        {
        }
    }

    public void PlayVortexSound()
    {
        if (vortexSound != null)
        {
            sfxSource.PlayOneShot(vortexSound);
        }
        else
        {
        }
    }

    public void PlayGrowShrinkSound()
    {
        if (growShrinkSound != null)
        {
            sfxSource.PlayOneShot(growShrinkSound);
        }
        else
        {
        }
    }

    public void PlayWowSound()
    {
        if (wowSound != null)
        {
            sfxSource.PlayOneShot(wowSound);
        }
        else
        {
        }
    }

    public void PlayFooSound()
    {
        if (fooSound != null)
        {
            sfxSource.PlayOneShot(fooSound);
        }
        else
        {
        }
    }
    #endregion

    #region Utility Methods
    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlaySoundWithPitch(AudioClip clip, float pitch)
    {
        if (clip != null)
        {
            float originalPitch = sfxSource.pitch;
            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(clip);
            sfxSource.pitch = originalPitch;
        }
    }

    private int GetSoundCount()
    {
        int count = 0;
        if (mergeSound != null) count++;
        if (deathSound != null) count++;
        if (freezeSound != null) count++;
        if (vortexSound != null) count++;
        if (growShrinkSound != null) count++;
        if (wowSound != null) count++;
        if (fooSound != null) count++;
        return count;
    }

    public bool IsMuted()
    {
        return masterVolume <= 0f;
    }
    #endregion
}
