﻿using BattleTech;
using CustomAmmoCategoriesLog;
using Localize;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CustomSettings;
using Log = CustomAmmoCategoriesLog.Log;

namespace CustAmmoCategories {
  [System.AttributeUsage(System.AttributeTargets.Property)]
  public class GameplaySafe : System.Attribute {
    public GameplaySafe() { }
  }
  [SelfDocumentedClass("Settings", "CustomAmmoCategoriesSettings", "Settings")]
  public class Settings {
    [SkipDocumentation, GameplaySafe]
    public bool debugLog { get; set; }
    public bool forbiddenRangeEnable { get; set; } = true;
    public bool AmmoCanBeExhausted { get; set; } = true;
    public bool Joke { get; set; } = false;
    public float ClusterAIMult { get; set; } = 0.2f;
    public float PenetrateAIMult { get; set; } = 0.4f;
    public float JamAIAvoid { get; set; } = 1.0f;
    public float DamageJamAIAvoid { get; set; } = 2.0f;
    public bool modHTTPServer { get; set; } = true;
    public string modHTTPListen { get; set; } = "http://localhost:65080/";
    public string WeaponRealizerStandalone { get; set; } = "WeaponRealizer.dll";
    public string AIMStandalone { get; set; } = "AttackImprovementMod.dll";
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("list of strings")]
    public List<string> DynamicDesignMasksDefs { get; set; } = new List<string>();
    public string BurningTerrainDesignMask { get; set; } = "DesignMaskBurningTerrain";
    public string BurningForestDesignMask { get; set; } = "DesignMaskBurningForest";
    [GameplaySafe]
    public string BurningFX { get; set; } = "vfxPrfPrtl_fireTerrain_lrgLoop";
    [GameplaySafe]
    public string BurnedFX { get; set; } = "vfxPrfPrtl_miningSmokePlume_lrg_loop";
    [GameplaySafe]
    public float BurningScaleX { get; set; } = 1f;
    [GameplaySafe]
    public float BurningScaleY { get; set; } = 1f;
    [GameplaySafe]
    public float BurningScaleZ { get; set; } = 1f;
    [GameplaySafe]
    public float BurnedScaleX { get; set; } = 1f;
    [GameplaySafe]
    public float BurnedScaleY { get; set; } = 1f;
    [GameplaySafe]
    public float BurnedScaleZ { get; set; } = 1f;
    [GameplaySafe]
    public float BurnedOffsetX { get; set; } = 0f;
    [GameplaySafe]
    public float BurnedOffsetY { get; set; } = 0f;
    [GameplaySafe]
    public float BurnedOffsetZ { get; set; } = 0f;
    [GameplaySafe]
    public float BurningOffsetX { get; set; } = 0f;
    [GameplaySafe]
    public float BurningOffsetY { get; set; } = 0f;
    [GameplaySafe]
    public float BurningOffsetZ { get; set; } = 0f;
    public string BurnedForestDesignMask { get; set; } = "DesignMaskBurnedForest";
    public int BurningForestCellRadius { get; set; } = 3;
    public int BurningForestTurns { get; set; } = 3;
    public int BurningForestStrength { get; set; } = 5;
    public float BurningForestBaseExpandChance { get; set; } = 0.5f;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("list of strings")]
    public List<string> AdditinalAssets { get; set; } = new List<string>();
    [GameplaySafe]
    public bool DontShowNotDangerouceJammMessages { get; set; } = false;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("list of strings")]
    public List<string> NoForestBiomes { get; set; } = new List<string>();
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("dictionary { \"<string>\":<float>}")]
    public Dictionary<string, float> ForestBurningDurationBiomeMult { get; set; } = new Dictionary<string, float>();
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("dictionary { \"<string>\":<float>}")]
    public Dictionary<string, float> WeaponBurningDurationBiomeMult { get; set; } = new Dictionary<string, float>();
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("dictionary { \"<string>\":<float>}")]
    public Dictionary<string, float> ForestBurningStrengthBiomeMult { get; set; } = new Dictionary<string, float>();
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("dictionary { \"<string>\":<float>}")]
    public Dictionary<string, float> WeaponBurningStrengthBiomeMult { get; set; } = new Dictionary<string, float>();
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("dictionary { \"<string>\":<float>}")]
    public Dictionary<string, float> LitFireChanceBiomeMult { get; set; } = new Dictionary<string, float>();
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("dictionary { \"<string>\":<float>}")]
    public Dictionary<string, float> MineFieldPathingMods { get; set; } = new Dictionary<string, float>();
    public int JumpLandingMineAttractRadius { get; set; } = 2;
    public int AttackSequenceMaxLength { get; set; } = 15000;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("BurnedTreesSettings structure")]
    [GameplaySafe]
    public BurnedTreesSettings BurnedTrees { get; set; } = new BurnedTreesSettings();
    [GameplaySafe]
    public bool DontShowBurnedTrees { get; set; } = false;
    [GameplaySafe]
    public bool DontShowBurnedTreesTemporary { get; set; } = false;
    [GameplaySafe]
    public bool DontShowScorchTerrain { get; set; } = false;
    public float AAMSAICoeff { get; set; } = 0.2f;
    public bool AIPeerToPeerNodeEnabled { get; set; } = false;
    public bool AIPeerToPeerFirewallPierceThrough { get; set; } = false;
    public string WeaponRealizerSettings { get; set; } = "WeaponRealizerSettings.json";
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("AmmoCookoffSettings structure")]
    public AmmoCookoffSettings AmmoCookoff { get; set; } = new AmmoCookoffSettings();
    //public bool WaterHeightFix { get; set; }
    public float TerrainFiendlyFireRadius { get; set; } = 10f;
    public bool AdvancedCirtProcessing { get; set; } = true;
    public bool DestroyedComponentsCritTrap { get; set; } = true;
    public bool CritLocationTransfer { get; set; } = true;
    public float APMinCritChance { get; set; } = 0.1f;
    public string RemoveFromCritRollStatName { get; set; } = "IgnoreDamage";
    public bool SpawnMenuEnabled { get; set; } = false;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("boolean")]
    public bool NoCritFloatieMessage { get { return FNoCritFloatieMessage == TripleBoolean.True; } set { FNoCritFloatieMessage = value ? TripleBoolean.True : TripleBoolean.False; } }
    [JsonIgnore, SkipDocumentation]
    public TripleBoolean FNoCritFloatieMessage { get; private set; } = TripleBoolean.NotSet;
    [JsonIgnore, SkipDocumentation]
    public TripleBoolean MechEngineerDetected { get; set; } = TripleBoolean.NotSet;
    public void MechEngineerDetect() {
      //Log.M.WL(0, "Detecting MechEngineer:");
      if (MechEngineerDetected != TripleBoolean.NotSet) { return; }
      foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
        Log.M.WL(1, assembly.GetName().Name);
        if (assembly.GetName().Name == "MechEngineer") { MechEngineerDetected = TripleBoolean.True; return; }
      }
      MechEngineerDetected = TripleBoolean.False;
    }
    public bool NoCritFloatie() {
      if (FNoCritFloatieMessage != TripleBoolean.NotSet) { return FNoCritFloatieMessage != TripleBoolean.False; }
      MechEngineerDetect();
      return MechEngineerDetected != TripleBoolean.False;
    }
    [SkipDocumentation, JsonIgnore]
    public HashSet<Strings.Culture> patchWeaponSlotsOverflowCultures { get; private set; } = new HashSet<Strings.Culture>();
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("list of Strings.Culture")]
    public List<Strings.Culture> PatchWeaponSlotsOverflowCultures { get { return patchWeaponSlotsOverflowCultures.ToList(); } set { patchWeaponSlotsOverflowCultures = value.ToHashSet(); } }
    [GameplaySafe]
    public int FiringPreviewRecalcTrottle { get; set; } = 500;
    [GameplaySafe]
    public int SelectionStateMoveBaseProcessMousePosTrottle { get; set; } = 4;
    [GameplaySafe]
    public int UpdateReticleTrottle { get; set; } = 8;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("BloodSettings structure")]
    [GameplaySafe]
    public BloodSettings bloodSettings { get; set; } = new BloodSettings();
    public bool fixPrewarmRequests { get; set; } = true;
    [SkipDocumentation, JsonIgnore]
    public string directory { get; set; }
    [GameplaySafe]
    public ShowMissBehavior showMissBehavior { get; set; } = ShowMissBehavior.Default;
    public bool extendedBraceBehavior { get; set; } = true;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("dictionary of { \"<string>\": { AoEModifiers structure } }")]
    public Dictionary<string, AoEModifiers> TagAoEDamageMult { get; set; } = new Dictionary<string, AoEModifiers>();
    [JsonIgnore]
    private Dictionary<UnitType, AoEModifiers> FDefaultAoEDamageMult = new Dictionary<UnitType, AoEModifiers>();
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("dictionary of { \"<UnitType enum>\": { AoEModifiers structure } }")]
    public Dictionary<UnitType, AoEModifiers> DefaultAoEDamageMult {
      get { return FDefaultAoEDamageMult; }
      set {
        //Log.M.TWL(0, "set DefaultAoEDamageMult");
        foreach (var val in value) {
          //Log.M.WL(1, val.Key.ToString() + " = {range:" + val.Value.Range + " damage:" + val.Value.Damage + "}");
          FDefaultAoEDamageMult[val.Key] = val.Value;
        }
      }
    }
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("list of strings")]
    [GameplaySafe]
    public List<string> screamsIds { get; set; } = new List<string>();
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("list of strings")]
    [GameplaySafe]
    public List<string> uiIcons { get; set; } = new List<string>();
    public bool NullifyDestoryedLocationDamage { get; set; } = true;
    public bool DestoryedLocationDamageTransferStructure { get; set; } = true;
    public bool DestoryedLocationCriticalAllow { get; set; } = true;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("list of strings")]
    public List<string> TransferHeatDamageToNormalTag { get; set; } = new List<string>();
    [GameplaySafe]
    public float WeaponPanelBackWidthScale { get; set; } = 1.1f;
    [GameplaySafe]
    public float WeaponPanelHeightScale { get; set; } = 1f;
    [GameplaySafe]
    public float WeaponPanelWidthScale { get; set; } = 1f;
    [GameplaySafe]
    public float OrderButtonWidthScale { get; set; } = 0.5f;
    [GameplaySafe]
    public float OrderButtonPaddingScale { get; set; } = 0.3f;
    public float AttackSequenceTimeout { get; set; } = 60f;
    [GameplaySafe]
    public bool SidePanelInfoSelfExternal { get; set; } = false;
    [GameplaySafe]
    public bool SidePanelInfoTargetExternal { get; set; } = false;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("list of strings")]
    public List<string> MechHasNoStabilityTag { get; set; } = new List<string>();
    [GameplaySafe]
    public bool InfoPanelDefaultState { get; set; } = false;
    [GameplaySafe]
    public bool AttackLogWrite { get; set; } = false;
    public bool ShowAttackGroundButton { get; set; } = false;
    public bool ShowWeaponOrderButtons { get; set; } = false;
    public float ToHitSelfJumped { get; set; } = 2f;
    public float ToHitMechFromFront { get; set; } = 0f;
    public float ToHitMechFromSide { get; set; } = -1f;
    public float ToHitMechFromRear { get; set; } = -2f;
    public float ToHitVehicleFromFront { get; set; } = 0f;
    public float ToHitVehicleFromSide { get; set; } = -1f;
    public float ToHitVehicleFromRear { get; set; } = -2f;
    public string MinefieldDetectorStatName { get; set; } = "MinefieldDetection";
    public string MinefieldIFFStatName { get; set; } = "MinefieldIFF";
    [GameplaySafe]
    public bool AmmoNameInSidePanel { get; set; } = true;
    [GameplaySafe]
    public bool ShowApplyHeatSinkMessage { get; set; } = true;
    [GameplaySafe]
    public string ApplyHeatSinkMessageTemplate { get; set; } = "APPLY HEAT SINKS:{0}=>{1} HCAP:{1} USED:{2}=>{3}";
    [GameplaySafe]
    public string ResetHeatSinkMessageTemplate { get; set; } = "USED HEAT SINKS:{0}=>{1}";
    public string ApplyHeatSinkActorStat { get; set; } = "CACOverrallHeatSinked";
    public string OverrallShootsCountWeaponStat { get; set; } = "CACOverallShoots";
    public bool AmmoGenericBoxUINameAsName { get; set; } = true;
    [GameplaySafe]
    public bool NoSVGCacheClear { get; set; } = true;
    [GameplaySafe]
    public bool AMSCantFireFloatie { get; set; } = false;
    [GameplaySafe]
    public bool ShowJammChance { get; set; } = true;
    [GameplaySafe]
    public bool ShowEvasiveAsNumber { get; set; } = true;
    [GameplaySafe]
    public float EvasiveNumberFontSize { get; set; } = 24f;
    [GameplaySafe]
    public float EvasiveNumberWidth { get; set; } = 20f;
    [GameplaySafe]
    public float EvasiveNumberHeight { get; set; } = 25f;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("list of strings")]
    public List<string> RemoveToHitModifiers { get; set; } = new List<string>();
    public bool ImprovedBallisticByDefault { get; set; } = true;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("dictionary of { \"<string>\": { DesignMaskMoveCostInfo structure } }")]
    public Dictionary<string, DesignMaskMoveCostInfo> DefaultMoveCosts { get; set; } = new Dictionary<string, DesignMaskMoveCostInfo>();
    public bool DestroyedLocationsCritTransfer { get; set; } = false;
    public string OnlineServerHost { get; set; } = "192.168.78.162";
    public int OnlineServerServicePort { get; set; } = 143;
    public int OnlineServerDataPort { get; set; } = 443;
    [GameplaySafe]
    public bool UseFastPreloading { get; set; } = false;
    public bool AoECanCrit { get; set; } = false;
    public void ApplyLocal(Settings local) {
      PropertyInfo[] props = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
      Log.M.TWL(0, "Settings.ApplyLocal");
      foreach(PropertyInfo prop in props) {
        bool skip = true;
        object[] attrs = prop.GetCustomAttributes(true);
        foreach(object attr in attrs) { if ((attr as GameplaySafe) != null) { skip = false; break; } };
        if (skip) { continue; }
        Log.M.WL(1, "updating:"+prop.Name);
        prop.SetValue(CustomAmmoCategories.Settings, prop.GetValue(local));
      }
    }
    public string SerializeLocal() {
      Log.M.TWL(0, "Settings.SerializeLocal");
      JObject json = JObject.FromObject(this);
      PropertyInfo[] props = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
      foreach (PropertyInfo prop in props) {
        bool skip = true;
        object[] attrs = prop.GetCustomAttributes(true);
        foreach (object attr in attrs) { if ((attr as GameplaySafe) != null) { skip = false; break; } };
        if (skip) {
          if (json[prop.Name] != null) {
            json.Remove(prop.Name);
          }
        }
      }
      return json.ToString(Formatting.Indented);
    }
    public Settings() {
      //directory = string.Empty;
      //debugLog = true;
      //modHTTPServer = true;
      //forbiddenRangeEnable = true;
      //Joke = false;
      //AmmoCanBeExhausted = true;
      //ClusterAIMult = 0.2f;
      //PenetrateAIMult = 0.4f;
      //JamAIAvoid = 1.0f;
      //DamageJamAIAvoid = 2.0f;
      //WeaponRealizerStandalone = "";
      //OnlineServerHost = "192.168.78.162";
      //OnlineServerServicePort = 143;
      //OnlineServerDataPort = 443;
      //modHTTPListen = "http://localhost:65080";
      //DynamicDesignMasksDefs = new List<string>();
      //BurningForestDesignMask = "DesignMaskBurningForest";
      //BurnedForestDesignMask = "DesignMaskBurnedForest";
      //BurningTerrainDesignMask = "DesignMaskBurningTerrain";
      //BurningForestCellRadius = 3;
      //BurningForestTurns = 3;
      //BurningForestStrength = 5;
      //BurningForestBaseExpandChance = 0.5f;
      //BurningFX = "vfxPrfPrtl_fireTerrain_lrgLoop";
      //BurnedFX = "vfxPrfPrtl_miningSmokePlume_lrg_loop";
      //BurningScaleX = 1f;
      //BurningScaleY = 1f;
      //BurningScaleZ = 1f;
      //BurnedScaleX = 1f;
      //BurnedScaleY = 1f;
      //BurnedScaleZ = 1f;
      //BurnedOffsetX = 0f;
      //BurnedOffsetY = 0f;
      //BurnedOffsetZ = 0f;
      //BurningOffsetX = 0f;
      //BurningOffsetY = 0f;
      //BurningOffsetZ = 0f;
      //AttackSequenceMaxLength = 15000;
      //AdditinalAssets = new List<string>();
      //DontShowNotDangerouceJammMessages = false;
      //NoForestBiomes = new List<string>();
      //ForestBurningDurationBiomeMult = new Dictionary<string, float>();
      //WeaponBurningDurationBiomeMult = new Dictionary<string, float>();
      //ForestBurningStrengthBiomeMult = new Dictionary<string, float>();
      //WeaponBurningStrengthBiomeMult = new Dictionary<string, float>();
      //LitFireChanceBiomeMult = new Dictionary<string, float>();
      //MineFieldPathingMods = new Dictionary<string, float>();
      //JumpLandingMineAttractRadius = 2;
      //BurnedTrees = new BurnedTreesSettings();
      //DontShowBurnedTrees = false;
      //DontShowScorchTerrain = false;
      //AIPeerToPeerNodeEnabled = false;
      //AIPeerToPeerFirewallPierceThrough = false;
      //AAMSAICoeff = 0.2f;
      //WeaponRealizerSettings = "WeaponRealizerSettings.json";
      //WeaponRealizerStandalone = "WeaponRealizer.dll";
      //AIMStandalone = "AttackImprovementMod.dll";
      //AmmoCookoff = new AmmoCookoffSettings();
      //DontShowBurnedTreesTemporary = false;
      ////WaterHeightFix = true;
      //TerrainFiendlyFireRadius = 10f;
      //AdvancedCirtProcessing = true;
      //DestroyedComponentsCritTrap = true;
      //CritLocationTransfer = true;
      //APMinCritChance = 0.1f;
      //RemoveFromCritRollStatName = "IgnoreDamage";
      //FNoCritFloatieMessage = TripleBoolean.NotSet;
      //MechEngineerDetected = TripleBoolean.NotSet;
      //SpawnMenuEnabled = false;
      //patchWeaponSlotsOverflowCultures = new HashSet<Strings.Culture>();
      //FiringPreviewRecalcTrottle = 500;
      //SelectionStateMoveBaseProcessMousePosTrottle = 4;
      //UpdateReticleTrottle = 8;
      //bloodSettings = new BloodSettings();
      //fixPrewarmRequests = true;
      //showMissBehavior = ShowMissBehavior.Default;
      //extendedBraceBehavior = true;
      FDefaultAoEDamageMult = new Dictionary<UnitType, AoEModifiers>();
      foreach (UnitType t in Enum.GetValues(typeof(UnitType))) {
        FDefaultAoEDamageMult[t] = new AoEModifiers();
      }
      FDefaultAoEDamageMult[UnitType.Building].Range = 1.5f;
      FDefaultAoEDamageMult[UnitType.Building].Damage = 5f;
      //screamsIds = new List<string>();
      //TagAoEDamageMult = new Dictionary<string, AoEModifiers>();
      //uiIcons = new List<string>();
      //NullifyDestoryedLocationDamage = true;
      //DestoryedLocationDamageTransferStructure = true;
      //DestoryedLocationCriticalAllow = true;
      //TransferHeatDamageToNormalTag = new List<string>();
      //WeaponPanelBackWidthScale = 1.1f;
      //OrderButtonWidthScale = 0.5f;
      //OrderButtonPaddingScale = 0.3f;
      //AttackSequenceTimeout = 60f;
      //SidePanelInfoSelfExternal = false;
      //MechHasNoStabilityTag = new List<string>();
      //InfoPanelDefaultState = false;
      //AttackLogWrite = false;
      //ShowAttackGroundButton = false;
      //ShowWeaponOrderButtons = false;
      //ToHitSelfJumped = 2f;
      //ToHitMechFromFront = 0f;
      //ToHitMechFromSide = -1f;
      //ToHitMechFromRear = -2f;
      //ToHitVehicleFromFront = 0f;
      //ToHitVehicleFromSide = -1f;
      //ToHitVehicleFromRear = -2f;
      //WeaponPanelHeightScale = 1f;
      //MinefieldDetectorStatName = "MinefieldDetection";
      //MinefieldIFFStatName = "MinefieldIFF";
      //AmmoNameInSidePanel = true;
      //ShowApplyHeatSinkMessage = true;
      //ResetHeatSinkMessageTemplate = "USED HEAT SINKS:{0}=>{1}";
      //ApplyHeatSinkMessageTemplate = "APPLY HEAT SINKS:{0}=>{1} HCAP:{1} USED:{2}=>{3}";
      //ApplyHeatSinkActorStat = "CACOverrallHeatSinked";
      //OverrallShootsCountWeaponStat = "CACOverallShoots";
      //AmmoGenericBoxUINameAsName = true;
      //NoSVGCacheClear = true;
      //AMSCantFireFloatie = false;
      //ShowJammChance = true;
      //ShowEvasiveAsNumber = true;
      //EvasiveNumberFontSize = 24f;
      //EvasiveNumberWidth = 20f;
      //EvasiveNumberHeight = 25f;
      //RemoveToHitModifiers = new List<string>();
      //ImprovedBallisticByDefault = true;
      //DefaultMoveCosts = new Dictionary<string, DesignMaskMoveCostInfo>();
      //DestroyedLocationsCritTransfer = false;
    }
  }
}