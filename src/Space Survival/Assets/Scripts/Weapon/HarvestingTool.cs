using UnityEngine;
using System.Collections;

public class HarvestingTool : MeleeWeapon
{
    [Header("Harvesting")]
    [Tooltip("The level tier of resource this tool can harvest")]
    [SerializeField] int tier;
    [Tooltip("The type of resource this tool can harvest")]
    [SerializeField] HarvestTypes harvestType;
    [Tooltip("If this tool can harvest voxels")]
    [SerializeField] bool harvestVoxel;

    IHarvestableVoxel voxelChunk;
    IHarvestable harvestable;

    protected override IEnumerator DelayedAttack()
    {
        yield return base.DelayedAttack();
        //Apply damage to resource
        if (hit.collider != null) {
            //All object in radius
            if (hitRadius > 0) {
                //Harvest all surrounding harvestables
                Collider[] _colliders = Physics.OverlapSphere(hit.transform.position, hitRadius, mask);
                foreach (Collider _collider in _colliders) {
                    harvestable = _collider.transform.GetComponent<IHarvestable>();
                    if (harvestable != null) {
                        harvestable.TakeDamage(damage, harvestType, tier);
                    }
                }
            }

            //One object through raycast
            else {
                //Apply damage to resource
                harvestable = hit.collider.transform.GetComponent<IHarvestable>();
                if (harvestable != null) {
                    harvestable.TakeDamage(damage, harvestType, tier);
                }
            }

            if (harvestVoxel) {
                //Harvest all voxels in radius
                if (harvestVoxel) {
                    //Get chunk reference
                    voxelChunk = hit.transform.GetComponent<IHarvestableVoxel>();
                    if (voxelChunk != null) {
                        voxelChunk.Harvest(hit.point - (hit.normal * .01f), hitRadius, harvestType, tier);
                    }
                }
            }
        }
    }
}
