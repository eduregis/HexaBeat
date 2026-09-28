using UnityEngine;
using UnityEngine.InputSystem.OnScreen;

[RequireComponent(typeof(OnScreenStick))] // ou o nome do seu componente
public class StickEnabler : MonoBehaviour
{
    [SerializeField] private GameObject backgroundHandle; // visual

    void Awake()
    {
        bool isMobile = SystemInfo.deviceType == DeviceType.Handheld;

        if (!isMobile)
        {
            // Desativa só o visual (mantém o script ativo se quiser)
            backgroundHandle.SetActive(false);

            // Ou desativa o componente do stick completamente:
            // GetComponent<OnScreenStick>().enabled = false;
        }
    }
}