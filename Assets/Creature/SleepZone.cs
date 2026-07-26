using UnityEngine;

    public class SleepZone : MonoBehaviour
{
    public static bool PlayerInAnyZone { get; private set; }

    [Header("References")]
    [SerializeField] private SatietyUI satietyUI;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private ImageSlideshow imageSlideshow;

    [Header("Settings")]
    [SerializeField] private int requiredEatCount = 3;
    [SerializeField] private float holdDuration = 1f;
    [SerializeField] private KeyCode sleepKey = KeyCode.S;

    private float holdTimer;
    private bool playerInZone;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            PlayerInAnyZone = true;
            OneTimeTip.FindByTipId("sleep")?.Show();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            PlayerInAnyZone = false;
            holdTimer = 0f;
        }
    }

    private void Update()
    {
        if (!playerInZone) return;
        if (satietyUI == null || satietyUI.CurrentEatCount < requiredEatCount) return;

        if (Input.GetKey(sleepKey))
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= holdDuration)
            {
                holdTimer = 0f;
                ActivateSleep();
            }
        }
        else
            holdTimer = 0f;
    }

    private void ActivateSleep()
    {
        satietyUI.ConsumeSatiety(requiredEatCount);
        playerHealth?.SetRespawnPosition(transform.position);
        playerHealth?.Respawn();
        imageSlideshow?.Reset();
        Debug.Log("Player slept!");
    }
}
