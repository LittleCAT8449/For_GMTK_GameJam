using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageSlideshow : UIMove
{
    [SerializeField] private Image displayImage;
    [SerializeField] private List<Sprite> sprites;
    [SerializeField] private float cycleInterval = 30f;
    [SerializeField] private float showDuration = 5f;

    [Header("Deadline")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private SatietyUI satietyUI;
    [SerializeField] private int requiredEatCount = 3;

    [Header("Show Audio")]
    [SerializeField] private AudioClip normalShowClip;
    [Range(0, 1)] [SerializeField] private float normalShowVolume = 0.5f;
    [SerializeField] private AudioClip urgencyClip;
    [Range(0, 1)] [SerializeField] private float urgencyVolume = 0.8f;

    private int currentIndex;
    private float stateTimer;

    private AudioSource normalShowAudio;
    private AudioSource urgencyAudio;
    private bool isContinuous;

    private enum SlideState { Idle, Opening, Showing, Closing }
    private SlideState state;

    private void Start()
    {
        if (sprites == null || sprites.Count == 0) return;

        normalShowAudio = gameObject.AddComponent<AudioSource>();
        normalShowAudio.playOnAwake = false;
        normalShowAudio.loop = false;
        normalShowAudio.spatialBlend = 0f;

        urgencyAudio = gameObject.AddComponent<AudioSource>();
        urgencyAudio.playOnAwake = false;
        urgencyAudio.loop = true;
        urgencyAudio.spatialBlend = 0f;

        displayImage.sprite = sprites[0];
        Open();
        state = SlideState.Showing;
        stateTimer = 0f;

        PlayNormalShow(0);
        PlayUrgencyIfNeeded(0);
    }

    private void Update()
    {
        stateTimer += Time.deltaTime;

        switch (state)
        {
            case SlideState.Idle:
                if (stateTimer >= cycleInterval)
                {
                    stateTimer = 0f;
                    Open();
                    state = SlideState.Opening;
                }
                break;

            case SlideState.Opening:
                if (stateTimer >= duration)
                {
                    stateTimer = 0f;
                    currentIndex = (currentIndex + 1) % sprites.Count;
                    displayImage.sprite = sprites[currentIndex];

                    PlayNormalShow(currentIndex);
                    PlayUrgencyIfNeeded(currentIndex);

                    state = SlideState.Showing;
                }
                break;

            case SlideState.Showing:
                if (stateTimer >= showDuration)
                {
                    stateTimer = 0f;

                    if (currentIndex == sprites.Count - 1)
                    {
                        if (SleepZone.PlayerInAnyZone && satietyUI != null && satietyUI.CurrentEatCount >= requiredEatCount)
                        {
                            satietyUI.ConsumeSatiety(requiredEatCount);
                            playerHealth?.Respawn();
                        }
                        else
                        {
                            playerHealth?.Kill();
                        }
                        Reset();
                        return;
                    }

                    normalShowAudio.Stop();

                    Close();
                    state = SlideState.Closing;
                }
                break;

            case SlideState.Closing:
                if (stateTimer >= duration)
                {
                    stateTimer = 0f;
                    state = SlideState.Idle;
                }
                break;
        }
    }

    private void PlayNormalShow(int index)
    {
        if (normalShowClip == null || normalShowAudio == null) return;

        normalShowAudio.Stop();
        normalShowAudio.clip = normalShowClip;
        normalShowAudio.volume = normalShowVolume;
        normalShowAudio.loop = false;
        normalShowAudio.Play();
    }

    private void PlayUrgencyIfNeeded(int index)
    {
        bool continuous = index >= sprites.Count - 4;
        if (continuous && urgencyClip != null && urgencyAudio != null && !urgencyAudio.isPlaying)
        {
            urgencyAudio.clip = urgencyClip;
            urgencyAudio.volume = urgencyVolume;
            urgencyAudio.Play();
            isContinuous = true;
        }
    }

    public void Reset()
    {
        normalShowAudio?.Stop();
        urgencyAudio?.Stop();
        isContinuous = false;
        currentIndex = 0;
        displayImage.sprite = sprites[0];
        state = SlideState.Showing;
        stateTimer = 0f;
        Open();
    }
}
