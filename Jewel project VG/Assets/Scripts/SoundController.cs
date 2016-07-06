using UnityEngine;
using System.Collections;

public class SoundController : MonoBehaviour {

    //==============================================
    // Constants
    //==============================================

    public static SoundController Instance;

    //==============================================
    // Fields
    //==============================================

    public AudioSource sfxSource;
    public AudioSource musicSource;

    public float lowPitchRange = 1f;
    public float highPitchRange = 1f;

    //==============================================
    // Unity Methods
    //==============================================

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        if (PlayerPrefs.HasKey("SFX Volume"))
        {
            sfxSource.volume = PlayerPrefs.GetFloat("SFX Volume");
        }
        if (PlayerPrefs.HasKey("Music Volume"))
        {
            musicSource.volume = PlayerPrefs.GetFloat("Music Volume");
        }
    }

    //==============================================
    // Methods
    //==============================================

    public void playSingleClip(AudioClip clip)
    {
        sfxSource.clip = clip;
        float randomPitch = Random.Range(lowPitchRange, highPitchRange);
        //sfxSource.pitch = randomPitch;

        //sfxSource.PlayOneShot(clip);
        sfxSource.Play();
    }

    public void playOneShotClip(AudioClip clip)
    {
        //sfxSource.clip = clip;
        float randomPitch = Random.Range(lowPitchRange, highPitchRange);
        //sfxSource.pitch = randomPitch;

        sfxSource.PlayOneShot(clip);
        //sfxSource.Play();
    }

    public void changeSfxVolume(float newVolume)
    {
        SoundController.Instance.sfxSource.volume = newVolume;
    }

    public void changeMusicVolume(float newVolulme)
    {
        SoundController.Instance.musicSource.volume = newVolulme;
    }
}
