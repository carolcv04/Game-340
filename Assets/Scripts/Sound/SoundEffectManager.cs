using UnityEngine;
using UnityEngine.UI;

public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager Instance;
    private static SoundEffectLibrary soundEffectLibrary;

    private static AudioSource sfxSource;           // Regular SFX
    private static AudioSource footstepSource;      // Footsteps with random pitch
    private static AudioSource uiSource;            // UI sounds

    [SerializeField] private Slider sfxSlider;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            AudioSource[] audioSources = GetComponents<AudioSource>();
            
            // Check if we have enough AudioSources (now need 3)
            if (audioSources.Length < 3)
            {
                Debug.LogError("SoundEffectManager needs 3 AudioSource components!");
                return;
            }
            
            sfxSource = audioSources[0];
            footstepSource = audioSources[1];
            uiSource = audioSources[2];
            
            soundEffectLibrary = GetComponent<SoundEffectLibrary>();
            
            if (soundEffectLibrary == null)
            {
                Debug.LogError("SoundEffectLibrary component not found!");
            }
            
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public static void Play(string soundName, bool randomPitch = false)
    {
        if (Instance == null || soundEffectLibrary == null) return;
        
        AudioClip audioClip = soundEffectLibrary.GetRandomClip(soundName);
        if (audioClip == null) return;

        if (randomPitch)
        {
            footstepSource.pitch = Random.Range(0.9f, 1.1f);
            footstepSource.PlayOneShot(audioClip);
        }
        else
        {
            sfxSource.PlayOneShot(audioClip);
        }
    }
    
    public static void PlayUI(string soundName)
    {
        if (Instance == null || soundEffectLibrary == null) return;
        
        AudioClip audioClip = soundEffectLibrary.GetRandomClip(soundName);
        if (audioClip != null)
        {
            uiSource.PlayOneShot(audioClip);
        }
    }
    
    void Start()
    {
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(delegate { OnValueChanged(); });
        }
    }
    
    public static void SetVolume(float volume)
    {
        if (sfxSource != null) sfxSource.volume = volume;
        if (footstepSource != null) footstepSource.volume = volume;
        if (uiSource != null) uiSource.volume = volume;
    }
    
    public void OnValueChanged()
    {
        SetVolume(sfxSlider.value);
    }
}