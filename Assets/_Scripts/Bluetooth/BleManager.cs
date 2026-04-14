using Android.BLE.Commands;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Android.BLE
{
    /// <summary>
    /// Gerenciador Singleton que controla todas as interações BLE do plugin.
    /// 
    /// <para><b>Como usar:</b></para>
    /// <code>
    /// // 1. Obter instância
    /// BleManager.Instance.Initialize();
    /// 
    /// // 2. Enviar comando
    /// DiscoverDevices comando = new DiscoverDevices(OnDeviceFound, 10000);
    /// BleManager.Instance.QueueCommand(comando);
    /// </code>
/// 
    /// <para><b>Importante:</b></para>
    /// <list type="bullet">
    /// <item>Este é um Singleton - use <c>BleManager.Instance</c></item>
    /// <item>Todos os comandos são enfileirados e executados sequencialmente</item>
    /// <item>Comandos paralelos (ex: SubscribeToCharacteristic) rodam simultaneamente</item>
    /// </list>
    /// </summary>
    public class BleManager : MonoBehaviour
    {
      #region Singleton Pattern
   
        /// <summary>
 /// Obtém a instância Singleton do <see cref="BleManager"/>
        /// ou cria uma se não existir.
        /// </summary>
   public static BleManager Instance
     {
            get
 {
   if (_instance != null)
         return _instance;
     else
              {
 CreateBleManagerObject();
   return _instance;
  }
            }
        }
        private static BleManager _instance;
        
     #endregion

        #region Properties

    /// <summary>
        /// Retorna <see langword="true"/> se o <see cref="BleManager"/> está inicializado.
        /// Verifique antes de enviar comandos.
      /// </summary>
     public static bool IsInitialized { get => _initialized; }
 private static bool _initialized = false;

        #endregion

        #region Inspector Fields

   [SerializeField]
        private BleAdapter _adapter;

        /// <summary>
        /// Se <see langword="true"/>, <see cref="Initialize"/> é chamado automaticamente no <see cref="Awake"/>.
  /// Desative se quiser controlar a inicialização manualmente.
        /// </summary>
   [Tooltip("Use Initialize() manualmente se desmarcar esta opção")]
  public bool InitializeOnAwake = true;

        [Header("Configurações de Log")]
   
   /// <summary>
    /// Se <see langword="true"/>, todas as interações com o <see cref="BleManager"/> serão logadas.
        /// Útil para debug, mas pode gerar muito log.
    /// </summary>
        [Tooltip("Loga todas as mensagens que passam pelo BleManager (muito verbose)")]
        public bool LogAllMessages = false;

        /// <summary>
        /// Se <see langword="true"/>, logs do Android serão exibidos no <see cref="Debug.Log(object)"/> do Unity.
        /// </summary>
        [Tooltip("Envia mensagens para o sistema de Debug.Log do Unity")]
        public bool UseUnityLog = true;

        /// <summary>
        /// Se <see langword="true"/>, logs do Unity serão enviados para o LogCat do Android.
        /// </summary>
        [Tooltip("Envia mensagens para o Logcat do Android (visualize via adb logcat)")]
   public bool UseAndroidLog = false;

        #endregion

  #region Internal Fields

        /// <summary>
        /// Hook para a biblioteca Java BleManager.
        /// </summary>
        internal static AndroidJavaObject _bleLibrary = null;

        /// <summary>
        /// Fila de <see cref="BleCommand"/> aguardando processamento.
        /// Comandos são executados em ordem (FIFO - First In, First Out).
     /// </summary>
        private readonly Queue<BleCommand> _commandQueue = new Queue<BleCommand>();

        /// <summary>
        /// Stack de <see cref="BleCommand"/> executando em paralelo.
        /// Usado para comandos contínuos como SubscribeToCharacteristic.
        /// </summary>
        private readonly List<BleCommand> _parrallelStack = new List<BleCommand>();

        /// <summary>
        /// Comando ativo não-paralelo ou contínuo.
        /// Apenas um comando não-paralelo pode estar ativo por vez.
        /// </summary>
    private static BleCommand _activeCommand = null;

        /// <summary>
        /// Timer para rastrear tempo de execução do comando ativo.
        /// Usado para implementar timeout de comandos.
        /// </summary>
        private static float _activeTimer = 0f;

        #endregion

   #region Unity Lifecycle

    private void Awake()
        {
            // Garante que só existe uma instância
            if (_instance != null && _instance != this)
    {
      Destroy(gameObject);
         return;
          }

      _instance = this;
            //DontDestroyOnLoad(gameObject); // Persiste entre cenas

     if (InitializeOnAwake)
        Initialize();

            // Registra callbacks do adapter
     if (_adapter != null)
            {
    _adapter.OnMessageReceived += OnBleMessageReceived;
   _adapter.OnErrorReceived += OnErrorReceived;
      }
        }

    private void Update()
  {
            // Verifica timeout do comando ativo
            if (_activeCommand != null)
   {
          _activeTimer += Time.deltaTime;

     if (_activeTimer > _activeCommand.Timeout)
          {
  CheckForLog($"⏱️ Timeout: {_activeCommand.GetType().Name} ({_activeCommand.Timeout}s)");

                 // Finaliza comando por timeout
                  _activeTimer = 0f;
     _activeCommand.EndOnTimeout();

          // Processa próximo comando na fila
       ProcessNextCommand();
              }
    }
        }

        private void OnDestroy()
        {
            //DeInitialize();
        }

        #endregion

        #region Initialization

        /// <summary>
    /// Inicializa a instância do <see cref="BleManager"/>.
   /// Configura os hooks da biblioteca Java e prepara o <see cref="BleAdapter"/> para receber mensagens.
        /// 
        /// <para><b>Chamada manual:</b></para>
        /// <code>
        /// BleManager.Instance.Initialize();
    /// </code>
        /// </summary>
        public void Initialize()
   {
if (_initialized)
            {
       Debug.LogWarning("BleManager já está inicializado!");
   return;
     }

            try
            {
      // Cria instância Singleton se necessário
   if (_instance == null)
           CreateBleManagerObject();

    // Prepara BleAdapter para receber mensagens
        SetupAdapter();

                // Conecta com a biblioteca Android
     SetupAndroidLibrary();

 _initialized = true;
      Debug.Log("✅ BleManager inicializado com sucesso!");
            }
    catch (Exception ex)
            {
    Debug.LogError($"❌ Erro ao inicializar BleManager: {ex.Message}");
        throw;
         }
     }

     /// <summary>
        /// Configura ou cria o BleAdapter.
        /// </summary>
     private void SetupAdapter()
    {
            if (_adapter == null)
            {
       _adapter = FindFirstObjectByType<BleAdapter>();
                
                if (_adapter == null)
{
      GameObject bleAdapter = new GameObject(nameof(BleAdapter));
     bleAdapter.transform.SetParent(Instance.transform);
            _adapter = bleAdapter.AddComponent<BleAdapter>();
               
  Debug.Log("📡 BleAdapter criado automaticamente");
    }

    // Registra callbacks
    _adapter.OnMessageReceived += OnBleMessageReceived;
        _adapter.OnErrorReceived += OnErrorReceived;
            }
        }

        /// <summary>
        /// Conecta com a biblioteca Android BLE.
    /// </summary>
        private void SetupAndroidLibrary()
      {
            #if UNITY_ANDROID && !UNITY_EDITOR
if (_bleLibrary == null)
  {
         try
         {
 AndroidJavaClass librarySingleton = new AndroidJavaClass("com.velorexe.unityandroidble.UnityAndroidBLE");
      _bleLibrary = librarySingleton.CallStatic<AndroidJavaObject>("getInstance");
  
    Debug.Log("📱 Conectado à biblioteca Android BLE");
                }
        catch (Exception ex)
   {
          Debug.LogError($"❌ Erro ao conectar biblioteca Android: {ex.Message}");
          throw;
         }
       }
            #else
        Debug.LogWarning("⚠️ BLE só funciona em dispositivos Android. Rodando em modo simulação.");
            #endif
        }

      /// <summary>
/// Finaliza todos os comandos em execução e libera recursos.
        /// Chamado automaticamente no OnDestroy.
        /// </summary>
        public void DeInitialize()
        {
        if (!_initialized)
            return;

        // Finaliza comandos paralelos
            foreach (BleCommand command in _parrallelStack)
    {
     try
      {
         command.End();
  }
    catch (Exception ex)
       {
          Debug.LogError($"Erro ao finalizar comando {command.GetType().Name}: {ex.Message}");
         }
}
   _parrallelStack.Clear();

    // Finaliza comando ativo
        if (_activeCommand != null)
     {
      _activeCommand.End();
      _activeCommand = null;
  }

         // Limpa fila
       _commandQueue.Clear();

 // Libera biblioteca Java
            _bleLibrary?.Dispose();
            _bleLibrary = null;

        // Remove adapter
   if (_adapter != null)
            {
   _adapter.OnMessageReceived -= OnBleMessageReceived;
                _adapter.OnErrorReceived -= OnErrorReceived;
  Destroy(_adapter.gameObject);
        _adapter = null;
            }

            _initialized = false;
            Debug.Log("🛑 BleManager finalizado");
        }

     #endregion

        #region Message Handling

        /// <summary>
        /// Chamado quando uma nova mensagem é recebida pelo <see cref="BleAdapter"/>.
      /// Distribui a mensagem para os comandos apropriados.
  /// </summary>
        /// <param name="obj">O <see cref="BleObject"/> recebido da biblioteca Java.</param>
      private void OnBleMessageReceived(BleObject obj)
  {
            if (LogAllMessages)
      {
          CheckForLog("📨 Mensagem BLE recebida:");
           CheckForLog(JsonUtility.ToJson(obj, true));
            }

         // Tenta processar com comando ativo
if (_activeCommand != null && _activeCommand.CommandReceived(obj))
            {
         _activeCommand.End();
           ProcessNextCommand();
   }

      // Processa comandos paralelos
          for (int i = _parrallelStack.Count - 1; i >= 0; i--)
            {
         try
        {
          if (_parrallelStack[i].CommandReceived(obj))
  {
                  if (!_parrallelStack[i].RunContiniously)
      {
 _parrallelStack[i].End();
        _parrallelStack.RemoveAt(i);
         }
         }
                }
    catch (Exception ex)
                {
         Debug.LogError($"Erro no comando paralelo {_parrallelStack[i].GetType().Name}: {ex.Message}");
        _parrallelStack.RemoveAt(i);
    }
            }
        }

        /// <summary>
        /// Processa o próximo comando na fila.
 /// </summary>
    private void ProcessNextCommand()
     {
      if (_commandQueue.Count > 0)
      {
   _activeCommand = _commandQueue.Dequeue();
     _activeTimer = 0f;
                
        try
         {
   _activeCommand?.Start();
        CheckForLog($"▶️ Executando comando: {_activeCommand?.GetType().Name}");
        }
   catch (Exception ex)
    {
      Debug.LogError($"Erro ao iniciar comando {_activeCommand?.GetType().Name}: {ex.Message}");
           _activeCommand = null;
 ProcessNextCommand(); // Tenta próximo
        }
     }
  else
{
         _activeCommand = null;
       }
        }

  #endregion

        #region Command Queue

        /// <summary>
  /// Enfileira um novo <see cref="BleCommand"/> para execução.
        /// 
        /// <para><b>Exemplo:</b></para>
        /// <code>
        /// DiscoverDevices comando = new DiscoverDevices(OnDeviceFound, 10000);
        /// BleManager.Instance.QueueCommand(comando);
        /// </code>
        /// </summary>
        /// <param name="command">O comando a ser executado.</param>
        public void QueueCommand(BleCommand command)
      {
        if (!_initialized)
    {
           Debug.LogError("❌ BleManager não está inicializado! Chame Initialize() primeiro.");
       return;
            }

            if (command == null)
         {
    Debug.LogError("❌ Tentativa de enfileirar comando nulo!");
    return;
   }

            CheckForLog($"➕ Enfileirando comando: {command.GetType().Name}");

       try
            {
    // Comandos paralelos ou contínuos vão para a stack paralela
                if (command.RunParallel || command.RunContiniously)
           {
           _parrallelStack.Add(command);
   command.Start();
              CheckForLog($"🔄 Comando paralelo iniciado: {command.GetType().Name}");
     }
      else
          {
        // Comandos normais vão para a fila
  if (_activeCommand == null)
           {
           _activeTimer = 0f;
        _activeCommand = command;
   _activeCommand.Start();
              CheckForLog($"▶️ Comando iniciado imediatamente: {command.GetType().Name}");
       }
  else
            {
          _commandQueue.Enqueue(command);
       CheckForLog($"⏳ Comando enfileirado (posição {_commandQueue.Count}): {command.GetType().Name}");
        }
        }
   }
      catch (Exception ex)
            {
         Debug.LogError($"❌ Erro ao enfileirar comando {command.GetType().Name}: {ex.Message}");
            }
}

        #endregion

        #region Error Handling & Logging

        /// <summary>
        /// Chamado quando um erro é recebido da biblioteca Android.
      /// </summary>
   private void OnErrorReceived(string errorMessage)
        {
            Debug.LogError($"❌ Erro BLE: {errorMessage}");
        }

     /// <summary>
        /// Verifica configurações de log e exibe mensagem se apropriado.
      /// </summary>
        private static void CheckForLog(string logMessage)
        {
            if (Instance == null)
   return;

            if (Instance.UseUnityLog)
         Debug.Log($"[BLE] {logMessage}");
        
      if (Instance.UseAndroidLog)
     AndroidLog(logMessage);
        }

        /// <summary>
        /// Envia log para o LogCat do Android.
  /// Útil para debug em dispositivo real.
        /// </summary>
        public static void AndroidLog(string message)
        {
   #if UNITY_ANDROID && !UNITY_EDITOR
 if (_initialized && _bleLibrary != null)
            {
                try
{
           _bleLibrary.CallStatic("androidLog", message);
      }
     catch (Exception ex)
 {
      Debug.LogError($"Erro ao enviar log para Android: {ex.Message}");
    }
       }
            #endif
        }

        #endregion

     #region Internal Commands

/// <summary>
 /// Chama um método da biblioteca Java que corresponde ao <paramref name="command"/>.
   /// Usado internamente pelos comandos BLE.
        /// </summary>
        /// <param name="command">Nome do método na biblioteca Java.</param>
        /// <param name="parameters">Parâmetros adicionais definidos pelo método Java.</param>
    internal static void SendCommand(string command, params object[] parameters)
        {
      if (!_initialized)
            {
      Debug.LogError("❌ Tentativa de enviar comando com BleManager não inicializado!");
   return;
            }

        if (Instance.LogAllMessages)
   CheckForLog($"📤 Chamando comando Java: {command}");

   #if UNITY_ANDROID && !UNITY_EDITOR
 try
            {
             _bleLibrary?.Call(command, parameters);
            }
            catch (Exception ex)
    {
  Debug.LogError($"❌ Erro ao chamar comando {command}: {ex.Message}");
        }
        #else
        Debug.LogWarning($"⚠️ Comando {command} não executado (não está em Android)");
            #endif
        }

        #endregion

    #region Helper Methods

        /// <summary>
        /// Cria um novo <see cref="GameObject"/> para o <see cref="BleManager"/>.
        /// </summary>
   private static void CreateBleManagerObject()
        {
    if (_instance != null)
            return;

          GameObject managerObject = new GameObject("BleManager");
            _instance = managerObject.AddComponent<BleManager>();
      DontDestroyOnLoad(managerObject);

            Debug.Log("🔨 BleManager GameObject criado");
        }

  #endregion

        #region Public Utility Methods

        /// <summary>
        /// Cancela todos os comandos pendentes na fila.
        /// Comandos já em execução não são afetados.
        /// </summary>
        public void ClearCommandQueue()
        {
            int count = _commandQueue.Count;
            _commandQueue.Clear();
    Debug.Log($"🗑️ {count} comando(s) removido(s) da fila");
 }

   /// <summary>
   /// Retorna o número de comandos aguardando na fila.
        /// </summary>
        public int GetQueuedCommandCount()
        {
   return _commandQueue.Count;
   }

        /// <summary>
    /// Retorna o número de comandos paralelos em execução.
        /// </summary>
        public int GetParallelCommandCount()
        {
            return _parrallelStack.Count;
        }

        /// <summary>
 /// Retorna informações sobre o estado atual do gerenciador.
        /// Útil para debug.
        /// </summary>
        public string GetStatusInfo()
        {
         return $"BleManager Status:\n" +
         $"- Inicializado: {_initialized}\n" +
     $"- Comando Ativo: {_activeCommand?.GetType().Name ?? "Nenhum"}\n" +
           $"- Fila: {_commandQueue.Count} comando(s)\n" +
       $"- Paralelos: {_parrallelStack.Count} comando(s)";
        }

        #endregion
    }
}