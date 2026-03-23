using System;
using System.Net;
using UnityEngine;

public sealed class CallbackProxy : AndroidJavaProxy
{
    private Action mOnDiscoveryStarted;
    private Action<string, string> mOnDiscoveryDeviceFound;
    private Action mOnDiscoveryFinished;

    private Action<string> mOnConnecting;
    private Action<string> mOnConnectDeviceNotFound;
    private Action<string> mOnConnected;
    private Action<string> mOnDisconnected;

    private Action<sbyte[]> mOnDataReceived;
    private Action<sbyte[]> mOnDataNotSent;

    private Action<string> mOnError;

    public CallbackProxy(Action _OnDiscoveryStarted, Action<string, string> _OnDiscoveryDeviceFound, Action _OnDiscoveryFinished,
                        Action<string> _OnConnecting, Action<string> _OnConnectDeviceNotFound, Action<string> _OnConnected, Action<string> _OnDisconnected,
                        Action<sbyte[]> _OnDataReceived, Action<sbyte[]> _OnDataNotSent,
                        Action<string> _OnError) : base("com.benstudio.blueunity.ICallBack")

    {
        mOnDiscoveryStarted = _OnDiscoveryStarted;
        mOnDiscoveryDeviceFound = _OnDiscoveryDeviceFound;
        mOnDiscoveryFinished = _OnDiscoveryFinished;

        mOnConnecting = _OnConnecting;
        mOnConnectDeviceNotFound = _OnConnectDeviceNotFound;
        mOnConnected = _OnConnected;
        mOnDisconnected = _OnDisconnected;

        mOnDataReceived = _OnDataReceived;
        mOnDataNotSent = _OnDataNotSent;

        mOnError = _OnError;

    }


    public void onDiscoveryStarted() => mOnDiscoveryStarted?.Invoke();
    public void onDiscoveryDeviceFound(string name, string address) => mOnDiscoveryDeviceFound?.Invoke(name, address);
    public void onDiscoveryFinished() => mOnDiscoveryFinished?.Invoke();

    public void onConnecting(string address) => mOnConnecting?.Invoke(address);
    public void onConnectDeviceNotFound(string address) => mOnConnectDeviceNotFound?.Invoke(address);
    public void onConnected(string address) => mOnConnected?.Invoke(address);
    public void onDisconnected(string address) => mOnDisconnected?.Invoke(address);

    public void onDataReceived(sbyte[] data) => mOnDataReceived?.Invoke(data);
    public void onDataNotSent(sbyte[] data) => mOnDataNotSent?.Invoke(data);

    public void onError(string error) => mOnError?.Invoke(error);





}
