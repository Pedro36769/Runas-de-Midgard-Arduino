using Android.BLE.Commands;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android; // 1. Namespace adicionado para permissões

namespace Android.BLE
{
    public class BleManager : MonoBehaviour
    {
        #region Singleton Pattern
   
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
        public static bool IsInitialized { get => _initialized; }
        private static bool _initialized = false;
        #endregion

        #region Inspector Fields
        [SerializeField] private BleAdapter _adapter;
        public bool InitializeOnAwake = true;

        [Header("Configurações de Log")]
        public bool LogAllMessages = false;
        public bool UseUnityLog = true;
        public bool UseAndroidLog = false;
        #endregion

        #region Internal Fields
        internal static AndroidJavaObject _bleLibrary = null;
        private readonly Queue<BleCommand> _commandQueue = new Queue<BleCommand>();
        private readonly List<BleCommand> _parrallelStack = new List<BleCommand>();
        private static BleCommand _activeCommand = null;
        private static float _activeTimer = 0f;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (InitializeOnAwake)
                Initialize();

            if (_adapter != null)
            {
                _adapter.OnMessageReceived += OnBleMessageReceived;
                _adapter.OnErrorReceived += OnErrorReceived;
            }
        }

        private void Update()
        {
            if (_activeCommand != null)
            {
                _activeTimer += Time.deltaTime;
                if (_activeTimer > _activeCommand.Timeout)
                {
                    CheckForLog($"⏱️ Timeout: {_activeCommand.GetType().Name} ({_activeCommand.Timeout}s)");
                    _activeTimer = 0f;
                    _activeCommand.EndOnTimeout();
                    ProcessNextCommand();
                }
            }
        }

        private void OnDestroy()
        {
            DeInitialize();
        }
        #endregion

        #region Initialization

        public void Initialize()
        {
            if (_initialized)
            {
                Debug.LogWarning("BleManager já está inicializado!");
                return;
            }

            try
            {
                if (_instance == null)
                    CreateBleManagerObject();

                // 2. Solicita permissões antes de configurar a biblioteca
                RequestPermissions();

                SetupAdapter();
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
        /// Solicita permissões de Bluetooth necessárias para Android 12 (API 31) ou superior.
        /// </summary>
        private void RequestPermissions()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            // Lista de permissões críticas para Android 12+
            string[] permissions = { 
                "android.permission.BLUETOOTH_SCAN", 
                "android.permission.BLUETOOTH_CONNECT",
                "android.permission.ACCESS_FINE_LOCATION" 
            };

            foreach (string permission in permissions)
            {
                if (!Permission.HasUserAuthorizedPermission(permission))
                {
                    CheckForLog($"Prompt: Solicitando permissão {permission}");
                    Permission.RequestUserPermission(permission);
                }
            }
            #endif
        }

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

                _adapter.OnMessageReceived += OnBleMessageReceived;
                _adapter.OnErrorReceived += OnErrorReceived;
            }
        }

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

        public void DeInitialize()
        {
            if (!_initialized) return;

            foreach (BleCommand command in _parrallelStack)
            {
                try { command.End(); }
                catch (Exception ex) { Debug.LogError($"Erro ao finalizar comando {command.GetType().Name}: {ex.Message}"); }
            }
            _parrallelStack.Clear();

            if (_activeCommand != null)
            {
                _activeCommand.End();
                _activeCommand = null;
            }

            _commandQueue.Clear();
            _bleLibrary?.Dispose();
            _bleLibrary = null;

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

        #region Restante do Código (Message Handling, Queue, etc)
        // [Mantido igual ao seu código original para brevidade]
        #endregion

        private void OnBleMessageReceived(BleObject obj)
        {
            if (LogAllMessages)
            {
                CheckForLog("📨 Mensagem BLE recebida:");
                CheckForLog(JsonUtility.ToJson(obj, true));
            }

            if (_activeCommand != null && _activeCommand.CommandReceived(obj))
            {
                _activeCommand.End();
                ProcessNextCommand();
            }

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
                    ProcessNextCommand();
                }
            }
            else
            {
                _activeCommand = null;
            }
        }

        public void QueueCommand(BleCommand command)
        {
            if (!_initialized)
            {
                Debug.LogError("❌ BleManager não está inicializado! Chame Initialize() primeiro.");
                return;
            }

            if (command == null) return;

            CheckForLog($"➕ Enfileirando comando: {command.GetType().Name}");

            try
            {
                if (command.RunParallel || command.RunContiniously)
                {
                    _parrallelStack.Add(command);
                    command.Start();
                }
                else
                {
                    if (_activeCommand == null)
                    {
                        _activeTimer = 0f;
                        _activeCommand = command;
                        _activeCommand.Start();
                    }
                    else
                    {
                        _commandQueue.Enqueue(command);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Erro ao enfileirar comando {command.GetType().Name}: {ex.Message}");
            }
        }

        private void OnErrorReceived(string errorMessage)
        {
            Debug.LogError($"❌ Erro BLE: {errorMessage}");
        }

        private static void CheckForLog(string logMessage)
        {
            if (Instance == null) return;
            if (Instance.UseUnityLog) Debug.Log($"[BLE] {logMessage}");
            if (Instance.UseAndroidLog) AndroidLog(logMessage);
        }

        public static void AndroidLog(string message)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            if (_initialized && _bleLibrary != null)
            {
                try { _bleLibrary.CallStatic("androidLog", message); }
                catch (Exception ex) { Debug.LogError($"Erro ao enviar log para Android: {ex.Message}"); }
            }
            #endif
        }

        internal static void SendCommand(string command, params object[] parameters)
        {
            if (!_initialized) return;
            #if UNITY_ANDROID && !UNITY_EDITOR
            try { _bleLibrary?.Call(command, parameters); }
            catch (Exception ex) { Debug.LogError($"❌ Erro ao chamar comando {command}: {ex.Message}"); }
            #endif
        }

        private static void CreateBleManagerObject()
        {
            if (_instance != null) return;
            GameObject managerObject = new GameObject("BleManager");
            _instance = managerObject.AddComponent<BleManager>();
            DontDestroyOnLoad(managerObject);
        }

        public void ClearCommandQueue() => _commandQueue.Clear();
        public int GetQueuedCommandCount() => _commandQueue.Count;
        public int GetParallelCommandCount() => _parrallelStack.Count;
        public string GetStatusInfo() => $"Status: {_initialized} | Ativo: {_activeCommand?.GetType().Name} | Fila: {_commandQueue.Count}";
    }
}