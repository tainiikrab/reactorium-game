using UnityEngine;
using UnityEngine.SceneManagement;

public class Grabber2DPhysicsSmooth : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask layer;

    private IDraggable grabbed;

    // void Start()
    // {
    //     cam = Camera.main;
    // }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (Input.GetMouseButtonDown(0))
        {
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, layer);

            if (hit.collider != null)
            {
                grabbed = hit.collider.GetComponent<IDraggable>();
                if (grabbed != null)
                {
                    Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
                    mousePos.z = 0;
                    grabbed.OnGrab(mousePos);
                }
            }
        }

        if (grabbed != null && Input.GetMouseButton(0))
        {
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            grabbed.OnDrag(mousePos);
        }

        if (Input.GetMouseButtonUp(0) && grabbed != null)
        {
            grabbed.OnRelease();
            grabbed = null;
        }
        
        if (grabbed != null)
        {
            if (Input.GetMouseButton(1))
                grabbed.OnRotateHold();
            else
                grabbed.OnRotateRelease();
        }
    }
}