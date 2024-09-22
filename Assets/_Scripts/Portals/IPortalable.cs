using System.Collections.Generic;
using FishNet.CodeGenerating;
using FishNet.Serializing;
using UnityEngine;

[UseGlobalCustomSerializer]
public abstract class IPortalable : MonoBehaviour
{
    internal static Dictionary<int, IPortalable> portalables = new();

    void Awake()
    {
        // GetInstanceID is not consistent across builds
        // Instead, hash the hierarchy path (including the sibling index at each step)
        // Vulnerable if objects are getting added as siblings to portalables at runtime
        // So just don't do that
        // TODO one alternative is to generate an ID and store it in the instance
        portalables.Add(GetHierarchyID(), this);
    }

    internal int GetHierarchyID()
    {
        return gameObject.GetFullPathName().GetHashCode();
    }

    void OnDestroy()
    {
        portalables.Remove(GetHierarchyID());
    }

    // TODO replace primary bool with an enum
    public abstract Portal PlacePortal(PortalGun portalGun, bool primary, out Portal replacedPortal);
    public abstract void RemovePortal();

    public abstract bool IsActive();
    public abstract Portal GetPortal();
}

public static class PortalableSerializer
{
    public static void WriteIPortalable(this Writer writer, IPortalable portalable)
    {
        writer.WriteInt32(portalable == null ? -1 : portalable.GetHierarchyID());
    }
    
    public static IPortalable ReadIPortalable(this Reader reader)
    {
        int hierarchyID = reader.ReadInt32();
        if (hierarchyID == -1) return null;
        return IPortalable.portalables[hierarchyID];
    }
    
    // TODO move this somewhere generalized for game object extensions?
    public static string GetFullPathName(this GameObject obj)
    {
        string path = "/" + obj.name + obj.transform.GetSiblingIndex();
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = "/" + obj.name + obj.transform.GetSiblingIndex() + path;
        }
        return path;
    }
}