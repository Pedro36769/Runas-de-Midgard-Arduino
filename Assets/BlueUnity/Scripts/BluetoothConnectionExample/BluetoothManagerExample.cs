using BlueUnity;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BluetoothManagerExample : MonoBehaviour
{
    [SerializeField] private Transform devicesContainer;
    [SerializeField] private Transform receivedDataContainer;
    [SerializeField] private BluetoothDevice bluetoothDevice;
    [SerializeField] private TMP_Text receivedData;
    [SerializeField] private TMP_InputField data2Send;
    [SerializeField] private TMP_Text status;
    [SerializeField] private Toggle pairingToggle;
    [SerializeField] private int discoverableDuration = 120;
    private BluetoothHandler bluetooth;
    private bool isNewDevice = false;
    private string deviceName = string.Empty;
    private string deviceAddress = string.Empty;
    private bool isNewData = false;
    private string receivedDataString = string.Empty;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        bluetooth = BluetoothHandler.Instance;
        status.text = "BlueUnity waiting for enable bluetooth";
        yield return new WaitUntil(() => bluetooth.Enabled);

        bluetooth.ScanStartedAction += ScanStarted;
        bluetooth.ScanDeviceFoundAction += DeviceFound;
        bluetooth.ScanFinishedAction += ScanFinished;

        bluetooth.ConnectingAction += Connecting;
        bluetooth.ConnectedAction += Connected;
        bluetooth.DisconnectedAction += Disconnected;
        bluetooth.DataReceivedAction += DataReceived;
        bluetooth.ErrorAction += Error;

        bluetooth.SetDeviceName("BlueUnity");

        status.text = "BlueUnity Init OK";

        pairingToggle.onValueChanged.AddListener(SetPairing);

    }

    private void Update()
    {
        if (isNewDevice)
        {
            isNewDevice = false;
            Instantiate(bluetoothDevice, devicesContainer).Setup(deviceName, deviceAddress, ConnectClient);
        }

        if (isNewData)
        {
            isNewData = false;
            Instantiate(receivedData, receivedDataContainer).text = receivedDataString;
        }
    }

    private void ScanStarted()
    {
        Debug.Log("Scan Started");
        status.text = "Scan Started";
    }

    private void DeviceFound(string name, string address)
    {
        Debug.Log($"Device Found {deviceName} {deviceAddress}");
        deviceName = name;
        deviceAddress = address;
        isNewDevice = true;
    }

    private void ScanFinished()
    {
        Debug.Log("Scan Finished");
        status.text = "Scan Finished";
    }

    private void Connecting(string address)
    {
        Debug.Log($"Connecting {address}");
        status.text = $"Connecting with {address}";
    }

    private void Connected(string address)
    {
        Debug.Log($"Connected {address}");
        status.text = $"Connected with {address}";
    }

    private void Disconnected(string address)
    {
        Debug.Log($"Disconnected {address}");
        status.text = $"Disconnected with {address}";
    }

    private void DataReceived(byte[] data)
    {
        Debug.Log($"DataReceived: {Encoding.UTF8.GetString(data)}");
        receivedDataString = Encoding.UTF8.GetString(data);
        isNewData = true;
    }

    private void Error(string error)
    {
        Debug.Log($"Error: {error}");
        status.text = $"Error: {error}";
    }

    public void StartScanDevices()
    {
        foreach (RectTransform child in devicesContainer)
            Destroy(child.gameObject);

        bluetooth.StartScan();

        status.text = "Scanning Devices";
    }

    public void SetPairing(bool isOn)
    {
        bluetooth.SetPairing(isOn);
    }

    public void StopScanDevices()
    {
        bluetooth.StopScan();
        status.text = "Stop Scanning Devices";
    }

    public void GetPairedDevices()
    {
        BluetoothDeviceInfo[] devices = bluetooth.PairedDevices;

        foreach (RectTransform child in devicesContainer)
            Destroy(child.gameObject);

        foreach (BluetoothDeviceInfo deviceInfo in devices)
        {
            Debug.Log($"Paired device: {deviceInfo.name}  {deviceInfo.address}");
            Instantiate(bluetoothDevice, devicesContainer).Setup(deviceInfo.name, deviceInfo.address, ConnectClient);
        }
    }

    public void ConnectClient(string address)
    {
        if(string.IsNullOrEmpty(address))
            return;

        Disconnect();
        bluetooth.ConnectAsClient(address);
    }

    public void StartDiscoverable()
    {
        Disconnect();
        bluetooth.StartDiscoverable(discoverableDuration);
        StartServer();
    }

    public void StartServer()
    {
        bluetooth.StartServer();
        status.text = $"Start Server";
    }

    public void Disconnect()
    {
        bluetooth.Disconnect();
    }

    public void DeleteReceivedData()
    {
        foreach (RectTransform child in receivedDataContainer)
            Destroy(child.gameObject);
    }

    public void SendData()
    {
        bluetooth.Write(Encoding.UTF8.GetBytes(data2Send.text));
    }

    private void OnDestroy()
    {
        pairingToggle.onValueChanged.RemoveAllListeners();
        bluetooth.Cleanup();
    }


}
