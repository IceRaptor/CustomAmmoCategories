﻿/*  
 *  This file is part of CustomUnits.
 *  CustomUnits is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using BattleTech;
using CustAmmoCategories;
using HarmonyLib;
using HBS.Collections;
using IRBTModUtils;
using Localize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace CustomUnits {
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch("WalkSpeed")]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_WalkSpeed {
    public static void Postfix(Mech __instance, ref float __result) {
      try {
        if (__instance is FakeVehicleMech vehicle) {
          __result = vehicle.CruiseSpeed;
          //Log.TWL(0, "FakeVehicleMech.WalkSpeed " + __instance.MechDef.Description.Id+" "+__result);
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch("RunSpeed")]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_RunSpeed {
    public static void Postfix(Mech __instance, ref float __result) {
      try {
        if (__instance is FakeVehicleMech vehicle) {
          __result = vehicle.FlankSpeed;
          //Log.TWL(0, "FakeVehicleMech.RunSpeed " + __instance.MechDef.Description.Id + " " + __result);
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  public class FakeVehicleMech: CustomMech, ICustomMech{
    public static List<ArmorLocation> locations = new List<ArmorLocation>() { ArmorLocation.Head, ArmorLocation.LeftLeg, ArmorLocation.RightLeg, ArmorLocation.LeftArm, ArmorLocation.RightArm };
    public FakeVehicleMech(MechDef mDef, PilotDef pilotDef, TagSet additionalTags, string UID, CombatGameState combat, string spawnerId, HeraldryDef customHeraldryDef)
      : base(mDef, pilotDef, additionalTags, UID, combat, spawnerId, customHeraldryDef) {
    }
    public override bool _MoveMultiplierOverride { get { return true; } }
    public override float _MoveMultiplier { get { return 1f; } }
    public override bool IsDead {
      get {
        if (this.HasHandledDeath) { return true; }
        if (this.pilot.IsIncapacitated) { return true; }
        if (this.pilot.HasEjected) { return true; }
        foreach (ArmorLocation alocation in FakeVehicleMech.locations) {
          ChassisLocations location = MechStructureRules.GetChassisLocationFromArmorLocation(alocation);
          LocationDef locDef = this.MechDef.Chassis.GetLocationDef(location);
          if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
          if (this.GetCurrentStructure(location) <= Core.Epsilon) { return true; }
        }
        return false;
      }
    }
    public override List<int> GetPossibleHitLocations(AbstractActor attacker) {
      Dictionary<ArmorLocation, int> unitHitTable = this.GetHitTable(this.IsProne ? AttackDirection.ToProne : this.Combat.HitLocation.GetAttackDirection(attacker.CurrentPosition, (ICombatant)this));
      if (unitHitTable == null)
        return (List<int>)null;
      HashSet<int> result = new HashSet<int>();
      foreach (var loc in unitHitTable) {
        if (loc.Value > 0)
          result.Add((int)loc.Key);
      }
      ArmorLocation specialLocation = ArmorLocation.None;
      UnitCustomInfo info = this.GetCustomInfo();
      if ((info != null) && (info.customStructure.is_empty)) { info.customStructure = CustomStructureDef.Search(DefaultStructureDef); }
      if ((info != null) && (info.customStructure.is_empty == false)) { specialLocation = info.customStructure.ClusterSpecialLocation; }
      if ((this.CanBeHeadShot == false) && (result.Contains((int)specialLocation))) { result.Remove((int)specialLocation); }
      return result.ToList();
    }
    public override Dictionary<ArmorLocation, int> GetHitTable(AttackDirection from) {
      string specialHitTable = Thread.CurrentThread.peekFromStack<string>(CustomAmmoCategories.SPECIAL_HIT_TABLE_NAME);
      if (string.IsNullOrEmpty(specialHitTable)) { specialHitTable = "default"; }
      if (GetHitTable_cache.TryGetValue(specialHitTable, out var GetHitTable_cache_sp) == false) {
        GetHitTable_cache_sp = new Dictionary<AttackDirection, Dictionary<ArmorLocation, int>>();
        GetHitTable_cache.Add(specialHitTable, GetHitTable_cache_sp);
      }
      if (GetHitTable_cache_sp.TryGetValue(from, out var result)) { return result; }
      Log.TWL(0, $"FakeVehcile.GetHitTable {this.PilotableActorDef.ChassisID} table name:{specialHitTable} attack direction:{from}");
      UnitCustomInfo info = this.GetCustomInfo();
      result = new Dictionary<ArmorLocation, int>();
      Dictionary<ArmorLocation, int> hittable = null;
      if (info == null) { goto call_native; }
      if (info.customStructure.is_empty) { info.customStructure = CustomStructureDef.Search(DefaultStructureDef); }
      if (info.customStructure.is_empty) { goto call_native; }
      CustomHitTableDef hittabledef = null;
      Log.WL(1, $"searching hittable:{specialHitTable} in {info.customStructure.Id}");
      if (info.customStructure.tables.TryGetValue(specialHitTable, out hittabledef) == false) {
        hittabledef = null;
        var fallbackStructure = CustomStructureDef.Search(this.DefaultStructureDef);
        Log.WL(1, $"searching hittable:{specialHitTable} in {fallbackStructure.Id}");
        if (fallbackStructure.tables.TryGetValue(specialHitTable, out hittabledef)) {
          Log.WL(2, "found");
          goto process_hittable;
        }
        Log.WL(1, $"searching hittable:default in {info.customStructure.Id}");
        if (info.customStructure.tables.TryGetValue("default", out hittabledef) == false) {
          hittabledef = null;
          Log.WL(1, $"searching hittable:default in {fallbackStructure.Id}");
          if (fallbackStructure.tables.TryGetValue("default", out hittabledef) == false) {
            hittabledef = null;
            goto call_native;
          } else {
            Log.WL(2, "found");
          }
        } else {
          Log.WL(2, "found");
        }
      } else {
        Log.WL(2, "found");
      }
    process_hittable:
      if (hittabledef == null) { goto call_native; }
      if (hittabledef.HitTable.TryGetValue(from, out hittable) == false) {
        goto call_native;
      }
      if (hittable == null) { goto call_native; }
      goto return_result;
    call_native:
      AttackDirection effectiveFrom = from;
      if (from == AttackDirection.ToProne) { effectiveFrom = AttackDirection.FromArtillery; };
      Log.WL(1, $"fallback");
      Dictionary<VehicleChassisLocations, int> vres = this.Combat.HitLocation.GetVehicleHitTable(effectiveFrom);
      hittable = new Dictionary<ArmorLocation, int>();
      foreach (var vloc in vres) { hittable.Add(vloc.Key.toFakeArmor(), vloc.Value); }
    return_result:
      Log.W(1, $"result: ");
      foreach (var loc in hittable) {
        LocationDef locationDef = this.MechDef.Chassis.GetLocationDef(MechStructureRules.GetChassisLocationFromArmorLocation(loc.Key));
        if ((locationDef.MaxArmor <= 0f) && (locationDef.InternalStructure <= 1f)) { continue; }
        result.Add(loc.Key, loc.Value);
        Log.W(1, $"{loc.Key}:{loc.Value}");
      }
      Log.WL(0, "");
      GetHitTable_cache_sp.Add(from, result);
      return (result.Count > 0) ? result : null;
    }
    public override int GetHitLocation(AbstractActor attacker, Vector3 attackPosition, float hitLocationRoll, int calledShotLocation, float bonusMultiplier) {
      AttackDirection attackDirection = this.Combat.HitLocation.GetAttackDirection(attackPosition, this);
      if (attackDirection == AttackDirection.FromBack && this.StatCollection.GetValue<bool>("GuaranteeNextBackHit")) {
        this.StatCollection.Set<bool>("GuaranteeNextBackHit", false);
        return (int)(VehicleChassisLocations.Rear.toFakeArmor());
      }
      Dictionary<ArmorLocation, int> hitTable = this.GetHitTable(this.IsProne ? AttackDirection.ToProne : this.Combat.HitLocation.GetAttackDirection(attackPosition, this));
      ArmorLocation specialLocation = ArmorLocation.None;
      UnitCustomInfo info = this.GetCustomInfo();
      if ((info != null) && (info.customStructure.is_empty)) { info.customStructure = CustomStructureDef.Search(DefaultStructureDef); }
      if ((info != null) && (info.customStructure.is_empty == false)) { specialLocation = info.customStructure.ClusterSpecialLocation; }
      if ((this.CanBeHeadShot == false) && (hitTable.ContainsKey(specialLocation))) { hitTable.Remove(specialLocation); }
      Thread.CurrentThread.pushActor(this);
      int result = (int)(hitTable != null ? HitLocation.GetHitLocation<ArmorLocation>(hitTable, hitLocationRoll, (ArmorLocation)calledShotLocation, bonusMultiplier) : ArmorLocation.None);
      Thread.CurrentThread.clearActor();
      Log.TW(0, "FakeVehicleMech.GetHitLocation "+this.PilotableActorDef.ChassisID+" attacker:"+attacker.PilotableActorDef.ChassisID+" hitTable:");
      foreach(var ht in hitTable) {
        Log.W(1,ht.Key+"="+ht.Value);
      }
      Log.WL(1,"result:"+((ArmorLocation)result));
      return result;
      //return (int)this.Combat.HitLocation.GetHitLocation(attackPosition, this, hitLocationRoll, ((ArmorLocation)calledShotLocation), bonusMultiplier).toFakeArmor();
    }
    public override int GetAdjacentHitLocation(Vector3 attackPosition, float randomRoll, int previousHitLocation, float originalMultiplier = 1f, float adjacentMultiplier = 1f) {
      return base.GetAdjacentHitLocation(attackPosition, randomRoll, previousHitLocation, originalMultiplier, adjacentMultiplier);
    }
    private static HashSet<ArmorLocation> DFASelfDamageLocations = new HashSet<ArmorLocation>() { VehicleChassisLocations.Front.toFakeArmor(), VehicleChassisLocations.Rear.toFakeArmor(), VehicleChassisLocations.Left.toFakeArmor(), VehicleChassisLocations.Right.toFakeArmor() };
    public override HashSet<ArmorLocation> GetDFASelfDamageLocations() {
      return DFASelfDamageLocations;
    }
    private static HashSet<ArmorLocation> LandmineDamageArmorLocations = new HashSet<ArmorLocation>() { VehicleChassisLocations.Front.toFakeArmor(), VehicleChassisLocations.Rear.toFakeArmor(), VehicleChassisLocations.Left.toFakeArmor(), VehicleChassisLocations.Right.toFakeArmor() };
    public override HashSet<ArmorLocation> GetLandmineDamageArmorLocations() {
      return LandmineDamageArmorLocations;
    }
    private static HashSet<ArmorLocation> BurnDamageArmorLocations_Turret = new HashSet<ArmorLocation>() { VehicleChassisLocations.Front.toFakeArmor(), VehicleChassisLocations.Rear.toFakeArmor(), VehicleChassisLocations.Left.toFakeArmor(), VehicleChassisLocations.Right.toFakeArmor(), VehicleChassisLocations.Turret.toFakeArmor() };
    private static HashSet<ArmorLocation> BurnDamageArmorLocations_TurretLess = new HashSet<ArmorLocation>() { VehicleChassisLocations.Front.toFakeArmor(), VehicleChassisLocations.Rear.toFakeArmor(), VehicleChassisLocations.Left.toFakeArmor(), VehicleChassisLocations.Right.toFakeArmor() };
    public override HashSet<ArmorLocation> GetBurnDamageArmorLocations() {
      LocationDef def = this.MechDef.Chassis.GetLocationDef(VehicleChassisLocations.Turret.toFakeChassis());
      if ((def.MaxArmor == 0f) && (def.InternalStructure <= 1f)) { return BurnDamageArmorLocations_TurretLess; }
      return BurnDamageArmorLocations_Turret;
    }
    public override Dictionary<int, float> GetAOESpreadArmorLocations() {
      return CustomAmmoCategories.FakeVehicleLocations;
    }
    public override void _ApplyArmorStatDamage(ArmorLocation location, float damage, WeaponHitInfo hitInfo) {
      this.statCollection.ModifyStat<float>(hitInfo.attackerId, hitInfo.stackItemUID, this.GetStringForArmorLocation(location), StatCollection.StatOperation.Float_Subtract, damage);
      this.OnArmorDamaged((int)location, hitInfo, damage);
      var specialLocations = new HashSet<ArmorLocation>() { ArmorLocation.None };
      UnitCustomInfo info = this.GetCustomInfo();
      if (info != null) { specialLocations = new HashSet<ArmorLocation>() { (ArmorLocation)info.MechVehicleCrewLocation }; }
      if (specialLocations.Contains(location)) {
        this.pilot.SetNeedsInjury(InjuryReason.HeadHit);
      }
    }
    public override List<int> GetAOEPossibleHitLocations(Vector3 attackPos) {
      Dictionary<ArmorLocation, int> vehicleHitTable = this.GetHitTable(this.Combat.HitLocation.GetAttackDirection(attackPos, (ICombatant)this));
      if (vehicleHitTable == null) return (List<int>)null;
      List<int> intList = new List<int>();
      foreach (ArmorLocation key in vehicleHitTable.Keys) {
        ChassisLocations chassisLoc = MechStructureRules.GetChassisLocationFromArmorLocation(key);
        LocationDef location = this.MechDef.Chassis.GetLocationDef(chassisLoc);
        if ((location.MaxArmor <= 0f) && (location.InternalStructure <= 1f)) { continue; }
        intList.Add((int)key);
      }
      return intList;
    }
    public override Text GetLongArmorLocation(ArmorLocation location) {
      return new Text(Vehicle.GetLongChassisLocation(location.toFakeVehicleChassis()));
    }
    private static Dictionary<ArmorLocation, ArmorLocation> GetAdjacentLocations_cache = new Dictionary<ArmorLocation, ArmorLocation>();
    public override ArmorLocation GetAdjacentLocations(ArmorLocation location) {
      UnitCustomInfo info = this.GetCustomInfo();
      var result = ArmorLocation.None;
      if ((info != null) && (info.customStructure.is_empty)) {
        info.customStructure = CustomStructureDef.Search(DefaultStructureDef);
      }
      if ((info != null) && (info.customStructure.is_empty == false)) {
        if (info.customStructure.AdjacentLocations.TryGetValue(location, out result)) {
          return result;
        }
      }
      //if (GetAdjacentLocations_cache.TryGetValue(location, out ArmorLocation result)) { return result; }
      switch (location.toFakeVehicleChassis()) {
        case VehicleChassisLocations.Turret:
          result = VehicleChassisLocations.Rear.toFakeArmor() | VehicleChassisLocations.Right.toFakeArmor() | VehicleChassisLocations.Left.toFakeArmor() | VehicleChassisLocations.Front.toFakeArmor();
        break;
        case VehicleChassisLocations.Front:
        case VehicleChassisLocations.Rear:
          result = VehicleChassisLocations.Turret.toFakeArmor() | VehicleChassisLocations.Left.toFakeArmor() | VehicleChassisLocations.Right.toFakeArmor();
        break;
        case VehicleChassisLocations.Left:
        case VehicleChassisLocations.Right:
          result = VehicleChassisLocations.Turret.toFakeArmor() | VehicleChassisLocations.Front.toFakeArmor() | VehicleChassisLocations.Rear.toFakeArmor();
        break;
        default:
        return VehicleChassisLocations.None.toFakeArmor();
      }
      //GetAdjacentLocations_cache.Add(location, result);
      return result;
    }
    //private Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>> GetClusterTable_cache = new Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>();
    public override string DefaultStructureDef { get { return "vehicle"; } }
    public override Dictionary<ArmorLocation, int> GetClusterTable(ArmorLocation originalLocation, Dictionary<ArmorLocation, int> hitTable) {
      //if (GetClusterTable_cache.TryGetValue(originalLocation, out Dictionary<ArmorLocation, int> result)) { return result; }
      ArmorLocation specialLocation = ArmorLocation.None;
      UnitCustomInfo info = this.GetCustomInfo();
      if ((info != null) && (info.customStructure.is_empty)) { info.customStructure = CustomStructureDef.Search(DefaultStructureDef); }
      if ((info != null) && (info.customStructure.is_empty == false)) { specialLocation = info.customStructure.ClusterSpecialLocation; }
      ArmorLocation adjacentLocations = this.GetAdjacentLocations(originalLocation);
      Dictionary<ArmorLocation, int> dictionary = new Dictionary<ArmorLocation, int>();
      foreach (KeyValuePair<ArmorLocation, int> keyValuePair in hitTable) {
        if (keyValuePair.Key != specialLocation || !this.Combat.Constants.ToHit.ClusterChanceNeverClusterHead || originalLocation == specialLocation) {
          if (keyValuePair.Key == specialLocation && this.Combat.Constants.ToHit.ClusterChanceNeverMultiplyHead)
            dictionary.Add(keyValuePair.Key, keyValuePair.Value);
          else if (originalLocation == keyValuePair.Key)
            dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)this.Combat.Constants.ToHit.ClusterChanceOriginalLocationMultiplier));
          else if ((adjacentLocations & keyValuePair.Key) == keyValuePair.Key)
            dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)this.Combat.Constants.ToHit.ClusterChanceAdjacentMultiplier));
          else
            dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)this.Combat.Constants.ToHit.ClusterChanceNonadjacentMultiplier));
        }
      }
      //GetClusterTable_cache.Add(originalLocation, dictionary);
      return dictionary;
    }
    //private Dictionary<AttackDirection, Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>> GetClusterHitTable_cache = new Dictionary<AttackDirection, Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>>();
    public override Dictionary<ArmorLocation, int> GetHitTableCluster(AttackDirection from, ArmorLocation originalLocation) {
      string specialHitTable = Thread.CurrentThread.peekFromStack<string>(CustomAmmoCategories.SPECIAL_HIT_TABLE_NAME);
      if (string.IsNullOrEmpty(specialHitTable)) { specialHitTable = "default"; }
      if (GetClusterHitTable_cache.TryGetValue(specialHitTable, out var GetClusterHitTable_cache_sp) == false) {
        GetClusterHitTable_cache_sp = new Dictionary<AttackDirection, Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>>();
        GetClusterHitTable_cache.Add(specialHitTable, GetClusterHitTable_cache_sp);
      }
      if (GetClusterHitTable_cache_sp.TryGetValue(from, out var clusterTables) == false) {
        clusterTables = new Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>();
        //GetClusterHitTable_cache.Add(from, clusterTables);
      }
      if (clusterTables.TryGetValue(originalLocation, out Dictionary<ArmorLocation, int> result)) {
        return result;
      }
      Dictionary<ArmorLocation, int> hitTable = this.GetHitTable(from);
      CustomAmmoCategoriesLog.Log.AIM.TW(0, $"Generating cluster {specialHitTable} table {from} location:{originalLocation.toFakeVehicleChassis()} based on:");
      foreach (var hit in hitTable) { CustomAmmoCategoriesLog.Log.AIM?.W(1, $"{hit.Key.toFakeVehicleChassis()}:{hit.Value}"); }
      CustomAmmoCategoriesLog.Log.AIM.WL(0, "");
      result = GetClusterTable(originalLocation, hitTable);
      clusterTables.Add(originalLocation, result);
      if (GetClusterHitTable_cache_sp.ContainsKey(from) == false) {
        CustomAmmoCategoriesLog.Log.AIM.WL(1, $"adding to cache as {from}");
        GetClusterHitTable_cache_sp.Add(from, clusterTables);
      }
      this.DumpClusterTableCache(CustomAmmoCategoriesLog.Log.AIM);
      return result;
    }
    public override bool isSquad { get { return false; } }
    public override bool isVehicle { get { return true; } }
    public float CruiseSpeed => this.statCollection.GetValue<float>(nameof(CruiseSpeed));
    public float FlankSpeed => this.statCollection.GetValue<float>(nameof(FlankSpeed));
    public override void ApplyBraced() {}
    public override bool CanBeEntrenched { get { return false; } }
    public override string UnitTypeNameDefault { get { return "VEHICLE"; } }

    protected override void InitStats() {
      Log.TWL(0, "FakeVehicleMech.InitStats");
      this.statCollection.AddStatistic<float>("CruiseSpeed", this.MovementCaps.MaxWalkDistance);
      this.statCollection.AddStatistic<float>("FlankSpeed", this.MovementCaps.MaxSprintDistance);
      base.InitStats();
    }
  }
}