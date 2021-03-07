using System;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using R2API;
using R2API.Utils;
using EntityStates;
using RoR2;
using RoR2.Skills;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using KinematicCharacterController;
using System.Security;
using System.Security.Permissions;
using MegamanXVile.SkillStates;
using MegamanXVile.Materials;
using EntityStates.ExampleSurvivorStates;
using System.IO;
using BepInEx.Configuration;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace MegamanXVileSurvivor
{

    [BepInDependency("com.bepis.r2api")]

    [BepInPlugin(MODUID, "MegamanXVile", "1.0.0")] // put your own name and version here
    [R2APISubmoduleDependency(nameof(PrefabAPI), nameof(SurvivorAPI), nameof(LoadoutAPI), nameof(ItemAPI), nameof(DifficultyAPI), nameof(BuffAPI))] // need these dependencies for the mod to work properly


    public class MegamanXVile : BaseUnityPlugin
    {
        public const string MODUID = "com.BLKNeko.MegamanXVile"; // put your own names here

        public static GameObject characterPrefab; // the survivor body prefab
        public GameObject characterDisplay; // the prefab used for character select
        public GameObject doppelganger; // umbra shit

        //public static GameObject arrowProjectile; // prefab for our survivor's primary attack projectile
        //public static GameObject EletricSpark;
        


        private static readonly Color characterColor = new Color(0.35f, 0.05f, 0.4f); // color used for the survivor

        public static ConfigEntry<int> skinConfig { get; set; }

        private void Awake()
        {
            //------------------------START CONFIG--------------------------
            skinConfig = Config.Bind<int>(
            "SKIN_SELECTOR",
            "SkinIndex",
            0,
            "Vile Default Skin = 0 // Vile MK-II Skin = 1"
            );


            //------------------------END CONFIG----------------------------

            Assets.PopulateAssets(); // first we load the assets from our assetbundle
            CreatePrefab(); // then we create our character's body prefab

            RegisterProjectiles.Register();

            RegisterStates(); // register our skill entitystates for networking
            RegisterCharacter(); // and finally put our new survivor in the game
            CreateDoppelganger(); // not really mandatory, but it's simple and not having an umbra is just kinda lame
            Skins.RegisterSkins();
        }

        private static GameObject CreateModel(GameObject main)
        {
            Destroy(main.transform.Find("ModelBase").gameObject);
            Destroy(main.transform.Find("CameraPivot").gameObject);
            Destroy(main.transform.Find("AimOrigin").gameObject);

            int skinIndex = skinConfig.Value;

            GameObject model;

            switch (skinIndex)
            {
                case 0:
                    model = Assets.MainAssetBundle.LoadAsset<GameObject>("mdlVile");
                    break;
                case 1:
                    model = Assets.MainAssetBundle.LoadAsset<GameObject>("mdlVileMKII");
                    break;
                default:
                    model = Assets.MainAssetBundle.LoadAsset<GameObject>("mdlVile");
                    break;
            }

            // make sure it's set up right in the unity project
            //GameObject model = Assets.MainAssetBundle.LoadAsset<GameObject>("mdlVile");

            return model;
        }

        internal static void CreatePrefab()
        {
            // first clone the commando prefab so we can turn that into our own survivor
            characterPrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody"), "VileBody", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanXVile\\MegamanXVile\\MegamanXVile\\MegamanXVile.cs", "CreatePrefab", 151);

            characterPrefab.GetComponent<NetworkIdentity>().localPlayerAuthority = true;

            // create the model here, we're gonna replace commando's model with our own
            GameObject model = CreateModel(characterPrefab);

            GameObject gameObject = new GameObject("ModelBase");
            gameObject.transform.parent = characterPrefab.transform;
            gameObject.transform.localPosition = new Vector3(0f, -0.81f, 0f);
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(1f, 1f, 1f);

            GameObject gameObject2 = new GameObject("CameraPivot");
            gameObject2.transform.parent = gameObject.transform;
            gameObject2.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            gameObject2.transform.localRotation = Quaternion.identity;
            gameObject2.transform.localScale = Vector3.one;

            GameObject gameObject3 = new GameObject("AimOrigin");
            gameObject3.transform.parent = gameObject.transform;
            gameObject3.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            gameObject3.transform.localRotation = Quaternion.identity;
            gameObject3.transform.localScale = Vector3.one;

            Transform transform = model.transform;
            transform.parent = gameObject.transform;
            transform.localPosition = Vector3.zero;
            transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            transform.localRotation = Quaternion.identity;

            CharacterDirection characterDirection = characterPrefab.GetComponent<CharacterDirection>();
            characterDirection.moveVector = Vector3.zero;
            characterDirection.targetTransform = gameObject.transform;
            characterDirection.overrideAnimatorForwardTransform = null;
            characterDirection.rootMotionAccumulator = null;
            characterDirection.modelAnimator = model.GetComponentInChildren<Animator>();
            characterDirection.driveFromRootRotation = false;
            characterDirection.turnSpeed = 720f;

            // set up the character body here
            CharacterBody bodyComponent = characterPrefab.GetComponent<CharacterBody>();
            bodyComponent.bodyIndex = -1;
            bodyComponent.baseNameToken = "VILE_NAME"; // name token
            bodyComponent.subtitleNameToken = "VILE_SUBTITLE"; // subtitle token- used for umbras
            bodyComponent.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
            bodyComponent.bodyFlags = CharacterBody.BodyFlags.IgnoreFallDamage;
            bodyComponent.rootMotionInMainState = false;
            bodyComponent.mainRootSpeed = 0;
            bodyComponent.baseMaxHealth = 140;
            bodyComponent.levelMaxHealth = 25;
            bodyComponent.baseRegen = 0.4f;
            bodyComponent.levelRegen = 0.28f;
            bodyComponent.baseMaxShield = 0;
            bodyComponent.levelMaxShield = 0.5f;
            bodyComponent.baseMoveSpeed = 6f;
            bodyComponent.levelMoveSpeed = 0.1f;
            bodyComponent.baseAcceleration = 75;
            bodyComponent.baseJumpPower = 25;
            bodyComponent.levelJumpPower = 0.4f;
            bodyComponent.baseDamage = 24;
            bodyComponent.levelDamage = 3f;
            bodyComponent.baseAttackSpeed = 1;
            bodyComponent.levelAttackSpeed = 0.05f;
            bodyComponent.baseCrit = 1;
            bodyComponent.levelCrit = 0.25f;
            bodyComponent.baseArmor = 1;
            bodyComponent.levelArmor = 1f;
            bodyComponent.baseJumpCount = 1;
            bodyComponent.sprintingSpeedMultiplier = 1.4f;
            bodyComponent.wasLucky = false;
            bodyComponent.hideCrosshair = false;
            bodyComponent.crosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/SMGCrosshair");
            bodyComponent.aimOriginTransform = gameObject3.transform;
            bodyComponent.hullClassification = HullClassification.Human;
            bodyComponent.portraitIcon = Assets.charPortrait;
            bodyComponent.isChampion = false;
            bodyComponent.currentVehicle = null;
            bodyComponent.skinIndex = 0U;

            // the charactermotor controls the survivor's movement and stuff
            CharacterMotor characterMotor = characterPrefab.GetComponent<CharacterMotor>();
            characterMotor.walkSpeedPenaltyCoefficient = 1f;
            characterMotor.characterDirection = characterDirection;
            characterMotor.muteWalkMotion = false;
            characterMotor.mass = 100f;
            characterMotor.airControl = 0.25f;
            characterMotor.disableAirControlUntilCollision = false;
            characterMotor.generateParametersOnAwake = true;
            characterMotor.useGravity = true;
            characterMotor.isFlying = false;

            InputBankTest inputBankTest = characterPrefab.GetComponent<InputBankTest>();
            inputBankTest.moveVector = Vector3.zero;

            CameraTargetParams cameraTargetParams = characterPrefab.GetComponent<CameraTargetParams>();
            cameraTargetParams.cameraParams = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponent<CameraTargetParams>().cameraParams;
            cameraTargetParams.cameraPivotTransform = null;
            cameraTargetParams.aimMode = CameraTargetParams.AimType.Standard;
            cameraTargetParams.recoil = Vector2.zero;
            cameraTargetParams.idealLocalCameraPos = Vector3.zero;
            cameraTargetParams.dontRaycastToPivot = false;

            // this component is used to locate the character model(duh), important to set this up here
            ModelLocator modelLocator = characterPrefab.GetComponent<ModelLocator>();
            modelLocator.modelTransform = transform;
            modelLocator.modelBaseTransform = gameObject.transform;
            modelLocator.dontReleaseModelOnDeath = false;
            modelLocator.autoUpdateModelTransform = true;
            modelLocator.dontDetatchFromParent = false;
            modelLocator.noCorpse = false;
            modelLocator.normalizeToFloor = false; // set true if you want your character to rotate on terrain like acrid does
            modelLocator.preserveModel = false;

            // childlocator is something that must be set up in the unity project, it's used to find any child objects for things like footsteps or muzzle flashes
            // also important to set up if you want quality
            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            // this component is used to handle all overlays and whatever on your character, without setting this up you won't get any cool effects like burning or freeze on the character
            // it goes on the model object of course
            CharacterModel characterModel = model.AddComponent<CharacterModel>();
            characterModel.body = bodyComponent;
            characterModel.baseRendererInfos = new CharacterModel.RendererInfo[]
            {
                // set up multiple rendererinfos if needed, but for this example there's only the one
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = model.GetComponentInChildren<SkinnedMeshRenderer>().material,
                    renderer = model.GetComponentInChildren<SkinnedMeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                }
            };

            characterModel.autoPopulateLightInfos = true;
            characterModel.invisibilityCount = 0;
            characterModel.temporaryOverlays = new List<TemporaryOverlay>();

            characterModel.mainSkinnedMeshRenderer = characterModel.baseRendererInfos[0].renderer.GetComponent<SkinnedMeshRenderer>();


            TeamComponent teamComponent = null;
            if (characterPrefab.GetComponent<TeamComponent>() != null) teamComponent = characterPrefab.GetComponent<TeamComponent>();
            else teamComponent = characterPrefab.GetComponent<TeamComponent>();
            teamComponent.hideAllyCardDisplay = false;
            teamComponent.teamIndex = TeamIndex.None;

            HealthComponent healthComponent = characterPrefab.GetComponent<HealthComponent>();
            healthComponent.health = 90f;
            healthComponent.shield = 0f;
            healthComponent.barrier = 0f;
            healthComponent.magnetiCharge = 0f;
            healthComponent.body = null;
            healthComponent.dontShowHealthbar = false;
            healthComponent.globalDeathEventChanceCoefficient = 1f;

            characterPrefab.GetComponent<Interactor>().maxInteractionDistance = 3f;
            characterPrefab.GetComponent<InteractionDriver>().highlightInteractor = true;

            // this disables ragdoll since the character's not set up for it, and instead plays a death animation
            CharacterDeathBehavior characterDeathBehavior = characterPrefab.GetComponent<CharacterDeathBehavior>();
            characterDeathBehavior.deathStateMachine = characterPrefab.GetComponent<EntityStateMachine>();
            characterDeathBehavior.deathState = new SerializableEntityStateType(typeof(GenericCharacterDeath));

            // edit the sfxlocator if you want different sounds
            SfxLocator sfxLocator = characterPrefab.GetComponent<SfxLocator>();
            sfxLocator.deathSound = Sounds.vileDie;
            sfxLocator.barkSound = "";
            sfxLocator.openSound = "";
            sfxLocator.landingSound = "Play_char_land";
            sfxLocator.fallDamageSound = "Play_char_land_fall_damage";
            sfxLocator.aliveLoopStart = "";
            sfxLocator.aliveLoopStop = "";

            Rigidbody rigidbody = characterPrefab.GetComponent<Rigidbody>();
            rigidbody.mass = 100f;
            rigidbody.drag = 0f;
            rigidbody.angularDrag = 0f;
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            rigidbody.interpolation = RigidbodyInterpolation.None;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rigidbody.constraints = RigidbodyConstraints.None;

            CapsuleCollider capsuleCollider = characterPrefab.GetComponent<CapsuleCollider>();
            capsuleCollider.isTrigger = false;
            capsuleCollider.material = null;
            capsuleCollider.center = new Vector3(0f, 0f, 0f);
            capsuleCollider.radius = 0.5f;
            capsuleCollider.height = 1.82f;
            capsuleCollider.direction = 1;

            KinematicCharacterMotor kinematicCharacterMotor = characterPrefab.GetComponent<KinematicCharacterMotor>();
            kinematicCharacterMotor.CharacterController = characterMotor;
            kinematicCharacterMotor.Capsule = capsuleCollider;
            kinematicCharacterMotor.Rigidbody = rigidbody;

            capsuleCollider.radius = 0.5f;
            capsuleCollider.height = 1.82f;
            capsuleCollider.center = new Vector3(0, 0, 0);
            capsuleCollider.material = null;

            kinematicCharacterMotor.DetectDiscreteCollisions = false;
            kinematicCharacterMotor.GroundDetectionExtraDistance = 0f;
            kinematicCharacterMotor.MaxStepHeight = 0.2f;
            kinematicCharacterMotor.MinRequiredStepDepth = 0.1f;
            kinematicCharacterMotor.MaxStableSlopeAngle = 55f;
            kinematicCharacterMotor.MaxStableDistanceFromLedge = 0.5f;
            kinematicCharacterMotor.PreventSnappingOnLedges = false;
            kinematicCharacterMotor.MaxStableDenivelationAngle = 55f;
            kinematicCharacterMotor.RigidbodyInteractionType = RigidbodyInteractionType.None;
            kinematicCharacterMotor.PreserveAttachedRigidbodyMomentum = true;
            kinematicCharacterMotor.HasPlanarConstraint = false;
            kinematicCharacterMotor.PlanarConstraintAxis = Vector3.up;
            kinematicCharacterMotor.StepHandling = StepHandlingMethod.None;
            kinematicCharacterMotor.LedgeHandling = true;
            kinematicCharacterMotor.InteractiveRigidbodyHandling = true;
            kinematicCharacterMotor.SafeMovement = false;

            // this sets up the character's hurtbox, kinda confusing, but should be fine as long as it's set up in unity right
            HurtBoxGroup hurtBoxGroup = model.AddComponent<HurtBoxGroup>();

            HurtBox componentInChildren = model.GetComponentInChildren<CapsuleCollider>().gameObject.AddComponent<HurtBox>();
            componentInChildren.gameObject.layer = LayerIndex.entityPrecise.intVal;
            componentInChildren.healthComponent = healthComponent;
            componentInChildren.isBullseye = true;
            componentInChildren.damageModifier = HurtBox.DamageModifier.Normal;
            componentInChildren.hurtBoxGroup = hurtBoxGroup;
            componentInChildren.indexInGroup = 0;

            hurtBoxGroup.hurtBoxes = new HurtBox[]
            {
                componentInChildren
            };

            hurtBoxGroup.mainHurtBox = componentInChildren;
            hurtBoxGroup.bullseyeCount = 1;

            //----------------------------------------------------------------------------------------------------------------
            //BurningDrive HitBox
            HitBoxGroup hitBoxGroup = model.AddComponent<HitBoxGroup>();

            GameObject GroundBox = new GameObject("GroundBox");
            GroundBox.transform.parent = childLocator.FindChild("GroundBox");
            GroundBox.transform.localPosition = new Vector3(0f, 0f, 0f);
            GroundBox.transform.localRotation = Quaternion.identity;
            GroundBox.transform.localScale = new Vector3(950f, 950f, 950f);

            HitBox hitBox = GroundBox.AddComponent<HitBox>();
            GroundBox.layer = LayerIndex.projectile.intVal;

            hitBoxGroup.hitBoxes = new HitBox[]
            {
                hitBox
            };

            hitBoxGroup.groupName = "GroundBox";

            //----------------------------------------------------------------------------------------------------------------------

            // this is for handling footsteps, not needed but polish is always good
            FootstepHandler footstepHandler = model.AddComponent<FootstepHandler>();
            footstepHandler.baseFootstepString = "Play_player_footstep";
            footstepHandler.sprintFootstepOverrideString = "";
            footstepHandler.enableFootstepDust = true;
            footstepHandler.footstepDustPrefab = Resources.Load<GameObject>("Prefabs/GenericFootstepDust");

            // ragdoll controller is a pain to set up so we won't be doing that here..
            RagdollController ragdollController = model.AddComponent<RagdollController>();
            ragdollController.bones = null;
            ragdollController.componentsToDisableOnRagdoll = null;

            // this handles the pitch and yaw animations, but honestly they are nasty and a huge pain to set up so i didn't bother
            AimAnimator aimAnimator = model.AddComponent<AimAnimator>();
            aimAnimator.inputBank = inputBankTest;
            aimAnimator.directionComponent = characterDirection;
            aimAnimator.pitchRangeMax = 55f;
            aimAnimator.pitchRangeMin = -50f;
            aimAnimator.yawRangeMin = -44f;
            aimAnimator.yawRangeMax = 44f;
            aimAnimator.pitchGiveupRange = 30f;
            aimAnimator.yawGiveupRange = 10f;
            aimAnimator.giveupDuration = 8f;


            //trying to add a passive
            LoadoutAPI.AddSkill(typeof(PassiveState));
            EntityStateMachine stateMachine = bodyComponent.GetComponent<EntityStateMachine>();
            stateMachine.mainStateType = new SerializableEntityStateType(typeof(PassiveState));

        }

        private void RegisterCharacter()
        {
            // now that the body prefab's set up, clone it here to make the display prefab
            characterDisplay = PrefabAPI.InstantiateClone(characterPrefab.GetComponent<ModelLocator>().modelBaseTransform.gameObject, "VileDisplay", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanXVile\\MegamanXVile\\MegamanXVile\\MegamanXVile.cs", "RegisterCharacter", 153);
            characterDisplay.AddComponent<NetworkIdentity>();

            // write a clean survivor description here!
            string desc = "Vile, the EX-Maverick Hunter.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Vile's Cherry Blast has a low start so use it after any skill for a momentary buff and faster start" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Vile is slow but powerful, try to use his skills to get out of trouble" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > When activated, Vile's Passive give him a life steal buff for 6 seconds, so try to cause the maximum damage possible" + Environment.NewLine + Environment.NewLine;
            //desc = desc + "< ! > Sample Text 4.</color>" + Environment.NewLine + Environment.NewLine;

            // add the language tokens
            LanguageAPI.Add("VILE_NAME", "Vile");
            LanguageAPI.Add("VILE_DESCRIPTION", desc);
            LanguageAPI.Add("VILE_SUBTITLE", "EX-Maverick-Hunter");

            // add our new survivor to the game~
            SurvivorDef survivorDef = new SurvivorDef
            {
                name = "VILE_NAME",
                unlockableName = "",
                descriptionToken = "VILE_DESCRIPTION",
                primaryColor = characterColor,
                bodyPrefab = characterPrefab,
                displayPrefab = characterDisplay
            };


            SurvivorAPI.AddSurvivor(survivorDef);

            // set up the survivor's skills here
            SkillSetup();

            // gotta add it to the body catalog too
            BodyCatalog.getAdditionalEntries += delegate (List<GameObject> list)
            {
                list.Add(characterPrefab);
            };
        }

        void SkillSetup()
        {
            // get rid of the original skills first, otherwise we'll have commando's loadout and we don't want that
            foreach (GenericSkill obj in characterPrefab.GetComponentsInChildren<GenericSkill>())
            {
                BaseUnityPlugin.DestroyImmediate(obj);
            }

            PassiveSetup();
            PrimarySetup();
            SecondarySetup();
            UtilitySetup();
            SpecialSetup();
        }

        void RegisterStates()
        {
            // register the entitystates for networking reasons
            LoadoutAPI.AddSkill(typeof(CherryBlast));
            LoadoutAPI.AddSkill(typeof(EletricSpark));
            LoadoutAPI.AddSkill(typeof(BumpityBoom));
            LoadoutAPI.AddSkill(typeof(BumpityBoom2));
            LoadoutAPI.AddSkill(typeof(BurningDrive));
            LoadoutAPI.AddSkill(typeof(FrontRunner));
            LoadoutAPI.AddSkill(typeof(CerberusPhantom));
            LoadoutAPI.AddSkill(typeof(ShotgunIce));
        }

        void PassiveSetup()
        {
            // set up the passive skill here if you want
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add("VILE_PASSIVE_NAME", "Passive");
            LanguageAPI.Add("VILE_PASSIVE_DESCRIPTION", "<style=cIsUtility>Vile won't give up that easily from a fight, moved by his anger he get stronger in a critical state.</style> <style=cIsHealing>When in low health vile gain 10 seconds of buffs</style>.");

            component.passiveSkill.enabled = true;
            component.passiveSkill.skillNameToken = "VILE_PASSIVE_NAME";
            component.passiveSkill.skillDescriptionToken = "VILE_PASSIVE_DESCRIPTION";
            component.passiveSkill.icon = Assets.iconP;
        }

        void PrimarySetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add("VILE_PRIMARY_NAME", "CherryBlast");
            LanguageAPI.Add("VILE_PRIMARY_DESCRIPTION", "Vile's gatling can fire super fast bullets after completely heated, dealing <style=cIsDamage>25% damage</style>.");

            // set up your primary skill def here!

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(CherryBlast));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.iconCB;
            mySkillDef.skillDescriptionToken = "VILE_PRIMARY_DESCRIPTION";
            mySkillDef.skillName = "VILE_PRIMARY_NAME";
            mySkillDef.skillNameToken = "VILE_PRIMARY_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            component.primary = characterPrefab.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            component.primary.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.primary.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };


            // add this code after defining a new skilldef if you're adding an alternate skill

            /*Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = newSkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(newSkillDef.skillNameToken, false, null)
            };*/
        }

        void SecondarySetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add("VILE_SECONDARY_NAME", "BumpityBoom");
            LanguageAPI.Add("VILE_SECONDARY_DESCRIPTION", "Vile throws two granades, dealing <style=cIsDamage>250% damage</style>.");

            // set up your primary skill def here!

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(BumpityBoom));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 3;
            mySkillDef.baseRechargeInterval = 7f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.iconBB;
            mySkillDef.skillDescriptionToken = "VILE_SECONDARY_DESCRIPTION";
            mySkillDef.skillName = "VILE_SECONDARY_NAME";
            mySkillDef.skillNameToken = "VILE_SECONDARY_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            component.secondary = characterPrefab.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            component.secondary.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.secondary.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };

            // alternate skill secondary 

            LanguageAPI.Add("VILE_SECONDARY_V_NAME", "Front Runner");
            LanguageAPI.Add("VILE_SECONDARY_V_DESCRIPTION", "A cannon shot that explodes on impact, dealing <style=cIsDamage>300% damage</style>.");

            // set up your primary skill def here!

            mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(FrontRunner));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 5;
            mySkillDef.baseRechargeInterval = 7;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.iconFR;
            mySkillDef.skillDescriptionToken = "VILE_SECONDARY_V_DESCRIPTION";
            mySkillDef.skillName = "VILE_SECONDARY_V_NAME";
            mySkillDef.skillNameToken = "VILE_SECONDARY_V_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);


            // add this code after defining a new skilldef if you're adding an alternate skill



            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }

        void UtilitySetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add("VILE_UTILITY_NAME", "Electric Shock Round");
            LanguageAPI.Add("VILE_UTILITY_DESCRIPTION", "Fire an eletric bomb, dealing <style=cIsDamage>1000% damage</style> and paralize enemies for 5s.");

            // set up your primary skill def here!

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(EletricSpark));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 10f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.iconES;
            mySkillDef.skillDescriptionToken = "VILE_UTILITY_DESCRIPTION";
            mySkillDef.skillName = "VILE_UTILITY_NAME";
            mySkillDef.skillNameToken = "VILE_UTILITY_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            component.utility = characterPrefab.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            component.utility.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.utility.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };

            // alternate skill Special

            LanguageAPI.Add("VILE_UTILITY_V_NAME", "Shotgun Ice");
            LanguageAPI.Add("VILE_UTILITY_V_DESCRIPTION", "A powerful ice shot that cause <style=cIsDamage>400% damage</style> and freezing the enemies.");

            // set up your primary skill def here!

            mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ShotgunIce));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 2;
            mySkillDef.baseRechargeInterval = 8.5f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.iconSI;
            mySkillDef.skillDescriptionToken = "VILE_UTILITY_V_DESCRIPTION";
            mySkillDef.skillName = "VILE_UTILITY_V_NAME";
            mySkillDef.skillNameToken = "VILE_UTILITY_V_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);


            // add this code after defining a new skilldef if you're adding an alternate skill

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }

        void SpecialSetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add("VILE_SPECIAL_NAME", "Burning Drive");
            LanguageAPI.Add("VILE_SPECIAL_DESCRIPTION", "Create a powerful ball of flame using nearby oxygen as fuel, dealing <style=cIsDamage>1000% damage</style>.");

            // set up your primary skill def here!

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(BurningDrive));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 8f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.iconBD;
            mySkillDef.skillDescriptionToken = "VILE_SPECIAL_DESCRIPTION";
            mySkillDef.skillName = "VILE_SPECIAL_NAME";
            mySkillDef.skillNameToken = "VILE_SPECIAL_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            component.special = characterPrefab.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            component.special.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.special.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };

            // alternate skill Special

            LanguageAPI.Add("VILE_SPECIAL_V_NAME", "Cerberus Phantom");
            LanguageAPI.Add("VILE_SPECIAL_V_DESCRIPTION", "Shoot a spread of 3 lasers, dealing <style=cIsDamage>250% damage</style>.");

            // set up your primary skill def here!

            mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(CerberusPhantom));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 2;
            mySkillDef.baseRechargeInterval = 8;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.iconCP;
            mySkillDef.skillDescriptionToken = "VILE_SPECIAL_V_DESCRIPTION";
            mySkillDef.skillName = "VILE_SPECIAL_V_NAME";
            mySkillDef.skillNameToken = "VILE_SPECIAL_V_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);


            // add this code after defining a new skilldef if you're adding an alternate skill

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }

        private void CreateDoppelganger()
        {
            // set up the doppelganger for artifact of vengeance here
            // quite simple, gets a bit more complex if you're adding your own ai, but commando ai will do

            doppelganger = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterMasters/CommandoMonsterMaster"), "VileMonsterMaster", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanXVile\\MegamanXVile\\MegamanXVile\\MegamanXVile.cs", "CreateDoppelganger", 159);

            MasterCatalog.getAdditionalEntries += delegate (List<GameObject> list)
            {
                list.Add(doppelganger);
            };

            CharacterMaster component = doppelganger.GetComponent<CharacterMaster>();
            component.bodyPrefab = characterPrefab;
        }
    }



    // get the assets from your assetbundle here
    // if it's returning null, check and make sure you have the build action set to "Embedded Resource" and the file names are right because it's not gonna work otherwise
    public static class Assets
    {
        public static AssetBundle MainAssetBundle = null;
        public static AssetBundleResourcesProvider Provider;

        public static Texture charPortrait;

        public static GameObject BurningDriveVFX;
        public static GameObject RedEyeVFX;

        public static Sprite iconP;
        public static Sprite iconCB;
        public static Sprite iconBB;
        public static Sprite iconES;
        public static Sprite iconBD;
        public static Sprite iconSI;
        public static Sprite iconFR;
        public static Sprite iconCP;

        public static void PopulateAssets()
        {
            if (MainAssetBundle == null)
            {
                using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MegamanXVile.megamanxvilebundle"))
                {
                    MainAssetBundle = AssetBundle.LoadFromStream(assetStream);
                    Provider = new AssetBundleResourcesProvider("@MegamanXVile", MainAssetBundle);
                }
            }

            // include this if you're using a custom soundbank
            using (Stream manifestResourceStream2 = Assembly.GetExecutingAssembly().GetManifestResourceStream("MegamanXVile.VileSB.bnk"))
            {
                byte[] array = new byte[manifestResourceStream2.Length];
                manifestResourceStream2.Read(array, 0, array.Length);
                SoundAPI.SoundBanks.Add(array);
            }



            // and now we gather the assets
            charPortrait = MainAssetBundle.LoadAsset<Sprite>("Vile_Icon").texture;

            iconP = MainAssetBundle.LoadAsset<Sprite>("Vile_Icon");
            iconCB = MainAssetBundle.LoadAsset<Sprite>("SkillIconCB");
            iconBB = MainAssetBundle.LoadAsset<Sprite>("SkillIconBB");
            iconES = MainAssetBundle.LoadAsset<Sprite>("SkillIconES");
            iconBD = MainAssetBundle.LoadAsset<Sprite>("SkillIconBD");
            iconSI = MainAssetBundle.LoadAsset<Sprite>("SkillIconSI");
            iconFR = MainAssetBundle.LoadAsset<Sprite>("SkillIconFR");
            iconCP = MainAssetBundle.LoadAsset<Sprite>("SkillIconCP");


            BurningDriveVFX = Assets.LoadEffect("MagicFireBig", "");

            RedEyeVFX = Assets.LoadEffect("RedEye", "");


        }

        private static GameObject LoadEffect(string resourceName, string soundName)
        {
            GameObject newEffect = MainAssetBundle.LoadAsset<GameObject>(resourceName);

            newEffect.AddComponent<DestroyOnTimer>().duration = 12;
            newEffect.AddComponent<NetworkIdentity>();
            newEffect.AddComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Always;
            var effect = newEffect.AddComponent<EffectComponent>();
            effect.applyScale = false;
            effect.effectIndex = EffectIndex.Invalid;
            effect.parentToReferencedTransform = true;
            effect.positionAtReferencedTransform = true;
            effect.soundName = soundName;

            EffectAPI.AddEffect(newEffect);

            return newEffect;
        }

    }
}


