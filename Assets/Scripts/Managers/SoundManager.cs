using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance = null;

    public AudioSource audioSrc;

    [Serializable]
    public struct NamedAudio
    {
        public string name;
        public AudioClip[] audio;
    }
    public List<NamedAudio> audioClips;

    public AudioSource menuMusic;

    public List<AudioSource> battleMusics;

    private Coroutine fadeInCoroutine = null;
    private Coroutine fadeOutCoroutine = null;

    private void Awake()
    {
        if (instance != this)
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private void Start()
    {
        // PlayMenuMusic();
    }

    public void PlayMenuMusic()
    {
        foreach (AudioSource battleMusic in battleMusics)
        {
            if (battleMusic.isPlaying)
            {
                Debug.Log($"Stopping battle music");
                fadeOutCoroutine = StartCoroutine(fadeOutMusic(1f, battleMusic));
            }
        }
        menuMusic.volume = 0;
        menuMusic.Play();
        fadeInCoroutine = StartCoroutine(fadeInMusic(2f, menuMusic));
    }

    public void PlayBattleMusic()
    {
        var random = new System.Random();

        if (menuMusic.isPlaying)
        {
            Debug.Log($"Stopping menu music");
            fadeOutCoroutine = StartCoroutine(fadeOutMusic(0.5f, menuMusic));
        }

        var battleMusic = battleMusics[random.Next(battleMusics.Count)];
        battleMusic.volume = 0;
        battleMusic.Play();
        fadeInCoroutine = StartCoroutine(fadeInMusic(0.5f, battleMusic));
    }

    protected IEnumerator fadeInMusic(float timeToFade, AudioSource music)
    {
        float baseVolume = 0f;
        float endVolume = 1f;
        float timeLeft = timeToFade;
        while (timeLeft > 0)
        {
            yield return null;
            timeLeft -= Time.deltaTime;
            var newVolume = Mathf.Lerp(baseVolume, endVolume, 1 - (timeLeft / timeToFade));
            music.volume = newVolume;
        }

        fadeInCoroutine = null;
    }

    protected IEnumerator fadeOutMusic(float timeToFade, AudioSource music)
    {
        float baseVolume = 1f;
        float endVolume = 0f;
        float timeLeft = timeToFade;
        while (timeLeft > 0)
        {
            yield return null;
            timeLeft -= Time.deltaTime;
            var newVolume = Mathf.Lerp(baseVolume, endVolume, 1 - (timeLeft / timeToFade));
            music.volume = newVolume;
        }

        music.Stop();
        fadeOutCoroutine = null;
    }

    public void PlayOneShot(string audioName)
    {
        var random = new System.Random();

        var clip = audioClips.FirstOrDefault(x => x.name == audioName).audio;
        if (clip != null)
        {
            audioSrc.PlayOneShot(clip[random.Next(clip.Length)]);
        }
    }

    public void PlayKeyHit(char carac)
    {
        var clip = audioClips.FirstOrDefault(x => x.name == "unlockedLetters").audio;
        if (clip != null)
        {
            audioSrc.pitch = (float)carac / 'z' + 0.5f;
            audioSrc.PlayOneShot(clip[0]);
        }
    }
}