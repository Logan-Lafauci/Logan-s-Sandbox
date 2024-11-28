using System.Collections.Generic;
using UnityEngine;

public class PortalTraveller : MonoBehaviour
{
    public GameObject ObjectModel;
    public GameObject GraphicsClone { get; set; }
    public Vector3 previousOffsetFromPortal {get; set;}

    public Material[] OriginalMaterials { get; set;}
    public Material[] CloneMaterials { get; set; }
    public virtual void Teleport (Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
    }

    //Called when entering the portal
    public virtual void EnterPortalThreshold()
    {
        if (GraphicsClone == null)
        {
            GraphicsClone = Instantiate(ObjectModel);
            GraphicsClone.transform.parent = ObjectModel.transform.parent;
            GraphicsClone.transform.localScale = ObjectModel.transform.localScale;
            OriginalMaterials = GetMaterials(ObjectModel);
            CloneMaterials = GetMaterials(GraphicsClone);
        }
        else
        {
            GraphicsClone.SetActive(true);
        }
    }

    //Called when exiting the portal
    public virtual void ExitPortalThreshold() 
    {
        GraphicsClone.SetActive(false);
        // Disable slicing
        for (int i = 0; i < OriginalMaterials.Length; i++)
        {
            OriginalMaterials[i].SetVector("sliceNormal", Vector3.zero);
        }
    }

    public void SetSliceOffsetDst(float dst, bool clone)
    {
        for (int i = 0; i < OriginalMaterials.Length; i++)
        {
            if (clone)
            {
                CloneMaterials[i].SetFloat("sliceOffsetDst", dst);
            }
            else
            {
                OriginalMaterials[i].SetFloat("sliceOffsetDst", dst);
            }

        }
    }

    Material[] GetMaterials(GameObject g)
    {
        var renderers = g.GetComponentsInChildren<MeshRenderer>();
        var matList = new List<Material>();
        foreach (var renderer in renderers)
        {
            foreach (var mat in renderer.materials)
            {
                matList.Add(mat);
            }
        }
        return matList.ToArray();
    }
}
