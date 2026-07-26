using Cinemachine;
using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin perlin;

    private void Awake()
    {
        if (virtualCamera == null)
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
        if (virtualCamera != null)
            perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void Shake(float duration, float amplitude = 1f, float frequency = 2f)
    {
        if (perlin == null)
        {
            Debug.LogWarning("CameraShake: 请在 Virtual Camera 上添加 CinemachineBasicMultiChannelPerlin 扩展");
            return;
        }
        StopAllCoroutines();
        StartCoroutine(ShakeCoroutine(duration, amplitude, frequency));
    }

    private IEnumerator ShakeCoroutine(float duration, float amplitude, float frequency)
    {
        perlin.m_AmplitudeGain = amplitude;
        perlin.m_FrequencyGain = frequency;
        yield return new WaitForSeconds(duration);
        perlin.m_AmplitudeGain = 0f;
        perlin.m_FrequencyGain = 0f;
    }
}