public static class Sounds
{
    public static readonly string vileAttack = "Call_Vile_Attack";
    public static readonly string vilePassive = "Call_Vile_Passive";
    public static readonly string vileDie = "Call_Vile_Die";
    public static readonly string vileCherryBlast = "Call_Vile_Cherry_Blast";
    public static readonly string vileCerberusPhantom = "Call_Vile_Cerberus_Phantom";

}



namespace EntityStates.ExampleSurvivorStates
{
    public class PassiveState : GenericCharacterMain
    {
        public float Timer = 5f;
        public float ChillTime;
        public float ChillDelay = 1.5f;
        public float PassiveTimer = 0f;
        public bool isHeated;
        public float HeatTime = 5f;
        public float baseDuration = 1f;
        public double MinHP;
        public static bool isCrit;

        private float duration;
        private Animator animator;
        public override void OnEnter()
        {
            base.OnEnter();

        }
        public override void OnExit()
        {
            base.OnExit();
        }

        
        public override void Update()
        {
            base.Update();

            ChillTime += Time.deltaTime;

            if (base.inputBank.skill1.justReleased)
                ChillDelay = 0.1f;


            if(ChillTime >= 2f)
            {
                if (ChillDelay >= 1.5f - (base.attackSpeedStat / 10))
                    ChillDelay = 1.5f - (base.attackSpeedStat / 10);
                else
                    ChillDelay += 0.25f;

                ChillTime = 0f;
            }

            CherryBlast.Chilldelay = ChillDelay;

        }
        

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            Timer += Time.fixedDeltaTime;

