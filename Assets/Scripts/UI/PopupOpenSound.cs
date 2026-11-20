using UnityEngine;

/// <summary>Plays a one-shot sound whenever its popup becomes visible.</summary>
[DisallowMultipleComponent]
public sealed class PopupOpenSound : MonoBehaviour
{
    [SerializeField] private AudioClip sound;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    private void OnEnable()
    {
        if (!AudioSettingsStorage.IsSoundEnabled)
        {
            return;
        }

        if (sound == null)
        {
            Debug.LogWarning("PopupOpenSound: Sound is not assigned.", this);
            return;
        }

        GameObject soundObject = new GameObject($"{gameObject.name} Sound");
        DontDestroyOnLoad(soundObject);

        AudioSource audioSource = soundObject.AddComponent<AudioSource>();
        audioSource.clip = sound;
        audioSource.volume = volume;
        audioSource.spatialBlend = 0f;
        audioSource.ignoreListenerPause = true;
        audioSource.Play();

        Destroy(soundObject, sound.length + 0.1f);
    }
}
