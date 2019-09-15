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
            item.OnUngrabEvent += OnExplosiveUngrab;
            item.OnGrabEvent += OnExplosiveGrab;
            


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
            
       
                    
        }

        public void OnExplosiveGrab(Handle handle, Interactor interactor)
        {
            CancelInvoke("Explosion");
        }

        public void OnExplosiveUngrab(Handle handle, Interactor interactor, bool throwing)
        {
            if(module.timerToExplodeOnUnGrab != 0)
            {
                if (handle == item.mainHandleLeft || handle == item.mainHandleRight)
                {
                    Invoke("Explosion", module.timerToExplodeOnUnGrab);                    
                }
            }

        }
        public void OnExplosiveCollision(ref CollisionStruct collisionInstance)
        {

            if (hasExploded)
            {
                return;
            }

            if (!module.explodesOnContact)
            {
                return;
            }

            if(collisionInstance.targetType == CollisionStruct.TargetType.Avatar && module.ignorePlayerCollision)
            {
                return;
            }

                                
            if (collisionInstance.targetType == CollisionStruct.TargetType.NPC || !module.explodesOnNPCsOnly)
            {                
                Explosion();                
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
            if(module.explosionSFX != "None")
            {
                foreach (AudioSource audio in explosionSFX.GetComponentsInChildren<AudioSource>())
                {
                    audio.Play();
                }
            }

            if(module.explosionVFX != "None")
            {
                foreach (ParticleSystem particle in explosionVFX.GetComponentsInChildren<ParticleSystem>())
                {
                    particle.Play();
                }
            }

        }


        void Explosion()
        {
            CancelInvoke("Explosion");
            hasExploded = true;
            ExplosionFX();

            foreach (Creature npc in Creature.list)
            {
                if (npc != Creature.player)
                {
                    float distNPC = Vector3.Distance(npc.ragdoll.GetPart(HumanBodyBones.Chest).transf.position, item.transform.position);
                    if (distNPC < module.explosionRadius)
                    {
                        float expReductor = (module.explosionRadius - distNPC) / module.explosionRadius;
                        expReductor = expReductor * 0.5f + 0.5f;
                        if (npc.state != Creature.State.Dead)
                        {
                            npc.ragdoll.SetState(Creature.State.Destabilized);
                        }
                                               

                        foreach (RagdollPart ragdollPart in npc.ragdoll.parts)
                        {
                            Vector3 expForceDirection = ragdollPart.transf.position - item.transform.position;
                            expForceDirection.Normalize();
                            ragdollPart.rb.AddForce(expForceDirection * module.expForceMultiplier * expReductor, ForceMode.Impulse);

                        }
                        npc.health.currentHealth -= module.maxDamage * expReductor;

                    }

                }

            }
            

            if (!module.despawnItemAfterExplosion)
            {
                Invoke("Cooldown", module.cooldownTimer);
            }
            else
            {
                foreach(Handle handle in item.handles)
                {
                    handle.Release();
                }
                
                foreach(MeshRenderer mesh in item.gameObject.GetComponentsInChildren<MeshRenderer>())
                {
                    mesh.enabled = false;
                }
                
                foreach(Collider col in item.gameObject.GetComponentsInChildren<Collider>())
                {
                    col.enabled = false;
                }

                item.rb.isKinematic = true;

                Destroy(item.gameObject, 1f);
            }
        }
    }
}