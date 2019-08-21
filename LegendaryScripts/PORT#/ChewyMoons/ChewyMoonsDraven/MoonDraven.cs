// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MoonDraven.cs" company="ChewyMoon">
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
//   The MoonDraven class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MoonDraven
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
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
    using static EnsoulSharp.SDK.Interrupter;
    using SharpDX;
    using SPrediction;

    //using Color = System.Drawing.Color;

    /// <summary>
    ///     The MoonDraven class.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
        Justification = "Reviewed. Suppression is OK here.")]
    internal class MoonDraven
    {


        /// <summary>
        ///     Gets or sets the e.
        /// </summary>
        /// <value>
        ///     The e.
        /// </value>
        public Spell E { get; set; }

        /// <summary>
        ///     Gets the mana percent.
        /// </summary>
        /// <value>
        ///     The mana percent.
        /// </value>
        public float ManaPercent
        {
            get
            {
                return this._Player.Mana / this._Player.MaxMana * 100;
            }
        }

        /// <summary>
        ///     Gets or sets the menu.
        /// </summary>
        /// <value>
        ///     The menu.
        /// </value>
        public Menu Menu { get; set; }

        /// <summary>
        ///     Gets or sets the orbwalker.
        /// </summary>
        /// <value>
        ///     The orbwalker.
        /// </value>
       //public Orbwalking.Orbwalker Orbwalker { get; set; }

        /// <summary>
        ///     Gets the _Player.
        /// </summary>
        /// <value>
        ///     The _Player.
        /// </value>
        public AIHeroClient _Player
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
        public Spell Q { get; set; }

        /// <summary>
        ///     Gets the q count.
        /// </summary>
        /// <value>
        ///     The q count.
        /// </value>
        public int QCount
        {
            get
            {
                return (this._Player.HasBuff("dravenspinning") ? 1 : 0)
                       + (this._Player.HasBuff("dravenspinningleft") ? 1 : 0) + this.QReticles.Count;
            }
        }

        /// <summary>
        ///     Gets or sets the q reticles.
        /// </summary>
        /// <value>
        ///     The q reticles.
        /// </value>
        public List<QRecticle> QReticles { get; set; }

        /// <summary>
        ///     Gets or sets the r.
        /// </summary>
        /// <value>
        ///     The r.
        /// </value>
        public Spell R { get; set; }

        /// <summary>
        ///     Gets or sets the w.
        /// </summary>
        /// <value>
        ///     The w.
        /// </value>
        public Spell W { get; set; }
        public static Font thm;
        public static Font thn;




        /// <summary>
        ///     Gets or sets the last axe move time.
        /// </summary>
        private int LastAxeMoveTime { get; set; }





        /// <summary>
        ///     Loads this instance.
        /// </summary>
        public void Load()
        {
            // Create spells
            this.Q = new Spell(SpellSlot.Q, Player.Instance.GetRealAutoAttackRange(this._Player));
            this.W = new Spell(SpellSlot.W);
            this.E = new Spell(SpellSlot.E, 1050);
            this.R = new Spell(SpellSlot.R);

            this.E.SetSkillshot(0.25f, 130, 1400, false,false, SkillshotType.Line);
            this.R.SetSkillshot(0.4f, 160, 2000, true,false, SkillshotType.Line);
            thm = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 15, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            thn = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 22, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            this.QReticles = new List<QRecticle>();

            this.CreateMenu();

            Chat.Print("<font color=\"#7CFC00\"><b>MoonDraven:</b></font> Loaded");

            AIBaseClient.OnNewPath += this.AIBaseClient_OnNewPath;
            GameObject.OnCreate += this.GameObjectOnOnCreate;
            GameObject.OnDelete += this.GameObjectOnOnDelete;
            Gapcloser.OnGapcloser += this.AntiGapcloserOnOnEnemyGapcloser;
            Interrupter.OnInterrupterSpell += this.Interrupter2OnOnInterruptableTarget;
            Drawing.OnDraw += this.DrawingOnOnDraw;
            Game.OnUpdate += this.GameOnOnUpdate;
        }

        public static void DrawFont(Font vFont, string vText, float jx, float jy, ColorBGRA jc)
        {
            vFont.DrawText(null, vText, (int)jx, (int)jy, jc);
        }





        /// <summary>
        ///     Called on an enemy gapcloser.
        /// </summary>
        /// <param name="gapcloser">The gapcloser.</param>
        private void AntiGapcloserOnOnEnemyGapcloser(AIHeroClient sender, Gapcloser.GapcloserArgs gapcloser)
        {
            if (!this.Menu["UseEGapcloser"].GetValue<MenuBool>().Enabled || !this.E.IsReady()
                || !sender.IsValidTarget(this.E.Range))
            {
                return;
            }

            this.E.Cast(sender);
        }

        /// <summary>
        ///     Catches the axe.
        /// </summary>
        private void CatchAxe()
        {
            var catchOption = this.Menu["AxeMode"].GetValue<MenuList>().Index;
            if (((catchOption == 0 && Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                 || (catchOption == 1 && Orbwalker.ActiveMode != OrbwalkerMode.None))
                || catchOption == 2)
            {
                var bestReticle =
                    this.QReticles.Where(
                        x =>
                        x.Object.Position.Distance(Game.CursorPosRaw)
                        < this.Menu["CatchAxeRange"].GetValue<MenuSlider>().Value)
                        .OrderBy(x => x.Position.Distance(this._Player.Position))
                        .ThenBy(x => x.Position.Distance(Game.CursorPosRaw))
                        .ThenBy(x => x.ExpireTime)
                        .FirstOrDefault();

                if (bestReticle != null && bestReticle.Object.Position.Distance(this._Player.Position) > 100)
                {
                    var eta = 1000 * (this._Player.Distance(bestReticle.Position) / this._Player.MoveSpeed);
                    var expireTime = bestReticle.ExpireTime - Environment.TickCount;

                    if (eta >= expireTime && this.Menu["UseWForQ"].GetValue<MenuBool>().Enabled)
                    {
                        this.W.Cast();
                    }

                    if (this.Menu["DontCatchUnderTurret"].GetValue<MenuBool>().Enabled) // debug this?
                    {
                        var turret = Menu["CTurret"].GetValue<MenuKeyBind>().Active;
                        // If we're under the turret as well as the axe, catch the axe
                        if (_Player.IsUnderEnemyTurret() && bestReticle.Object.Position.IsUnderEnemyTurret())//(Player.IsUnderEnemyTurret() && bestReticle.Object.Position.IsUnderEnemyTurret())//this._Player.UnderTurret(true) && bestReticle.Object.Position.UnderTurret(true))
                        {
                            Orbwalker.SetOrbwalkerPosition(bestReticle.Position);

                        }
                        else if (!bestReticle.Position.IsUnderEnemyTurret())//bestReticle.Position.IsUnderAllyTurret(true))
                        {
                            Orbwalker.SetOrbwalkerPosition(bestReticle.Position);
                        }
                    }
                    else
                    {
                        Orbwalker.SetOrbwalkerPosition(bestReticle.Position);
                    }
                }
                else
                {
                    Orbwalker.SetOrbwalkerPosition(Game.CursorPosRaw);
                }
            }
            else
            {
                //Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPosRaw);
                Orbwalker.SetOrbwalkerPosition(Game.CursorPosRaw);
            }
        }

        /// <summary>
        ///     Does the combo.
        /// </summary>
        private void Combo()
        {
            var target = TargetSelector.GetTarget(this.E.Range, DamageType.Physical);

            if (!target.IsValidTarget())
            {
                return;
            }

            var useQ = this.Menu["UseQCombo"].GetValue<MenuBool>().Enabled;
            var useW = this.Menu["UseWCombo"].GetValue<MenuBool>().Enabled;
            var useE = this.Menu["UseECombo"].GetValue<MenuBool>().Enabled;
            var useR = this.Menu["UseRCombo"].GetValue<MenuBool>().Enabled;

            if (useQ && this.QCount < this.Menu["MaxAxes"].GetValue<MenuSlider>().Value - 1 && this.Q.IsReady()
                && target.InAutoAttackRange() && !this._Player.Spellbook.IsAutoAttack)
            {
                this.Q.Cast();
            }

            if (useW && this.W.IsReady()
                && this.ManaPercent > this.Menu["UseWManaPercent"].GetValue<MenuSlider>().Value)
            {
                if (this.Menu["UseWSetting"].GetValue<MenuBool>().Enabled)
                {
                    this.W.Cast();
                }
                else
                {
                    if (!this._Player.HasBuff("dravenfurybuff"))
                    {
                        this.W.Cast();
                    }
                }
            }

            if (useE && this.E.IsReady())
            {
                this.E.Cast(target);
            }

            if (!useR || !this.R.IsReady())
            {
                return;
            }

            // Patented Advanced Algorithms D321987
            var killableTarget =
                GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(2000))
                    .FirstOrDefault(
                        x =>
                        this._Player.GetSpellDamage(x, SpellSlot.R) * 2 > x.Health
                        && (!target.InAutoAttackRange() || this._Player.CountEnemyHeroesInRange(this.E.Range) > 2));

            if (killableTarget != null)
            {
                this.R.Cast(killableTarget);
            }
        }

        /// <summary>
        ///     Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            this.Menu = new Menu("MoonDraven", "cmMoonDraven", true);

            // Target Selector
            var tsMenu = new Menu("Target Selector", "ts");
            TargetSelector.GetTarget(tsMenu);
            this.Menu.Add(tsMenu);

            // Combo
            var comboMenu = new Menu("Combo", "combo");
            comboMenu.Add(new MenuBool("UseQCombo", "Use Q"));
            comboMenu.Add(new MenuBool("UseWCombo", "Use W"));
            comboMenu.Add(new MenuBool("UseECombo", "Use E"));
            comboMenu.Add(new MenuBool("UseRCombo", "Use R"));
            comboMenu.Add(new MenuBool("DrawTR", "Draw Text Under Turret"));
            comboMenu.Add(new MenuKeyBind("CTurret", "Don't GO [Q] UnderTurret", System.Windows.Forms.Keys.T, KeyBindType.Toggle)).Permashow();
            this.Menu.Add(comboMenu);

            // Harass
            var harassMenu = new Menu("Harass", "harass");
            harassMenu.Add(new MenuBool("UseEHarass", "Use E"));
            harassMenu.Add(new MenuKeyBind("UseHarassToggle", "Harass! (Toggle)", System.Windows.Forms.Keys.G, KeyBindType.Toggle));
            this.Menu.Add(harassMenu);

            // Lane Clear
            var laneClearMenu = new Menu("Wave Clear", "waveclear");
            laneClearMenu.Add(new MenuBool("UseQWaveClear", "Use Q"));
            laneClearMenu.Add(new MenuBool("UseWWaveClear", "Use W"));
            laneClearMenu.Add(new MenuBool("UseEWaveClear", "Use E"));
            laneClearMenu.Add(new MenuSlider("WaveClearManaPercent", "Mana Percent", 0, 50, 100));
            this.Menu.Add(laneClearMenu);

            // Axe Menu
            var axeMenu = new Menu("Axe Settings", "axeSetting");
            axeMenu.Add(new MenuList("AxeMode", "Catch Axe on Mode:", new[] { "Combo", "Any", "Always" }) { Index = 2 });
            axeMenu.Add(new MenuSlider("CatchAxeRange", "Catch Axe Range", 800, 120, 1500));
            axeMenu.Add(new MenuSlider("MaxAxes", "Maximum Axes", 2, 1, 3));
            axeMenu.Add(new MenuBool("UseWForQ", "Use W if Axe too far"));
            axeMenu.Add(new MenuBool("DontCatchUnderTurret", "Don't Catch Axe Under Turret"));
            this.Menu.Add(axeMenu);
            // Drawing
            var drawMenu = new Menu("Drawing", "draw");
            drawMenu.Add(new MenuBool("DrawE", "Draw E"));
            drawMenu.Add(new MenuBool("DrawAxeLocation", "Draw Axe Location"));
            drawMenu.Add(new MenuBool("DrawAxeRange", "Draw Axe Catch Range"));
            this.Menu.Add(drawMenu);
            // Misc Menu
            var miscMenu = new Menu("Misc", "misc");
            miscMenu.Add(new MenuBool("UseWSetting", "Use W Instantly(When Available)"));
            miscMenu.Add(new MenuBool("UseEGapcloser", "Use E on Gapcloser"));
            miscMenu.Add(new MenuBool("UseEInterrupt", "Use E to Interrupt"));
            miscMenu.Add(new MenuSlider("UseWManaPercent", "Use W Mana Percent", 0, 50, 100));
            miscMenu.Add(new MenuBool("UseWSlow", "Use W if Slowed"));
            this.Menu.Add(miscMenu);
            this.Menu.Attach();
        }

        /// <summary>
        ///     Called when the game draws itself.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>

        private void DrawingOnOnDraw(EventArgs args)
        {
            var drawE = this.Menu["DrawE"].GetValue<MenuBool>().Enabled;
            var drawAxeLocation = this.Menu["DrawAxeLocation"].GetValue<MenuBool>().Enabled;
            var drawAxeRange = this.Menu["DrawAxeRange"].GetValue<MenuBool>().Enabled;

            if (drawE)
            {
                Render.Circle.DrawCircle(
                    ObjectManager.Player.Position,
                    this.E.Range,
                    this.E.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawAxeLocation)
            {
                var bestAxe =
                    this.QReticles.Where(
                        x =>
                        x.Position.Distance(Game.CursorPosRaw) < this.Menu["CatchAxeRange"].GetValue<MenuSlider>().Value)
                        .OrderBy(x => x.Position.Distance(this._Player.Position))
                        .ThenBy(x => x.Position.Distance(Game.CursorPosRaw))
                        .FirstOrDefault();

                if (bestAxe != null)
                {
                    Render.Circle.DrawCircle(bestAxe.Position, 120, Color.LimeGreen);
                }

                foreach (var axe in
                    this.QReticles.Where(x => x.Object.NetworkId != (bestAxe == null ? 0 : bestAxe.Object.NetworkId)))
                {
                    Render.Circle.DrawCircle(axe.Position, 120, Color.Yellow);
                }
            }

            if (drawAxeRange)
            {
                Render.Circle.DrawCircle(
                    Game.CursorPosRaw,
                    this.Menu["CatchAxeRange"].GetValue<MenuSlider>().Value,
                    Color.DodgerBlue);
            }

            if (Menu["DrawTR"].GetValue<MenuBool>().Enabled)
            {

                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                if (Menu["CTurret"].GetValue<MenuKeyBind>().Active)
                {
                    DrawFont(thm, "Use E Under Turret : Disable", (float)(ft[0] - 70), (float)(ft[1] + 50), SharpDX.Color.White);
                }
                else
                {
                    DrawFont(thm, "Use E Under Turret : Enable", (float)(ft[0] - 70), (float)(ft[1] + 50), SharpDX.Color.Red);
                }
            }
        }


        /// <summary>
        ///     Called when a game object is created.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void GameObjectOnOnCreate(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Draven_Base_Q_reticle_self.troy"))
            {
                return;
            }

            this.QReticles.Add(new QRecticle(sender, Environment.TickCount + 1800));
            DelayAction.Add(1800, () => this.QReticles.RemoveAll(x => x.Object.NetworkId == sender.NetworkId));
        }

        /// <summary>
        ///     Called when a game object is deleted.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void GameObjectOnOnDelete(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Draven_Base_Q_reticle_self.troy"))
            {
                return;
            }

            this.QReticles.RemoveAll(x => x.Object.NetworkId == sender.NetworkId);
        }

        /// <summary>
        ///     Called when the game updates.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void GameOnOnUpdate(EventArgs args)
        {
            this.QReticles.RemoveAll(x => x.Object.IsDead);

            this.CatchAxe();

            if (this.W.IsReady() && this.Menu["UseWSlow"].GetValue<MenuBool>().Enabled && this._Player.HasBuffOfType(BuffType.Slow))
            {
                this.W.Cast();
            }
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Harass:
                    this.Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    this.LaneClear();
                    break;
                case OrbwalkerMode.Combo:
                    this.Combo();
                    break;
            }

            if (this.Menu["UseHarassToggle"].GetValue<MenuBool>().Enabled)
            {
                this.Harass();
            }
        }

        /// <summary>
        ///     Harasses the enemy.
        /// </summary>
        private void Harass()
        {
            var target = TargetSelector.GetTarget(this.E.Range, DamageType.Physical);

            if (!target.IsValidTarget())
            {
                return;
            }

            if (this.Menu["UseEHarass"].GetValue<MenuBool>().Enabled && this.E.IsReady())
            {
                this.E.Cast(target);
            }
        }

        /// <summary>
        ///     Interrupts an interruptable target.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="Interrupter2.InterruptableTargetEventArgs" /> instance containing the event data.</param>
        private void Interrupter2OnOnInterruptableTarget(
            AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (!this.Menu["UseEInterrupt"].GetValue<MenuBool>().Enabled || !this.E.IsReady() || !sender.IsValidTarget(this.E.Range))
            {
                return;
            }

            if (args.DangerLevel == Interrupter.DangerLevel.Medium || args.DangerLevel == Interrupter.DangerLevel.High)
            {
                this.E.Cast(sender);
            }
        }

        /// <summary>
        ///     Clears the lane of minions.
        /// </summary>
        private void LaneClear()
        {
            var useQ = this.Menu["UseQWaveClear"].GetValue<MenuBool>().Enabled;
            var useW = this.Menu["UseWWaveClear"].GetValue<MenuBool>().Enabled;
            var useE = this.Menu["UseEWaveClear"].GetValue<MenuBool>().Enabled;

            if (this.ManaPercent < this.Menu["WaveClearManaPercent"].GetValue<MenuSlider>().Value)
            {
                return;
            }

            if (useQ && this.QCount < this.Menu["MaxAxes"].GetValue<MenuSlider>().Value - 1 && Q.IsReady() &&
                /*&& this.Orbwalker.GetTarget() is AIMinionClient &&*/ !this._Player.Spellbook.IsAutoAttack
                && !this._Player.IsWindingUp)
            {
                this.Q.Cast();
            }

            if (useW && this.W.IsReady()
                && this.ManaPercent > this.Menu["UseWManaPercent"].GetValue<MenuSlider>().Value)
            {
                if (this.Menu["UseWSetting"].GetValue<MenuBool>().Enabled)
                {
                    this.W.Cast();
                }
                else
                {
                    if (!this._Player.HasBuff("dravenfurybuff"))
                    {
                        this.W.Cast();
                    }
                }
            }

            if (!useE || !this.E.IsReady())
            {
                return;
            }

            var bestLocation = this.E.GetLineFarmLocation(MinionManager.GetMinions(this.E.Range));

            if (bestLocation.MinionsHit > 1)
            {
                this.E.Cast(bestLocation.Position);
            }
        }

        /// <summary>
        ///     Fired when the OnNewPath event is called.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectNewPathEventArgs" /> instance containing the event data.</param>
        private void AIBaseClient_OnNewPath(AIBaseClient sender, AIBaseClientNewPathEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            this.CatchAxe();
        }



        /// <summary>
        ///     A represenation of a Q circle on Draven.
        /// </summary>
        internal class QRecticle
        {


            /// <summary>
            ///     Initializes a new instance of the <see cref="QRecticle" /> class.
            /// </summary>
            /// <param name="rectice">The rectice.</param>
            /// <param name="expireTime">The expire time.</param>
            public QRecticle(GameObject rectice, int expireTime)
            {
                this.Object = rectice;
                this.ExpireTime = expireTime;
            }





            /// <summary>
            ///     Gets or sets the expire time.
            /// </summary>
            /// <value>
            ///     The expire time.
            /// </value>
            public int ExpireTime { get; set; }

            /// <summary>
            ///     Gets or sets the object.
            /// </summary>
            /// <value>
            ///     The object.
            /// </value>
            public GameObject Object { get; set; }

            /// <summary>
            ///     Gets the position.
            /// </summary>
            /// <value>
            ///     The position.
            /// </value>
            public Vector3 Position
            {
                get
                {
                    return this.Object.Position;
                }
            }


        }
    }
}
