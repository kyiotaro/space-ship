using UnityEngine;

public class AudioManager : MonoBehaviour
{   
    public AudioSource musicSource;
    public AudioSource sfxSource;

    public AudioClip BGM;
    public AudioClip ShootSFX;

    void Start()
    {
        musicSource.clip = BGM;
        musicSource.Play();
    }

    public void playSFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
}
