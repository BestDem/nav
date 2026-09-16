using System.Collections;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public static SoundController singletonSound { get; private set; }
    [SerializeField] private GameObject sourse3DPref;
    [SerializeField] private GameObject sourse2DPref;
    [SerializeField] private AudioClip[] audioClips;

    private void Start()
    {
        if (singletonSound == null)
            singletonSound = this;
        else
            Destroy(gameObject);
    }

    public void Play3DSongByIndex(int index, Vector3 transformSound)
    {
        if (index >= 0 && index < audioClips.Length)
            PlaySound3D(audioClips[index], transformSound);
    }

    public void Play2DSongByIndex(int index)
    {
        if (index >= 0 && index < audioClips.Length)
            PlaySound2D(audioClips[index]);
    }
    
    private void PlaySound3D(AudioClip audioClip, Vector3 transformSound)
    {
        GameObject AudioSource2D = Instantiate(sourse3DPref, transformSound, Quaternion.identity);

        AudioSource2D.TryGetComponent(out AudioSource _audioSource);
        _audioSource.clip = audioClip;
        _audioSource.Play();

        StartCoroutine(StopSound(AudioSource2D));
    }

    private void PlaySound2D(AudioClip audioClip)
    {
        GameObject AudioSource2D = Instantiate(sourse2DPref, transform.position, Quaternion.identity);

        AudioSource2D.TryGetComponent(out AudioSource _audioSource);
        _audioSource.clip = audioClip;
        _audioSource.Play();

        StartCoroutine(StopSound(AudioSource2D));
    }

    
    IEnumerator StopSound(GameObject _prefSound)   //я не знаю как по другому проверять когда музыка закончилась 
    {
        _prefSound.TryGetComponent(out AudioSource _audioSource);

        yield return new WaitUntil(() => !_audioSource.isPlaying);

        Destroy(_prefSound);
    }
}
