using System;
using System.Collections.Generic;
using System.Text;
using EntityStates;
using RoR2;
using UnityEngine;
using RoR2.Projectile;
using UnityEngine.Networking;
using EntityStates.ExampleSurvivorStates;
using System.Reflection;

namespace MegamanXVile.SkillStates
{
    public class CherryBlast : BaseSkillState
    {
        public float damageCoefficient = 0.25f;
        public float baseDuration = 1f;
        public float recoil = 1f;
        public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerClayBruiserMinigun");
        public static GameObject hitEffectPrefab = Resources.Load<GameObject>("prefabs/effects/impacteffects/Hitspark1");
        public int bulletcount;

        public static bool heat;
        public static int buffSkillIndex;

        public float shootdelay = 1.5f;
        public float timer = 2f;

        private float duration;
        private float fireDuration;
        private bool hasFired = true;
        private Animator animator;
        private string muzzleString;

        public override void OnEnter()
        {
            base.OnEnter();
            if (bulletcount == 0)
                bulletcount = 1;

            this.duration = this.baseDuration / base.attackSpeedStat;
            this.fireDuration = 0.25f * this.duration;
            base.characterBody.SetAimTimer(2f);
            this.animator = base.GetModelAnimator();
            this.muzzleString = "Weapon";

            
            if (heat)
                shootdelay = 0.86f;
            else
                shootdelay -= (base.attackSpeedStat / 10);


            base.PlayAnimation("Gesture, Override", "FireArrow", "FireArrow.playbackRate", this.duration);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void FireArrow()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;

                base.characterBody.AddSpreadBloom(0.6f);
                Ray aimRay = base.GetAimRay();
                EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol.effectPrefab, base.gameObject, this.muzzleString, false);

                if (base.isAuthority)
                {
                    //ProjectileManager.instance.FireProjectile(MegamanXVileSurvivor.MegamanXVile.arrowProjectile, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageCoefficient * this.damageStat, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                    /*
                    new BulletAttack
                    {
                        owner = base.gameObject,
                        weapon = base.gameObject,
                        origin = aimRay.origin,
                        aimVector = aimRay.direction,
                        minSpread = 0.1f,
                        maxSpread = 0.6f,
                        damage = damageCoefficient * this.damageStat,
                        damageType = (Util.CheckRoll(5f, base.characterBody.master) ? DamageType.SlowOnHit : DamageType.Generic),
                        procChainMask = default(ProcChainMask),
                        force = 45f,
                        radius = 0.4f,
                        sniper = true,
                        spreadPitchScale = 0.5f,
                        spreadYawScale = 0.5f,
                        tracerEffectPrefab = CherryBlast.tracerEffectPrefab,
                        hitMask = LayerIndex.CommonMasks.bullet,
                        falloffModel = BulletAttack.FalloffModel.None,
                        muzzleName = muzzleString,
                        hitEffectPrefab = hitEffectPrefab,
                        queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                        isCrit = Util.CheckRoll(base.critStat, base.characterBody.master)
                    }.Fire();
                    */
                    BulletAttack BT = new BulletAttack();
                    BT.owner = base.gameObject;
                    BT.weapon = base.gameObject;
                    BT.origin = aimRay.origin;
                    BT.aimVector = aimRay.direction;
                    BT.minSpread = 0.1f;
                    BT.maxSpread = 0.6f;
                    BT.damage = damageCoefficient * this.damageStat;
                    BT.procChainMask = default(ProcChainMask);
                    BT.force = 45f;
                    BT.radius = 0.4f;
                    BT.sniper = true;
                    BT.spreadPitchScale = 0.5f;
                    BT.spreadYawScale = 0.5f;
                    BT.tracerEffectPrefab = CherryBlast.tracerEffectPrefab;
                    BT.hitMask = LayerIndex.CommonMasks.bullet;
                    BT.falloffModel = BulletAttack.FalloffModel.None;
                    BT.muzzleName = muzzleString;
                    BT.hitEffectPrefab = hitEffectPrefab;
                    BT.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
                    BT.isCrit = Util.CheckRoll(base.critStat, base.characterBody.master);

                    switch(buffSkillIndex)
                    {
                        case 0:
                            BT.damageType = (Util.CheckRoll(5f, base.characterBody.master) ? DamageType.SlowOnHit : DamageType.Generic);
                            break;
                        case 1:
                            BT.damageType = (Util.CheckRoll(8f, base.characterBody.master) ? DamageType.Stun1s : DamageType.Generic);
                            break;
                        case 2:
                            BT.damageType = (Util.CheckRoll(8f, base.characterBody.master) ? DamageType.Shock5s : DamageType.Generic);
                            break;
                        case 3:
                            BT.damageType = (Util.CheckRoll(8f, base.characterBody.master) ? DamageType.IgniteOnHit : DamageType.Generic);
                            break;
                        default:
                            BT.damageType = (Util.CheckRoll(5f, base.characterBody.master) ? DamageType.SlowOnHit : DamageType.Generic);
                            break;

                    }

                    BT.Fire();
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            timer += Time.deltaTime;

            if (base.inputBank.skill1.down && hasFired)
            {
                if (timer > shootdelay)
                {
                    if (shootdelay <= 0.06f)
                        shootdelay = 0.06f;
                    else
                        shootdelay -= (0.15f + (base.attackSpeedStat / 50));

                    timer = 0;
                    hasFired = false;
                    base.characterBody.SetAimTimer(1f);
                    FireArrow();
                }
            }


            // if (base.fixedAge >= this.fireDuration)
            // {
            //FireArrow();
            //}

            if (base.fixedAge >= this.duration && base.isAuthority && !base.inputBank.skill1.down)
            {
                shootdelay = 1.5f;
                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}


