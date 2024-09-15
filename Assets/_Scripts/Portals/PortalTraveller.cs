using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Portals
{
    public class PortalTraveller : MonoBehaviour
    {

        [SerializeField] GameObject graphicsObject;

        // TODO why public?
        public Material[] originalMaterials;
        public Material[] cloneMaterials;
    
        public Vector3 previousPhysicsStepPosition { get; private set; }
        Vector3 currentPhysicsStepPosition;
    
        public Vector3 previousUpdateStepPosition { get; private set; }
        public Vector3 currentUpdateStepPosition { get; private set; }

        /// <summary>
        /// Portals whose tracking thresholds the traveller is currently within
        /// </summary>
        // TODO pool the graphics clones so we're not constantly instantiating and destroying them
        Dictionary<Portal, GameObject> trackingPortalsToGraphicsClones = new();
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
            
            // iterate through all tracked portals and update the graphics clones positions
            foreach (var portal in trackingPortalsToGraphicsClones.Keys)
            {
                GameObject graphicsClone = trackingPortalsToGraphicsClones[portal];
                bool isPortalLinked = portal.LinkedPortal != null;
                if (!graphicsClone.activeSelf && isPortalLinked)
                {
                    graphicsClone.SetActive(true);
                } else if (graphicsClone.activeSelf && !isPortalLinked)
                {
                    graphicsClone.SetActive(false);
                }
                Assert.AreEqual(graphicsClone.activeSelf, isPortalLinked);

                // Each graphics clone needs to be positioned relative to the linked portal as we are to the current portal
                Portal linkedPortal = portal.LinkedPortal;
                Assert.IsNotNull(portal.LinkedPortal);
                var m = linkedPortal!.transform.localToWorldMatrix * portal.transform.worldToLocalMatrix * transform.localToWorldMatrix;
                graphicsClone.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);
                // Debug.Log($"{graphicsClone.name} {transform.position} ({transform.eulerAngles})");
            }
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
        public virtual void EnterPortalThreshold(Portal portal)
        {
            Debug.Log($"ENTERING PORTAL THRESHOLD {portal.name}");
            GameObject graphicsClone = Instantiate(graphicsObject);
            graphicsClone.transform.parent = graphicsObject.transform.parent;
            graphicsClone.transform.localScale = graphicsObject.transform.localScale;
            originalMaterials = GetMaterials(graphicsObject);
            cloneMaterials = GetMaterials(graphicsClone);
            bool isPortalLinked = portal.LinkedPortal != null;
            graphicsClone.SetActive(isPortalLinked);

            trackingPortalsToGraphicsClones[portal] = graphicsClone;
            
            //TODO this totally breaks down when the two portal thresholds overlap
            // it will be easiest to have a graphical clone for all portals that are currently tracking us
            // could be more than two with multiplayer (but really shouldn't with good level design)!
            // there's still a conceivable case where portals are so close together that the clone actually intercepts a DIFFERENT portal than the linked output of the one the traveller is intersecting
            // but we should really try to prevent that case from happening through level design/editor limitations
        }

        // TODO figure out "Except when teleporting" - Does that just mean it's not called unless there's a barrier cross?
        // Called once a traveller is no longer touching a portal (except when teleporting)
        public virtual void ExitPortalThreshold(Portal portal)
        {
            Debug.Log($"EXITING PORTAL THRESHOLD {portal.name}");
            // trackingPortalsToGraphicsClones[portal].SetActive(false);
            Destroy(trackingPortalsToGraphicsClones[portal]);
            trackingPortalsToGraphicsClones.Remove(portal);
            
            // Disable mesh slicing (TODO colliders)
            // TODO full revisit mesh slicing
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
