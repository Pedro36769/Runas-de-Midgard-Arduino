using Android.BLE;
using Android.BLE.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gerencia a comunicação bidirecional com o dispositivo BLE.
/// 
/// <para><b>Responsabilidades:</b></para>
/// <list type="bullet">
/// <item>Inscrever para receber notificações do dispositivo</item>
/// <item>Enviar comandos para o dispositivo</item>
/// <item>Processar dados recebidos</item>
/// <item>Notificar outros scripts sobre dados recebidos</item>
/// </list>
/// 
/// <para><b>?? Importante:</b></para>
/// <list type="bullet">
/// <item>BLE suporta no máximo 20 bytes por transmissão</item>
/// <item>Use delimitadores claros (ex: ';', '\n') para separar dados</item>
/// <item>Este GameObject deve persistir entre cenas (DontDestroyOnLoad)</item>
/// </list>
/// 
/// <para><b>Formato de dados padrão:</b></para>
/// <code>
/// Arduino/ESP32 envia: "25.5;60.2\n" (temperatura;umidade)
/// Unity recebe array: ["25.5", "60.2"]
/// </code>
/// </summary>
public class GerenciarComunicacao : MonoBehaviour
{
    #region Inspector Fields

    [Header("Configuração BLE")]
    [SerializeField]
    [Tooltip("UUID do serviço BLE (padrão: ffe0)")]
    private string _servico = "ffe0";

    [SerializeField]
    [Tooltip("UUID da característica BLE (padrão: ffe1)")]
    private string _caracteristica = "ffe1";

    [Header("Configuração de Dados")]
    [SerializeField]
    [Tooltip("Caractere usado para separar dados recebidos")]
    private char _separadorDados = ';';

    [SerializeField]
    [Tooltip("Adicionar \\n no final ao enviar dados")]
    private bool _adicionarNovaLinha = true;

    [Header("Debug")]
    [SerializeField]
    [Tooltip("Mostra logs detalhados de comunicação")]
    private bool _modoDebug = true;

    #endregion

    #region Private Fields

    // Comando de subscrição ativo
    public SubscribeToCharacteristic sb;

    // UUID do dispositivo conectado
    private string _deviceUuid = string.Empty;

    // Callback para receber dados processados
    private Action<String[]> Recebedor;

  // Buffer para dados fragmentados (BLE envia em pacotes pequenos)
    private StringBuilder _bufferRecepcao = new StringBuilder();

    // Estatísticas
    private int _totalMensagensRecebidas = 0;
    private int _totalMensagensEnviadas = 0;
    private DateTime _ultimaMensagemRecebida;
    private DateTime _ultimaMensagemEnviada;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        // Impede que este GameObject seja destruído ao trocar de cena
        DontDestroyOnLoad(gameObject);

        // Impede que a tela desligue durante uso BLE
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Valida configuração
        ValidarConfiguracao();

