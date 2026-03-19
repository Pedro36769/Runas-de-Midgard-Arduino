using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

namespace BlueUnity
{
    [Serializable]
    public sealed class BluetoothDeviceInfo
    {
        public string name;
        public string address;
    }

    [Serializable]
    public sealed class BluetoothDeviceList
    {
        public List<BluetoothDeviceInfo> devices;
    }

    public sealed class BluetoothHandler
    {
        public Action ScanStartedAction;
        public Action<string, string> ScanDeviceFoundAction;
        public Action ScanFinishedAction;

        public Action<string> ConnectingAction;
        public Action<string> ConnectDeviceNotFoundAction;
        public Action<string> ConnectedAction;
        public Action<string> DisconnectedAction;

        public Action<byte[]> DataReceivedAction;
        public Action<byte[]> DataNotSentAction;

        public Action<string> ErrorAction;

        private readonly static Lazy<BluetoothHandler> _instance = new(() => new BluetoothHandler());
        public AndroidJavaClass blueUnityPlugin;
        public AndroidJavaObject handlerClass;

        #region Init
        public static BluetoothHandler Instance => _instance.Value;
        public bool Enabled => handlerClass?.Call<bool>("isEnabled") ?? false;

        private BluetoothHandler()
        {
            string[] permissions = new string[]
            {
                Permission.CoarseLocation,
                Permission.FineLocation,
                "android.permission.BLUETOOTH",
                "android.permission.BLUETOOTH_ADMIN",
                "android.permission.BLUETOOTH_SCAN",
                "android.permission.BLUETOOTH_ADVERTISE",
                "android.permission.BLUETOOTH_CONNECT"
            };

            bool allGranted = true;

            foreach (var perm in permissions)
            {
                if (!Permission.HasUserAuthorizedPermission(perm))
                {
                    allGranted = false;
                    break;
                }
            }

            if (!allGranted)
            {
                Permission.RequestUserPermissions(permissions);
            }

            blueUnityPlugin = new AndroidJavaClass("com.benstudio.blueunity.BluetoothHandler");
            handlerClass = blueUnityPlugin.CallStatic<AndroidJavaObject>("getInstance");
            CallbackProxy cp = new
                (
                OnScanStarted, OnScanDviceFound, OnScanFinished,
                OnConnecting, OnConnectDeviceNotFound, OnConnected, OnDisconnected,
                OnDataReceived, OnDataNotSent,
                OnError
                );
            RegisterCallbackListener(cp);

        }

        #endregion

        #region Callback Events
        private void RegisterCallbackListener(CallbackProxy cp) => handlerClass?.Call("registerCallbackEvents", cp);

        private void OnScanStarted() => ScanStartedAction?.Invoke();

        private void OnScanDviceFound(string name, string address) => ScanDeviceFoundAction?.Invoke(name, address);

        private void OnScanFinished() => ScanFinishedAction?.Invoke();

        private void OnConnecting(string address) => ConnectingAction?.Invoke(address);

        private void OnConnectDeviceNotFound(string address) => ConnectDeviceNotFoundAction?.Invoke(address);

        private void OnConnected(string address) => ConnectedAction?.Invoke(address);

        private void OnDisconnected(string address) => DisconnectedAction?.Invoke(address);

        private void OnDataReceived(sbyte[] data) => DataReceivedAction?.Invoke(Array.ConvertAll(data, b => (byte)b));

        private void OnDataNotSent(sbyte[] data) => DataNotSentAction?.Invoke(Array.ConvertAll(data, b => (byte)b));

        private void OnError(string error) => ErrorAction?.Invoke(error);

        #endregion

        #region Public Methods

        public void SetDeviceName(string name) => handlerClass?.Call("setDeviceName", name);

        public void StartScan() => handlerClass?.Call("scanForDevices");

        public void StopScan() => handlerClass?.Call("stopScanForDevices");

        public BluetoothDeviceInfo[] PairedDevices
        {
            get
            {
                string devicesJson = handlerClass?.Call<string>("getPairedDevices");

                if (string.IsNullOrEmpty(devicesJson))
                    return new BluetoothDeviceInfo[0];

                var deviceList = JsonUtility.FromJson<BluetoothDeviceList>(devicesJson);
                return deviceList.devices.ToArray();
            }
        }

        public void ConnectAsClient(string address) => handlerClass?.Call("connectAsClient", address);

        public void SetPairing(bool isOn) => handlerClass?.Call("SetPairing", isOn);

        public void StartDiscoverable(int duration) => handlerClass?.Call("startDiscoverable", duration);

        public void StartServer() => handlerClass?.Call("startServer");

        public void CloseServer() => handlerClass?.Call("closeServer");

        public void Disconnect() => handlerClass?.Call("disconnect");

        public void Write(byte[] data)
        {
            sbyte[] sbytes = Array.ConvertAll(data, b => unchecked((sbyte)b));
            handlerClass?.Call("write", sbytes);
        }

        public void Cleanup()
        {
            ScanStartedAction = null;
            ScanDeviceFoundAction = null;
            ScanFinishedAction = null;

            ConnectingAction = null;
            ConnectDeviceNotFoundAction = null;
            ConnectedAction = null;
            DisconnectedAction = null;

            DataReceivedAction = null;
            DataNotSentAction = null;

            ErrorAction = null;

            Disconnect();
            handlerClass?.Call("cleanup");
        }
        #endregion
    }
}

