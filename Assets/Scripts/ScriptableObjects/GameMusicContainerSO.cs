using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameMusicContainer", menuName = "Game/Audio/Game Music Data")]
public class GameMusicContainerSO : ScriptableObject
{
    public List<AudioClip> musicClips;

    public AudioClip GetMusicFrom(int index)
    {
        if (index < 0 || index >= musicClips.Count)
        {
            Debug.LogWarning("Index out of range for music clips.");
            return null;
        }
        return musicClips[index];
    }
}
