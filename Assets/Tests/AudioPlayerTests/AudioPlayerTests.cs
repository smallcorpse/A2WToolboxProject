using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using A2W;

public class AudioPlayerTests : MonoBehaviour
{
    [SerializeField] AudioClip bgm;
    [SerializeField] AudioClip sound;

    [SerializeField] Slider bgmVolumeSlider;
    [SerializeField] Slider soundVolumeSlider;

    private void Awake()
    {
        bgmVolumeSlider.value = AudioPlayer.instance.bgmVolume;
        soundVolumeSlider.value = AudioPlayer.instance.soundVolume;
    }

    public void OnBgmVolumeChange(float volume)
    {
        AudioPlayer.instance.bgmVolume = volume;
    }

    public void OnSoundVolumeChange(float volume)
    {
        AudioPlayer.instance.soundVolume = volume;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AudioPlayer.instance.PlayBGM(bgm);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AudioPlayer.instance.PlaySound(sound);
        }
    }
}
