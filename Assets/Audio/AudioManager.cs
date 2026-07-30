using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources — 拖入 AudioSource 组件")]
    public AudioSource sfxSource;
    public AudioSource bgmSource;
    public AudioSource climbSource;

    [Header("Player")]
    public AudioClip landClip;
    [Range(0, 1)] public float landVolume = 1f;

    public AudioClip foodPickupClip;
    [Range(0, 1)] public float foodPickupVolume = 1f;

    [Header("Climb")]
    public AudioClip climbClip;
    [Range(0, 1)] public float climbVolume = 1f;

    [Header("Sleep")]
    public AudioClip sleepClip;
    [Range(0, 1)] public float sleepVolume = 1f;

    [Header("BGM")]
    public AudioClip bgmClip;
    [Range(0, 1)] public float bgmVolume = 0.5f;

    [Range(0, 1)] public float masterVolume = 1f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (bgmClip != null && bgmSource != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.volume = bgmVolume * masterVolume;
            bgmSource.Play();
        }
    }

    public void PlayLand()
    {
        if (sfxSource == null || landClip == null) return;
        sfxSource.PlayOneShot(landClip, landVolume * masterVolume);
    }

    public void PlayFoodPickup()
    {
        if (sfxSource == null || foodPickupClip == null) return;
        sfxSource.PlayOneShot(foodPickupClip, foodPickupVolume * masterVolume);
    }

    public void PlaySleep()
    {
        if (sfxSource == null || sleepClip == null) return;
        sfxSource.PlayOneShot(sleepClip, sleepVolume * masterVolume);
    }

    public void StartClimb()
    {
        if (climbSource == null || climbClip == null) return;
        if (climbSource.isPlaying) return;
        climbSource.clip = climbClip;
        climbSource.loop = true;
        climbSource.volume = climbVolume * masterVolume;
        climbSource.Play();
    }

    public void StopClimb()
    {
        if (climbSource != null)
            climbSource.Stop();
    }
}
