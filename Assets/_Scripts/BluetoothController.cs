using UnityEngine;
using UnityEngine.UI;
using BlueUnity;
using TMPro;
using System.Collections;
using System.Text;

public class BluetoothController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text status;
    [SerializeField] private TMP_InputField dataToSend;
    [SerializeField] private Toggle pairingToggle;
    [SerializeField] private TMP_Text receivedTextDisplay; // Para mostrar a última mensagem

    [Header("Settings")]
    [SerializeField] private int discoverableDuration = 120;
    
    private BluetoothHandler bluetooth;
    private string receivedDataString = string.Empty;

    IEnumerator Start()
    {
        bluetooth = BluetoothHandler.Instance;
        status.text = "BlueUnity waiting for enable bluetooth";
        
        yield return new WaitUntil(() => bluetooth.Enabled);

        // Assinando os Eventos do Plugin
        bluetooth.ScanStartedAction += () => status.text = "Escaneando...";
        bluetooth.ConnectedAction += (address) => status.text = "Conectado: " + address;
        bluetooth.DisconnectedAction += (address) => status.text = "Desconectado";
        bluetooth.ErrorAction += (error) => status.text = "Erro: " + error;
        
        // Evento crucial para RECEBER dados
        bluetooth.DataReceivedAction += OnDataReceived;

        bluetooth.SetDeviceName("MeuAppUnity");
        status.text = "Pronto para Conectar";

        pairingToggle.onValueChanged.AddListener(SetPairing);
    }

    // --- ENVIAR MENSAGEM ---
    // Chame esta função em um Botão no Unity (OnClick)
    public void SendToArduino()
    {
        if (bluetooth != null && !string.IsNullOrEmpty(dataToSend.text))
        {
            // Converte o texto do InputField para Bytes e envia
            byte[] bytes = Encoding.UTF8.GetBytes(dataToSend.text);
            bluetooth.Write(bytes);
            
            Debug.Log("Enviado: " + dataToSend.text);
            dataToSend.text = ""; // Limpa o campo após enviar
        }
    }

    // --- RECEBER MENSAGEM ---
    private void OnDataReceived(byte[] data)
    {
        // Converte os bytes que o Arduino enviou de volta para string
        receivedDataString = Encoding.UTF8.GetString(data);
        
        // Atualiza o texto na tela (deve ser feito na thread principal)
        Debug.Log("Recebido do Arduino: " + receivedDataString);

        if(receivedDataString == "magnetOn")
        {
            //escolher runa    
        }

        if(receivedDataString == "touchPress")
        {
            SoundManager.Instance.PlaySFX("RiskyCharge");
        }
    }

    private void Update()
    {
        // Como o evento de recebimento pode vir de outra thread, 
        // atualizamos a UI aqui para evitar erros de thread do Unity
        if (!string.IsNullOrEmpty(receivedDataString))
        {
            receivedTextDisplay.text = "Arduino diz: " + receivedDataString;
        }
    }

    public void SetPairing(bool isOn) => bluetooth.SetPairing(isOn);
    
    private void OnDestroy()
    {
        if (bluetooth != null) bluetooth.Cleanup();
    }
}