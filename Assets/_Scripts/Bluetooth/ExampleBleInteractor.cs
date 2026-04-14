using Android.BLE;
using Android.BLE.Commands;
using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Exemplo de interação com dispositivos BLE (Bluetooth Low Energy).
/// 
/// <para><b>Fluxo básico:</b></para>
/// <list type="number">
/// <item>Usuário digita nome do dispositivo BLE</item>
/// <item>Clica em "Scan" para procurar dispositivos</item>
/// <item>Quando encontra, conecta automaticamente</item>
/// <item>Inscreve para receber notificações do dispositivo</item>
/// <item>Navega para próxima cena para comunicação</item>
/// </list>
/// 
/// <para><b>⚠️ Importante:</b></para>
/// <list type="bullet">
/// <item>Certifique-se de ter permissões BLUETOOTH_SCAN e BLUETOOTH_CONNECT</item>
/// <item>O dispositivo BLE deve estar anunciando (advertising)</item>
/// <item>Use o nome exato do dispositivo (case-sensitive)</item>
/// </list>
/// </summary>
public class ExampleBleInteractor : MonoBehaviour
{
    #region Inspector Fields

    [Header("Configuração da UI")]
    [SerializeField]
    [Tooltip("Campo de texto onde o usuário digita o nome do dispositivo BLE")]
    private TMP_InputField InputNomeBlueTooh;

    [SerializeField]
    [Tooltip("Botão que inicia o scan de dispositivos")]
    private Button botaoScan;

    [SerializeField] private TMP_Text status;

    [SerializeField]
    [Tooltip("Prefab do botão de dispositivo (se usar lista de dispositivos)")]
    private GameObject _deviceButton;

    [Header("Configuração do Scan")]
	[SerializeField]
    [Tooltip("Tempo máximo de scan em segundos")]
    private int _scanTime = 10;

    #endregion

    #region Private Fields

    // Nome do dispositivo BLE a procurar
  private string nomeBlueTooth;

    // Timer para controlar duração do scan
    private float _scanTimer = 0f;

    // Flag indicando se está fazendo scan
    private bool _isScanning = false;

    // UUID do dispositivo encontrado
    private string _deviceUuid = string.Empty;

    // Nome do dispositivo conectado
  private string _deviceName = string.Empty;

  // Comando de conexão (guardado para desconectar depois)
    private ConnectToDevice _connectCommand;

    // Flag indicando se está conectado
    private bool _isConnected = false;

 // Constante para PlayerPrefs
    private const string PREF_NOME_BLUETOOTH = "nomeBlueTooth";

    #endregion

 #region Unity Lifecycle

    private void Start()
    {
        // Carrega nome salvo anteriormente
    CarregarNomeSalvo();

        // Registra listener para mudanças no input
        if (InputNomeBlueTooh != null)
 {
    InputNomeBlueTooh.onValueChanged.AddListener(OnInputFieldValueChanged);
   }
        else
        {
        Debug.LogError("❌ InputNomeBlueTooh não está configurado no Inspector!");
    }

        // Configura texto do botão
        AtualizarTextoBotao();

  // Atualiza status inicial
        AtualizarStatus("Aguardando scan...");
    }

    private void Update()
    {
        // Atualiza timer de scan
if (_isScanning)
        {
_scanTimer += Time.deltaTime;
          
            // Mostra progresso
            float progresso = (_scanTimer / _scanTime) * 100f;
    AtualizarStatus($"Procurando... {progresso:F0}%");

  // Termina scan após tempo limite
            if (_scanTimer > _scanTime)
            {
              _scanTimer = 0f;
    _isScanning = false;
                AtualizarStatus("Dispositivo não encontrado. Tente novamente.");
         Debug.LogWarning($"⚠️ Scan finalizado. Dispositivo '{nomeBlueTooth}' não foi encontrado.");
            }
        }
    }

    private void OnDestroy()
    {
        // Remove listener ao destruir
        if (InputNomeBlueTooh != null)
        {
            InputNomeBlueTooh.onValueChanged.RemoveListener(OnInputFieldValueChanged);
        }

        // Desconecta se estiver conectado
        // if (_isConnected && _connectCommand != null)
        // {
        //     _connectCommand.Disconnect();
        // }
 }

    #endregion

    #region UI Callbacks

    /// <summary>
    /// Chamado quando o valor do campo de texto muda.
    /// </summary>
    private void OnInputFieldValueChanged(string novoNome)
    {
        nomeBlueTooth = novoNome;
        AtualizarTextoBotao();
        Debug.Log($"📝 Nome do dispositivo atualizado: {nomeBlueTooth}");
    }

