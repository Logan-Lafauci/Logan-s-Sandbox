using UnityEngine;

public class MainCamera : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Portal[] portals;

    void Awake()
    {
        portals = FindObjectsByType<Portal>(FindObjectsSortMode.None);
    }

    void OnPreCull()
    {

        for (int i = 0; i < portals.Length; i++)
        {
            //portals[i].PrePortalRender();
        }
        for (int i = 0; i < portals.Length; i++)
        {
            portals[i].Render();
        }
        for (int i = 0; i < portals.Length; i++)
        {
            //portals[i].PostPortalRender();
        }

    }
}
