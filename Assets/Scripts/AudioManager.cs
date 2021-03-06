﻿using UnityEngine.Audio;
using System;
using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    void Awake ()
    {
        foreach (Sound s in sounds)
        {
           s.source = gameObject.AddComponent<AudioSource>();
           s.source.clip = s.clip;
           s.source.volume = s.volume;
           s.source.pitch = s.pitch;
           s.source.loop = s.loop;
           //s.source.priority = s.priority;
        }
    }

    public void Play (string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            print("WARNING: sound with name " + name + " not found!");
            return;
        }

        s.source.Play();

    }

    public void Play (string name, float delay)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            print("WARNING: sound with name " + name + " not found!");
            return;
        }

        s.source.PlayDelayed(delay);

    }

    public void Stop (string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            print("WARNING: sound with name " + name + " not found!");
            return;
        }

        s.source.Stop();

    }

    public void StopAll ()
    {
        foreach (Sound s in sounds)
        {
            s.source.Stop();
        }
    }
    // public IEnumerator FadeOut (string name)
    // {
    //     Sound s = Array.Find(sounds, sound => sound.name == name);

    //     if (s == null)
    //     {
    //         print("WARNING: sound with name " + name + " not found!");
    //         yield return null;
    //     }

    //     float startVolume = s.volume;
    //     float fadeTimeSeconds = 2;
        
    //     while (s.volume > 0) {
    //         s.volume -= startVolume * Time.deltaTime / fadeTimeSeconds;
    //         yield return null;
    //     }

    //     s.source.Stop();
    //     s.volume = startVolume;
    // }
}
