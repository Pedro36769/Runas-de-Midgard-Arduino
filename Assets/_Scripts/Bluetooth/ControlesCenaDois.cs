using System;
using TMPro;
using UnityEngine;

public class ControlesCenaDois : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI recebidos;

    Action<String> Enviador;

    public static ControlesCenaDois Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this.gameObject);
        else Instance = this; DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        GameObject gm = GameObject.Find("Comunicacao");
        GerenciarComunicacao gc = gm.GetComponent<GerenciarComunicacao>();
        gc.RegistraRecebedor(Receber);
        Enviador = gc.Enviar;
    }

    public void Receber(string[] dados)
    {
        recebidos.text = dados[0];
    }
    
    public void Enviar(string dados)
    {
        Enviador(dados);
    }
}
