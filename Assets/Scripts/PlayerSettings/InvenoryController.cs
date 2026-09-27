using UnityEngine;
using UnityEngine.UI;

public class InvenoryController : MonoBehaviour
{
    [SerializeField] private GameObject Head;
    [SerializeField] private GameObject aim;
    private RaycastHit forwardHit;
    
    private void Update()
    {
        if (MenuInputGate.IsBlocked)
        {
            if (aim != null) aim.SetActive(false);
            return;
        }
        CastForwardRay();
    }
    private PassengerDialogue focusedPassenger;
    public void Interact()
    {
        if (MenuInputGate.IsBlocked) return;
        CastForwardRay(true);
    }
    private void CastForwardRay(bool requested = false)
    {
        focusedPassenger = null;
        if (aim != null) aim.SetActive(false);
        var camera = Camera.main;
        if (camera == null && Head == null)
        {
            if ((requested || Input.GetKeyDown(KeyCode.F))) Debug.LogWarning("[VSM Interaction] Не найдена камера или Head.", this);
            return;
        }
        // Cast through the visible crosshair, not a potentially offset Cinemachine target.
        Ray ray = camera != null
            ? camera.ViewportPointToRay(new Vector3(.5f, .5f, 0))
            : new Ray(Head.transform.position, Head.transform.forward);
        // Hit the nearest collider, including walls, so passengers cannot be used through them.
        if (!Physics.Raycast(ray, out forwardHit, 3f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if ((requested || Input.GetKeyDown(KeyCode.F))) Debug.Log("[VSM Interaction] Перед прицелом нет коллайдера в пределах 3 м.", this);
            return;
        }
        if ((requested || Input.GetKeyDown(KeyCode.F)))
            Debug.Log($"[VSM Interaction] Луч попал в {forwardHit.collider.name}, расстояние {forwardHit.distance:F2} м.", forwardHit.collider);
        var passenger = forwardHit.collider.GetComponentInParent<PassengerDialogue>();
        if (passenger != null)
        {
            if (!passenger.CanInteract)
            {
                if ((requested || Input.GetKeyDown(KeyCode.F))) Debug.Log("[VSM Interaction] У пассажира нет активного сценария или он уже завершён.", passenger);
                return;
            }
            focusedPassenger = passenger;
            if (aim != null) aim.SetActive(true);
            if ((requested || Input.GetKeyDown(KeyCode.F))) DialogueUI.OpenPassenger(passenger);
            return;
        }
        if ((requested || Input.GetKeyDown(KeyCode.F))) Debug.Log("[VSM Interaction] У объекта под прицелом нет PassengerDialogue. Возможно, это обычный пассажир или препятствие.", forwardHit.collider);
        // Ordinary passengers have no interaction; preserve E for inventory objects.
        if (forwardHit.collider.GetComponentInParent<IPeople>() != null) return;
        var usable = forwardHit.collider.GetComponentInParent<InteractObject>();
        if (usable == null) return;
        if (aim != null) aim.SetActive(true);
        if (requested || Input.GetKeyDown(KeyCode.E)) usable.Use();
    }

    private void OnGUI()
    {
        if (MenuInputGate.IsBlocked || focusedPassenger == null || !focusedPassenger.CanInteract) return;
        GUI.Box(new Rect(Screen.width / 2f - 140, Screen.height / 2f + 35, 280, 38),
            MobileGameControls.Visible ? "Нажмите «Поговорить»" : "F · Поговорить с пассажиром");
    }

    public RaycastHit CastForwardRayDynamic(float Distance)
    {
        RaycastHit outHit;
        int layerMask = 1 << 3;

        Vector3 from = Head.transform.position;
        Vector3 to = Head.transform.TransformDirection(Vector3.forward);

        Physics.Raycast(from, to, out outHit, Distance, layerMask);

        return outHit;
    }
}
