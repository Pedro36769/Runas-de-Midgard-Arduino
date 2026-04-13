
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// Exemplo prático de uso do novo Input System para Android/Mobile
/// INSTRUÇÕES:
/// 1. Abra o Unity (ele vai importar o pacote e pedir para reiniciar)
/// 2. Descomente este código (remova o bloco de comentário)
/// 3. Adicione este script a um GameObject na sua cena
/// 4. Teste no Editor (mouse simula touch) ou no Android
/// </summary>
public class ExemploNovoInputSystem : MonoBehaviour
{
 [Header("Configurações")]
  [SerializeField] private bool mostrarLogs = true;
    [SerializeField] private bool habilitarSimulacaoMouse = true;

    private void OnEnable()
    {
        // OBRIGATÓRIO: Habilitar suporte aprimorado para touch
        EnhancedTouchSupport.Enable();
        
        // OPCIONAL: Simular touch com mouse no Editor
  if (habilitarSimulacaoMouse)
        {
            TouchSimulation.Enable();
        }
        
     if (mostrarLogs)
        Debug.Log("[Novo Input System] Touch e simulação habilitados!");
    }

    private void OnDisable()
    {
   EnhancedTouchSupport.Disable();
        
        if (habilitarSimulacaoMouse)
        {
            TouchSimulation.Disable();
        }
    }

    private void Update()
    {
      // ========================================
   // MÉTODO 1: Enhanced Touch (Recomendado para Mobile)
        // ========================================
 DetectarTouchesEnhanced();
 
        // ========================================
        // MÉTODO 2: Mouse (Funciona em PC e Mobile)
 // ========================================
        // DetectarComMouse();
        
        // ========================================
  // MÉTODO 3: Touchscreen Direto
  // ========================================
        // DetectarTouchscreen();
        
        // ========================================
        // MÉTODO 4: Pointer Universal
        // ========================================
        // DetectarComPointer();
    }

    // ========================================
    // MÉTODO 1: Enhanced Touch
    // ========================================
    private void DetectarTouchesEnhanced()
    {
        if (Touch.activeTouches.Count == 0)
       return;

        foreach (Touch touch in Touch.activeTouches)
        {
  switch (touch.phase)
   {
           case UnityEngine.InputSystem.TouchPhase.Began:
              OnTouchBegan(touch);
        break;
    
        case UnityEngine.InputSystem.TouchPhase.Moved:
         OnTouchMoved(touch);
            break;
           
    case UnityEngine.InputSystem.TouchPhase.Ended:
       OnTouchEnded(touch);
      break;
     
     case UnityEngine.InputSystem.TouchPhase.Canceled:
    OnTouchCanceled(touch);
                break;
       }
        }
    }

    private void OnTouchBegan(Touch touch)
    {
        if (mostrarLogs)
        {
         Debug.Log($"[Touch Began] ID: {touch.touchId}, Posição: {touch.screenPosition}");
        }
        
        // EXEMPLO: Detectar se tocou em UI ou objeto 3D
        DetectarObjetoTocado(touch.screenPosition);
    }

    private void OnTouchMoved(Touch touch)
    {
        if (mostrarLogs)
    {
    Debug.Log($"[Touch Moved] ID: {touch.touchId}, Delta: {touch.delta}");
        }
        
  // Útil para gestos de arraste
    }

    private void OnTouchEnded(Touch touch)
  {
        if (mostrarLogs)
        {
            Debug.Log($"[Touch Ended] ID: {touch.touchId}, Posição Final: {touch.screenPosition}");
        }
  }

    private void OnTouchCanceled(Touch touch)
    {
        if (mostrarLogs)
 {
            Debug.Log($"[Touch Canceled] ID: {touch.touchId}");
        }
    }

    // ========================================
    // MÉTODO 2: Usando Mouse (Simples)
    // ========================================
    private void DetectarComMouse()
    {
        if (Mouse.current == null)
   return;

     // Detectar clique/toque
  if (Mouse.current.leftButton.wasPressedThisFrame)
        {
       Vector2 posicao = Mouse.current.position.ReadValue();
if (mostrarLogs)
      Debug.Log($"[Mouse] Clique/Touch detectado em: {posicao}");
   
            DetectarObjetoTocado(posicao);
    }

        // Detectar arraste
        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
  if (delta.magnitude > 0.1f && mostrarLogs)
    {
        Debug.Log($"[Mouse] Arrastando, Delta: {delta}");
         }
        }

