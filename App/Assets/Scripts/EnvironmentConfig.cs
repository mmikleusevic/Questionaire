using UnityEngine;

[CreateAssetMenu(fileName = "EnvironmentConfig", menuName = "Config/EnvironmentConfig")]
public class EnvironmentConfig : ScriptableObject
{
    [SerializeField] private string developmentUrl;
    [SerializeField] private string productionUrl;
    
    private static EnvironmentConfig instance;

    private static EnvironmentConfig Instance
    {
        get
        {
            if (instance) return instance;
            
            instance = Resources.Load<EnvironmentConfig>("EnvironmentConfig");
                
            if (!instance)
            {
                Debug.LogError("Failed to load EnvironmentConfig. Ensure it exists in a Resources folder.");
            }
            
            return instance;
        }
    }
    
    public static string ApiBaseUrl
    {
        get
        {
#if UNITY_EDITOR || DEVELOPMENT
            Debug.Log("Using Development URL");
            return Instance.developmentUrl;
#else
            Debug.Log("Using Production URL");
            return Instance.productionUrl;
#endif
        }
    }
}