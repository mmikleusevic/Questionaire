using UnityEngine;

[CreateAssetMenu(fileName = "EnvironmentConfig", menuName = "Config/EnvironmentConfig")]
public class EnvironmentConfig : ScriptableObject
{
    [SerializeField] private string developmentUrl;
    [SerializeField] private string productionUrl;
    //[SerializeField] private string productionCertificateThumbprint;
    
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
            return Instance.developmentUrl;
#else
            return Instance.productionUrl;
#endif
        }
    }

    //Don't need this since we are bypassing ssl certificate handling
//     public static string CertificateThumbprint
//     {
//         get
//         {
// #if UNITY_EDITOR || DEVELOPMENT
//             return null;
// #else
//             return Instance.productionCertificateThumbprint;
// #endif
//         }
//     }
}