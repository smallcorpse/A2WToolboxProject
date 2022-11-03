using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace A2W
{
    public class AudioPlayer : Singleton<AudioPlayer>
    {
        public float bgmVolume
        {
            get
            {
                return _bgmVolume;
            }
            set
            {
                _bgmVolume = value;
                if (bgmPlayer is null) return;
                bgmPlayer.volume = _bgmVolume;
            }
        }
        public float soundVolume
        {
            get
            {
                return _soundVolume;
            }
            set
            {
                _soundVolume = value;
                if (soundPlayers is null) return;
                foreach (var player in soundPlayers)
                {
                    player.volume = _soundVolume;
                }
            }
        }

        private float _bgmVolume = 0.5f;
        private float _soundVolume = 0.5f;

        private AudioSource bgmPlayer;
        private List<AudioSource> soundPlayers;

        public void PlayBGM(AudioClip clip)
        {
            if (bgmPlayer is null)
            {
                bgmPlayer = CreateAudioSource("BGMPlayer");
            }
            bgmPlayer.clip = clip;
            bgmPlayer.loop = true;
            bgmPlayer.volume = bgmVolume;
            bgmPlayer.Play();
        }

        public void PauseBGM()
        {
            bgmPlayer.Pause();
        }

        public void UnPauseBGM()
        {
            bgmPlayer.UnPause();
        }

        public void StopBGM()
        {
            bgmPlayer.Stop();
        }

        public void PlaySound(AudioClip clip)
        {
            AudioSource soundPlayer = GetEmptyAudioSource();
            soundPlayer.volume = soundVolume;
            soundPlayer.PlayOneShot(clip);
        }

        public void PauseAllSound()
        {
            foreach (var player in soundPlayers)
            {
                player.Pause();
            }
        }

        public void UnPauseAllSound()
        {
            foreach (var player in soundPlayers)
            {
                player.UnPause();
            }
        }

        public void StopAllSound()
        {
            foreach (var player in soundPlayers)
            {
                player.Stop();
            }
        }

        private AudioSource GetEmptyAudioSource()
        {
            if (soundPlayers is null)
            {
                soundPlayers = new List<AudioSource>();
            }

            foreach (var player in soundPlayers)
            {
                if (player.isPlaying is false)
                {
                    return player;
                }
            }

            AudioSource audioSource = CreateAudioSource("SoundPlayer");
            soundPlayers.Add(audioSource);
            return audioSource;
        }

        private AudioSource CreateAudioSource(string name)
        {
            GameObject go = new GameObject();
            go.name = name;
            go.transform.SetParent(transform, false);
            return go.AddComponent<AudioSource>();
        }
    }
}


