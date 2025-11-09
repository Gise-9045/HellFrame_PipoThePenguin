using UnityEngine;

[DefaultExecutionOrder(-6)]
public class MainManager : MonoBehaviour
{
    [Header("Important References")]
    [SerializeField] private GameObject playerGameobject;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private RailCamera railCamera;

    // Public getters
    public GameObject PlayerGameobject => playerGameobject;
    public Camera MainCamera => mainCamera;
    public RailCamera RailCamera => railCamera;

    public static MainManager Instance { get; private set; }

    private void Awake()
    {
        InitializeSingleton();
    }

    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        ValidateReferences();
    }

    private void ValidateReferences()
    {
        if (playerGameobject == null)
            Debug.LogWarning("MainManager: Player GameObject must be assigned");
        
        if (mainCamera == null)
            Debug.LogWarning("MainManager: Main Camera must be assigned");
    }
}