        LogDebug("? GerenciarComunicacao iniciado");
    }

    private void OnDestroy()
    {
        // Desinscreve do serviço BLE ao destruir
        if (sb != null)
        {
     try
        {
    sb.End();
    LogDebug("?? Desinscrito do serviço BLE");
            }
         catch (Exception ex)
     {
        Debug.LogError($"? Erro ao desinscrever: {ex.Message}");
     }
  }

        // Restaura configuração de tela
    Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }

    #endregion

    #region Configuration

    /// <summary>
    /// Valida a configuração do componente.
    /// </summary>
    private void ValidarConfiguracao()
  {
        if (string.IsNullOrEmpty(_servico))
        {
       Debug.LogError("? UUID do serviço não configurado!");
        }

        if (string.IsNullOrEmpty(_caracteristica))
   {
            Debug.LogError("? UUID da característica não configurado!");
        }

  LogDebug($"?? Configuração BLE:\n" +
            $"   Serviço: {_servico}\n" +
       $"   Característica: {_caracteristica}\n" +
    $"   Separador: '{_separadorDados}'");
    }

    #endregion

    #region BLE Subscription

    /// <summary>
    /// Inscreve para receber notificações de um serviço BLE específico.
    /// 
    /// <para><b>?? Importante:</b></para>
    /// <list type="bullet">
    /// <item>Chame este método apenas após conexão bem-sucedida</item>
    /// <item>O dispositivo BLE deve suportar notificações na característica</item>
    /// <item>Cada dado recebido dispara o callback <see cref="Receber"/></item>
    /// </list>
    /// </summary>
    /// <param name="_dvcUuid">UUID do dispositivo BLE conectado</param>
    public void SubscribeServico(string _dvcUuid)
    {
        // Validações
        if (string.IsNullOrEmpty(_dvcUuid))
 {
            Debug.LogError("? UUID do dispositivo vazio! Não é possível inscrever.");
       return;
        }

        if (sb != null)
        {
     Debug.LogWarning("?? Já está inscrito! Cancelando inscrição anterior.");
            sb.End();
            sb = null;
        }

        // Guarda UUID do dispositivo
        _deviceUuid = _dvcUuid;

        try
        {
   LogDebug($"?? Inscrevendo no serviço BLE...\n" +
         $"   Dispositivo: {_deviceUuid}\n" +
                     $"   Serviço: {_servico}\n" +
  $"   Característica: {_caracteristica}");

 // Cria comando de subscrição
   sb = new SubscribeToCharacteristic(
          _deviceUuid,
         _servico,
            _caracteristica,
    (byte[] value) => { Receber(value); }
    );

     // Enfileira e inicia comando
    BleManager.Instance.QueueCommand(sb);
        sb.Start();

  LogDebug("? Inscrição realizada com sucesso!");
   }
        catch (Exception ex)
        {
    Debug.LogError($"? Erro ao inscrever no serviço BLE: {ex.Message}");
     sb = null;
        }
    }

    /// <summary>
    /// Cancela a inscrição do serviço BLE.
    /// </summary>
public void UnsubscribeServico()
    {
        if (sb == null)
        {
       Debug.LogWarning("?? Não está inscrito em nenhum serviço!");
     return;
        }

        try
        {
        sb.End();
     sb = null;
            LogDebug("?? Desinscrito do serviço BLE");
        }
        catch (Exception ex)
        {
       Debug.LogError($"? Erro ao desinscrever: {ex.Message}");
 }
    }

    #endregion

    #region Send Data

