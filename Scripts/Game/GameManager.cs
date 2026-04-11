using System;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class GameManager : MonoBehaviour
{
    private SpaceshipController spaceship;

    [Header("State debug")]
    [SerializeField] private GameState currentState;

    public static GameManager instance = null;
    public event Action<GameState> OnStateChanged;
    public event Action<SpaceshipController> OnPlayerRegistered;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ChangeState(GameState.Menu);
    }

    public void RegisterShip(SpaceshipController spaceship)
    {
        this.spaceship = spaceship;
        OnPlayerRegistered?.Invoke(spaceship);
    }

    public void UnregisterShip(SpaceshipController spaceship)
    {
        if(this.spaceship == spaceship)
        {
            this.spaceship = null;
        }
    }

    public void ChangeState(GameState newState)
    {
        Debug.Log($"STATE CHANGE: {currentState} -> {newState}");
        currentState = newState;
        OnStateChanged?.Invoke(currentState);
        HandleStateChanged();
    }

    private void HandleStateChanged()
    {
        switch(currentState)
        {
            case GameState.Menu:
                ApplyMenu();
                break;
            case GameState.Playing:
                ApplyPlaying();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(currentState), currentState, null);
        }
    }

    private void ApplyMenu()
    {
        MusicManager.instance.PlayMenuMusic();
    }

    private void ApplyPlaying()
    {
        MusicManager.instance.PlayRandom();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    #region Getter / Setter
    public SpaceshipController GetSpaceshipController()
    {
        return this.spaceship;
    }
    #endregion
}
