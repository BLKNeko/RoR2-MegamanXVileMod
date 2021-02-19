using EntityStates;
using RoR2;
using UnityEngine;
using RoR2.Projectile;
using UnityEngine.Networking;
using EntityStates.ExampleSurvivorStates;
using System.Reflection;

namespace MegamanXVile.SkillStates
{
    public class TestSkill2 : BaseSkillState
    {
        public float damageCoefficient = 3f;
        public float baseDuration = 0.5f;
        public float recoil = 1f;
        public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerToolbotRebar");

        private float duration;
        private float fireDuration;
        private bool hasFired;
        private Animator animator;
        private string muzzleString;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = this.baseDuration;
            this.fireDuration = 0.25f * this.duration;
            base.characterBody.SetAimTimer(2f);
            this.animator = base.GetModelAnimator();
            this.muzzleString = "Weapon";



            //base.PlayAnimation("AttackL", "GranadeL", "attackSpeed", this.duration);
        }

        public override void OnExit()
        {

            base.OnExit();
        }

        private void FireES()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;

                base.characterBody.AddSpreadBloom(0.15f);
                Ray aimRay = base.GetAimRay();
                EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol.effectPrefab, base.gameObject, this.muzzleString, false);
                float spread_value = ((aimRay.direction.x) - (aimRay.direction.x * 3f));
                //EffectManager.SimpleMuzzleFlash(EntityStates.Mage.Weapon.FireLaserbolt.impactEffectPrefab, base.gameObject, this.muzzleString, false);
                Vector3 raygun1 = new Vector3(aimRay.direction.x + 0.2f,aimRay.direction.y,aimRay.direction.z);
                Vector3 raygun2 = new Vector3(aimRay.direction.x - 0.2f, aimRay.direction.y, aimRay.direction.z);
                //Vector3 raygun3 = Vector3.Cross(Vector3.left, aimRay.origin).normalized;

                if (base.isAuthority)
                {

                    ProjectileManager.instance.FireProjectile(Materials.RegisterProjectiles.TestSkill, aimRay.origin, Util.QuaternionSafeLookRotation(raygun1.normalized), base.gameObject, this.damageCoefficient * this.damageStat, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                    ProjectileManager.instance.FireProjectile(Materials.RegisterProjectiles.TestSkill, aimRay.origin, Util.QuaternionSafeLookRotation(raygun2.normalized), base.gameObject, this.damageCoefficient * this.damageStat, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                    ProjectileManager.instance.FireProjectile(Materials.RegisterProjectiles.TestSkill, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageCoefficient * this.damageStat, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                    Chat.AddMessage(aimRay.direction.ToString());


                    /*
                    new BulletAttack
                    {
                        owner = base.gameObject,
                        weapon = base.gameObject,
                        origin = aimRay.origin,
                        aimVector = aimRay.direction,
                        minSpread = 0f,
                        maxSpread = base.characterBody.spreadBloomAngle,
                        procCoefficient = 1f,
                        damage = base.characterBody.damage,
                        force = 500,
                        falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                        tracerEffectPrefab = TestSkill2.tracerEffectPrefab,
                        //hitEffectPrefab = this.hitEffectPrefab,
                        isCrit = base.RollCrit(),
                        HitEffectNormal = false,
                        stopperMask = LayerIndex.world.mask,
                        smartCollision = true,
                    }.Fire();
                    */
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.fireDuration)
            {
                FireES();
            }

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
