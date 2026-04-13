// TEMPLATE LIMPO - Copie e modifique conforme necessário
// IMPORTANTE: Este arquivo só funcionará após você abrir o Unity e reiniciar

/*
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// Template básico para usar o novo Input System
/// Use este como base para seus próprios scripts
/// </summary>
public class TemplateNovoInput : MonoBehaviour
{
void OnEnable()
    {
   // Habilitar Enhanced Touch para mobile
        EnhancedTouchSupport.Enable();
        
        // Habilitar simulação com mouse (opcional, útil para testar no Editor)
        TouchSimulation.Enable();
    }

    void OnDisable()
    {
      EnhancedTouchSupport.Disable();
        TouchSimulation.Disable();
    }

    void Update()
    {
  // ESCOLHA UM DOS MÉTODOS ABAIXO:
        
        // Método 1: Enhanced Touch (Melhor para Mobile)
        UsarEnhancedTouch();
        
    // Método 2: Mouse (Simples, funciona em PC e Mobile)
    // UsarMouse();
        
        // Método 3: Pointer (Universal)
        // UsarPointer();
 }

    // ==========================================
    // MÉTODO 1: Enhanced Touch
    // ==========================================
    void UsarEnhancedTouch()
    {
        if (Touch.activeTouches.Count > 0)
        {
            Touch touch = Touch.activeTouches[0];
            
        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
         Vector2 posicao = touch.screenPosition;
                Debug.Log($"Touch em: {posicao}");
       
        // Sua lógica aqui
            }
}
    }

    // ==========================================
    // MÉTODO 2: Mouse
    // ==========================================
    void UsarMouse()
    {
        if (Mouse.current != null && 
      Mouse.current.leftButton.wasPressedThisFrame)
        {
       Vector2 posicao = Mouse.current.position.ReadValue();
            Debug.Log($"Clique/Touch em: {posicao}");
      
      // Sua lógica aqui
        }
    }

    // ==========================================
    // MÉTODO 3: Pointer Universal
    // ==========================================
    void UsarPointer()
    {
        if (Pointer.current != null && 
     Pointer.current.press.wasPressedThisFrame)
        {
     Vector2 posicao = Pointer.current.position.ReadValue();
            Debug.Log($"Pointer em: {posicao}");
    
       // Sua lógica aqui
        }
    }
}
*/
