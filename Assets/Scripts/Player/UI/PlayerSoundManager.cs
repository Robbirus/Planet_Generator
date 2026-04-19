using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSoundManager : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource movementAudio;
    [SerializeField] private AudioSource actionAudio;

    [Header("Audio Clip")]
    [SerializeField] private WeaponManager weaponManager;

    private void OnEnable()
    {
        if(weaponManager != null)
        {
            weaponManager.OnPlayFireSound += PlayGunShot;
        }
    }

    private void OnDisable()
    {
        if(weaponManager != null)
        {
            weaponManager.OnPlayFireSound -= PlayGunShot;
        }
    }

    public void PlayGunShot(AudioClip clip)
    {
        if(clip == null || actionAudio == null) return;
        actionAudio.PlayOneShot(clip);
    }

    public void PlayGunShotCrit()
    {

    }

    public void PlayReload()
    {

    }
}