using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    [Header("Audio Source")]
    [SerializeField] private AudioSource musicSource;

    [Header("Audio Clip")]
    [SerializeField] private GameMusicContainerSO musics;

    [Header("Current Music (debug)")]
    [SerializeField] private AudioClip currentMusic;
    [SerializeField] private int currentMusicIndex;
    [SerializeField] private int previousMusicIndex;

    private System.Random rng;

    private void Awake() 
    { 
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);

        rng = SeedManager.GetRNG("music");
    }

    private void PlayMusic(AudioClip clip)
    {
        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.volume = 0.7f;
        musicSource.loop = false;
        musicSource.Play();
    }

    public void PlayMenuMusic()
    {
        currentMusicIndex = 0;
        
        currentMusic = musics.GetMusicFrom(currentMusicIndex);
        previousMusicIndex = currentMusicIndex;

        musicSource.Stop();
        musicSource.clip = currentMusic;
        musicSource.volume = 1f;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayRandom()
    {
        currentMusicIndex = (int)SeedManager.Range(1f, musics.musicClips.Count, rng);

        currentMusic = musics.GetMusicFrom(currentMusicIndex);
        previousMusicIndex = currentMusicIndex;

        PlayMusic(currentMusic);

        // Start the coroutine to randomly play music
        StartCoroutine(RandomlyPlaying());
    }

    IEnumerator RandomlyPlaying()
    {
        while (true)
        {
            // Wait until the current music finishes playing
            yield return new WaitUntil(() => !musicSource.isPlaying);

            // Get a random music clip from the container
            currentMusicIndex = UnityEngine.Random.Range(0, musics.musicClips.Count);
            if (currentMusicIndex == previousMusicIndex)
            {
                // If the random index is the same as the previous one, get a new random index
                currentMusicIndex = (currentMusicIndex + 1) % musics.musicClips.Count;
            }

            currentMusic = musics.GetMusicFrom(currentMusicIndex);
            PlayMusic(currentMusic);

            // Wait for a short delay before playing the next music
            yield return new WaitForSeconds(4f);
        }
    }
}