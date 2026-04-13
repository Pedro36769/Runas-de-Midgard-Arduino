using UnityEngine;

public class GerenciamentoBluetooth : MonoBehaviour
{
    public static GerenciamentoBluetooth Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        transform.SetParent(null); 
        
        DontDestroyOnLoad(gameObject);
    }
}