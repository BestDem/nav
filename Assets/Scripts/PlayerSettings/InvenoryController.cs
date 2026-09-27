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
    private void CastForwardRay()
    {
        Vector3 from = Head.transform.position;
        Vector3 to = Head.transform.TransformDirection(Vector3.forward);

        int layerMask = 1 << 6;

        Physics.Raycast(from, to, out forwardHit, 10, layerMask);
        //if (!aim) return;
        if (forwardHit.transform)
        {
            GameObject target = forwardHit.transform.gameObject;
            if (target.TryGetComponent(out InteractObject usableObject)) // || target.tag.Equals("Prop")
            {
                aim.SetActive(true);
                if(Input.GetKeyDown(KeyCode.E))
                {
                    usableObject.Use();
                }

            }
        } else
        {
            aim.SetActive(false);
        }
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