            if (base.inputBank.skill2.justReleased || base.inputBank.skill3.justReleased || base.inputBank.skill4.justReleased)
            {
                Timer = 0f;
            }

            if (Timer <= HeatTime)
                CherryBlast.heat = true;
            else
            {
                CherryBlast.heat = false;
                CherryBlast.buffSkillIndex = 0;
            }

            if (base.inputBank.skill2.justReleased && (Timer <= HeatTime))
                CherryBlast.buffSkillIndex = 1;

            if (base.inputBank.skill3.justReleased && (Timer <= HeatTime))
                CherryBlast.buffSkillIndex = 2;

            if (base.inputBank.skill4.justReleased && (Timer <= HeatTime))
                CherryBlast.buffSkillIndex = 3;

            //-------PASSIVE EFFECT

            PassiveTimer -= Time.fixedDeltaTime;

            MinHP = 0.35 + (base.characterBody.level / 200);
            if (base.characterBody.healthComponent.combinedHealthFraction < MinHP && PassiveTimer < 5f)
            {
                Util.PlaySound(Sounds.vilePassive, base.gameObject);
                EffectManager.SimpleMuzzleFlash(MegamanXVileSurvivor.Assets.RedEyeVFX, base.gameObject, "EYE", true);
                //base.healthComponent.AddBarrierAuthority(base.characterBody.healthComponent.fullHealth / 2f);
                if (NetworkServer.active)
                {
                    base.characterBody.AddTimedBuff(BuffIndex.LifeSteal, 6f);
                    base.characterBody.AddTimedBuff(BuffIndex.FullCrit, 10f);
                    base.characterBody.AddTimedBuff(BuffIndex.Warbanner, 10f);
                    base.characterBody.AddTimedBuff(BuffIndex.NoCooldowns, 3f);
                }
                PassiveTimer = 50f;

            }

            //------------------------ Check Crit because passive interfer on the primary skill

            if (base.characterBody.HasBuff(BuffIndex.FullCrit))
                isCrit = true;
            else
            isCrit = Util.CheckRoll(base.critStat, base.characterBody.master);

            return;

        }

        public static bool GetHeat()
        {
            PassiveState VP = new PassiveState();


            bool heat = VP.isHeated;

            return heat;
        }


        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}

