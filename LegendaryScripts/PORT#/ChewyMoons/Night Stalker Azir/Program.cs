// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="ChewyMoon">
//   Copyright (C) 2015 ChewyMoon
//   
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// <summary>
//   The program class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Night_Stalker_Azir
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System;
    using System.Collections.Generic;
    using EnsoulSharp.SDK;
    using EnsoulSharp.SDK.MenuUI;
    using EnsoulSharp.SDK.MenuUI.Values;
    using EnsoulSharp.SDK.Prediction;
    using EnsoulSharp.SDK.Utility;
    using EnsoulSharp;
    using SharpDX;
    using Color = System.Drawing.Color;
    using static EnsoulSharp.SDK.Items;
    using SharpDX.Direct3D9;

    using SharpDX;
    using SPrediction;

    /// <summary>
    ///     The program class.
    /// </summary>
    internal class Program
    {
        #region Public Properties

        /// <summary>
        ///     Gets the sand soldiers.
        /// </summary>
        /// <value>
        ///     The sand soldiers.
        /// </value>
        public static IEnumerable<AIBaseClient> SandSoldiers
        {
            get
            {
                return
                    ObjectManager.Get<AIBaseClient>()
                        .Where(x => x.IsAlly && x.CharacterData.Name.Equals("azirsoldier"));
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the azir automatic attack range.
        /// </summary>
        /// <value>
        ///     The azir automatic attack range.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        private static int AzirSoldierAutoAttackRange
        {
            get
            {
                return 250;
            }
        }

        /// <summary>
        ///     Gets or sets the e.
        /// </summary>
        /// <value>
        ///     The e.
        /// </value>
        private static Spell E { get; set; }

        /// <summary>
        ///     Gets or sets the flash.
        /// </summary>
        /// <value>
        ///     The flash.
        /// </value>
        private static Spell Flash { get; set; }

        /// <summary>
        ///     Gets or sets the last insec notifcation.
        /// </summary>
        /// <value>
        ///     The last insec notifcation.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        private static int LastInsecNotifcation { get; set; }

        /// <summary>
        ///     Gets or sets the menu.
        /// </summary>
        /// <value>
        ///     The menu.
        /// </value>
        private static Menu Menu { get; set; }

        /// <summary>
        ///     Gets or sets the orbwalker.
        /// </summary>
        /// <value>
        ///     The orbwalker.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]

        /// <summary>
        ///     Gets the player.
        /// </summary>
        /// <value>
        ///     The player.
        /// </value>
        private static AIHeroClient Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }

        /// <summary>
        ///     Gets or sets the q.
        /// </summary>
        /// <value>
        ///     The q.
        /// </value>
        private static Spell Q { get; set; }

        /// <summary>
        ///     Gets or sets the r.
        /// </summary>
        /// <value>
        ///     The r.
        /// </value>
        private static Spell R { get; set; }

        /// <summary>
        ///     Gets or sets the w.
        /// </summary>
        /// <value>
        ///     The w.
        /// </value>
        private static Spell W { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Counters an incoming enemy gapcloser.
        /// </summary>
        /// <param name="gapcloser">The gapcloser.</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        private static void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, Gapcloser.GapcloserArgs args)
        {
            if (!Menu["UseRGapCloser"].GetValue<MenuBool>())
            {
                return;
            }

            if (R.IsInRange(sender, R.Range + 150))
            {
                R.Cast(Player.Position.Extend(sender.Position, R.Range));
            }
        }

        /// <summary>
        ///     Creates the menu.
        /// </summary>
        private static void CreateMenu()
        {
            Menu = new Menu("Night Stalker Azir", "NSAzir", true);

            var targetSelectorMenu = new Menu("Target Selector", "TS");
            TargetSelector.GetTarget(targetSelectorMenu);
            Menu.Add(targetSelectorMenu);

            var comboMenu = new Menu("Combo Settings", "Combo");
            comboMenu.Add(new MenuBool("UseQCombo", "Use Q"));
            comboMenu.Add(new MenuBool("UseWCombo", "Use W"));
            comboMenu.Add(new MenuBool("UseECombo", "Use E to Get into AA Range"));
            comboMenu.Add(new MenuBool("UseRComboFinisher", "Use R if Killable"));
            Menu.Add(comboMenu);

            var harassMenu = new Menu("Harass Settings", "Harass");
            harassMenu.Add(new MenuBool("UseQHarass", "Use Q if not in Soldier AA Range"));
            harassMenu.Add(new MenuBool("UseWHarass", "Use W"));
            harassMenu.Add(new MenuKeyBind("HarassToggle", "Harass! (Toggle)", System.Windows.Forms.Keys.C, KeyBindType.Toggle));
            harassMenu.Add(new MenuSlider("HarassToggleMana", "Harass Mana (Toggle only)",0,50,100));
            Menu.Add(harassMenu);

            var laneClear = new Menu("Lane Clear Settings", "LaneClear");
            laneClear.Add(new MenuBool("UseQLaneClear", "Use Q"));
            laneClear.Add(new MenuBool("UseWLaneClear", "Use W"));
            laneClear.Add(new MenuSlider("LaneClearMana", "Lane Clear Mana Percent",0,50,100));
            Menu.Add(laneClear);

            var fleeMenu = new Menu("Flee Settings", "Flee");
            fleeMenu.Add(
                new MenuList("FleeOption", "Flee Mode", new[] { "E -> Q", "Q -> E" }) {Index = 0 });
            fleeMenu.Add(new MenuKeyBind("Flee", "Flee" , System.Windows.Forms.Keys.Z, KeyBindType.Toggle));
            Menu.Add(fleeMenu);

            var ksMenu = new Menu("Kill Steal Settings", "KS");
            ksMenu.Add(new MenuBool("UseQKS", "Use Q"));
            ksMenu.Add(new MenuBool("UseRKS", "Use R"));
            Menu.Add(ksMenu);

            var miscMenu = new Menu("Miscellaneous Settings", "Misc");
            miscMenu.Add(new MenuBool("UseRInterrupt", "Interrupt with R"));
            miscMenu.Add(new MenuBool("UseRGapCloser", "Use R on Gapcloser"));
            Menu.Add(miscMenu);

            var insecMenu = new Menu("Insec Settings", "Insec");
            insecMenu.Add(
                new MenuKeyBind("InsecActive", "Insec! (Press)", System.Windows.Forms.Keys.Z, KeyBindType.Toggle));       
            Menu.Add(insecMenu);

            var drawingMenu = new Menu("Drawing Settings", "Drawing");
            drawingMenu.Add(new MenuBool("DrawQ", "Draw Q"));
            drawingMenu.Add(new MenuBool("DrawW", "Draw W"));
            drawingMenu.Add(new MenuBool("DrawE", "Draw E"));
            drawingMenu.Add(new MenuBool("DrawR", "Draw R"));
            Menu.Add(drawingMenu);

            Menu.Attach();
        }

        /// <summary>
        ///     Gets the damages to unit.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>The damage.</returns>
        private static float DamageToUnit(AIHeroClient target)
        {
            var damage = 0f;

            if (Q.IsReady())
            {
                damage += Q.GetDamage(target);
            }

            if (Orbwalker.CanAttack())
            {
                damage +=
                    SandSoldiers.Where(x => target.Distance(x) < AzirSoldierAutoAttackRange)
                        .Sum(soldier => W.GetDamage(target));
                damage += (float)Player.GetAutoAttackDamage(target);
            }

            if (R.IsReady())
            {
                damage += R.GetDamage(target);
            }

            return damage;
        }

        /// <summary>
        ///     Does the combo.
        /// </summary>
        private static void DoCombo()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);

            if (!target.IsValidTarget())
            {
                return;
            }

            var useQCombo = Menu["UseQCombo"].GetValue<MenuBool>();
            var useWCombo = Menu["UseWCombo"].GetValue<MenuBool>();
            var useECombo = Menu["UseECombo"].GetValue<MenuBool>();
            var useRComboFinisher = Menu["UseRComboFinisher"].GetValue<MenuBool>();

            if (W.IsReady() && useWCombo
                && (W.IsInRange(target, W.Range + AzirSoldierAutoAttackRange)
                    || (Q.IsReady() || Q.Instance.State == SpellState.Cooldown)))
            {
                W.Cast(Player.Position.Extend(target.Position, W.Range));
            }

            if (Q.IsReady() && useQCombo
                && SandSoldiers.Any(x => GameObjects.EnemyHeroes.All(y => y.Distance(x) > AzirSoldierAutoAttackRange)))
            {
                Q.Cast(target);
            }

            if (E.IsReady() && useECombo && !target.InAutoAttackRange())
            {
                var soldier =
                    SandSoldiers.FirstOrDefault(
                        x =>
                        x.Distance(target) < target.GetRealAutoAttackRange(Player)
                        && x.Distance(target) > AzirSoldierAutoAttackRange);

                if (soldier != null)
                {
                    E.CastOnUnit(soldier);
                }
            }

            if (R.IsReady() && useRComboFinisher && R.GetDamage(target) > target.Health
                && target.Health - R.GetDamage(target) > -100)
            {
                R.Cast(target);
            }
        }

        /// <summary>
        ///     Does the harass.
        /// </summary>
        /// <param name="toggleCall">If the harass was called by the toggle.</param>
        private static void DoHarass(bool toggleCall)
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (!target.IsValidTarget())
            {
                return;
            }

            if (toggleCall)
            {
                var manaMenuSlider = Menu["HarassToggleMana"].GetValue<MenuSlider>().Value;

                if (Player.ManaPercent <= manaMenuSlider)
                {
                    return;
                }
            }

            var useQHarass = Menu["UseQHarass"].GetValue<MenuBool>();
            var useWHarass = Menu["UseWHarass"].GetValue<MenuBool>();

            if (useWHarass && W.IsReady() && W.IsInRange(target, W.Range + AzirSoldierAutoAttackRange))
            {
                W.Cast(Player.Position.Extend(target.Position, W.Range));
            }

            if (useQHarass && SandSoldiers.Any(x => x.Distance(target) > AzirSoldierAutoAttackRange))
            {
                Q.Cast(target);
            }
        }

        /// <summary>
        ///     Does the insec.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        private static void DoInsec()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPosRaw);

            var target = TargetSelector.GetTarget(900);

            if (!target.IsValidTarget())
            {
                if (Environment.TickCount - LastInsecNotifcation >= 5000)
                {
                    Chat.Print(
                        "<font color=\"#7CFC00\"><b>Night Stalker Azir: PORTED BY DEATHGODX</b></font> Please select a target by left clicking them!");
                    LastInsecNotifcation = Environment.TickCount;
                }

                return;
            }

            if (!Q.IsInRange(target))
            {
                return;
            }

            if (W.IsReady() && E.IsReady() && (Q.IsReady()) && R.IsReady()
                && Flash.IsReady())
            {
                W.Cast(Player.Position.Extend(target.Position, W.Range));

                DelayAction.Add(
                    (int)(W.Delay * 1000),
                    () =>
                    {
                        E.CastOnUnit(SandSoldiers.OrderBy(x => x.Distance(Player)).FirstOrDefault());
                        Q.Cast(Player.Position.Extend(target.Position, Q.Range));

                        DelayAction.Add(
                            (int)(Q.Delay * 1000 + Player.Distance(target) / 2500 * 1000),
                            () =>
                            {
                                Flash.Cast(Player.Position.Extend(target.Position, Flash.Range));

                                var nearestUnit =
                                        ObjectManager.Get<AIBaseClient>()
                                            .OrderBy(x => x.Distance(Player))
                                            .FirstOrDefault(
                                                x => !x.IsMe || !x.CharacterData.Name.Equals("AzirSoldier"));

                                if (nearestUnit != null)
                                {
                                    R.Cast(Player.Position.Extend(nearestUnit.Position, R.Range));
                                }
                            });
                    });
            }
        }

        /// <summary>
        ///     Does the lane clear.
        /// </summary>
        private static void DoLaneClear()
        {
            var manaPercentage = Menu["LaneClearMana"].GetValue<MenuSlider>().Value;

            if (Player.ManaPercent <= manaPercentage)
            {
                return;
            }

            var useQLaneClear = Menu["UseQLaneClear"].GetValue<MenuBool>();
            var useWLaneClear = Menu["UseWLaneClear"].GetValue<MenuBool>();

            if (useWLaneClear && W.IsReady())
            {
                var position = W.GetCircularFarmLocation(
                    MinionManager.GetMinions(W.Range + AzirSoldierAutoAttackRange),
                    AzirSoldierAutoAttackRange);

                if (position.MinionsHit > 1)
                {
                    W.Cast(position.Position);
                }
            }

            if (useQLaneClear && (Q.IsReady() )
                && SandSoldiers.Any(
                    x =>
                    MinionManager.GetMinions(x.Position, AzirSoldierAutoAttackRange)
                        .All(y => y.Distance(x) > AzirSoldierAutoAttackRange)))
            {
                var position = Q.GetCircularFarmLocation(
                    MinionManager.GetMinions(Q.Range + AzirSoldierAutoAttackRange),
                    AzirSoldierAutoAttackRange);

                if (position.MinionsHit > 1)
                {
                    Q.Cast(position.Position);
                }
            }
        }

        /// <summary>
        ///     Called when the game draws itself.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void Drawing_OnDraw(EventArgs args)
        {
            var drawQ = Menu["DrawQ"].GetValue<MenuBool>();
            var drawW = Menu["DrawW"].GetValue<MenuBool>();
            var drawE = Menu["DrawE"].GetValue<MenuBool>();
            var drawR = Menu["DrawR"].GetValue<MenuBool>();

            if (drawQ)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawW)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawE)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawR)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Aqua : Color.Red);
            }
        }

        /// <summary>
        ///     Flees this instance.
        /// </summary>
        private static void Flee()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPosRaw);

            if (
                !((Q.IsReady() ) && W.IsReady()
                  && (E.IsReady())))
            {
                return;
            }

            var option = Menu["FleeOption"].GetValue<MenuList>().Index;

            if (option == 0)
            {
                W.Cast(Player.Position.Extend(Game.CursorPosRaw, W.Range));

                DelayAction.Add(
                    (int)(W.Delay * 1000),
                    () =>
                    {
                        E.CastOnUnit(SandSoldiers.OrderBy(x => x.Distance(Player)).FirstOrDefault());
                        Q.Cast(Player.Position.Extend(Game.CursorPosRaw, Q.Range));
                    });
            }
            else if (option == 1)
            {
                W.Cast(Player.Position.Extend(Game.CursorPosRaw, W.Range));

                DelayAction.Add(
                    (int)(W.Delay * 1000),
                    () =>
                    {
                        Q.Cast(Player.Position.Extend(Game.CursorPosRaw, Q.Range));
                        E.CastOnUnit(SandSoldiers.OrderBy(x => x.Distance(Player)).FirstOrDefault());
                    });
            }
        }

        /// <summary>
        ///     Called when the game loads.
        /// </summary>
        /// <param name="args">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private static void Game_OnGameLoad()
        {
            if (Player.CharacterData.Name != "Azir")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 800 + AzirSoldierAutoAttackRange);
            W = new Spell(SpellSlot.W, 450);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 250);
            Flash = new Spell(Player.GetSpellSlot("summonerflash"), 425);

            Q.SetSkillshot(7.5f / 30, 70, 1000, false,false, SkillshotType.Line);
            W.Delay = 0.25f;
            E.SetSkillshot(7.5f / 30, 100, 2000, true,false, SkillshotType.Line);

            CreateMenu();

            //Utility.HpBarDamageIndicator.DamageToUnit = DamageToUnit;
            //Utility.HpBarDamageIndicator.Enabled = true;

            Chat.Print("<font color=\"#7CFC00\"><b>Night Stalker Azir:</b></font> by ChewyMoon & Shiver loaded");

            Game.OnUpdate += Game_OnUpdate;
            Interrupter.OnInterrupterSpell += Interrupter2_OnInterruptableTarget;
            Gapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        /// <summary>
        ///     The game on update.
        /// </summary>
        /// <param name="args">
        ///     The args.
        /// </param>
        private static void Game_OnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.LastHit:
                    break;
                case OrbwalkerMode.Harass:
                    DoHarass(false);
                    break;
                case OrbwalkerMode.LaneClear:
                    DoLaneClear();
                    break;
                case OrbwalkerMode.Combo:
                    DoCombo();
                    break;
                case OrbwalkerMode.None:
                    break;
            }

            if (Menu["HarassToggle"].GetValue<MenuBool>() && Orbwalker.ActiveMode != OrbwalkerMode.Harass)
            {
                DoHarass(true);
            }

            if (Menu["InsecActive"].GetValue<MenuBool>())
            {
                DoInsec();
            }

            if (Menu["Flee"].GetValue<MenuBool>())
            {
                Flee();
            }

            KillSteal();
        }

        /// <summary>
        ///     Interrupters the interruptable target.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="Interrupter2.InterruptableTargetEventArgs" /> instance containing the event data.</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        private static void Interrupter2_OnInterruptableTarget(
            AIHeroClient sender,
            Interrupter.InterruptSpellArgs args)
        {
            if (!sender.IsValidTarget(R.Range) || args.DangerLevel != Interrupter.DangerLevel.High
                || args.EndTime - Environment.TickCount < 500 || !Menu["UseRInterrupt"].GetValue<MenuBool>())
            {
                return;
            }

            R.Cast(sender);
        }

        /// <summary>
        ///     Steals kills.
        /// </summary>
        private static void KillSteal()
        {
            var useQ = Menu["UseQKS"].GetValue<MenuBool>();
            var useR = Menu["UseRKS"].GetValue<MenuBool>();

            if (useQ && (Q.IsReady() || Q.Instance.State == SpellState.Disabled))
            {
                var bestTarget =
                    GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Q.Range) && Q.GetDamage(x) >= x.Health)
                        .OrderByDescending(x => x.Distance(Player))
                        .FirstOrDefault();

                if (bestTarget != null)
                {
                    Q.Cast(bestTarget);
                }
            }
            else if (useR && R.IsReady())
            {
                var bestTarget =
                    GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(R.Range) && R.GetDamage(x) >= x.Health)
                        .OrderByDescending(x => x.Distance(Player))
                        .FirstOrDefault();

                if (bestTarget != null)
                {
                    R.Cast(bestTarget);
                }
            }
        }

        /// <summary>
        ///     Called when the program starts.
        /// </summary>
        /// <param name="args">The arguments.</param>
        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            GameEvent.OnGameLoad += Game_OnGameLoad;
        }

        #endregion
    }
}
