using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectERA.Data;
using ERAUtils.Enum;
using System.Threading.Tasks;
using System.Threading;
using ERAUtils;

namespace ProjectERA.Services.Data
{
    internal static partial class ContentDatabase
    {
        /// <summary>
        /// Populates the contentDatabase with default values
        /// </summary>
        public static void Populate()
        {
#if !NOMULTITHREAD
            Task _populatingTask = Task.Factory.StartNew(() => {

                Interlocked.Increment(ref _asyncOperations);
#endif
                ERAUtils.Logger.Logger.Info("ContentDatabase started populating default content");

                // DEFAULT DATA >> ALWAYS create

                /// //////////////////////
                /// WEAPONS
                /// //////////////////////
                ContentDatabase.SetEquipmentWeapon(
                    new Equipment(
                        MongoObjectId.Empty, 1, 
                        EquipmentType.Weapon, 
                        EquipmentPart.Right, 
                        "Stick", 
                        String.Empty, 
                        "stick_sword", 
                        0.00, 
                        1.00, 
                        ElementType.None, 
                        ItemFlags.NoActions, 
                        BattlerValues.None
                    )
                );

                /// //////////////////////
                /// ARMORS
                /// //////////////////////
                ContentDatabase.SetEquipmentArmor(
                    Equipment.Empty);
                ContentDatabase.SetEquipmentArmor(
                    new Equipment(
                        MongoObjectId.Empty, 2, 
                        EquipmentType.Armor, 
                        EquipmentPart.Top, 
                        "Sloppy Shirt", 
                        String.Empty, 
                        "sloppy_top", 
                        0.00, 
                        1.00, 
                        ElementType.None, 
                        ItemFlags.NoActions, 
                        BattlerValues.None
                    )
                );
                ContentDatabase.SetEquipmentArmor(
                    new Equipment(
                        MongoObjectId.Empty, 3, 
                        EquipmentType.Armor, 
                        EquipmentPart.Bottom, 
                        "Sloppy Trousers", 
                        String.Empty, 
                        "sloppy_bottom", 
                        0.00, 
                        1.00, 
                        ElementType.None, 
                        ItemFlags.NoActions, 
                        BattlerValues.None
                    )
                );

#if DEBUG || !NOFAILSAFE      // SHOULD LOAD THIS FROM FILE
                /// //////////////////////
                /// Colors
                /// //////////////////////
                ContentDatabase.SetColorByte("Blonde", 1);
                ContentDatabase.SetColorByte("Brown", 2);
                ContentDatabase.SetColorByte("Black", 3);
                ContentDatabase.SetColorByte("Grey", 4);

                ContentDatabase.SetColorByte("Red", 5);

                ContentDatabase.SetColorByte("GreyBlonde", 64);
                ContentDatabase.SetColorByte("DarkBlonde", 65);
                ContentDatabase.SetColorByte("GoldBlonde", 66);

                /// //////////////////////
                /// STATES
                /// //////////////////////
                ContentDatabase.SetBattlerModifier(
                    new BattlerModifier(1, 
                        "Knocked out",
                        "When knocked out, no actions are allowed and no experience is gained. You will be respawned at your last respawn location.",
                        String.Empty,
                        ActionRestriction.NoAction,
                        BattlerModifierFlags.State | BattlerModifierFlags.ZeroHealth | BattlerModifierFlags.NoExperience,
                        ProjectERA.Data.BattlerModifier.DamageValues.None,
                        ProjectERA.Data.BattlerModifier.ReleaseValues.None,
                        ProjectERA.Data.BattlerModifier.TimeValues.None,
                        BattlerValues.None, 
                        null)
                    );

                ContentDatabase.SetBattlerModifier(
                    new BattlerModifier(2,
                        "Exhausted",
                        String.Empty,
                        String.Empty,
                        ActionRestriction.None,
                        BattlerModifierFlags.State | BattlerModifierFlags.ZeroConcentration | BattlerModifierFlags.StackOnCondition,
                        ProjectERA.Data.BattlerModifier.DamageValues.None,
                        ProjectERA.Data.BattlerModifier.ReleaseValues.Generate(100, 0, 0),
                        ProjectERA.Data.BattlerModifier.TimeValues.Generate(TimeSpan.FromSeconds(60), TimeSpan.Zero),
                        BattlerValues.Generate(-50, -50, -50, -50, -100, -50, -50, -50),
                        null)
                    );

                // No Action
                ContentDatabase.SetBattlerModifier(
                    new BattlerModifier(3, 
                        "Stunned",
                        String.Empty,
                        String.Empty,
                        ActionRestriction.NoAction,
                        BattlerModifierFlags.State,
                        ProjectERA.Data.BattlerModifier.DamageValues.None,
                        ProjectERA.Data.BattlerModifier.ReleaseValues.Generate(100, 0, 0),
                        ProjectERA.Data.BattlerModifier.TimeValues.Generate(TimeSpan.FromSeconds(30), TimeSpan.Zero),
                        BattlerValues.None,
                        null)
                    );

                // Slip Damage
                ContentDatabase.SetBattlerModifier(
                    new BattlerModifier(4,
                        "Poisoned",
                        String.Empty,
                        String.Empty,
                        ActionRestriction.None,
                        BattlerModifierFlags.State,
                        ProjectERA.Data.BattlerModifier.DamageValues.GenerateSlip(0, 1),
                        ProjectERA.Data.BattlerModifier.ReleaseValues.None,
                        ProjectERA.Data.BattlerModifier.TimeValues.Generate(TimeSpan.Zero, TimeSpan.FromSeconds(1)),
                        BattlerValues.None,
                        null)
                    );

                // CON -
                ContentDatabase.SetBattlerModifier(
                    new BattlerModifier(5, 
                        "Dazzled",
                        String.Empty,
                        String.Empty,
                        ActionRestriction.None,
                        BattlerModifierFlags.State,
                        ProjectERA.Data.BattlerModifier.DamageValues.None,
                        ProjectERA.Data.BattlerModifier.ReleaseValues.Generate(50, 0, 0),
                        ProjectERA.Data.BattlerModifier.TimeValues.Generate(TimeSpan.FromSeconds(30), TimeSpan.Zero),
                        BattlerValues.GenerateCon(-80),
                        null)
                    );

                // No Skill
                ContentDatabase.SetBattlerModifier(
                    new BattlerModifier(6, 
                        "Muted",
                        String.Empty,
                        String.Empty,
                        ActionRestriction.NoSkill,
                        BattlerModifierFlags.State,
                        ProjectERA.Data.BattlerModifier.DamageValues.None,
                        ProjectERA.Data.BattlerModifier.ReleaseValues.Generate(50, 0, 0),
                        ProjectERA.Data.BattlerModifier.TimeValues.Generate(TimeSpan.FromSeconds(30), TimeSpan.Zero),
                        BattlerValues.None,
                        null)
                    );

                // Attack allies
                ContentDatabase.SetBattlerModifier(
                    new BattlerModifier(7, 
                        "Confused",
                        String.Empty,
                        String.Empty,
                        ActionRestriction.AttackAllies,
                        BattlerModifierFlags.State,
                        ProjectERA.Data.BattlerModifier.DamageValues.None,
                        ProjectERA.Data.BattlerModifier.ReleaseValues.Generate(50, 25, 0),
                        ProjectERA.Data.BattlerModifier.TimeValues.Generate(TimeSpan.FromSeconds(30), TimeSpan.Zero),
                        BattlerValues.None,
                        null)
                    );

                // No Action
                ContentDatabase.SetBattlerModifier(
                    new BattlerModifier(8, 
                        "Asleep",
                        String.Empty,
                        String.Empty,
                        ActionRestriction.NoAction,
                        BattlerModifierFlags.State | BattlerModifierFlags.NoEvading,
                        ProjectERA.Data.BattlerModifier.DamageValues.None,
                        ProjectERA.Data.BattlerModifier.ReleaseValues.Generate(50, 50, 0),
                        ProjectERA.Data.BattlerModifier.TimeValues.Generate(TimeSpan.FromSeconds(30), TimeSpan.Zero),
                        BattlerValues.None,
                        null)
                    );

                // No Action
                ContentDatabase.SetBattlerModifier(
                    new BattlerModifier(9, 
                        "Paralyzed",
                        String.Empty,
                        String.Empty,
                        ActionRestriction.NoAction,
                        BattlerModifierFlags.State,
                        ProjectERA.Data.BattlerModifier.DamageValues.None,
                        ProjectERA.Data.BattlerModifier.ReleaseValues.Generate(25, 0, 0),
                        ProjectERA.Data.BattlerModifier.TimeValues.Generate(TimeSpan.FromSeconds(30), TimeSpan.Zero),
                        BattlerValues.None,
                        null)
                    );

                // STR -
                ContentDatabase.SetBattlerModifier(
                    new BattlerModifier(10, 
                        "Weakened",
                        String.Empty,
                        String.Empty,
                        ActionRestriction.None,
                        BattlerModifierFlags.State,
                        ProjectERA.Data.BattlerModifier.DamageValues.None,
                        ProjectERA.Data.BattlerModifier.ReleaseValues.Generate(100, 0, 0),
                        ProjectERA.Data.BattlerModifier.TimeValues.Generate(TimeSpan.FromSeconds(60), TimeSpan.Zero),
                        BattlerValues.GenerateStr(-50),
                        null)
                    );

                // CON -
                ContentDatabase.SetBattlerModifier(
                    new BattlerModifier(11, 
                        "Clumsy",
                        String.Empty,
                        String.Empty,
                        ActionRestriction.None,
                        BattlerModifierFlags.State,
                        ProjectERA.Data.BattlerModifier.DamageValues.None,
                        ProjectERA.Data.BattlerModifier.ReleaseValues.Generate(100, 0, 0),
                        ProjectERA.Data.BattlerModifier.TimeValues.Generate(TimeSpan.FromSeconds(60), TimeSpan.Zero),
                        BattlerValues.GenerateCon(-50),
                        null)
                    );

                // HAS -
                ContentDatabase.SetBattlerModifier(
                    new BattlerModifier(12, 
                        "Delayed",
                        String.Empty,
                        String.Empty,
                        ActionRestriction.None,
                        BattlerModifierFlags.State,
                        ProjectERA.Data.BattlerModifier.DamageValues.None,
                        ProjectERA.Data.BattlerModifier.ReleaseValues.Generate(100, 0, 0),
                        ProjectERA.Data.BattlerModifier.TimeValues.Generate(TimeSpan.FromSeconds(60), TimeSpan.Zero),
                        BattlerValues.GenerateHas(-50),
                        null)
                    );

                // ESS -
                ContentDatabase.SetBattlerModifier(
                    new BattlerModifier(12,
                        "Feeble",
                        String.Empty,
                        String.Empty,
                        ActionRestriction.None,
                        BattlerModifierFlags.State,
                        ProjectERA.Data.BattlerModifier.DamageValues.None,
                        ProjectERA.Data.BattlerModifier.ReleaseValues.Generate(100, 0, 0),
                        ProjectERA.Data.BattlerModifier.TimeValues.Generate(TimeSpan.FromSeconds(60), TimeSpan.Zero),
                        BattlerValues.GenerateEss(-50),
                        null)
                    );

                // MDEF -
                // DEF -
                // MSTR -
                // END -

                // CON +
                ContentDatabase.SetBattlerModifier(
                    new BattlerModifier(13, 
                        "Sharpened",
                        String.Empty,
                        String.Empty,
                        ActionRestriction.None,
                        BattlerModifierFlags.State,
                        ProjectERA.Data.BattlerModifier.DamageValues.None,
                        ProjectERA.Data.BattlerModifier.ReleaseValues.Generate(100, 0, 0),
                        ProjectERA.Data.BattlerModifier.TimeValues.Generate(TimeSpan.FromSeconds(90), TimeSpan.Zero),
                        BattlerValues.GenerateCon(50),
                        null) 
                    );

                // DEF +
                ContentDatabase.SetBattlerModifier(
                    new BattlerModifier(14, 
                        "Barriered",
                        String.Empty,
                        String.Empty,
                        ActionRestriction.None,
                        BattlerModifierFlags.Buff,
                        ProjectERA.Data.BattlerModifier.DamageValues.None,
                        ProjectERA.Data.BattlerModifier.ReleaseValues.Generate(100, 0, 0),
                        ProjectERA.Data.BattlerModifier.TimeValues.Generate(TimeSpan.FromSeconds(90), TimeSpan.Zero),
                        BattlerValues.GenerateDef(50),
                        null) 
                    );

                // MDEF + 
                ContentDatabase.SetBattlerModifier(
                    new BattlerModifier(15, 
                        "Resisted",
                        String.Empty,
                        String.Empty,
                        ActionRestriction.None,
                        BattlerModifierFlags.Buff,
                        ProjectERA.Data.BattlerModifier.DamageValues.None,
                        ProjectERA.Data.BattlerModifier.ReleaseValues.Generate(100, 0, 0),
                        ProjectERA.Data.BattlerModifier.TimeValues.Generate(TimeSpan.FromSeconds(90), TimeSpan.Zero),
                        BattlerValues.GenerateMDef(50),
                        null) );

                // HAS +
                ContentDatabase.SetBattlerModifier(
                    new BattlerModifier(16, 
                        "Blinked",
                        String.Empty,
                        String.Empty,
                        ActionRestriction.None,
                        BattlerModifierFlags.Buff,
                        ProjectERA.Data.BattlerModifier.DamageValues.None,
                        ProjectERA.Data.BattlerModifier.ReleaseValues.Generate(100, 0, 0),
                        ProjectERA.Data.BattlerModifier.TimeValues.Generate(TimeSpan.FromSeconds(90), TimeSpan.Zero),
                        BattlerValues.GenerateHas(50),
                        null) );

                // STR + 
                ContentDatabase.SetBattlerModifier(
                    new BattlerModifier(17, 
                        "Pumped",
                        String.Empty,
                        String.Empty,
                        ActionRestriction.None,
                        BattlerModifierFlags.State,
                        ProjectERA.Data.BattlerModifier.DamageValues.None,
                        ProjectERA.Data.BattlerModifier.ReleaseValues.Generate(100, 0, 0),
                        ProjectERA.Data.BattlerModifier.TimeValues.Generate(TimeSpan.FromSeconds(90), TimeSpan.Zero),
                        BattlerValues.GenerateStr(50),
                        null) );

                // MSTR + 
                ContentDatabase.SetBattlerModifier(
                    new BattlerModifier(18, 
                        "Meditated",
                        String.Empty,
                        String.Empty,
                        ActionRestriction.None,
                        BattlerModifierFlags.State,
                        ProjectERA.Data.BattlerModifier.DamageValues.None,
                        ProjectERA.Data.BattlerModifier.ReleaseValues.Generate(100, 0, 0),
                        ProjectERA.Data.BattlerModifier.TimeValues.Generate(TimeSpan.FromSeconds(90), TimeSpan.Zero),
                        BattlerValues.GenerateMStr(50),
                        null) );

                // ESS +
                // END +

                // HP-
                ContentDatabase.SetBattlerModifier(
                    new BattlerModifier(19, 
                        "Drained",
                        String.Empty,
                        String.Empty,
                        ActionRestriction.None,
                        BattlerModifierFlags.State,
                        ProjectERA.Data.BattlerModifier.DamageValues.GenerateSlip(0, 1),
                        ProjectERA.Data.BattlerModifier.ReleaseValues.Generate(100, 0, 0),
                        ProjectERA.Data.BattlerModifier.TimeValues.Generate(TimeSpan.FromSeconds(50), TimeSpan.FromSeconds(1)),
                        BattlerValues.None,
                        null) );

                // HP+
                ContentDatabase.SetBattlerModifier(
                    new BattlerModifier(20, 
                        "Draining",
                        String.Empty,
                        String.Empty,
                        ActionRestriction.None,
                        BattlerModifierFlags.Buff,
                        ProjectERA.Data.BattlerModifier.DamageValues.GenerateSlip(0, -1),
                        ProjectERA.Data.BattlerModifier.ReleaseValues.Generate(100, 0, 0),
                        ProjectERA.Data.BattlerModifier.TimeValues.Generate(TimeSpan.FromSeconds(50), TimeSpan.FromSeconds(1)),
                        BattlerValues.None,
                        null) );


                /// ////////////////////
                /// SKILLS
                /// ////////////////////


                /// ////////////////////
                /// CLASSES
                /// ////////////////////
                
                BattlerClass confusedTraveller = BattlerClass.Generate(1, "Confused Traveller", 0);
                BattlerClass bard = BattlerClass.Generate(2, "Bard", confusedTraveller.DatabaseId);
                BattlerClass mercenary = BattlerClass.Generate(3, "Mercenary", confusedTraveller.DatabaseId);
                BattlerClass stalker = BattlerClass.Generate(4, "Stalker", confusedTraveller.DatabaseId);

                // Talent trees
                TalentTree confusedBardTree = TalentTree.Generate();
                confusedBardTree.PointId = bard.DatabaseId;
                TalentTree confusedMercenaryTree = TalentTree.Generate();
                confusedMercenaryTree.PointId = mercenary.DatabaseId;
                TalentTree confusedStalkerTree = TalentTree.Generate();
                confusedStalkerTree.PointId = stalker.DatabaseId;

                // End nodes
                TalentTree.Node toBard = TalentTree.ClassNode.Generate(0, 15, bard.DatabaseId);
                TalentTree.Node toMercenary = TalentTree.ClassNode.Generate(0, 15, mercenary.DatabaseId);
                TalentTree.Node toStalker = TalentTree.ClassNode.Generate(0, 15, stalker.DatabaseId);
                confusedBardTree.AddRoot(toBard);
                confusedMercenaryTree.AddRoot(toMercenary);
                confusedStalkerTree.AddRoot(toStalker);

                // Starting nodes
                TalentTree.SkillNode slash = TalentTree.SkillNode.Generate(0, 0, 1) as TalentTree.SkillNode;
                confusedBardTree.AddRoot(slash.Empty());
                confusedMercenaryTree.AddRoot(slash.Empty());
                confusedStalkerTree.AddRoot(slash.Empty());

                // Skill nodes
                TalentTree.Node bard1 = TalentTree.SkillNode.Generate(0, 0, 2);
                TalentTree.Node bard2 = TalentTree.SkillNode.Generate(0, 0, 3);
                TalentTree.Node mercenary1 = TalentTree.SkillNode.Generate(0, 4);
                TalentTree.Node mercenary2 = TalentTree.SkillNode.Generate(0, 5);
                TalentTree.Node stalker1 = TalentTree.SkillNode.Generate(0, 0, 6);
                TalentTree.Node stalker2 = TalentTree.SkillNode.Generate(0, 0, 7);

                // Add skills
                confusedBardTree.AddRoot(bard1);
                confusedBardTree.AddRoot(bard2);
                confusedMercenaryTree.AddRoot(mercenary1);
                confusedMercenaryTree.AddRoot(mercenary2);
                confusedStalkerTree.AddRoot(stalker1);
                confusedStalkerTree.AddRoot(stalker2);

                confusedTraveller.TalentTrees.Add(confusedBardTree);
                confusedTraveller.TalentTrees.Add(confusedMercenaryTree);
                confusedTraveller.TalentTrees.Add(confusedStalkerTree);

                ContentDatabase.SetBattlerClass(confusedTraveller);
                ContentDatabase.SetBattlerClass(bard);
                ContentDatabase.SetBattlerClass(mercenary);
                ContentDatabase.SetBattlerClass(stalker);

                /// ////////////////////
                /// RACES
                /// ////////////////////

                BattlerRace lewan = BattlerRace.Generate(1, "Lewan", BattlerValues.Generate(5, 5, 5, 5, 5, 5, 5, 5));
                BattlerRace sumnian = BattlerRace.Generate(2, "Sumnian", BattlerValues.Generate(5, 5, 5, 5, 5, 5, 5, 5));

                ContentDatabase.SetBattlerRace(lewan);
                ContentDatabase.SetBattlerRace(sumnian);
#endif

                ERAUtils.Logger.Logger.Info("ContentDatabase finished populating default content");


#if !NOMULTITHREAD
                Interlocked.Decrement(ref _asyncOperations);
            });
#endif
        }
    }
}
