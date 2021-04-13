﻿using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using System.Linq;
using TabletopTweaks.Extensions;
using TabletopTweaks.NewComponents;
using TabletopTweaks.Utilities;
using UnityEngine;

namespace TabletopTweaks.Bugfixes.Classes {
    class DragonDisciple {
        [HarmonyPatch(typeof(ResourcesLibrary), "InitializeLibrary")]
        static class ResourcesLibrary_InitializeLibrary_Patch {
            static bool Initialized;
            static bool Prefix() {
                if (Initialized) {
                    // When wrath first loads into the main menu InitializeLibrary is called by Kingmaker.GameStarter.
                    // When loading into maps, Kingmaker.Runner.Start will call InitializeLibrary which will
                    // clear the ResourcesLibrary.s_LoadedBlueprints cache which causes loaded blueprints to be garbage collected.
                    // Return false here to prevent ResourcesLibrary.InitializeLibrary from being called twice 
                    // to prevent blueprints from being garbage collected.
                    return false;
                }
                else {
                    return true;
                }
            }
            static void Postfix() {
                if (Initialized) return;
                Initialized = true;
                if (Resources.Fixes.DragonDisciple.DisableAllFixes) { return; }
                Main.LogHeader("Patching Dragon Disciple Resources");
                patchPrerequisites();
                PatchBloodlineSelection();
                Main.LogHeader("Patching Dragon Disciple Resources Complete");
                //Do Stuff
            }
            static void patchPrerequisites() {
                if (!Resources.Fixes.DragonDisciple.Fixes["Prerequisites"]) { return; }
                BlueprintFeatureSelection BloodOfDragonsSelection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("da48f9d7f697ae44ca891bfc50727988");
                BlueprintFeatureSelection BloodragerBloodlineSelection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("62b33ac8ceb18dd47ad4c8f06849bc01");
                BlueprintFeatureSelection SorcererBloodlineSelection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("24bef8d1bee12274686f6da6ccbc8914");
                BlueprintFeatureSelection SeekerBloodlineSelection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("7bda7cdb0ccda664c9eb8978cf512dbc");
                BlueprintFeature SylvanBloodlineProgression = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("a46d4bd93601427409d034a997673ece");
                BlueprintFeature SageBloodlineProgression = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("7d990675841a7354c957689a6707c6c2");
                BlueprintFeature EmpyrealBloodlineProgression = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>("8a95d80a3162d274896d50c2f18bb6b1");
                BlueprintCharacterClass DragonDiscipleClass = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("72051275b1dbb2d42ba9118237794f7c");
                // Patch Bloodline Selection
                BloodOfDragonsSelection.GetComponent<NoSelectionIfAlreadyHasFeature>().m_Features = BloodragerBloodlineSelection.m_AllFeatures;
                PrerequisiteNoFeaturesFromList ExcludeBloodlines = ScriptableObject.CreateInstance<PrerequisiteNoFeaturesFromList>();
                    ExcludeBloodlines.Features = new BlueprintFeatureReference[] {
                    BloodragerBloodlineSelection.ToReference<BlueprintFeatureReference>(),
                    SorcererBloodlineSelection.ToReference<BlueprintFeatureReference>(),
                    SeekerBloodlineSelection.ToReference<BlueprintFeatureReference>(),
                    SylvanBloodlineProgression.ToReference<BlueprintFeatureReference>(),
                    SageBloodlineProgression.ToReference<BlueprintFeatureReference>(),
                    EmpyrealBloodlineProgression.ToReference<BlueprintFeatureReference>(),
                };
                ExcludeBloodlines.Group = Prerequisite.GroupType.Any;
                DragonDiscipleClass.SetComponents(DragonDiscipleClass.ComponentsArray
                    .Where(c => !(c is PrerequisiteNoFeature)) // Remove old Bloodline Feature
                    .Where(c => !(c is PrerequisiteNoArchetype)) // Remove Sorcerer Archetype Restrictions
                    .Append(ExcludeBloodlines));
                Main.LogPatch("Patched", DragonDiscipleClass);
                patchSorcererArchetypes();
            }
            static void PatchBloodlineSelection() {
                if (!Resources.Fixes.DragonDisciple.Fixes["BloodlineSelection"]) { return; }
                BlueprintFeatureSelection BloodOfDragonsSelection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("da48f9d7f697ae44ca891bfc50727988");
                BlueprintFeatureSelection BloodragerBloodlineSelection = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("62b33ac8ceb18dd47ad4c8f06849bc01");
                BloodOfDragonsSelection.GetComponent<NoSelectionIfAlreadyHasFeature>().m_Features = BloodragerBloodlineSelection.m_AllFeatures;
            }
            static void patchSorcererArchetypes() {
                BlueprintArchetype EmpyrealSorcererArchetype = ResourcesLibrary.TryGetBlueprint<BlueprintArchetype>("aa00d945f7cf6c34c909a29a25f2df38");
                BlueprintArchetype SageSorcererArchetype = ResourcesLibrary.TryGetBlueprint<BlueprintArchetype>("00b990c8be2117e45ae6514ee4ef561c");
                BlueprintArchetype SylvanSorcererArchetype = ResourcesLibrary.TryGetBlueprint<BlueprintArchetype>("711d5024ecc75f346b9cda609c3a1f83");
                BlueprintArchetype SeekerSorcererArchetype = ResourcesLibrary.TryGetBlueprint<BlueprintArchetype>("7229db6fc0b07af4180e783eed43c4d9");
                BlueprintCharacterClass DragonDiscipleClass = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("72051275b1dbb2d42ba9118237794f7c");
                BlueprintCharacterClass SorcererClass = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("b3a505fb61437dc4097f43c3f8f9a4cf");

                BlueprintArchetype[] SorcererArchetypes = new BlueprintArchetype[] {
                EmpyrealSorcererArchetype,
                SageSorcererArchetype,
                SylvanSorcererArchetype,
                SeekerSorcererArchetype
            };
                foreach (var Archetype in SorcererArchetypes) {
                    var ArchetypeLevel = Helpers.Create<PrerequisiteArchetypeLevel>();
                    ArchetypeLevel.m_CharacterClass = SorcererClass.ToReference<BlueprintCharacterClassReference>();
                    ArchetypeLevel.m_Archetype = Archetype.ToReference<BlueprintArchetypeReference>();
                    ArchetypeLevel.Level = 1;
                    ArchetypeLevel.Group = Prerequisite.GroupType.Any;
                    var DragonDiscipleBlock = Helpers.Create<NewComponents.PrerequisiteNoClassLevelVisible>();
                    DragonDiscipleBlock.m_CharacterClass = DragonDiscipleClass.ToReference<BlueprintCharacterClassReference>();
                    DragonDiscipleBlock.Group = Prerequisite.GroupType.Any;

                    Archetype.AddComponents(ArchetypeLevel, DragonDiscipleBlock);
                    Main.Log($"{Archetype.Name} - Components: {Archetype.ComponentsArray.Count()}");
                }
            }
        }
    }
}