using UnityEngine;
using UnityEngine.UI;

/// <summary>Plays a UI tap sound when the assigned button is clicked.</summary>
[DisallowMultipleComponent]
public sealed class ButtonTapSound : MonoBehaviour
{
    [SerializeField] private Button targetButton;
    [SerializeField] private AudioClip tapSound;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    private void OnEnable()
    {
        if (targetButton == null)
        {
            Debug.LogError("ButtonTapSound: Target Button is not assigned.", this);
            return;
        }

        targetButton.onClick.AddListener(PlayTapSound);
    }

    private void OnDisable()
    {
        targetButton.onClick.RemoveListener(PlayTapSound);
    }

    private void PlayTapSound()
    {
        if (!AudioSettingsStorage.IsSoundEnabled)
        {
            return;
        }

        if (tapSound == null)
        {
            Debug.LogWarning("ButtonTapSound: Tap Sound is not assigned.", this);
            return;
        }

        GameObject soundObject = new GameObject("Button Tap Sound");
        DontDestroyOnLoad(soundObject);

        AudioSource audioSource = soundObject.AddComponent<AudioSource>();
        audioSource.clip = tapSound;
        audioSource.volume = volume;
        audioSource.spatialBlend = 0f;
        audioSource.ignoreListenerPause = true;
        audioSource.Play();

        Destroy(soundObject, tapSound.length + 0.1f);
    }
}