/// <summary>
  /// Envia dados para o dispositivo BLE.
    /// 
  /// <para><b>?? Limitações do BLE:</b></para>
    /// <list type="bullet">
 /// <item>Máximo de 20 bytes por transmissão (incluindo '\n')</item>
    /// <item>Se exceder, os dados serão truncados</item>
    /// <item>Use <see cref="EnviarDadosGrandes"/> para dados maiores</item>
    /// </list>
    /// 
    /// <para><b>Exemplo:</b></para>
    /// <code>
    /// Enviar("LED:ON");    // 7 bytes ?
    /// Enviar("TEMP:25.5"); // 10 bytes ?
    /// </code>
    /// </summary>
    /// <param name="value">String a ser enviada</param>
    public void Enviar(string value)
    {
        // Validações
        if (string.IsNullOrEmpty(_deviceUuid))
 {
         Debug.LogError("? Dispositivo não conectado! Conecte-se primeiro.");
       return;
        }

        if (string.IsNullOrEmpty(value))
        {
            Debug.LogWarning("?? Tentativa de enviar string vazia!");
    return;
        }

        try
        {
// Adiciona nova linha se configurado
            string dadosParaEnviar = _adicionarNovaLinha ? value + '\n' : value;

    // Converte para bytes ASCII
 byte[] msg = Encoding.ASCII.GetBytes(dadosParaEnviar);

      // Verifica tamanho (BLE permite máx. 20 bytes)
  if (msg.Length > 20)
  {
           Debug.LogWarning($"?? AVISO: Dados excedem 20 bytes ({msg.Length} bytes)!\n" +
      $"   Dados podem ser truncados ou perdidos.\n" +
             $"   Use EnviarDadosGrandes() para mensagens longas.");
       }

            LogDebug($"?? Enviando: '{value}' ({msg.Length} bytes)");

    // Cria e executa comando de escrita
            WriteToCharacteristic w = new WriteToCharacteristic(
         _deviceUuid,
                _servico,
     _caracteristica,
  msg
            );

       w.Start();

       // Atualiza estatísticas
    _totalMensagensEnviadas++;
   _ultimaMensagemEnviada = DateTime.Now;

            LogDebug($"? Enviado com sucesso! (Total: {_totalMensagensEnviadas})");
    }
        catch (Exception ex)
        {
   Debug.LogError($"? Erro ao enviar dados: {ex.Message}");
        }
    }

    /// <summary>
    /// Envia dados grandes fragmentando em pacotes de 20 bytes.
    /// Use para mensagens longas que não cabem em um único pacote BLE.
    /// </summary>
    /// <param name="value">String longa a ser enviada</param>
    /// <param name="intervalo">Intervalo entre pacotes (segundos)</param>
    public void EnviarDadosGrandes(string value, float intervalo = 0.1f)
    {
        if (string.IsNullOrEmpty(value))
        {
            Debug.LogWarning("?? Tentativa de enviar string vazia!");
    return;
    }

   StartCoroutine(EnviarFragmentado(value, intervalo));
    }

    /// <summary>
    /// Coroutine que envia dados fragmentados.
    /// </summary>
    private IEnumerator EnviarFragmentado(string mensagem, float intervalo)
    {
    const int tamanhoMaximo = 19; // 20 - 1 para \n

        LogDebug($"?? Enviando dados fragmentados: {mensagem.Length} caracteres em pacotes de {tamanhoMaximo}");

        for (int i = 0; i < mensagem.Length; i += tamanhoMaximo)
        {
         int tamanho = Mathf.Min(tamanhoMaximo, mensagem.Length - i);
            string fragmento = mensagem.Substring(i, tamanho);

            Enviar(fragmento);

    // Aguarda entre envios
      yield return new WaitForSeconds(intervalo);
        }

        LogDebug("? Todos os fragmentos enviados!");
 }

    #endregion

    #region Receive Data

  /// <summary>
    /// Processa dados recebidos do dispositivo BLE.
  /// Converte bytes em string e separa por delimitador.
    /// </summary>
    /// <param name="value">Array de bytes recebido</param>
    private void Receber(byte[] value)
    {
        if (value == null || value.Length == 0)
     {
        Debug.LogWarning("?? Dados recebidos vazios ou nulos!");
            return;
        }

   try
        {
         // Converte bytes para string ASCII
            string dadosRecebidos = Encoding.ASCII.GetString(value);

  // Atualiza estatísticas
 _totalMensagensRecebidas++;
            _ultimaMensagemRecebida = DateTime.Now;

            LogDebug($"?? Recebido: '{dadosRecebidos}' ({value.Length} bytes, Total: {_totalMensagensRecebidas})");

            // Adiciona ao buffer (caso dados venham fragmentados)
  _bufferRecepcao.Append(dadosRecebidos);

            // Verifica se tem mensagem completa (termina com \n ou sem \n)
            string buffer = _bufferRecepcao.ToString();

            // Se termina com \n, processa e limpa buffer
     if (buffer.EndsWith("\n") || buffer.EndsWith("\r\n"))
{
          buffer = buffer.TrimEnd('\r', '\n');
            ProcessarMensagem(buffer);
                _bufferRecepcao.Clear();
          }
    // Se não tem \n mas passou certo tempo, processa assim mesmo
            // (alguns dispositivos não enviam \n)
       else if (buffer.Length > 0)
     {
                // Processa após pequeno delay para garantir que recebeu tudo
         StartCoroutine(ProcessarBufferComDelay(0.1f));
}
     }
        catch (Exception ex)
        {
    Debug.LogError($"? Erro ao processar dados recebidos: {ex.Message}");
        }
    }

    /// <summary>
    /// Aguarda um pouco e processa buffer (para dados sem \n).
    /// </summary>
    private IEnumerator ProcessarBufferComDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

  string buffer = _bufferRecepcao.ToString();
        if (!string.IsNullOrEmpty(buffer))
        {
    ProcessarMensagem(buffer);
    _bufferRecepcao.Clear();
        }
    }

    /// <summary>
    /// Processa mensagem completa e notifica observers.
    /// </summary>
