using BepInEx;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace MegamanXVile.Materials
{

    public class RegisterProjectiles : BaseUnityPlugin
    {
        public static GameObject arrowProjectile; // prefab for our survivor's primary attack projectile
        public static GameObject EletricSpark;
        public static GameObject BumpityBombProjectile;
        public static GameObject TestSkill;
        public static GameObject TestSkill2;



        public static void Register()
        {
            // clone rex's syringe projectile prefab here to use as our own projectile
            arrowProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/SyringeProjectile"), "Prefabs/Projectiles/ExampleArrowProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanXVile\\MegamanXVile\\MegamanXVile\\MegamanXVile.cs", "RegisterCharacter", 155);

            // just setting the numbers to 1 as the entitystate will take care of those
            arrowProjectile.GetComponent<ProjectileController>().procCoefficient = 1f;
            arrowProjectile.GetComponent<ProjectileDamage>().damage = 1f;
            arrowProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;

            // register it for networking
            if (arrowProjectile) PrefabAPI.RegisterNetworkPrefab(arrowProjectile);

            //-------------------------------------START --------------------------------------------

            // clone rex's syringe projectile prefab here to use as our own projectile
            EletricSpark = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/MageLightningBombProjectile"), "Prefabs/Projectiles/ESparkProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanXVile\\MegamanXVile\\MegamanXVile\\MegamanXVile.cs", "RegisterCharacter", 155);

            // just setting the numbers to 1 as the entitystate will take care of those
            EletricSpark.GetComponent<ProjectileController>().procCoefficient = 1f;
            EletricSpark.GetComponent<ProjectileDamage>().damage = 1f;
            EletricSpark.GetComponent<ProjectileDamage>().damageType = DamageType.Shock5s;

            // register it for networking
            if (EletricSpark) PrefabAPI.RegisterNetworkPrefab(EletricSpark);

            //--------------------------------------END --------------------------------------------

            //-------------------------------------START --------------------------------------------

            //CommandoGrenadeProjectile (boa, quica uma vez e explode  depois de um tempo)
            //CryoCanisterBombletsProjectile (boa, ele apenas solta a granada no chão)
            BumpityBombProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/CommandoGrenadeProjectile"), "Prefabs/Projectiles/BombProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanXVile\\MegamanXVile\\MegamanXVile\\MegamanXVile.cs", "RegisterCharacter", 155);

            // just setting the numbers to 1 as the entitystate will take care of those
            BumpityBombProjectile.GetComponent<ProjectileController>().procCoefficient = 1f;
            BumpityBombProjectile.GetComponent<ProjectileDamage>().damage = 1f;
            BumpityBombProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;

            // register it for networking
            if (BumpityBombProjectile) PrefabAPI.RegisterNetworkPrefab(BumpityBombProjectile);

            //--------------------------------------END --------------------------------------------

            //-------------------------------------START --------------------------------------------

            //CommandoGrenadeProjectile (boa, quica uma vez e explode  depois de um tempo)
            //CryoCanisterBombletsProjectile (boa, ele apenas solta a granada no chão)
            TestSkill = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/MageFireBombProjectile"), "Prefabs/Projectiles/BombProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanXVile\\MegamanXVile\\MegamanXVile\\MegamanXVile.cs", "RegisterCharacter", 155);

            // just setting the numbers to 1 as the entitystate will take care of those
            TestSkill.GetComponent<ProjectileController>().procCoefficient = 1f;
            TestSkill.GetComponent<ProjectileDamage>().damage = 1f;
            TestSkill.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;

            // register it for networking
            if (TestSkill) PrefabAPI.RegisterNetworkPrefab(TestSkill);

            //--------------------------------------END --------------------------------------------

            //-------------------------------------START --------------------------------------------

            //CommandoGrenadeProjectile (boa, quica uma vez e explode  depois de um tempo)
            //CryoCanisterBombletsProjectile (boa, ele apenas solta a granada no chão)
            TestSkill2 = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/FMJ"), "Prefabs/Projectiles/BombProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanXVile\\MegamanXVile\\MegamanXVile\\MegamanXVile.cs", "RegisterCharacter", 155);

            // just setting the numbers to 1 as the entitystate will take care of those
            TestSkill2.GetComponent<ProjectileController>().procCoefficient = 1f;
            TestSkill2.GetComponent<ProjectileDamage>().damage = 1f;
            TestSkill2.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;

            // register it for networking
            if (TestSkill2) PrefabAPI.RegisterNetworkPrefab(TestSkill2);

            //--------------------------------------END --------------------------------------------

            // add it to the projectile catalog or it won't work in multiplayer
            ProjectileCatalog.getAdditionalEntries += list =>
            {
                list.Add(arrowProjectile);
                list.Add(EletricSpark);
                list.Add(BumpityBombProjectile);
                list.Add(TestSkill);
                list.Add(TestSkill2);
            };
        }

    }
}
