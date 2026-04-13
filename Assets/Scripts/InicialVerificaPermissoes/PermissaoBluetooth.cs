using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;

public class PermissaoBluetooth : MonoBehaviour
{
    [Header("Cenas")]
    public string cenaBluetooth = "CenaBluetooth";
    public string cenaPermissaoNegada = "CenaPermissaoNegada";

    private const string BluetoothScan = "android.permission.BLUETOOTH_SCAN";
    private const string BluetoothConnect = "android.permission.BLUETOOTH_CONNECT";

    private bool cenaJaFoiTrocada = false;
    private int permissoesConcedidas = 0;

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (TemTodasPermissoes())
        {
            IrParaCenaBluetooth();
            return;
        }

        SolicitarPermissoes();
#else
        IrParaCenaBluetooth();
#endif
    }

    private void SolicitarPermissoes()
    {
        PermissionCallbacks callbacks = new PermissionCallbacks();

        callbacks.PermissionGranted += OnPermissionGranted;
        callbacks.PermissionDenied += OnPermissionDenied;

        Permission.RequestUserPermissions(
            new string[] { BluetoothScan, BluetoothConnect },
            callbacks
        );
    }

    private bool TemTodasPermissoes()
    {
        return Permission.HasUserAuthorizedPermission(BluetoothScan) &&
               Permission.HasUserAuthorizedPermission(BluetoothConnect);
    }

    private void OnPermissionGranted(string permissionName)
    {
        if (cenaJaFoiTrocada)
            return;

        permissoesConcedidas++;

        if (TemTodasPermissoes())
        {
            IrParaCenaBluetooth();
        }
    }

    private void OnPermissionDenied(string permissionName)
    {
        if (cenaJaFoiTrocada)
            return;

        // Check if we should show rationale (user denied but can be asked again)
        // If ShouldShowRequestPermissionRationale returns false, it means "Don't Ask Again" was selected
        bool shouldShowRationale = Permission.ShouldShowRequestPermissionRationale(permissionName);

        // Navigate to denied scene regardless of whether it's permanent or temporary denial
        IrParaCenaPermissaoNegada();
    }

    private void IrParaCenaBluetooth()
    {
        if (cenaJaFoiTrocada)
            return;

        cenaJaFoiTrocada = true;
        SceneManager.LoadScene(cenaBluetooth);
    }

    private void IrParaCenaPermissaoNegada()
    {
        if (cenaJaFoiTrocada)
            return;

        cenaJaFoiTrocada = true;
        SceneManager.LoadScene(cenaPermissaoNegada);
    }
}