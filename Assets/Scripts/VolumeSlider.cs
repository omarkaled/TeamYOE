using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private VolumeSettings volumeSettings;

    private void Start()
    {
        // Load saved volume into slider
        volumeSlider.value = volumeSettings.masterVolume;

        // Optional: Apply to audio on start
        UpdateVolume(volumeSlider.value);

        // Listen for slider changes
        volumeSlider.onValueChanged.AddListener(UpdateVolume);
    }

    private void UpdateVolume(float value)
    {
        volumeSettings.masterVolume = value;

        // Example: if using AudioListener for global volume
        AudioListener.volume = value;

        // Or if using FMOD, you'd update a bus:
        // RuntimeManager.GetBus("bus:/").setVolume(value);
    }
}

