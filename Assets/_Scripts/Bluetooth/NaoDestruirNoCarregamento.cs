using UnityEngine;

public class NaoDestruirNoCarregamento : MonoBehaviour
{
    void Start()
    {
        transform.SetParent(null); 
        DontDestroyOnLoad(gameObject);
    }

}
