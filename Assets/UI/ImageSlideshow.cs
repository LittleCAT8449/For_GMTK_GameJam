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

    private int currentIndex;
    private float stateTimer;

    private enum SlideState { Idle, Opening, Showing, Closing }
    private SlideState state;

    private void Start()
    {
        if (sprites == null || sprites.Count == 0) return;
        displayImage.sprite = sprites[0];
        Open();
        state = SlideState.Showing;
        stateTimer = 0f;
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

    public void Reset()
    {
        currentIndex = 0;
        displayImage.sprite = sprites[0];
        state = SlideState.Showing;
        stateTimer = 0f;
        Open();
    }
}
