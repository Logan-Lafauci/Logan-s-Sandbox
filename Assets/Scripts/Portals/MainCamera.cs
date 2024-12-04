using Unity.Cinemachine;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    [SerializeField] private static CinemachinePanTilt cameraLookingAngle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Portal[] portals;

    //Different portal type that works similar
    MultiLinkPortal[] multiLinkPortals;

    void Awake()
    {
        portals = FindObjectsByType<Portal>(FindObjectsSortMode.None);
        multiLinkPortals = FindObjectsByType<MultiLinkPortal>(FindObjectsSortMode.None);

        if(cameraLookingAngle == null)
            cameraLookingAngle = FindFirstObjectByType<CinemachinePanTilt>();
    }

    void OnPreCull()
    {

        for (int i = 0; i < portals.Length; i++)
        {
            portals[i].PrePortalRender();
        }
        for (int i = 0; i < portals.Length; i++)
        {
            portals[i].Render();
        }
        for (int i = 0; i < portals.Length; i++)
        {
            portals[i].PostPortalRender();
        }

        //for multi linked portals
        for (int i = 0; i < multiLinkPortals.Length; i++)
        {
            multiLinkPortals[i].PrePortalRender();
        }
        for (int i = 0; i < multiLinkPortals.Length; i++)
        {
            multiLinkPortals[i].Render();
        }
        for (int i = 0; i < multiLinkPortals.Length; i++)
        {
            multiLinkPortals[i].PostPortalRender();
        }

    }

    //This function is used to change the camera angle when using cinemachine. It convert's euler angles to the same rotation as the camera
    public static void ChangeCameraAgle(Quaternion rotation)
    {
        cameraLookingAngle.TiltAxis.Value = rotation.eulerAngles.x > 180 ? rotation.eulerAngles.x - 360 : rotation.eulerAngles.x;
        cameraLookingAngle.PanAxis.Value = rotation.eulerAngles.y > 180 ? rotation.eulerAngles.y - 360 : rotation.eulerAngles.y;
    }
}
