using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Portal linkedPortal;
    public MeshRenderer screen;
    Camera playerCam;
    Camera portalCam;
    RenderTexture viewTexture;
    List<PortalTraveller> travellers;

    void Awake()
    { 
        playerCam = Camera.main;
        //Could set this as a serialize and use this if portalCam = null
        portalCam = GetComponentInChildren<Camera>();
        portalCam.enabled = false;
        travellers = new List<PortalTraveller>();
    }

    private void LateUpdate()
    {
        HandleTravellers();
    }

    private void HandleTravellers()
    {
        for (int i = 0; i < travellers.Count; i++)
        {
            PortalTraveller traveller = travellers[i];
            Transform travellerT = traveller.transform;

            //Similar function is used for the camera placement which makes the portal look see through.
            var m = linkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * travellerT.localToWorldMatrix;

            Vector3 offsetFromPortal = travellerT.position - transform.position;
            int portalSide = System.Math.Sign(Vector3.Dot(offsetFromPortal, transform.forward));
            int previousPortalSide = System.Math.Sign(Vector3.Dot(traveller.previousOffsetFromPortal, transform.forward));
            //Teleport the traveller if it has crossed from one side of the portal to the other
            if (portalSide != previousPortalSide)
            {
                var positionOld = travellerT.position;
                var rotOld = travellerT.rotation;
                traveller.Teleport(transform, linkedPortal.transform, (Vector3)m.GetColumn(3), m.rotation);
                traveller.GraphicsClone.transform.SetPositionAndRotation(positionOld, rotOld);

                //This updates travellers immediatly instead of waiting for OnTriggerEnter/Exit.
                linkedPortal.OnTravellerEnterPortal(traveller);
                travellers.RemoveAt(i);
                i--;
            }
            else
            {
                traveller.GraphicsClone.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);

                traveller.previousOffsetFromPortal = offsetFromPortal;
            }
        }
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
            portalCam.targetTexture = viewTexture;
            //Display the view texture on the screen of the linked portal
            linkedPortal.screen.material.SetTexture("_MainTex", viewTexture);
        }
    }

    //This function fixes issues with portals being too thin and seeing through them
    float ProtectScreenFromClipping(Vector3 viewPoint)
    {
        float halfHeight = playerCam.nearClipPlane * Mathf.Tan(playerCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * playerCam.aspect;
        float dstToNearClipPlaneCorner = new Vector3(halfWidth, halfHeight, playerCam.nearClipPlane).magnitude + 0.05f;//+.05 adds a little leigh way

        Transform screenT = screen.transform;
        bool camSameSideAsPortal = Vector3.Dot(transform.forward, transform.position - viewPoint) > 0;
        screenT.localScale = new Vector3(screenT.localScale.x, screenT.localScale.y, dstToNearClipPlaneCorner);
        screenT.localPosition = Vector3.forward * dstToNearClipPlaneCorner * ((camSameSideAsPortal) ? 0.5f : -0.5f);
        return dstToNearClipPlaneCorner;//This is the thickness of the screen
    }

    private void UpdateSliceParams(PortalTraveller traveller)
    {
        //calculate slice normal
        int side = SideOfPortal(traveller.transform.position);
        Vector3 sliceNormal = transform.forward * -side;
        Vector3 cloneSliceNormal = linkedPortal.transform.forward * side;

        //calculate slice centre
        Vector3 slicePos = transform.position;
        Vector3 cloneSlicePos = linkedPortal.transform.position;

        // Adjust slice offset so that when player standing on other side of portal to the object, the slice doesn't clip through
        float sliceOffsetDst = 0;
        float cloneSliceOffsetDst = 0;
        float screenThickness = screen.transform.localScale.z;

        bool playerSameSideAsTraveller = SameSideOfPortal(playerCam.transform.position, traveller.transform.position);
        if (!playerSameSideAsTraveller)
        {
            sliceOffsetDst = -screenThickness;
        }
        bool playerSameSideAsCloneAppearing = side != linkedPortal.SideOfPortal(playerCam.transform.position);
        if (!playerSameSideAsCloneAppearing)
        {
            cloneSliceOffsetDst = -screenThickness;
        }

        //Apply parameters
        for (int i = 0; i < traveller.OriginalMaterials.Length; i++)
        {
            traveller.OriginalMaterials[i].SetVector("sliceCenter", slicePos);
            traveller.OriginalMaterials[i].SetVector("sliceNormal", sliceNormal);
            traveller.OriginalMaterials[i].SetFloat("sliceOffsetDst", sliceOffsetDst);

            traveller.CloneMaterials[i].SetVector("sliceCenter", cloneSlicePos);
            traveller.CloneMaterials[i].SetVector("sliceNormal", cloneSliceNormal);
            traveller.CloneMaterials[i].SetFloat("sliceOffsetDst", cloneSliceOffsetDst);
        }
    }

    void HandleClipping()
    {
        // There are two main graphical issues when slicing travellers
        // 1. Tiny sliver of mesh drawn on backside of portal
        //    Ideally the oblique clip plane would sort this out, but even with 0 offset, tiny sliver still visible
        // 2. Tiny seam between the sliced mesh, and the rest of the model drawn onto the portal screen
        // This function tries to address these issues by modifying the slice parameters when rendering the view from the portal
        // Would be great if this could be fixed more elegantly, but this is the best I can figure out for now
        const float hideDst = -1000;
        const float showDst = 1000;
        float screenThickness = linkedPortal.ProtectScreenFromClipping(portalCam.transform.position);

        var portalCamPos = portalCam.transform.position;

        foreach (var traveller in travellers)
        {
            if (SameSideOfPortal(traveller.transform.position, portalCamPos))
            {
                // Addresses issue 1
                traveller.SetSliceOffsetDst(hideDst, false);
            }
            else
            {
                // Addresses issue 2
                traveller.SetSliceOffsetDst(showDst, false);
            }

            // Ensure clone is properly sliced, in case it's visible through this portal:
            int cloneSideOfLinkedPortal = -SideOfPortal(traveller.transform.position);
            bool camSameSideAsClone = linkedPortal.SideOfPortal(portalCamPos) == cloneSideOfLinkedPortal;
            if (camSameSideAsClone)
            {
                traveller.SetSliceOffsetDst(screenThickness, true);
            }
            else
            {
                traveller.SetSliceOffsetDst(-screenThickness, true);
            }
        }

        var offsetFromPortalToCam = portalCamPos - transform.position;
        foreach (var linkedTraveller in linkedPortal.travellers)
        {
            var travellerPos = linkedTraveller.ObjectModel.transform.position;
            var clonePos = linkedTraveller.GraphicsClone.transform.position;
            // Handle clone of linked portal coming through this portal:
            bool cloneOnSameSideAsCam = linkedPortal.SideOfPortal(travellerPos) != SideOfPortal(portalCamPos);
            if (cloneOnSameSideAsCam)
            {
                // Addresses issue 1
                linkedTraveller.SetSliceOffsetDst(hideDst, true);
            }
            else
            {
                // Addresses issue 2
                linkedTraveller.SetSliceOffsetDst(showDst, true);
            }

            // Ensure traveller of linked portal is properly sliced, in case it's visible through this portal:
            bool camSameSideAsTraveller = linkedPortal.SameSideOfPortal(linkedTraveller.transform.position, portalCamPos);
            if (camSameSideAsTraveller)
            {
                linkedTraveller.SetSliceOffsetDst(screenThickness, false);
            }
            else
            {
                linkedTraveller.SetSliceOffsetDst(-screenThickness, false);
            }
        }
    }

    //This bool could be repurposed as a good tool in general for rendering things only found in the bounds of the main camera
    //Look into it more in the future
    static bool VisibleFromCamera(Renderer renderer, Camera camera)
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
    }

    // Called before any portal cameras are rendered for the current frame
    public void PrePortalRender()
    {
        foreach (var traveller in travellers)
        {
            UpdateSliceParams(traveller);
        }
    }

    // Called once all portals have been rendered, but before the player camera renders
    public void PostPortalRender()
    {
        foreach (var traveller in travellers)
        {
            UpdateSliceParams(traveller);
        }
        ProtectScreenFromClipping(playerCam.transform.position);
    }

    public void Render()
    {
        if (!VisibleFromCamera(linkedPortal.screen, playerCam)) return;

        screen.enabled = false;
        CreateViewTexture();
        
        //Make the portal cam position and rotation the same relative to this portal, as the player cam relative to linked portal
        var m = transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix * playerCam.transform.localToWorldMatrix;
        portalCam.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);

        HandleClipping();
        portalCam.Render();

        screen.enabled = true;
    }

    void OnTravellerEnterPortal(PortalTraveller traveller) 
    {
        if(!travellers.Contains(traveller))
        {
            traveller.EnterPortalThreshold();
            traveller.previousOffsetFromPortal = traveller.transform.position - transform.position;
            travellers.Add(traveller);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var traveller = other.GetComponent<PortalTraveller>();
        if(traveller != null)
        {
            OnTravellerEnterPortal(traveller);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var traveller = other.GetComponent<PortalTraveller>();
        if (traveller != null && travellers.Contains(traveller))
        {
            traveller.ExitPortalThreshold();
            travellers.Remove(traveller);
        }
    }

    public Quaternion GetCameraRotation()
    {
        return portalCam.transform.rotation;
    }

    //helper functions
    int SideOfPortal (Vector3 pos) {
        return System.Math.Sign (Vector3.Dot (pos - transform.position, transform.forward));
    }

    bool SameSideOfPortal (Vector3 posA, Vector3 posB) {
        return SideOfPortal (posA) == SideOfPortal (posB);
    }
}
