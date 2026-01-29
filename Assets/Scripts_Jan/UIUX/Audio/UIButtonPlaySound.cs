using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class UIButtonPlaySound : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField] private AudioClip clip;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f; // 2D
    }

    public void PlaySound()
    {
        if (clip == null)
        {
            Debug.LogWarning($"[{name}] UIButtonPlaySound: Kein AudioClip gesetzt.");
            return;
        }

        source.PlayOneShot(clip, volume);
    }
}

