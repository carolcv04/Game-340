using UnityEngine;
using UnityEngine.UI;

public class SoundEffectManager : MonoBehaviour
{
    private static SoundEffectManager Instance;
    private static SoundEffectLibrary soundEffectLibrary;

    private static AudioSource audioSource;
    private static AudioSource randomPitchAudioSource;

    [SerializeField] private Slider sfxSlider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            AudioSource[] audioSources = GetComponents<AudioSource>();
            audioSource = audioSources[0];
            randomPitchAudioSource = audioSources[1];
            soundEffectLibrary = GetComponent<SoundEffectLibrary>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public static void Play(string soundName, bool randomPitch = false)
    {
        AudioClip audioClip = soundEffectLibrary.GetRandomClip(soundName);

        if (audioClip != null)
        {
            if (randomPitch)
            {
                randomPitchAudioSource.pitch = Random.Range(1f, 1.5f);
            }
            audioSource.PlayOneShot(audioClip);
        }
    }
    void Start()
    {
        sfxSlider.onValueChanged.AddListener(delegate { OnValueChanged(); });
    }
    public static void SetVolume(float volume)
    {
        audioSource.volume = volume;
        randomPitchAudioSource.volume = volume;
    }
    public void OnValueChanged()
    {
        SetVolume(sfxSlider.value);
    }
}