    /// <summary>
    /// Inicia o scan de dispositivos BLE.
    /// Chamado quando o usuário clica no botão "Scan".
    /// </summary>
    public void ScanForDevices()
    {
   // Validações
        if (string.IsNullOrEmpty(nomeBlueTooth))
        {
            Debug.LogWarning("⚠️ Nome do dispositivo vazio! Digite um nome válido.");
      AtualizarStatus("⚠️ Digite o nome do dispositivo!");
            return;
     }

        if (_isScanning)
     {
     Debug.LogWarning("⚠️ Scan já está em andamento!");
      return;
        }

      if (_isConnected)
 {
            Debug.LogWarning("⚠️ Já está conectado a um dispositivo!");
     return;
        }

 // Inicia scan
        Debug.Log($"🔍 Iniciando scan... Procurando por: {nomeBlueTooth}");
        AtualizarStatus($"🔍 Procurando {nomeBlueTooth}...");
        
        _isScanning = true;
        _scanTimer = 0f;

        // Cria e enfileira comando de descoberta
        try
        {
        DiscoverDevices comando = new DiscoverDevices(OnDeviceFound, _scanTime * 1000);
            BleManager.Instance.QueueCommand(comando);
        }
    catch (Exception ex)
        {
  Debug.LogError($"❌ Erro ao iniciar scan: {ex.Message}");
       AtualizarStatus($"❌ Erro: {ex.Message}");
 _isScanning = false;
        }
    }

  #endregion

    #region BLE Callbacks

    /// <summary>
    /// Callback chamado quando um dispositivo BLE é encontrado durante o scan.
    /// </summary>
    /// <param name="uuid">UUID único do dispositivo</param>
    /// <param name="deviceName">Nome do dispositivo</param>
    private void OnDeviceFound(string uuid, string deviceName)
    {
        Debug.Log($"📱 Dispositivo encontrado: {deviceName} ({uuid})");

      // Verifica se é o dispositivo que estamos procurando
        if (deviceName == nomeBlueTooth)
      {
 Debug.Log($"✅ Dispositivo alvo encontrado: {deviceName}");
            AtualizarStatus($"✅ Encontrado: {deviceName}");

 // Para o scan
     _isScanning = false;
            _scanTimer = 0f;

     // Guarda informações
            _deviceUuid = uuid;
  _deviceName = deviceName;

  // Tenta conectar
     Connect();
  }
    }

    /// <summary>
    /// Inicia conexão com o dispositivo BLE encontrado.
    /// </summary>
    public void Connect()
    {
        if (_isConnected)
        {
          Debug.LogWarning("⚠️ Já conectado!");
            return;
        }

  if (string.IsNullOrEmpty(_deviceUuid))
 {
         Debug.LogError("❌ UUID do dispositivo vazio! Faça o scan primeiro.");
            return;
        }

    Debug.Log($"🔗 Conectando ao dispositivo: {_deviceName} ({_deviceUuid})");
        AtualizarStatus($"🔗 Conectando...");

  try
        {
_connectCommand = new ConnectToDevice(_deviceUuid, OnConnected, OnDisconnected);
     BleManager.Instance.QueueCommand(_connectCommand);
        }
        catch (Exception ex)
  {
          Debug.LogError($"❌ Erro ao conectar: {ex.Message}");
          AtualizarStatus($"❌ Erro na conexão: {ex.Message}");
        }
    }

    /// <summary>
    /// Callback chamado quando a conexão é estabelecida com sucesso.
 /// </summary>
    /// <param name="deviceUuid">UUID do dispositivo conectado</param>
    private void OnConnected(string deviceUuid)
    {
_isConnected = true;

        Debug.Log($"✅ Conectado com sucesso ao: {_deviceName}");
        AtualizarStatus($"✅ Conectado: {_deviceName}");

   // Esconde botão de scan
 if (botaoScan != null)
        {
        botaoScan.gameObject.SetActive(false);
        }

        // Inscreve para receber notificações
   SubscribeToExampleService();
    }

    /// <summary>
    /// Callback chamado quando o dispositivo é desconectado.
    /// </summary>
    /// <param name="deviceUuid">UUID do dispositivo desconectado</param>
    private void OnDisconnected(string deviceUuid)
    {
        _isConnected = false;

      Debug.LogWarning($"⚠️ Desconectado de: {_deviceName}");
        AtualizarStatus($"⚠️ Desconectado");

        // Limpa informações
        _deviceUuid = string.Empty;
     _deviceName = string.Empty;
        _scanTimer = 0f;
 _isScanning = false;

        // Desconecta formalmente
        if (_connectCommand != null)
   {
     _connectCommand.Disconnect();
        _connectCommand = null;
        }

        // Mostra botão de scan novamente
        if (botaoScan != null)
        {
    botaoScan.gameObject.SetActive(true);
    }
 }

