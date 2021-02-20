using System;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using R2API;
using R2API.Utils;
using EntityStates;
using EntityStates.ExampleSurvivorStates;
using RoR2;
using RoR2.Skills;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using KinematicCharacterController;
using System.IO;
using static R2API.SoundAPI;
using R2API.AssetPlus;
using AK.Wwise;
using System.Collections;
using System.Text;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;

namespace MegamanXVile.Materials
{
    public static class Skins
    {
        public static Material commandoMat;

        public static SkinDef CreateSkinDef(string skinName, Sprite skinIcon, CharacterModel.RendererInfo[] rendererInfos, SkinnedMeshRenderer mainRenderer, GameObject root, string unlockName)
        {
            LoadoutAPI.SkinDefInfo skinDefInfo = new LoadoutAPI.SkinDefInfo
            {
                BaseSkins = Array.Empty<SkinDef>(),
                GameObjectActivations = new SkinDef.GameObjectActivation[0],
                Icon = skinIcon,
                MeshReplacements = new SkinDef.MeshReplacement[0],
                MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0],
                Name = skinName,
                NameToken = skinName,
                ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0],
                RendererInfos = rendererInfos,
                RootObject = root,
                UnlockableName = unlockName
            };

            SkinDef skin = LoadoutAPI.CreateNewSkinDef(skinDefInfo);

            return skin;
        }

        public static SkinDef CreateSkinDef(string skinName, Sprite skinIcon, CharacterModel.RendererInfo[] rendererInfos, SkinnedMeshRenderer mainRenderer, GameObject root, string unlockName, Mesh skinMesh)
        {
            LoadoutAPI.SkinDefInfo skinDefInfo = new LoadoutAPI.SkinDefInfo
            {
                BaseSkins = Array.Empty<SkinDef>(),
                GameObjectActivations = new SkinDef.GameObjectActivation[0],
                Icon = skinIcon,
                MeshReplacements = new SkinDef.MeshReplacement[]
                {
                    new SkinDef.MeshReplacement
                    {
                        renderer = mainRenderer,
                        mesh = skinMesh
                    }
                },
                MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0],
                Name = skinName,
                NameToken = skinName,
                ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0],
                RendererInfos = rendererInfos,
                RootObject = root,
                UnlockableName = unlockName
            };

            SkinDef skin = LoadoutAPI.CreateNewSkinDef(skinDefInfo);

            return skin;
        }

        public static Material CreateMaterial(string materialName)
        {
            return CreateMaterial(materialName, 0);
        }

        public static Material CreateMaterial(string materialName, float emission)
        {
            return CreateMaterial(materialName, emission, Color.black);
        }

        public static Material CreateMaterial(string materialName, float emission, Color emissionColor)
        {
            return CreateMaterial(materialName, emission, emissionColor, 0);
        }

        public static Material CreateMaterial(string materialName, float emission, Color emissionColor, float normalStrength)
        {
            if (!commandoMat) commandoMat = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial;

            Material mat = UnityEngine.Object.Instantiate<Material>(commandoMat);
            Material tempMat = MegamanXVileSurvivor.Assets.MainAssetBundle.LoadAsset<Material>(materialName);
            if (!tempMat)
            {
                return commandoMat;
            }

            mat.name = materialName;
            mat.SetColor("_Color", tempMat.GetColor("_Color"));
            mat.SetTexture("_MainTex", tempMat.GetTexture("_MainTex"));
            mat.SetColor("_EmColor", emissionColor);
            mat.SetFloat("_EmPower", emission);
            mat.SetTexture("_EmTex", tempMat.GetTexture("_EmissionMap"));
            mat.SetFloat("_NormalStrength", normalStrength);

            return mat;
        }

        public static void RegisterSkins()
        {
            GameObject bodyPrefab = MegamanXVileSurvivor.MegamanXVile.characterPrefab;

            GameObject model = bodyPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            ModelSkinController skinController = model.AddComponent<ModelSkinController>();
            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            SkinnedMeshRenderer mainRenderer = characterModel.mainSkinnedMeshRenderer;
            //SkinnedMeshRenderer mainRenderer = Reflection.GetFieldValue<SkinnedMeshRenderer>(characterModel, "mainSkinnedMeshRenderer");

            List<SkinDef> skinDefs = new List<SkinDef>();

            #region DefaultSkin
            CharacterModel.RendererInfo[] defaultRenderers = characterModel.baseRendererInfos;
            SkinDef defaultSkin = CreateSkinDef("VILE_DEFAULT_SKIN", MegamanXVileSurvivor.Assets.iconP, defaultRenderers, mainRenderer, model, "");
            defaultSkin.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = mainRenderer.sharedMesh,
                    renderer = defaultRenderers[0].renderer
                },
            };

            skinDefs.Add(defaultSkin);
            #endregion

            skinController.skins = skinDefs.ToArray();
        }
    }
}
