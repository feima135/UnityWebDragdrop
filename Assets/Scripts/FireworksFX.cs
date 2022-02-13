using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// randomly spawn firework particles on screen and cache them etc
/// </summary>
public class FireworksFX : MonoBehaviour
{
    /// <summary>
    /// sample 1 burst effect
    /// </summary>
    public ParticleSystem FireworksFXPrefab;

    /// <summary>
    /// to reuse
    /// </summary>
    private List<ParticleSystem> fireworksPool = new List<ParticleSystem>();

    private void Awake()
    {
        for(int index = 0; index < 5; ++index)
        {
            var clonedFirework = Instantiate(FireworksFXPrefab, transform);
            clonedFirework.gameObject.SetActive(false);

            fireworksPool.Add(clonedFirework);
        }
    }

    /// <summary>
    /// Main client call to play some recycled fireworks
    /// </summary>
    public void PlayFireworks()
    {
        foreach(var fireworkFX in fireworksPool)
        {
            fireworkFX.gameObject.SetActive(true);

            float halfWidth = Screen.width / 2.0f;
            float halfHeight = Screen.height / 2.0f;

            fireworkFX.transform.localPosition = new Vector2(Random.Range(-halfWidth, halfWidth),
                                                             Random.Range(-halfHeight, halfHeight));
        }
        fireworksPool.ForEach(item => item.gameObject.SetActive(true));
    }

    /// <summary>
    /// Main client call to Stop fireworks effect
    /// </summary>
    public void StopFireworks()
    {
        fireworksPool.ForEach(item => item.gameObject.SetActive(false));
    }
}
