using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlesCenaDois : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI recebidos;

    public static ControlesCenaDois Instance { get; private set; }
    
    // Variáveis para sincronização de threads
    private string _textoParaMostrar = "";
    private bool _precisaAtualizarUI = false;

    private Action<string> _metodoEnviar;

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        {
            Destroy(this.gameObject);
        }
        else 
        {
            Instance = this;
        }
    }

    void Start()
    {
        // Procura o objeto que persiste entre cenas
        GameObject gm = GameObject.Find("Comunicacao");
        
        if (gm != null)
        {
            GerenciarComunicacao gc = gm.GetComponent<GerenciarComunicacao>();
            gc.RegistraRecebedor(Receber);
            _metodoEnviar = gc.Enviar;
            Debug.Log("✅ Vinculado ao GerenciarComunicacao com sucesso.");
        }
        else
        {
            Debug.LogError("❌ Objeto 'Comunicacao' não encontrado! Verifique se ele tem DontDestroyOnLoad.");
        }
    }

    void Update()
    {
        // O Update roda na Main Thread, então aqui é seguro mexer na UI
        if (_precisaAtualizarUI)
        {
            if (recebidos != null)
            {
                recebidos.text = "Status: " + _textoParaMostrar;
            }
            _precisaAtualizarUI = false;
        }
    }

    // Este método é chamado pela thread do Bluetooth
    public void Receber(string[] dados)
    {
        if (dados != null && dados.Length > 0)
        {
            // Apenas guardamos o dado e avisamos a Main Thread
            _textoParaMostrar = dados[0]; 
            _precisaAtualizarUI = true;
            
            // O Log funciona em qualquer thread, use para debugar no Logcat
            Debug.Log($"📥 Arduino Ecoou: {dados[0]}");
        }
    }
    
    public void Enviar(string comando)
    {
        if (_metodoEnviar != null)
        {
            _metodoEnviar(comando);
            Debug.Log($"📤 Enviando: {comando}");
        }
        else
        {
            Debug.LogWarning("⚠️ Enviador não inicializado.");
        }
    }

    public void LoadCenaDois()
    {
        SceneManager.LoadScene("3_Cena2");
    }
}