private void ProcessarMensagem(string mensagem)
{
  // Separa dados pelo delimitador configurado
        string[] dados = mensagem.Split(_separadorDados);

        LogDebug($"?? Processando mensagem: {dados.Length} campo(s) encontrado(s)");
        for (int i = 0; i < dados.Length; i++)
        {
         LogDebug($"   [{i}] = '{dados[i]}'");
        }

     // Notifica callback registrado
        Recebedor?.Invoke(dados);
    }

    #endregion

    #region Observer Pattern

    /// <summary>
    /// Registra uma função callback para receber dados processados.
    /// 
    /// <para><b>Exemplo de uso:</b></para>
    /// <code>
    /// void Start()
    /// {
    ///     GerenciarComunicacao gc = FindObjectOfType&lt;GerenciarComunicacao&gt;();
    ///     gc.RegistraRecebedor(MeuCallbackDeDados);
    /// }
    /// 
    /// void MeuCallbackDeDados(string[] dados)
    /// {
    ///     float temperatura = float.Parse(dados[0]);
    ///     float umidade = float.Parse(dados[1]);
    ///     Debug.Log($"Temp: {temperatura}°C, Umid: {umidade}%");
    /// }
  /// </code>
    /// </summary>
 /// <param name="funcao">Função que receberá os dados como array de strings</param>
    public void RegistraRecebedor(Action<String[]> funcao)
    {
  if (funcao == null)
        {
          Debug.LogWarning("?? Tentativa de registrar callback nulo!");
       return;
        }

        Recebedor = funcao;
        LogDebug($"? Callback de recepção registrado: {funcao.Method.Name}");
    }

    /// <summary>
    /// Remove o callback de recepção.
 /// </summary>
    public void DesregistraRecebedor()
    {
      Recebedor = null;
        LogDebug("?? Callback de recepção removido");
    }

    #endregion

    #region Statistics & Debug

    /// <summary>
    /// Retorna estatísticas de comunicação.
    /// </summary>
    public string ObterEstatisticas()
    {
        return $"?? Estatísticas de Comunicação:\n" +
          $"   Mensagens Recebidas: {_totalMensagensRecebidas}\n" +
               $"   Mensagens Enviadas: {_totalMensagensEnviadas}\n" +
               $"   Última Recebida: {_ultimaMensagemRecebida:HH:mm:ss}\n" +
  $"   Última Enviada: {_ultimaMensagemEnviada:HH:mm:ss}\n" +
$"   Dispositivo: {(_deviceUuid ?? "Nenhum")}";
  }

    /// <summary>
    /// Reseta estatísticas.
    /// </summary>
 public void ResetarEstatisticas()
 {
     _totalMensagensRecebidas = 0;
        _totalMensagensEnviadas = 0;
        LogDebug("?? Estatísticas resetadas");
    }

    /// <summary>
    /// Loga mensagem se modo debug estiver ativo.
    /// </summary>
    private void LogDebug(string mensagem)
    {
    if (_modoDebug)
        {
         Debug.Log($"[GerenciarComunicacao] {mensagem}");
        }
    }

    #endregion

    #region Public Getters

    /// <summary>
    /// Retorna true se está inscrito em um serviço BLE.
    /// </summary>
    public bool EstaInscrito => sb != null;

    /// <summary>
    /// Retorna true se tem dispositivo conectado.
  /// </summary>
 public bool TemDispositivoConectado => !string.IsNullOrEmpty(_deviceUuid);

    /// <summary>
    /// Retorna UUID do dispositivo conectado.
    /// </summary>
    public string DispositivoUuid => _deviceUuid;

    #endregion
}
