using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Portals
{
    public class PortalTraveller : MonoBehaviour
    {

        [SerializeField] GameObject graphicsObject;
        public GameObject graphicsClone { get; set; }

        // TODO why public?
        public Material[] originalMaterials;
        public Material[] cloneMaterials;
    
        public Vector3 previousPhysicsStepPosition { get; private set; }
        Vector3 currentPhysicsStepPosition;
    
        public Vector3 previousUpdateStepPosition { get; private set; }
        public Vector3 currentUpdateStepPosition { get; private set; }

        int teleportTracking;
    
        void FixedUpdate()
        {
            previousPhysicsStepPosition = currentPhysicsStepPosition;
            currentPhysicsStepPosition = transform.position;
        }

        void Update()
        {
            previousUpdateStepPosition = currentUpdateStepPosition;
            currentUpdateStepPosition = transform.position;
        }

        public virtual void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
        {
            // Debug.Log($"TELEPORTING {name}: {pos} -> {rot}");
            // Debug.DrawRay(transform.position, pos - transform.position, Color.red, 4);
            transform.position = pos;
            currentPhysicsStepPosition = pos;
            previousPhysicsStepPosition = pos;
            currentUpdateStepPosition = pos;
            previousUpdateStepPosition = pos;
            transform.rotation = rot;

            StartCoroutine(TrackTeleport());
        }

        IEnumerator TrackTeleport()
        {
            ++teleportTracking;
            yield return new WaitForSeconds(2);
            --teleportTracking;
        }

        // Called when a traveller first touches a portal
        public virtual void EnterPortalThreshold()
        {
            if (graphicsClone == null)
            {
                // TODO graphics clone transformation (location placement) is wrong when going through secondary portal
                graphicsClone = Instantiate(graphicsObject);
                graphicsClone.transform.parent = graphicsObject.transform.parent;
                graphicsClone.transform.localScale = graphicsObject.transform.localScale;
                originalMaterials = GetMaterials(graphicsObject);
                cloneMaterials = GetMaterials(graphicsClone);
            }
            else
            {
                graphicsClone.SetActive(true);
            }
        }

        // TODO figure out "Except when teleporting" - Does that just mean it's not called unless there's a barrier cross?
        // Called once a traveller is no longer touching a portal (except when teleporting)
        public virtual void ExitPortalThreshold()
        {
            graphicsClone.SetActive(false);
            // Disable mesh slicing (TODO colliders)
            foreach (Material material in originalMaterials)
            {
                // Does this zero out all the calculations in the slice shader via a zero dot product? It would be a good idea
                // to investigate exactly why this mathematically works as a disable at some point
                material.SetVector("sliceNormal", Vector3.zero);
            }
        }

        Material[] GetMaterials(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<MeshRenderer>();
            var materialList = new List<Material>();
            foreach (var renderer in renderers)
            {
                materialList.AddRange(renderer.sharedMaterials);
                // TODO delete once this works
                // foreach (var material in renderer.materials)
                // {
                //     materialList.Add(material);
                // }
            }

            return materialList.ToArray();
        }

        public void SetSliceOffsetDistance(float distance, bool clone)
        {
            for (int i = 0; i < originalMaterials.Length; ++i)
            {
                if (clone)
                {
                    cloneMaterials[i].SetFloat("sliceOffsetDst", distance);
                }
                else
                {
                    originalMaterials[i].SetFloat("sliceOffsetDst", distance);
                }
            }
        }

        void OnGUI()
        {
            GUI.Label(new Rect(20, 200, 200, 20), $"TELEPORTS: {teleportTracking}");
        }
    }
}
