﻿/*  
 *  This file is part of CustomAmmoCategories.
 *  CustomAmmoCategories is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using BattleTech;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustAmmoCategories;
using Random = UnityEngine.Random;
using UnityEngine;
using CustomAmmoCategoriesLog;
using Localize;
using WeaponRealizer;


//This part of code is modified code of original WeaponRealizer by Joel Meador under MIT LICENSE

namespace CustAmmoCategories {
  public static class StatisticHelper {
    public static Statistic GetOrCreateStatisic<StatisticType>(this StatCollection collection, string statName, StatisticType defaultValue) {
      Statistic statistic = collection.GetStatistic(statName);
      if (statistic == null) {
        statistic = collection.AddStatistic<StatisticType>(statName, defaultValue);
      }
      return statistic;
    }
  }
  public class JammMessage {
    public AbstractActor actor;
    public string message;
    public JammMessage(AbstractActor act, string msg) {
      actor = act;
      message = msg;
    }
  }
  public class AMSJammInfoMessage {
    public Weapon weapon { get; set; } = null;
    public float chance { get; set; } = 0f;
    public string description { get; set; } = string.Empty;
    public bool damage { get; private set; }
    public bool destroy { get; private set; }
    public int cooldown { get; private set; }
    public AMSJammInfoMessage(Weapon w) {
      this.weapon = w;
      this.chance = weapon.FlatJammingChance(out string descr);
      this.description = descr;
      this.damage = weapon.DamageOnJamming();
      this.destroy = weapon.DestroyOnJamming();
      this.cooldown = weapon.Cooldown();
    }
  }
  public static partial class CustomAmmoCategories {
    private static Queue<JammMessage> jammQueue { get; set; } = new Queue<JammMessage>();
    public static Queue<AMSJammInfoMessage> jammAMSQueue { get; set; } = new Queue<AMSJammInfoMessage>();
    public static Queue<AmmunitionBox> ammoExposionQueue { get; set; } = new Queue<AmmunitionBox>();
    public static readonly string AmmoBoxGUID = "CACAmmoBoxGUID";
    public static void ClearJammingInfo() {
      jammQueue.Clear();
      jammAMSQueue.Clear();
      ammoExposionQueue.Clear();
    }
    public static void AddToExposionCheck(AmmunitionBox box) {
      if (CustomAmmoCategories.Settings.AmmoCanBeExhausted == false) { return; };
      if (box == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("AddToExposionCheck " + box.defId + " is not ammo box\n");
        return;
      }
      if (box.IsFunctional == false) {
        CustomAmmoCategoriesLog.Log.LogWrite("AddToExposionCheck " + box.defId + " is not functional\n");
      }
      ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(box.ammoDef.Description.Id);
      if (extAmmo.AmmoCategory.Index == CustomAmmoCategories.NotSetCustomAmmoCategoty.Index) {
        CustomAmmoCategoriesLog.Log.LogWrite("AddToExposionCheck " + box.defId + " has no ammo category\n");
        return;
      }
      if (extAmmo.CanBeExhaustedAt < CustomAmmoCategories.Epsilon) {
        CustomAmmoCategoriesLog.Log.LogWrite("AddToExposionCheck " + box.ammoDef.Description.Id + " not exposing\n");
        return;
      }
      ammoExposionQueue.Enqueue(box);
    }
    public static void createAmmoBoxGUID(AmmunitionBox box, string GUID = "") {
      if (box.StatCollection.ContainsStatistic(CustomAmmoCategories.AmmoBoxGUID) == false) {
        if (string.IsNullOrEmpty(GUID)) { GUID = Guid.NewGuid().ToString(); }
        box.StatCollection.AddStatistic<string>(CustomAmmoCategories.AmmoBoxGUID, GUID);
      }
    }
    public static string getAmmoBoxGUID(AmmunitionBox box) {
      if (box.StatCollection.ContainsStatistic(CustomAmmoCategories.AmmoBoxGUID) == false) {
        string GUID = Guid.NewGuid().ToString();
        CustomAmmoCategories.createAmmoBoxGUID(box, GUID);
        return GUID;
      }
      return box.StatCollection.GetStatistic(CustomAmmoCategories.AmmoBoxGUID).Value<string>();
    }
    public static void prosessExposion() {
      while (CustomAmmoCategories.ammoExposionQueue.Count > 0) {
        AmmunitionBox ammoBox = CustomAmmoCategories.ammoExposionQueue.Dequeue();
        HashSet<string> checkedGUIDs = new HashSet<string>();
        try {
          if (ammoBox.IsFunctional == false) {
            CustomAmmoCategoriesLog.Log.LogWrite("prosessExposion " + ammoBox.defId + " is not functional\n");
          }
          ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(ammoBox.ammoDef.Description.Id);
          if (extAmmo.AmmoCategory.Index == CustomAmmoCategories.NotSetCustomAmmoCategoty.Index) {
            CustomAmmoCategoriesLog.Log.LogWrite("prosessExposion " + ammoBox.defId + " has no ammo category\n");
            continue;
          }
          if (extAmmo.CanBeExhaustedAt < CustomAmmoCategories.Epsilon) {
            CustomAmmoCategoriesLog.Log.LogWrite("prosessExposion " + ammoBox.ammoDef.Description.Id + " not exposing\n");
            continue;
          }
          string GUID = CustomAmmoCategories.getAmmoBoxGUID(ammoBox);
          if (checkedGUIDs.Contains(GUID)) {
            CustomAmmoCategoriesLog.Log.LogWrite("prosessExposion GUID " + GUID + " already checked\n");
            continue;
          }
          checkedGUIDs.Add(GUID);
          float curAmmo = (float)ammoBox.CurrentAmmo / (float)ammoBox.AmmoCapacity;
          if (curAmmo < extAmmo.CanBeExhaustedAt) {
            float rollBorder = (extAmmo.CanBeExhaustedAt - curAmmo) / extAmmo.CanBeExhaustedAt;
            float exposedRoll = Random.Range(0f, 1f);
            CustomAmmoCategoriesLog.Log.LogWrite("roll " + exposedRoll + " " + rollBorder + "\n");
            if (exposedRoll <= rollBorder) {
              ammoBox.StatCollection.Set<ComponentDamageLevel>("DamageLevel", ComponentDamageLevel.Destroyed);
              ammoBox.parent.Combat.MessageCenter.PublishMessage(
                  new AddSequenceToStackMessage(
                      new ShowActorInfoSequence(ammoBox.parent, new Text("__/CAC.AMMOBOXEXHAUSTED/__", ammoBox.UIName), FloatieMessage.MessageNature.Debuff, true)));
            }
          } else {
            CustomAmmoCategoriesLog.Log.LogWrite("prosessExposion curAmmo " + curAmmo + " is not less than border " + extAmmo.CanBeExhaustedAt + "\n");
          }
        } catch (Exception e) {
          CustomAmmoCategoriesLog.Log.LogWrite("prosessExposion exception " + e.ToString() + "\n", true);
        }
      }
    }
    public static void prosessJummingMessages() {
      while (jammQueue.Count > 0) {
        JammMessage message = jammQueue.Dequeue();
        CustomAmmoCategoriesLog.Log.LogWrite("Publishing jamm message for " + message.actor.DisplayName + " " + message.message + "\n");
        message.actor.Combat.MessageCenter.PublishMessage(
            new AddSequenceToStackMessage(
                new ShowActorInfoSequence(message.actor, message.message, FloatieMessage.MessageNature.Debuff, true)));
      }
      prosessExposion();
    }
    public static void addJamMessage(AbstractActor actor, string message) {
      jammQueue.Enqueue(new JammMessage(actor, message));
    }
  }

  [HarmonyPatch(typeof(AbstractActor), "OnActivationEnd", MethodType.Normal)]
  public static class unJammingEnabler {
    public static bool Prefix(AbstractActor __instance) {
      var actor = __instance;
      CustomAmmoCategories.ClearEjection(actor);
      foreach (Weapon weapon in actor.Weapons) {
        //ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
        //if (extWeapon.IsAMS == true) { CustomAmmoCategories.setWeaponAMSShootsCount(weapon, 0); };
        if (weapon.roundsSinceLastFire <= 0) {
          weapon.setCantAMSFire(true);
          continue;
        };
        weapon.setCantAMSFire(false);
        if (CustomAmmoCategories.IsCooldown(weapon) > 0) {
          var removedJam = CustomAmmoCategories.AttemptToRemoveCooldown(actor, weapon);
          CustomAmmoCategoriesLog.Log.LogWrite($"Removed cooldown? {removedJam}\n");
        }
      }
      if (actor.IsShutDown) return true;

      foreach (Weapon weapon in actor.Weapons) {
        if (weapon.roundsSinceLastFire <= 0) { continue; };
        if (CustomAmmoCategories.IsJammed(weapon)) {
          var removedJam = CustomAmmoCategories.AttemptToRemoveJam(actor, weapon);
          CustomAmmoCategoriesLog.Log.LogWrite($"Removed Jam? {removedJam}\n");
        }
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("HasAmmo")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class JammingRealizer {
    public static string CantFireReason(this Weapon weapon) {
      string result = string.Empty;
      if (weapon.IsFunctional == false) { result+="DESTROYED;"; }
      if (weapon.StatCollection.GetValue<bool>("TemporarilyDisabled")) { result+="TEMP.DISABLED;"; }
      if (weapon.IsDisabled) { result += "DISABLED;"; }
      if (weapon.IsJammed()) { result += "JAMMED;"; }
      if (weapon.isWRJammed()) { result += "JAMMED;"; }
      if (weapon.IsCooldown() > 0) { result += "COOLDOWN;"; }
      if (weapon.isAMS() && weapon.isCantAMSFire()) { result += "USED AS WEAPON;"; }
      if ((weapon.isAMS() == false) && weapon.isCantNormalFire()) { result += "USED AS AMS;"; };
      if (weapon.info().isCurrentModeAvailable() == false) { return "MODE IS LOCKED;"; };
      if (weapon.mode().Disabeld) { return "MODE IS DISABLED;"; };
      if (weapon.isBlocked()) { return "BLOCKED;"; };
      if (weapon.isWeaponBlockedStat()) { return "BLOCKED;"; };
      if ((weapon.ammo().AmmoCategory.BaseCategory.Is_NotSet == false) && (weapon.CurrentAmmo <= 0)) { return "OUT OF AMMO;"; }
      if (weapon.IsEnabled == false) { return "NOT ENABLED;"; }
      return result;
      //return "UNKNOWN";
    }
    public static void Postfix(Weapon __instance, ref bool __result) {
      if (__result == false) { return; }
      if (__instance.IsJammed()) { __result = false; }
      if (__instance.isWRJammed()) { __result = false; }
      if (__instance.IsCooldown() > 0) { __result = false; }
      if (__instance.isAMS() && __instance.isCantAMSFire()) { __result = false; };
      if ((__instance.isAMS() == false) && __instance.isCantNormalFire()) { __result = false; };
      if (__instance.info().isCurrentModeAvailable() == false) { __result = false; };
      if (__instance.mode().Disabeld) { __result = false; };
      if (__instance.isBlocked()) { __result = false; };
      if (__instance.isWeaponBlockedStat()) { __result = false; };
    }
  }
  [HarmonyPatch(typeof(AttackDirector))]
  [HarmonyPatch("OnAttackComplete")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class JammingEnabler {
    //private static Dictionary<Weapon, JammInfo> normalJammProcess = new Dictionary<Weapon, JammInfo>();
    //private static Dictionary<Weapon, JammInfo> amsJammProcess = new Dictionary<Weapon, JammInfo>();
    //public static void normalJammInfoUpdate(this Weapon weapon) {
    //  if(normalJammProcess.TryGetValue(weapon, out JammInfo jammInfo) == false) {
    //    normalJammProcess.Add(weapon, new JammInfo(weapon, false));
    //  }
    //}
    //public static void amsJammInfoUpdate(this Weapon weapon) {
    //  if (amsJammProcess.TryGetValue(weapon, out JammInfo jammInfo) == false) {
    //    amsJammProcess.Add(weapon, new JammInfo(weapon, true));
    //  }
    //}
    //public static void clearNormalJammInfo() {
    //  normalJammProcess.Clear();
    //}
    //public static void clearAMSJammInfo() {
    //  amsJammProcess.Clear();
    //}
    //public static bool getAMSJammInfo(this Weapon weapon,out JammInfo info) {
    //  if(amsJammProcess.TryGetValue(weapon, out info)) {
    //    amsJammProcess.Remove(weapon);
    //    return true;
    //  }
    //  info = null;
    //  return false;
    //}
    //public static bool getNormalJammInfo(this Weapon weapon, out JammInfo info) {
    //  if (normalJammProcess.TryGetValue(weapon, out info)) {
    //    normalJammProcess.Remove(weapon);
    //    return true;
    //  }
    //  info = null;
    //  return false;
    //}
    public static bool Prefix(AttackDirector __instance, MessageCenterMessage message) {
      AttackCompleteMessage attackCompleteMessage = (AttackCompleteMessage)message;
      int sequenceId = attackCompleteMessage.sequenceId;
      AttackDirector.AttackSequence attackSequence = __instance.GetAttackSequence(sequenceId);
      if (attackSequence == null) {
        return true;
      }
      //JammingEnabler.jammQueue.Enqueue(attackSequence.attacker);
      //AbstractActor actor = attackSequence.target as AbstractActor;
      //JammingEnabler.jammAMS();
      //JammingEnabler.jamm(attackSequence.attacker);
      return true;
    }
    public static void jammAMS() {
      Log.M.TWL(0,"Jamming AMS sequence");
      while (CustomAmmoCategories.jammAMSQueue.Count > 0) {
        AMSJammInfoMessage jammInfo = CustomAmmoCategories.jammAMSQueue.Dequeue();
        Weapon weapon = jammInfo.weapon;
        int shootsCount = weapon.AMSShootsCount();
        Log.LogWrite(" " + weapon.parent.DisplayName + ":" + weapon.parent.GUID + ":" + weapon.UIName + " shots:" + shootsCount + "\n");
        if (shootsCount <= 0) {
          CustomAmmoCategoriesLog.Log.LogWrite($" AMS was idle\n");
          continue;
        }
        weapon.AMSShootsCount(0);
        weapon.AMSActivationsCount(0);
        weapon.setCantNormalFire(true);
        float flatJammingChance = jammInfo.chance;//weapon.FlatJammingChance(out string descr);
        string descr = jammInfo.description;
        Log.M.WL(2,"flatJammingChance " + flatJammingChance);
        if (flatJammingChance > CustomAmmoCategories.Epsilon) {
          Log.M.WL(2, "Try jamm weapon " + weapon.UIName);
          float Roll = Random.Range(0.0f, 1.0f);
          Log.M.WL(1, "Jamming chance " + flatJammingChance + " roll " + Roll);
          if (Roll < flatJammingChance) {
            Log.M.WL(1, "Jammed!");
            CustomAmmoCategories.AddJam(weapon.parent, weapon, jammInfo.damage, jammInfo.destroy);
          }
        }
        if (jammInfo.cooldown > 0) {
          weapon.Cooldown(jammInfo.cooldown, false);
        }
      }
    }
    public static void jammWeapon(this Weapon weapon, JammInfo info) {
      Log.M.TWL(1, "Try jamm weapon of " + weapon.parent.PilotableActorDef.Description.Id);
      float flatJammingChance = info.chance;
      Log.M.WL(2, "chance:" + flatJammingChance + " damage:" + info.damage + " destroy:" + info.destroy + " description:"+info.description);
      if (flatJammingChance > CustomAmmoCategories.Epsilon) {
        Log.M.WL(2, "Try jamm weapon " + weapon.UIName);
        float Roll = Random.Range(0.0f, 1.0f);
        Log.M.WL(2, "Jamming chance " + flatJammingChance + " roll " + Roll);
        if (Roll < flatJammingChance) {
          Log.M.WL(2, "Jammed!");
          CustomAmmoCategories.AddJam(weapon.parent, weapon, info.damage, info.destroy);
        }
      }
      if (info.cooldown > 0) {
        weapon.Cooldown(info.cooldown, true);
      }
    }
    //public static void jamm(AbstractActor actor) {
    //  Log.M.TWL(0, "Try jamm weapon");
    //  //while (JammingEnabler.jammQueue.Count > 0) {
    //  //AbstractActor actor = JammingEnabler.jammQueue.Dequeue();
    //  Log.M.WL(1, "Try jamm weapon of " + actor.PilotableActorDef.Description.Id);
    //  foreach (Weapon weapon in actor.Weapons) {
    //    Log.M.WL(2, "weapon " + weapon.UIName + " rounds since last fire " + weapon.roundsSinceLastFire);
    //    if (weapon.roundsSinceLastFire > 0) {
    //      continue;
    //    }
    //    float flatJammingChance = weapon.FlatJammingChance(out string descr);
    //    Log.M.WL(2, "flatJammingChance " + flatJammingChance);
    //    if (flatJammingChance > CustomAmmoCategories.Epsilon) {
    //      Log.M.WL(2, "Try jamm weapon " + weapon.UIName);
    //      float Roll = Random.Range(0.0f, 1.0f);
    //      Log.M.WL(2, "Jamming chance " + flatJammingChance + " roll " + Roll);
    //      if (Roll < flatJammingChance) {
    //        Log.M.WL(2, "Jammed!");
    //        CustomAmmoCategories.AddJam(actor, weapon);
    //      }
    //    }
    //    if (weapon.Cooldown() > 0) {
    //      weapon.Cooldown(weapon.Cooldown(), true);
    //    }
    //  }
    //  //}
    //}
    //public static void jamm(AbstractActor actor) {
    //  Log.M.TWL(0, "Try jamm weapon");
    //  //while (JammingEnabler.jammQueue.Count > 0) {
    //  //AbstractActor actor = JammingEnabler.jammQueue.Dequeue();
    //  Log.M.WL(1, "Try jamm weapon of " + actor.PilotableActorDef.Description.Id);
    //  foreach (Weapon weapon in actor.Weapons) {
    //    Log.M.WL(2, "weapon " + weapon.UIName + " rounds since last fire " + weapon.roundsSinceLastFire);
    //    if (weapon.roundsSinceLastFire > 0) {
    //      continue;
    //    }
    //    float flatJammingChance = weapon.FlatJammingChance(out string descr);
    //    Log.M.WL(2, "flatJammingChance " + flatJammingChance);
    //    if (flatJammingChance > CustomAmmoCategories.Epsilon) {
    //      Log.M.WL(2, "Try jamm weapon " + weapon.UIName);
    //      float Roll = Random.Range(0.0f, 1.0f);
    //      Log.M.WL(2, "Jamming chance " + flatJammingChance + " roll " + Roll);
    //      if (Roll < flatJammingChance) {
    //        Log.M.WL(2, "Jammed!");
    //        CustomAmmoCategories.AddJam(actor, weapon);
    //      }
    //    }
    //    if (weapon.Cooldown() > 0) {
    //      weapon.Cooldown(weapon.Cooldown(), true);
    //    }
    //  }
    //  //}
    //}
    public static void Postfix(AttackDirector __instance, MessageCenterMessage message) {
      System.Threading.Timer timer = null;
      timer = new System.Threading.Timer((obj) => {
        CustomAmmoCategories.prosessJummingMessages();
        timer.Dispose();
      }, null, 1500, System.Threading.Timeout.Infinite);
    }
  }
  //[HarmonyPatch(typeof(Weapon))]
  //[HarmonyPatch("InitStats")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { })]
  public static class Weapon_InitStatsJamm {
    public static void Postfix(Weapon __instance) {
      Log.LogWrite("Weapon.InitStats " + __instance.defId + ":" + __instance.parent.GUID + "\n");
      __instance.FlatJammChanceStat(0f);
      __instance.ModJammChanceStat(1f);
      __instance.WeaponBlockedStat(false);
      __instance.RegisterStatCollection();
    }
  }
  public static partial class CustomAmmoCategories {
    public static string JammedWeaponStatisticName = "CAC-JammedWeapon";
    public static string CooldownWeaponStatisticName = "CAC-CooldownWeapon";
    public static string NoNormalFireStatisticName = "CAC-NoNormalFire";
    public static string NoAMSFireStatisticName = "CAC-AMSFire";
    public static string FlatJammingChanceStatisticName = "CACFlatJammingChance";
    public static string ModJammingChanceStatisticName = "CACModJammingChance";
    public static string BlockedStatisticName = "CACWeaponBlocked";
    //public static string TemporarilyDisabledStatisticName = "TemporarilyDisabled";
    public static float Epsilon = 0.001f;
    public static float FlatJammChance(this ICombatant unit) {
      Statistic stat = unit.StatCollection.GetStatistic(FlatJammingChanceStatisticName);
      if (stat == null) { return 0f; }
      return stat.Value<float>();
    }
    public static void FlatJammChance(this ICombatant unit, float val) {
      if (unit.StatCollection.ContainsStatistic(FlatJammingChanceStatisticName) == false) {
        unit.StatCollection.AddStatistic<float>(FlatJammingChanceStatisticName, val);
      } else {
        unit.StatCollection.Set<float>(FlatJammingChanceStatisticName, val);
      }
    }
    public static float FlatJammChanceStat(this Weapon weapon) {
      Statistic stat = weapon.StatCollection.GetStatistic(FlatJammingChanceStatisticName);
      if (stat == null) { return 0f; }
      return stat.Value<float>();
    }
    public static float ModJammChanceStat(this Weapon weapon) {
      Statistic stat = weapon.StatCollection.GetStatistic(ModJammingChanceStatisticName);
      if (stat == null) { return 0f; }
      return stat.Value<float>();
    }
    public static void FlatJammChanceStat(this Weapon weapon, float val) {
      if (weapon.StatCollection.ContainsStatistic(FlatJammingChanceStatisticName) == false) {
        weapon.StatCollection.AddStatistic<float>(FlatJammingChanceStatisticName, val);
      } else {
        weapon.StatCollection.Set<float>(FlatJammingChanceStatisticName, val);
      }
    }
    public static void WeaponBlockedStat(this Weapon weapon, bool val) {
      if (weapon.StatCollection.ContainsStatistic(BlockedStatisticName) == false) {
        weapon.StatCollection.AddStatistic<bool>(BlockedStatisticName, val);
      } else {
        weapon.StatCollection.Set<bool>(BlockedStatisticName, val);
      }
    }
    public static bool isWeaponBlockedStat(this Weapon weapon) {
      return weapon.info().extDef.CanBeBlocked && weapon.StatCollection.GetOrCreateStatisic<bool>(BlockedStatisticName, false).Value<bool>();
    }
    public static void ModJammChanceStat(this Weapon weapon, float val) {
      if (weapon.StatCollection.ContainsStatistic(ModJammingChanceStatisticName) == false) {
        weapon.StatCollection.AddStatistic<float>(ModJammingChanceStatisticName, val);
      } else {
        weapon.StatCollection.Set<float>(ModJammingChanceStatisticName, val);
      }
    }
    public static bool isCantNormalFire(this Weapon weapon) {
      var statistic = StatisticHelper.GetOrCreateStatisic<bool>(weapon.StatCollection, NoNormalFireStatisticName, false);
      return statistic.Value<bool>();
    }
    public static bool isCantAMSFire(this Weapon weapon) {
      var statistic = StatisticHelper.GetOrCreateStatisic<bool>(weapon.StatCollection, NoAMSFireStatisticName, false);
      return statistic.Value<bool>();
    }
    public static void setCantNormalFire(this Weapon weapon, bool value) {
      if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.NoNormalFireStatisticName) == false) {
        weapon.StatCollection.AddStatistic<bool>(CustomAmmoCategories.NoNormalFireStatisticName, value);
      } else {
        weapon.StatCollection.Set<bool>(CustomAmmoCategories.NoNormalFireStatisticName, value);
      }
    }
    public static void setCantAMSFire(this Weapon weapon, bool value) {
      if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.NoAMSFireStatisticName) == false) {
        weapon.StatCollection.AddStatistic<bool>(CustomAmmoCategories.NoAMSFireStatisticName, value);
      } else {
        weapon.StatCollection.Set<bool>(CustomAmmoCategories.NoAMSFireStatisticName, value);
      }
    }
    public static void AddJam(AbstractActor actor, Weapon weapon, bool damage, bool destroy) {
      //bool damage = weapon.DamageOnJamming();
      //bool destroy = weapon.DestroyOnJamming();
      try {
        Log.M.TWL(0, "AddJamm " + new Text(actor.DisplayName).ToString() + " weapon:" + weapon.defId + " damage:" + damage + " destroy:" + destroy);
        if ((damage == false) && (destroy == false)) {
          if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.JammedWeaponStatisticName) == false) {
            weapon.StatCollection.AddStatistic<bool>(CustomAmmoCategories.JammedWeaponStatisticName, false);
          }
          weapon.StatCollection.Set<bool>(CustomAmmoCategories.JammedWeaponStatisticName, true);
          //weapon.StatCollection.Set<bool>(CustomAmmoCategories.TemporarilyDisabledStatisticName, true);
          if (CustomAmmoCategories.Settings.DontShowNotDangerouceJammMessages == false) {
            CustomAmmoCategories.addJamMessage(actor, $"{weapon.UIName} __/CAC.Jammed/__!");
          }
          //actor.Combat.MessageCenter.PublishMessage(
          //    new AddSequenceToStackMessage(
          //        new ShowActorInfoSequence(actor, $"{weapon.Name} Jammed!", FloatieMessage.MessageNature.Debuff,
          //            true)));
        } else {
          //var isDestroying = weapon.DamageLevel != ComponentDamageLevel.Functional;
          //if (destroy == true) { isDestroying = true; };
          //var damageLevel = isDestroying ? ComponentDamageLevel.Destroyed : ComponentDamageLevel.Penalized;
          WeaponHitInfo fakeHit = new WeaponHitInfo(-1, -1, -1, -1, weapon.parent.GUID, weapon.parent.GUID, 1, null, null, null, null, null, null, null, null, null, null, null);
          fakeHit.toHitRolls = new float[1] { 1.0f };
          fakeHit.locationRolls = new float[1] { 1.0f };
          fakeHit.dodgeRolls = new float[1] { 0.0f };
          fakeHit.dodgeSuccesses = new bool[1] { false };
          fakeHit.hitLocations = new int[1] { weapon.Location };
          fakeHit.hitVariance = new int[1] { 0 };
          fakeHit.hitQualities = new AttackImpactQuality[1] { AttackImpactQuality.Solid };
          fakeHit.attackDirections = new AttackDirection[1] { AttackDirection.FromArtillery };
          fakeHit.hitPositions = new Vector3[1] { weapon.parent.GameRep.GetHitPosition(weapon.Location) };
          fakeHit.secondaryTargetIds = new string[1] { null };
          fakeHit.secondaryHitLocations = new int[1] { 0 };
          Log.M.WL(1, $"CritComponent destroy:{destroy}");
          MechComponent sourceComponent = weapon.info().currentModeSource();
          if (sourceComponent != null) {
            sourceComponent.CritComponent(ref fakeHit, weapon, destroy);
          } else {
            weapon.CritComponent(ref fakeHit, weapon, destroy);
          }
          //weapon.DamageComponent(fakeHit, damageLevel, true);
          var message = weapon.DamageLevel == ComponentDamageLevel.Destroyed
              ? $"{weapon.UIName} __/CAC.misfire/__: __/CAC.Destroyed/__!"
              : $"{weapon.UIName} __/CAC.misfire/__: __/CAC.Damaged/__!";
          CustomAmmoCategories.addJamMessage(actor, message);
          //actor.Combat.MessageCenter.PublishMessage(
          //    new AddSequenceToStackMessage(
          //        new ShowActorInfoSequence(actor, message, FloatieMessage.MessageNature.Debuff, true)));
        }
      }catch(Exception e) {
        Log.M?.TWL(0,e.ToString(),true);
      }
    }
    public static void Cooldown(this Weapon weapon, int rounds, bool message) {
      if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.CooldownWeaponStatisticName) == false) {
        weapon.StatCollection.AddStatistic<int>(CustomAmmoCategories.CooldownWeaponStatisticName, 0);
      }
      if (rounds > 0) {
        weapon.StatCollection.Set<int>(CustomAmmoCategories.CooldownWeaponStatisticName, rounds);
        //weapon.StatCollection.Set<bool>(CustomAmmoCategories.TemporarilyDisabledStatisticName, true);
        if (message) {
          if (CustomAmmoCategories.Settings.DontShowNotDangerouceJammMessages == false) {
            CustomAmmoCategories.addJamMessage(weapon.parent, $"{weapon.UIName} __/CAC.Cooldown/__!");
          }
        }
      }
    }
    public static bool IsJammed(this Weapon weapon) {
      var statistic = StatisticHelper.GetOrCreateStatisic<bool>(weapon.StatCollection, JammedWeaponStatisticName, false);
      return statistic.Value<bool>();
    }
    public static int IsCooldown(this Weapon weapon) {
      var statistic = StatisticHelper.GetOrCreateStatisic<int>(weapon.StatCollection, CooldownWeaponStatisticName, 0);
      return statistic.Value<int>();
    }
    public static float MovedHexes(this AbstractActor unit) {
      return Mathf.Ceil(unit.DistMovedThisRound / CustomAmmoCategories.Settings.HexSizeForMods);
    }
    public static float GetRefireModifier(this Weapon weapon) {
      if (weapon.RefireModifier > 0 && weapon.roundsSinceLastFire < 2)
        return (float)weapon.RefireModifier;
      return 0.0f;
    }

    public static float FlatJammingChance(this Weapon weapon, out string description) {
      StringBuilder descr = new StringBuilder();
      ExtWeaponDef def = weapon.exDef();
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      float result = weapon.FlatJammChanceStat();
      result += weapon.parent.FlatJammChance();
      result += def.FlatJammingChance;
      result += ammo.FlatJammingChance;
      result += mode.FlatJammingChance;
      result += (def.RecoilJammingChance + ammo.RecoilJammingChance + mode.RecoilJammingChance) * weapon.GetRefireModifier();
      float mult = 0;
      float baseval = 0f;
      mult += ammo.GunneryJammingMult;
      if (ammo.GunneryJammingBase > 0f) { baseval = ammo.GunneryJammingBase; };
      mult += def.GunneryJammingMult;
      if ((def.GunneryJammingBase > 0f) && (baseval == 0f)) { baseval = def.GunneryJammingBase; };
      mult += mode.GunneryJammingMult;
      if ((mode.GunneryJammingBase > 0f) && (baseval == 0f)) { baseval = mode.GunneryJammingBase; };
      float evasiveModifier = 1f;
      float hexesModifier = 1f;
      if (weapon.parent != null) {
        if (weapon.parent.EvasivePipsCurrent > 0) {
          float evasiveMod = def.evasivePipsMods.FlatJammingChance + ammo.evasivePipsMods.FlatJammingChance + mode.evasivePipsMods.FlatJammingChance;
          if (Mathf.Abs(evasiveMod) > CustomAmmoCategories.Epsilon) evasiveModifier = Mathf.Pow((float)weapon.parent.EvasivePipsCurrent, evasiveMod);
        }
        if(weapon.parent.DistMovedThisRound > CustomAmmoCategories.Settings.HexSizeForMods) {
          float hexesMod = def.hexesMovedMod.FlatJammingChance + ammo.hexesMovedMod.FlatJammingChance + mode.hexesMovedMod.FlatJammingChance;
          if (Mathf.Abs(hexesMod) > CustomAmmoCategories.Epsilon) hexesModifier = Mathf.Pow((float)weapon.parent.MovedHexes(), hexesMod);
        }
      }
      result *= evasiveModifier;
      result *= hexesModifier;
      if (weapon.parent != null) {
        if (baseval == 0f) { baseval = 5f; }
        result += ((baseval - weapon.parent.SkillGunnery) * mult);
      }
      result *= weapon.ModJammChanceStat();
      //descr.Append("JAMM CHANCE = "+ result+" = ((");
      //descr.Append(def.FlatJammingChance >= 0f ? def.FlatJammingChance.ToString() : ("-" + def.FlatJammingChance));
      //descr.Append("(w)");
      //descr.Append(weapon.FlatJammChanceStat() >= 0f?"+"+weapon.FlatJammChanceStat():("-"+ weapon.FlatJammChanceStat()));
      //descr.Append("(st)");
      //descr.Append(ammo.FlatJammingChance >= 0f ? "+" + ammo.FlatJammingChance : ("-" + ammo.FlatJammingChance));
      //descr.Append("(a)");
      //descr.Append(mode.FlatJammingChance >= 0f ? "+" + mode.FlatJammingChance : ("-" + mode.FlatJammingChance));
      //descr.Append("(m)");
      //descr.Append(weapon.parent.FlatJammChance() >= 0f ? "+" + weapon.parent.FlatJammChance() : ("-" + weapon.parent.FlatJammChance()));
      //descr.Append("(u))x("+ evasiveModifier + "(evasive)+"+ hexesModifier + "(hexes))+(");
      //descr.Append(baseval >= 0f ? baseval.ToString() : ("-" + baseval));
      //descr.Append(weapon.parent.SkillGunnery >= 0f ? ("-" + weapon.parent.SkillGunnery) : ("+" + weapon.parent.SkillGunnery));
      //descr.Append("(sk))x(");
      //descr.Append(def.GunneryJammingMult >= 0f ? def.GunneryJammingMult.ToString() : ("-" + def.GunneryJammingMult));
      //descr.Append("(w)");
      //descr.Append(ammo.GunneryJammingMult >= 0f ? "+"+ammo.GunneryJammingMult : ("-" + ammo.GunneryJammingMult));
      //descr.Append("(a)");
      //descr.Append(mode.GunneryJammingMult >= 0f ? "+"+mode.GunneryJammingMult : ("-" + mode.GunneryJammingMult));
      //descr.Append("(m))) x ("+weapon.ModJammChanceStat()+") = "+result);
      descr.Append($"JAMM CHANCE: {Mathf.Round(result*1000f)/10f}%");
      description = descr.ToString();
      return result;
    }
    public static bool DamageOnJamming(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.DamageOnJamming != TripleBoolean.NotSet) { return mode.DamageOnJamming == TripleBoolean.True; }
      return weapon.exDef().DamageOnJamming == TripleBoolean.True;
    }
    public static bool DestroyOnJamming(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.DestroyOnJamming != TripleBoolean.NotSet) { return mode.DestroyOnJamming == TripleBoolean.True; }
      return weapon.exDef().DestroyOnJamming == TripleBoolean.True;
    }
    public static int Cooldown(this Weapon weapon) {
      return weapon.exDef().Cooldown + weapon.mode().Cooldown;
    }
    public static bool AttemptToRemoveJam(AbstractActor actor, Weapon weapon) {
      var skill = actor.SkillGunnery;
      var mitigationRoll = Random.Range(1, 10);
      StringBuilder sb = new StringBuilder();
      sb.AppendLine($"gunneryskill: {skill}");
      sb.AppendLine($"mitigationRoll: {mitigationRoll}");
      if (skill >= mitigationRoll) {
        CustomAmmoCategoriesLog.Log.LogWrite(sb.ToString() + "\n");
        RemoveJam(actor, weapon);
        return true;
      }

      CustomAmmoCategoriesLog.Log.LogWrite(sb.ToString() + "\n");
      return false;
    }
    public static bool AttemptToRemoveCooldown(AbstractActor actor, Weapon weapon) {
      int cooldown = weapon.StatCollection.GetStatistic(CustomAmmoCategories.CooldownWeaponStatisticName).Value<int>();
      if (cooldown <= 1) {
        weapon.StatCollection.Set<int>(CustomAmmoCategories.CooldownWeaponStatisticName, 0);
        //if (weapon.IsJammed() == false) { weapon.StatCollection.Set<bool>(TemporarilyDisabledStatisticName, false); };
        CustomAmmoCategoriesLog.Log.LogWrite($" Weapon " + weapon.UIName + " - operational\n");
        /*actor.Combat.MessageCenter.PublishMessage(
            new AddSequenceToStackMessage(
                new ShowActorInfoSequence(actor, $"{weapon.Name} Ready!", FloatieMessage.MessageNature.Buff,
                    true)));*/
        return true;
      } else {
        weapon.StatCollection.Set<int>(CustomAmmoCategories.CooldownWeaponStatisticName, cooldown - 1);
        //weapon.StatCollection.Set<bool>(TemporarilyDisabledStatisticName, true);
        CustomAmmoCategoriesLog.Log.LogWrite($" Weapon " + weapon.UIName + " - cooldown " + (cooldown - 1) + "\n");
        return false;
      }
    }
    private static void RemoveJam(AbstractActor actor, Weapon weapon) {
      weapon.StatCollection.Set<bool>(JammedWeaponStatisticName, false);
      //if (weapon.IsCooldown() <= 0) { weapon.StatCollection.Set<bool>(TemporarilyDisabledStatisticName, false); };
      actor.Combat.MessageCenter.PublishMessage(
          new AddSequenceToStackMessage(
              new ShowActorInfoSequence(actor, $"{weapon.UIName} __/CAC.Unjammed/__!", FloatieMessage.MessageNature.Buff,
                  true)));
    }
  }
}
