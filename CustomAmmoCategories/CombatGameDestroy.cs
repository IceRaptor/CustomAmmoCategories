﻿using BattleTech;
using CustAmmoCategories;
using CustAmmoCategoriesPatches;
using CustomAmmoCategoriesLog;
using Harmony;
using System;
using System.Threading;

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(CombatGameState))]
  [HarmonyPatch("OnCombatGameDestroyed")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatGameState_OnCombatGameDestroyed {
    public static bool Prefix(CombatGameState __instance) {
      Log.M.TWL(0,"CombatGameState.OnCombatGameDestroyed");
      try {
        WeaponStatCacheHelper.Clear();
        TempAmmoCountHelper.Clear();
        foreach (var ae in CustomAmmoCategories.additinalImpactEffects) {
          foreach (var sp in ae.Value) {
            try { sp.CleanupSelf(); } finally { };
          }
        }
        CustomAmmoCategories.additinalImpactEffects.Clear();
        AMSWeaponEffectStaticHelper.Clear();
        if ((DynamicMapHelper.asyncTerrainDesignMask.ThreadState != ThreadState.Aborted)
          && (DynamicMapHelper.asyncTerrainDesignMask.ThreadState == ThreadState.AbortRequested)) {
          DynamicMapHelper.asyncTerrainDesignMask.Abort();
        }
        DynamicMapHelper.ClearTerrain();
        MineFieldHelper.registredMovingDamage.Clear();
        BTCustomRenderer_DrawDecals.Clear();
        DynamicTreesHelper.Clean();
        CACDynamicTree.allCACTrees.Clear();
        CustomAmmoCategories.Settings.DontShowBurnedTreesTemporary = false;
        DynamicMapHelper.Combat = __instance;
        DynamicMapHelper.PoolDelayedGameObject();
        CombatHUDMechwarriorTray_InitAbilityButtons.attackGroundAbilities.Clear();
        CustomAmmoCategories.terrainHitPositions.Clear();
        DeferredEffectHelper.Clear();
        PersistentFloatieHelper.Clear();
        ASWatchdog.EndWatchDogThread();
        Weapon_InternalAmmo.Clear();
        BraceNode_Tick.Clear();
        AreaOfEffectHelper.Clear();
      } catch(Exception e) {
        Log.M.TWL(0, e.ToString());
      }
      return true;
    }
    public static void Postfix(CombatGameState __instance) {
      Log.M.TWL(0, "CombatGameState.OnCombatGameDestroyed");
      //CustomAmmoCategories.clearAllWeaponEffects();
    }
  }
}