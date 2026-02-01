using System;
using UnityEngine;
using UnityEngine.Events;

namespace ggj_2026_masks
{
    [CreateAssetMenu(fileName = "AudioChannel", menuName = "Scriptable Objects/AudioChannel")]
    public class AudioChannel : ScriptableObject
    {

        public UnityAction<string> OnAudioRequested;
        public UnityAction<string> OnAudioStopped;
        public Func<string, bool> OnIsAudioPlaying;

        public void PlayAudio(string soundName)
        {
            if (OnAudioRequested != null) {
                OnAudioRequested.Invoke(soundName);
            }
            else
            {
                Debug.Log("No AudioManager registered");
            }
        }

        public void StopAudio(string soundName)
        {
            if (OnAudioStopped != null)
            {
                OnAudioStopped.Invoke(soundName);
            }
            else
            {
                Debug.Log("No AudioManager registered");
            }
        }

        public bool IsAudioPlaying(string soundName)
        {
            return OnIsAudioPlaying != null && OnIsAudioPlaying(soundName);
        }

    }

}
