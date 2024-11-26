using UnityEngine;

public class Portal : MonoBehaviour
{
    public Portal linkedPortal;
    public MeshRenderer screen;
    Camera PlayerCam;
    Camera PortalCam;
    RenderTexture viewTexture;

    void Awake()
    { 
        PlayerCam = Camera.main;
        //Could set this as a serialize and use this if portalCam = null
        PortalCam = GetComponentInChildren<Camera>();
        PortalCam.enabled = false;
    }

    void CreateViewTexture()
    {
        if(viewTexture == null || viewTexture.width != Screen.width || viewTexture.height != Screen.height)
        {
            if(viewTexture != null)
            {
                viewTexture.Release();
            }
            viewTexture = new RenderTexture(Screen.width, Screen.height, 0);
            //Render the view from the portal camera to the view texture
            PortalCam.targetTexture = viewTexture;
            //Display the view texture on the screen of the linked portal
            linkedPortal.screen.material.SetTexture("_MainTex", viewTexture);
        }
    }

    //This bool could be repurposed as a good tool in general for rendering things only found in the bounds of the main camera
    //Look into it more in the future
    static bool VisibleFromCamera(Renderer renderer, Camera camera)
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
    }

    public void Render()
    {
        if (!VisibleFromCamera(linkedPortal.screen, PlayerCam)) return;

        screen.enabled = false;
        CreateViewTexture();
        
        //Make the portal cam position and rotation the same relative to this portal, as the player cam relative to linked portal
        var m = transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix * PlayerCam.transform.localToWorldMatrix;
        PortalCam.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);

        //render the camera
        PortalCam.Render();

        screen.enabled = true;
    }
}
