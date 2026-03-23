using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BluetoothDevice : MonoBehaviour
{
    [SerializeField] private TMP_Text m_Text;
    [SerializeField] private Button button;
    public void Setup(string name, string address, UnityAction<string> connectAction)
    {
        m_Text.text = $"{name}, {address}";
        button.onClick.AddListener(() => connectAction(address));
    }

    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
}