    /// <summary>
    /// Inscreve para receber notificações do serviço de exemplo.
    /// Delega para o GerenciarComunicacao que gerencia a comunicação de dados.
    /// </summary>
 public void SubscribeToExampleService()
    {
        try
  {
        GerenciarComunicacao gc = GameObject.Find("Comunicacao")?.GetComponent<GerenciarComunicacao>();

  if (gc == null)
       {
                Debug.LogError("❌ GerenciarComunicacao não encontrado! Certifique-se de ter um GameObject chamado 'Comunicacao' na cena.");
      return;
   }

            gc.SubscribeServico(_deviceUuid);
    Debug.Log("📡 Inscrito para receber notificações do dispositivo");
        }
  catch (Exception ex)
        {
      Debug.LogError($"❌ Erro ao inscrever no serviço: {ex.Message}");
        }
    }

    #endregion

    #region Navigation

    /// <summary>
    /// Navega para outra cena.
    /// Salva o nome do dispositivo antes de trocar de cena.
    /// </summary>
    /// <param name="cena">Nome da cena de destino</param>
    public void OnNavega(string cena)
    {
        if (string.IsNullOrEmpty(cena))
        {
            Debug.LogError("Nome da cena vazio!");
            return;
        }

        Debug.Log($"Navegando para cena: {cena}");

        // Salva nome do dispositivo
        SalvarNome();

        // Desativa este GameObject mas mantém o BleManager vivo
        gameObject.SetActive(false);

        // Carrega próxima cena
        try
        {
            SceneManager.LoadScene(cena);
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Erro ao carregar cena '{cena}': {ex.Message}");
            gameObject.SetActive(true); // Reativa em caso de erro
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Carrega o nome do dispositivo salvo nas preferências.
    /// </summary>
    private void CarregarNomeSalvo()
    {
        if (PlayerPrefs.HasKey(PREF_NOME_BLUETOOTH))
        {
 nomeBlueTooth = PlayerPrefs.GetString(PREF_NOME_BLUETOOTH);
            
            if (InputNomeBlueTooh != null)
      {
 InputNomeBlueTooh.text = nomeBlueTooth;
            }

   Debug.Log($"💾 Nome carregado: {nomeBlueTooth}");
   }
        else
        {
            nomeBlueTooth = "MIDGARD"; // Nome padrão
            Debug.Log($"💾 Usando nome padrão: {nomeBlueTooth}");
        }
    }

    /// <summary>
    /// Salva o nome do dispositivo nas preferências.
    /// </summary>
    private void SalvarNome()
    {
        PlayerPrefs.SetString(PREF_NOME_BLUETOOTH, nomeBlueTooth);
        PlayerPrefs.Save();
        Debug.Log($"💾 Nome salvo: {nomeBlueTooth}");
    }

    /// <summary>
    /// Atualiza o texto do botão de scan.
    /// </summary>
    private void AtualizarTextoBotao()
    {
        if (botaoScan != null)
        {
            Text textoBotao = botaoScan.GetComponentInChildren<Text>();
   
        if (textoBotao != null)
        {
   string nomeExibir = string.IsNullOrEmpty(nomeBlueTooth) ? "???" : nomeBlueTooth;
     textoBotao.text = $"🔍 Scan {nomeExibir}";
            }
        }
    }

    /// <summary>
    /// Atualiza o texto de status (se disponível).
    /// </summary>
    private void AtualizarStatus(string mensagem)
    {
        if (status != null)
        {
            status.text = mensagem;
  }

        Debug.Log($"[Status] {mensagem}");
    }

    #endregion

  #region Public Utility Methods

    /// <summary>
    /// Desconecta manualmente do dispositivo atual.
    /// Útil para botões de "Desconectar".
    /// </summary>
    public void DesconectarManualmente()
    {
        if (!_isConnected)
  {
            Debug.LogWarning("⚠️ Não está conectado a nenhum dispositivo!");
            return;
        }

  Debug.Log("🔌 Desconectando manualmente...");
        
 if (_connectCommand != null)
        {
    _connectCommand.Disconnect();
        }
 }

    /// <summary>
    /// Cancela o scan em andamento.
    /// </summary>
    public void CancelarScan()
    {
if (!_isScanning)
        {
            Debug.LogWarning("⚠️ Nenhum scan em andamento!");
return;
 }

        Debug.Log("❌ Scan cancelado pelo usuário");
        _isScanning = false;
 _scanTimer = 0f;
        AtualizarStatus("Scan cancelado");
    }

    #endregion
}