        // Detectar soltar
     if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
         if (mostrarLogs)
    Debug.Log("[Mouse] Botão solto");
        }
  }

    // ========================================
    // MÉTODO 3: Touchscreen Direto
    // ========================================
    private void DetectarTouchscreen()
    {
        if (Touchscreen.current == null)
        return;

        // Touch primário
   if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
     {
 Vector2 posicao = Touchscreen.current.primaryTouch.position.ReadValue();
            if (mostrarLogs)
          Debug.Log($"[Touchscreen] Touch em: {posicao}");
            
            DetectarObjetoTocado(posicao);
        }
    }

    // ========================================
    // MÉTODO 4: Pointer Universal
    // ========================================
  private void DetectarComPointer()
    {
   if (Pointer.current == null)
    return;

     if (Pointer.current.press.wasPressedThisFrame)
        {
   Vector2 posicao = Pointer.current.position.ReadValue();
         if (mostrarLogs)
  Debug.Log($"[Pointer] Pressionado em: {posicao}");
          
       DetectarObjetoTocado(posicao);
        }
    }

    // ========================================
    // HELPER: Detectar objeto tocado
    // ========================================
    private void DetectarObjetoTocado(Vector2 posicaoTela)
    {
        // Para UI
        // if (EventSystem.current.IsPointerOverGameObject())
        // {
        //     Debug.Log("Tocou em UI");
        //   return;
        // }

        // Para objetos 3D com Collider
        Ray ray = Camera.main.ScreenPointToRay(posicaoTela);
        RaycastHit hit;
    
        if (Physics.Raycast(ray, out hit, 100f))
        {
     if (mostrarLogs)
          Debug.Log($"[Raycast] Tocou em: {hit.collider.gameObject.name}");
            
   // Exemplo: Enviar comando BLE ao tocar em objeto
     // EnviarComandoBLE(hit.collider.gameObject);
        }
    }

    // ========================================
    // EXEMPLO: Multi-touch (Pinch/Zoom)
    // ========================================
    public void DetectarPinchZoom()
    {
    if (Touch.activeTouches.Count != 2)
            return;

        Touch touch0 = Touch.activeTouches[0];
        Touch touch1 = Touch.activeTouches[1];

        // Calcular distância atual
    float distanciaAtual = Vector2.Distance(touch0.screenPosition, touch1.screenPosition);

        // Calcular distância anterior
        Vector2 touch0PrevPos = touch0.screenPosition - touch0.delta;
        Vector2 touch1PrevPos = touch1.screenPosition - touch1.delta;
        float distanciaAnterior = Vector2.Distance(touch0PrevPos, touch1PrevPos);

      // Diferença = zoom ou pinch
 float diferenca = distanciaAtual - distanciaAnterior;

  if (mostrarLogs && Mathf.Abs(diferenca) > 0.1f)
        {
      if (diferenca > 0)
          Debug.Log("[Multi-touch] Zoom IN");
 else
      Debug.Log("[Multi-touch] Zoom OUT");
        }
    }

    // ========================================
    // EXEMPLO: Detectar Teclado
    // ========================================
    private void DetectarTeclado()
    {
    if (Keyboard.current == null)
   return;

        // Tecla espaço
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("[Teclado] Espaço pressionado");
      }

        // WASD
        if (Keyboard.current.wKey.isPressed)
     {
            Debug.Log("[Teclado] W mantido pressionado");
   }

        // Enter
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            Debug.Log("[Teclado] Enter pressionado");
        }
    }

    // ========================================
    // EXEMPLO: Integração com BLE
    // ========================================
    private void EnviarComandoBLE(GameObject objeto)
    {
        // Exemplo de como integrar com seu sistema BLE existente
        
        // GerenciarComunicacao gc = GameObject.Find("Comunicacao").GetComponent<GerenciarComunicacao>();
   // if (gc != null)
        // {
        //     string comando = $"OBJETO_TOCADO:{objeto.name}";
        //     gc.Enviar(comando);
        //Debug.Log($"[BLE] Enviado: {comando}");
    // }
    }
}

