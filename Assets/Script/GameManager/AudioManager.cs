using UnityEngine;

public class AudioManager : MonoBehaviour
{   
    public AudioSource musicSource;
    public AudioSource sfxSource;

    public AudioClip BGM;
    public AudioClip DieSFX;
    public AudioClip HitSFX;
    public AudioClip ShootSFX;
    public AudioClip ClickSFX;
    public AudioClip DashSFX;

    void Start()
    {
        musicSource.clip = BGM;
        musicSource.Play();
    }

    public void playSFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void playClickSFX()
    {
        if (sfxSource != null && ClickSFX != null)
            sfxSource.PlayOneShot(ClickSFX);
    }
}
