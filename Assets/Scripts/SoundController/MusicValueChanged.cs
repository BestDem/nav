using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MusicValueChanged : MonoBehaviour
{
    [SerializeField][CanBeNull] private PlayerController playerController;
    
    [SerializeField] private AudioMixer audioMixerSound;
    [SerializeField] private Slider volumeAudioSound;
    [SerializeField] private Slider volumeMouseSlider;

    private float soundVolume = 1f;
    private float volumeMouse = 5f;

    private void Start()
    {
        playerController = FindAnyObjectByType<PlayerController>();

        ResetValue();
    }

    public void ResetValue()
    {
        // Получаем значение именно в диапазоне 0-1
        soundVolume = PlayerPrefs.GetFloat("SoundVolume", 1f);
        volumeMouse = PlayerPrefs.GetFloat("VolumeMouse", 5f);

        // Устанавливаем Slider
        volumeAudioSound.value = soundVolume;
        volumeMouseSlider.value = volumeMouse;

        // Переводим 0-1 -> dB
        SetMixerVolume(soundVolume);
    }

    public void SetVolumeSound(float newVolume)
    {
        // Значение Slider всегда 0-1
        soundVolume = Mathf.Clamp01(newVolume);

        // Переводим в dB и устанавливаем в AudioMixer
        SetMixerVolume(soundVolume);

        // Сохраняем НЕ dB, а значение Slider 0-1
        PlayerPrefs.SetFloat("SoundVolume", soundVolume);
        PlayerPrefs.Save();
    }

    private void SetMixerVolume(float volume)
    {
        float volumeDB;

        if (volume <= 0.0001f)
        {
            // Полностью выключаем звук
            volumeDB = -80f;
        }
        else
        {
            volumeDB = Mathf.Log10(volume) * 20f;
        }

        audioMixerSound.SetFloat("SoundVolume", volumeDB);
    }

    public void SetMouseSen(float newVolume)
    {
        volumeMouse = newVolume;

        if (playerController != null)
        {
            playerController.ChangeMouseSens(volumeMouse);
        }

        PlayerPrefs.SetFloat("VolumeMouse", volumeMouse);
        PlayerPrefs.Save();
    }
}