using BS;

namespace ExplosiveItem
{
    // This create an item module that can be referenced in the item JSON
    public class ItemModuleExplosiveItem : ItemModule
    {
        public float timerToExplodeOnSpawn = 0f;
        public float timerToExplodeOnUnGrab = 0f;
        public float expForceMultiplier = 10f;
        public float explosionRadius = 5f;
        public float maxDamage = 200f;
        public bool ignorePlayerCollision = true;
        public bool explodesOnContact = true;
        public bool explodesOnNPCsOnly = false;
        public bool despawnItemAfterExplosion = true;
        public float cooldownTimer = 1f;
        public string explosionVFX = "None";
        public string explosionSFX = "None";

        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ItemExplosiveItem>();
        }
    }
}
