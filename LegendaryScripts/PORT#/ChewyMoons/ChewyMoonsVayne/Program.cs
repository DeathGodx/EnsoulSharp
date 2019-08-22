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
namespace ChewyVayne
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
    /// <summary>
    ///     The program class.
    /// </summary>
    internal class Program
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the E spell.
        /// </summary>
        /// <value>
        ///     The E spell.
        /// </value>
        private static Spell E { get; set; }
        public static Spell Ignite;
        /// <summary>
        ///     Gets or sets the menu.
        /// </summary>
        /// <value>
        ///     The menu.
        /// </value>
        private static Menu Menu { get; set; }
        public static Menu comboMenu, harassMenu, laneClear, fleeMenu, ksMenu, miscMenu, condemnMenu, waveClearMenu, drawMenu, Items;
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
        public static Item Botrk;
        public static Item Hydra;
        public static Item Tiamat;
        public static Item Bil;
        public static Item Youmuu;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static AIHeroClient Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }

        /// <summary>
        ///     Gets or sets the Q spell.
        /// </summary>
        /// <value>
        ///     The Q spell.
        /// </value>
        private static Spell Q { get; set; }

        /// <summary>
        ///     Gets or sets the R spell.
        /// </summary>
        /// <value>
        ///     The R Spell.
        /// </value>
        private static Spell R { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Called when there is an incoming gap closer.
        /// </summary>
        /// <param name="gapcloser">The gapcloser.</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        private static void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, Gapcloser.GapcloserArgs args)
        {
            if (!sender.IsValidTarget(E.Range) || !condemnMenu["GapcloseE"].GetValue<MenuBool>())
            {
                return;
            }

            E.Cast(sender);
        }

        /// <summary>
        ///     Determines whether this instance can condemn the specified target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="startPos">The start position.</param>
        /// <param name="casting">if set to <c>true</c>, will ward the bush when condemning.</param>
        /// <returns>Whether this instance can condemn the specified target.</returns>
        private static bool CanCondemnStun(AIBaseClient target, Vector3 startPos = default(Vector3), bool casting = true)
        {
            if (startPos == default(Vector3))
            {
                startPos = Player.Position;
            }

            var knockbackPos = startPos.Extend(
                target.Position,
                startPos.Distance(target.Position) + condemnMenu["EDistance"].GetValue<MenuSlider>().Value);

            var flags = NavMesh.GetCollisionFlags(knockbackPos);
            var collision = flags.HasFlag(CollisionFlags.Building) || flags.HasFlag(CollisionFlags.Wall);

            if (!casting || !condemnMenu["Wardbush"].GetValue<MenuBool>() || !NavMesh.IsWallOfGrass(knockbackPos, 200))
            {
                return collision;
            }

            // var wardItem = Items.GetWardSlot();

            if (!condemnMenu["Wardbush"].GetValue<MenuBool>())
            {
                return collision;
            }

            /* if (wardItem != default(InventorySlot))
             {
                 Player.Spellbook.CastSpell(wardItem.SpellSlot, knockbackPos);
             }
             else if (Items.CanUseItem(ItemData.Scrying_Orb_Trinket.Id))
             {
                 Items.UseItem(ItemData.Scrying_Orb_Trinket.Id, knockbackPos);
             }
             else if (Items.CanUseItem(ItemData.Farsight_Orb_Trinket.Id))
             {
                 Items.UseItem(ItemData.Farsight_Orb_Trinket.Id, knockbackPos);
             }
             */
            return collision;
        }

        /// <summary>
        ///     Creates the menu.
        /// </summary>
        private static void CreateMenu()
        {
            Menu = new Menu("ChewyVayne XD", "cmVayne", true);

            // Target Selector



            // Combo
            comboMenu = new Menu("Combo", "C-C-Combo Breaker!");
            comboMenu.Add(new MenuBool("UseQCombo", "Use Q"));
            comboMenu.Add(new MenuBool("UseECombo", "Use E"));
            comboMenu.Add(new MenuBool("UseRCombo", "Use R"));
            comboMenu.Add(new MenuSlider("RComboEnemies", "Enemies to use R", 3, 1, 5));
            Menu.Add(comboMenu);

            // Harass
            harassMenu = new Menu("Harass", "Herass XD");
            harassMenu.Add(new MenuBool("UseQHarass", "Use Q"));
            harassMenu.Add(new MenuBool("UseEHarass", "Use E"));
            Menu.Add(harassMenu);

            // Wave Clear
            waveClearMenu = new Menu("Wave Clear", "Waveclearino");
            waveClearMenu.Add(new MenuBool("UseQWaveClear", "Use Q"));
            Menu.Add(waveClearMenu);

            // Condemn Settings
            condemnMenu = new Menu("Condemn Settings", "ConDAMM Settings");
            condemnMenu.Add(new MenuSlider("EDistance", "E Push Distance", 450, 300, 600));
            condemnMenu.Add(new MenuBool("QIntoE", "Q to E target"));
            condemnMenu.Add(new MenuBool("EPeel", "Peel with E"));
            condemnMenu.Add(new MenuBool("EKS", "Finish with E"));
            condemnMenu.Add(new MenuBool("GapcloseE", "E on Gapcloser"));
            condemnMenu.Add(new MenuBool("InterruptE", "E to Interrupt"));
            condemnMenu.Add(new MenuBool("Wardbush", "Ward bush on E"));
            Menu.Add(condemnMenu);
            Items = new Menu("Items Settings", "Items");
            Items.Add(new MenuSeparator("Items Settings", "Items Settings"));
            Items.Add(new MenuBool("you", "Use [Youmuu]"));
            Items.Add(new MenuBool("BOTRK", "Use [BOTRK]"));
            Items.Add(new MenuSlider("ihp", "My HP Use BOTRK <=", 50));
            Items.Add(new MenuSlider("ihpp", "Enemy HP Use BOTRK <=", 50));
            Menu.Add(Items);
            // Drawing
            drawMenu = new Menu("Drawing", "Drawing");
            drawMenu.Add(new MenuBool("DrawQ", "Draw Q"));
            drawMenu.Add(new MenuBool("DrawE", "Draw E"));
            drawMenu.Add(new MenuBool("DrawEPos", "Draw Condemn Location"));
            Menu.Add(drawMenu);
            Menu.Attach();
            // Version info
            Menu.Add(new MenuBool("VersionInformation", "Version: " + Assembly.GetAssembly(typeof(Program)).GetName().Version));

            // Author
            Menu.Add(new MenuBool("Author", "By ChevyMoon & DEATHGODX"));
        }

        /// <summary>
        ///     Does the combo.
        /// </summary>
        private static void DoCombo()
        {
            var useQ = comboMenu["UseQCombo"].GetValue<MenuBool>().Enabled;
            var useE = comboMenu["UseECombo"].GetValue<MenuBool>().Enabled;
            var useEPeel = condemnMenu["EPeel"].GetValue<MenuBool>().Enabled;
            var qIntoE = condemnMenu["QIntoE"].GetValue<MenuBool>().Enabled;
            var useR = comboMenu["UseRCombo"].GetValue<MenuBool>().Enabled;
            var useREnemies = comboMenu["RComboEnemies"].GetValue<MenuSlider>().Value;
            var useEFinisher = condemnMenu["EKS"].GetValue<MenuBool>().Enabled;

            var target = TargetSelector.GetTarget(800);

            if (!target.IsValidTarget())
            {
                return;
            }

            if (Q.IsReady() && useQ && target.IsValidTarget(600))
            {
                Q.Cast(target.Position);
            }

            if (useE && E.IsReady() && CanCondemnStun(target))
            {
                E.Cast(target);
            }

            if (useEPeel && E.IsReady() && !Player.IsFacing(target))
            {
                E.Cast(target);
            }

            if (useR && R.IsReady() && Player.CountEnemyHeroesInRange(1000) >= useREnemies)
            {
                R.Cast();
            }

            if (useEFinisher && E.IsReady() && Player.GetSpellDamage(target, SpellSlot.E) > target.Health)
            {
                E.Cast(target);
            }
        }

        /// <summary>
        ///     Does the harass.
        /// </summary>
        private static void DoHarass()
        {
            var useQ = harassMenu["UseQHarass"].GetValue<MenuBool>().Enabled;
            var useE = harassMenu["UseEHarass"].GetValue<MenuBool>().Enabled;
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (!target.IsValidTarget())
            {
                return;
            }

            if (useE && E.IsReady() && CanCondemnStun(target))
            {
                E.Cast(target);
            }
            if (useQ && Q.IsReady() && target.IsValidTarget(600))
            {
                Q.Cast(target.Position);
            }
        }

        /// <summary>
        ///     Called when the game draws itself.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = drawMenu["DrawQ"].GetValue<MenuBool>().Enabled;
            var drawE = drawMenu["DrawE"].GetValue<MenuBool>().Enabled;
            var drawEPos = drawMenu["DrawEPos"].GetValue<MenuBool>().Enabled;

            if (drawQ && Q.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawE && E.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawEPos && E.IsReady())
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget()))
                {
                    var knockbackPos = Player.Position.Extend(
                        enemy.Position,
                        Player.Distance(enemy) + condemnMenu["EDistance"].GetValue<MenuSlider>().Value);

                    var flags = NavMesh.GetCollisionFlags(knockbackPos);

                    Drawing.DrawLine(
                        Drawing.WorldToScreen(enemy.Position),
                        Drawing.WorldToScreen(knockbackPos),
                        2,
                        flags.HasFlag(CollisionFlags.Building) || flags.HasFlag(CollisionFlags.Wall)
                            ? Color.Green
                            : Color.Red);
                }
            }
        }

        /// <summary>
        ///     Gets called when the game has loaded.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void Game_OnGameLoad()
        {
            if (Player.CharacterName != "Vayne")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 300);
            E = new Spell(SpellSlot.E, 550);
            R = new Spell(SpellSlot.R);
            Botrk = new Item(ItemId.Blade_of_the_Ruined_King, 400);
            Tiamat = new Item(ItemId.Tiamat_Melee_Only, 400);
            Hydra = new Item(ItemId.Ravenous_Hydra_Melee_Only, 400);
            Bil = new Item(3144, 475f);
            Youmuu = new Item(3142, 10);
            Ignite = new Spell(ObjectManager.Player.GetSpellSlot("summonerdot"), 600);
            //E.SetTargetted(0.25f, 1200);

            CreateMenu();
            Chat.Print("CHEWY MOON VAYNE PORTED BY DEATHGOD");
            var notification = new Notification("ChewyVayne Loaded", "ChewyVayne Loaded");
            {
                //TextColor = new ColorBGRA(Color.Aqua.R, Color.Aqua.G, Color.Aqua.B, Color.Aqua.A);
                //BorderColor = new ColorBGRA(Color.Teal.R, Color.Teal.G, Color.Teal.B, Color.Teal.A);
            };

            Notifications.Add(notification);

            Game.OnUpdate += GameOnOnUpdate;
            Orbwalker.OnAction += Orbwalking_OnAttack;
            Drawing.OnDraw += DrawingOnOnDraw;
            Gapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnInterrupterSpell += Interrupter2OnOnInterruptableTarget;
        }
        public static void Killsteal()
            {
            foreach (var target in GameObjects.EnemyHeroes.Where(hero => hero.IsValidTarget(500) && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("Undying Rage") && !hero.IsDead && !hero.IsZombie))
                if (target.Health + target.AllShield<_Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite))
                    {
                        Ignite.Cast(target);
                    }
}
        /// <summary>
        ///     Called when the game updates itself.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>

        private static void GameOnOnUpdate(EventArgs args)
        {
            Killsteal();
            Item();
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Harass:
                    DoHarass();
                    break;
                case OrbwalkerMode.Combo:
                    DoCombo();
                    break;
            }
        }
        public static void Item()
        {
            var item = Items["BOTRK"].GetValue<MenuBool>().Enabled;
            var yous = Items["you"].GetValue<MenuBool>().Enabled;
            var Minhp = Items["ihp"].GetValue<MenuSlider>().Value;
            var Minhpp = Items["ihpp"].GetValue<MenuSlider>().Value;

            foreach (var target in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(475) && !e.IsDead))
            {
                if (item && Bil.IsReady && Bil.IsOwned() && Bil.IsInRange(target))
                {
                    Bil.Cast(target);
                }

                if ((item && Botrk.IsReady && Botrk.IsOwned() && target.IsValidTarget(475)))
                {
                    Botrk.Cast(target);
                }

                if (yous && Youmuu.IsReady && Youmuu.IsOwned() && _Player.Distance(target) < 325 && Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.Combo))
                {
                    Youmuu.Cast();
                }
            }
        }
        /// <summary>
        ///     Gets the value.
        /// </summary>
        /// <typeparam name="T">Type of value to get.</typeparam>
        /// <param name="name">The name.</param>
        /// <returns>The type of value.</returns>

        /// <summary>
        ///     Called when a unit is casting a spell that can be interrupted.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="Interrupter2.InterruptableTargetEventArgs" /> instance containing the event data.</param>
        private static void Interrupter2OnOnInterruptableTarget(
            AIHeroClient sender,
            Interrupter.InterruptSpellArgs args)
        {
            if (!sender.IsValidTarget(E.Range) || args.DangerLevel == Interrupter.DangerLevel.Low)
            {
                return;
            }

            E.Cast(sender);
        }

        /// <summary>
        ///     Method that gets called when the program starts.
        /// </summary>
        /// <param name="args">The arguments.</param>
        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            GameEvent.OnGameLoad += Game_OnGameLoad;
        }

        /// <summary>
        ///     Called when the unit launches an auto attack missile.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="target">The target.</param>
        private static void Orbwalking_OnAttack(object unit, OrbwalkerActionArgs target)
        {
            if (!Q.IsReady() || Orbwalker.ActiveMode == OrbwalkerMode.None)
            {
                return;
            }

            if ((Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                || (Orbwalker.ActiveMode == OrbwalkerMode.Harass)
                || (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear))
            {
                return;
            }

            var minions = GameObjects.EnemyMinions.Where(e => e.IsValidTarget(E.Range));
            if (minions != null && Orbwalker.ActiveMode != OrbwalkerMode.LaneClear)
            {
                foreach (var minion in minions)
                    if (!minion.IsValidTarget(E.Range))
                    {
                        return;
                    }
            }

            if (!(target is AIHeroClient) && Orbwalker.ActiveMode != OrbwalkerMode.LaneClear)
            {
                return;
            }

            DelayAction.Add(
                (int)(Player.AttackCastDelay * 1000 + Game.Ping / 2f),
                () =>
                {
                    Q.Cast(Game.CursorPosRaw);
                    Orbwalker.ResetAutoAttackTimer();
                    //Orbwalker.ForceTarget((AIBaseClient)target);
                });
        }

        #endregion
    }
}
