using UnityEngine;

[CreateAssetMenu(fileName = "VolumeSettings", menuName = "Scriptable Objects/VolumeSettings")]
public class VolumeSettings : ScriptableObject
{
    [Range(0f, 1f)]
    public float masterVolume = 1f;
}
