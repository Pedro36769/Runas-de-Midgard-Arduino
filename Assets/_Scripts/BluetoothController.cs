using BlueUnity;
using System.Collections;
using System.Collections.Generic; // Necessário para a Queue
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BluetoothController : MonoBehaviour
{
    [Header("Configurações de UI")]
    [Tooltip("O container (ex: Content do ScrollView) onde os botões aparecerão")]
    public Transform devicesContainer;
    [Tooltip("Prefab do botão para cada dispositivo encontrado")]
    public GameObject deviceButtonPrefab;
    [Tooltip("Texto para exibir o status atual da conexão")]
    public TMP_Text statusText;

    private BluetoothHandler bluetooth;
    private bool isConnected = false;
    
    // Controle de Mensagens (Buffer para evitar strings cortadas)
    private string messageBuffer = string.Empty;
    private bool shouldSendReply = false;

    // Gerenciamento de UI e Threads
    private string statusMessageToUpdate = string.Empty;
    private bool hasNewStatus = false;
    private struct DeviceInfo { public string name; public string address; }
    private Queue<DeviceInfo> devicesFoundQueue = new Queue<DeviceInfo>();

    IEnumerator Start()
    {
        bluetooth = BluetoothHandler.Instance;
        UpdateStatus("Inicializando Bluetooth...");
        
        // Aguarda o Bluetooth estar pronto
        yield return new WaitUntil(() => bluetooth.Enabled);

        // Callbacks do BlueUnity
        bluetooth.ScanDeviceFoundAction += OnDeviceFound;
        bluetooth.ConnectedAction += OnConnected;
        bluetooth.DisconnectedAction += OnDisconnected;
        bluetooth.DataReceivedAction += OnDataReceived;
        bluetooth.ErrorAction += OnError;

        UpdateStatus("Bluetooth Pronto. Toque em 'Escanear'.");
        
        // Se quiser que ele comece a buscar sozinho ao abrir a tela, descomente a linha abaixo:
        // IniciarScan();
    }

    private void Update()
    {
        // 1. Processa novos dispositivos encontrados na Main Thread
        while (devicesFoundQueue.Count > 0)
        {
            DeviceInfo device = devicesFoundQueue.Dequeue();
            CriarBotaoDispositivo(device.name, device.address);
        }

        // 2. Atualiza texto de status
        if (hasNewStatus)
        {
            hasNewStatus = false;
            if (statusText != null) statusText.text = statusMessageToUpdate;
        }

        // 3. Verifica se precisa responder ao Arduino
        if (shouldSendReply)
        {
            shouldSendReply = false;
            ResponderRoxa();
        }
    }

    // --- MÉTODOS DE AÇÃO ---

    public void IniciarScan()
    {
        // Limpa lista visual antes de novo scan
        foreach (Transform child in devicesContainer) Destroy(child.gameObject);
        
        UpdateStatus("Buscando dispositivos...");
        bluetooth.StartScan();
    }

    private void Conectar(string endereco)
    {
        bluetooth.StopScan();
        UpdateStatus("Conectando... Verifique notificações de pareamento.");
        bluetooth.ConnectAsClient(endereco);
    }

    private void ResponderRoxa()
    {
        if (!isConnected) return;
        
        // Enviamos com \n para que o Arduino saiba onde a mensagem termina
        byte[] data = Encoding.UTF8.GetBytes("ROXA\n");
        bluetooth.Write(data);
        UpdateStatus("Comando 'ROXA' enviado!");
    }

    // --- CALLBACKS DO SISTEMA ---

    private void OnDeviceFound(string name, string address)
    {
        if (string.IsNullOrEmpty(name) || name.ToLower().Contains("unknown")) return; //caso não tenha nome ou seja desconhecido, não aparece na lista
        devicesFoundQueue.Enqueue(new DeviceInfo { name = name, address = address });
    }

    private void OnDataReceived(byte[] data)
    {
        string chunk = Encoding.UTF8.GetString(data);
        messageBuffer += chunk;

        // Se o buffer contém a palavra-chave
        if (messageBuffer.Contains("ESCOLHER_RUNA"))
        {
            messageBuffer = string.Empty; // Limpa para a próxima
            shouldSendReply = true;       // Ativa a flag para o Update()
        }
    }

    private void OnConnected(string address)
    {
        isConnected = true;
        UpdateStatus($"Conectado a: {address}");
    }

    private void OnDisconnected(string address)
    {
        isConnected = false;
        UpdateStatus("Dispositivo desconectado.");
    }

    private void OnError(string error)
    {
        UpdateStatus($"Erro: {error}");
    }

    // --- AUXILIARES ---

    private void CriarBotaoDispositivo(string nome, string endereco)
    {
        GameObject btnObj = Instantiate(deviceButtonPrefab, devicesContainer);
        
        // Configura o texto do botão
        TMP_Text txt = btnObj.GetComponentInChildren<TMP_Text>();
        if (txt != null) txt.text = $"{(string.IsNullOrEmpty(nome) ? "Desconhecido" : nome)}\n<size=60%>{endereco}</size>";

        // Configura o clique
        Button btn = btnObj.GetComponent<Button>();
        btn.onClick.AddListener(() => Conectar(endereco));
    }

    private void UpdateStatus(string msg)
    {
        statusMessageToUpdate = msg;
        hasNewStatus = true;
    }

    private void OnDestroy()
    {
        if (bluetooth != null)
        {
            bluetooth.Disconnect();
            bluetooth.Cleanup();
        }
    }
}