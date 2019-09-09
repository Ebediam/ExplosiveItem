using BS;
using UnityEngine;

namespace ExplosiveItem
{

    public class ItemExplosiveItem : MonoBehaviour
    {
       
        protected Item item;

        public ItemModuleExplosiveItem module;

        public bool hasExploded = false;

        public bool setToExplode = false;

        public Transform explosionSFX;
        public Transform explosionVFX;

        public Transform mesh;

        protected void Awake()
        {

            item = this.GetComponent<Item>();            
            item.OnCollisionEvent += OnExplosiveCollision;

            module = item.data.GetModule<ItemModuleExplosiveItem>();

            if(module.explosionSFX != "None")
            {
                if (item.transform.Find(module.explosionSFX))
                {
                    explosionSFX = item.transform.Find(module.explosionSFX);
                }
                else
                {
                    Debug.LogError("ExplosiveItem error: no explosionSFX found");
                }
            }
          
            if(module.explosionVFX != "None")
            {
                if (item.transform.Find(module.explosionVFX))
                {
                    explosionVFX = item.transform.Find(module.explosionVFX);
                }
                else
                {
                    Debug.LogError("ExplosiveItem error: no explosionVFX found");
                }
            }
            
            if(module.meshTransformName != "None")
            {
                if (item.transform.Find(module.meshTransformName))
                {
                    mesh = item.transform.Find(module.meshTransformName);
                }
                else
                {
                    Debug.LogError("ExplosiveItem error: no mesh found");
                }
            }              
                    
        }


        public void OnExplosiveCollision(ref CollisionStruct collisionInstance)
        {

            if(collisionInstance.targetType == CollisionStruct.TargetType.NPC || !module.explodeOnNPCsOnly)
            {
                if (!hasExploded)
                {
                    Explosion();
                }
                
            }
            
        }

        public void Update()
        {
            if(module.timerToExplodeOnSpawn != 0 && !setToExplode)
            {
                setToExplode = true;
                Invoke("Explosion", module.timerToExplodeOnSpawn);

            }
        }

        void Cooldown()
        {
            hasExploded = false;
        }
 

       void ExplosionFX()
        {
            foreach(AudioSource audio in explosionSFX.GetComponentsInChildren<AudioSource>())
            {
                audio.Play();
            }
            
            foreach(ParticleSystem particle in explosionVFX.GetComponentsInChildren<ParticleSystem>())
            {
                particle.Play();
            }            
        }


        void Explosion()
        {
            ExplosionFX();
            foreach (Creature npc in Creature.list)
            {
                if (npc != Creature.player)
                {
                    float distNPC = Vector3.Distance(npc.body.headBone.transform.position, item.transform.position);
                    if (distNPC < module.explosionRadius)
                    {
                        float expReductor = (module.explosionRadius - distNPC) / module.explosionRadius;
                        expReductor = expReductor * 0.5f + 0.5f;
                        Vector3 expForceDirection = npc.body.headBone.transform.position - item.transform.position;
                        expForceDirection.Normalize();

                        if (npc.state != Creature.State.Dead)
                        {
                            npc.ragdoll.SetState(BS.Ragdoll.State.Fallen);
                        }

                        foreach (RagdollPart ragdollPart in npc.ragdoll.parts)
                        {
                            ragdollPart.rb.AddForce(expForceDirection * module.expForceMultiplier * expReductor, ForceMode.Impulse);

                        }
                        npc.health.currentHealth -= module.maxDamage * expReductor;

                    }

                }

            }
            hasExploded = true;

            if (!module.explodeOnce)
            {
                Invoke("Cooldown", module.cooldownTimer);
            }
            else
            {
                mesh.gameObject.SetActive(false);                
                Destroy(item, 1f);
            }
        }
    }
}