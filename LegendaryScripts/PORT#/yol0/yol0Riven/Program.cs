using System;
using System.Linq;
using System;
using System.Collections.Generic;
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
// ReSharper disable InconsistentNaming
namespace yol0Riven
{
    internal class Program
    {
        public static Menu Menu, ComboMenu, HarassMenu, LaneClearMenu, JungleClearMenu, Misc, KillStealMenu, Items, Drawmenu;
        private static readonly Spell _Q = new Spell(SpellSlot.Q, 260);
        private static readonly Spell _W = new Spell(SpellSlot.W, 250);
        private static readonly Spell _E = new Spell(SpellSlot.E, 325);
        private static readonly Spell _R = new Spell(SpellSlot.R, 900);
        private static readonly Items.Item _Tiamat = new Items.Item(3077, 400);
        private static readonly Items.Item _Hydra = new Items.Item(3074, 400);
        private static readonly Items.Item _Ghostblade = new Items.Item(3142, 600);
        private static Menu _menu;
        private static int qCount;
        private static int lastQCast;
        public static Item Botrk;
        public static Item Hydra;
        public static Item Tiamat;
        public static Item Bil;
        public static Item Youmuu;
        private static bool ultiOn;
        private static bool ultiReady;
        private static bool WaitForMove;
        private static Spell nextSpell;
        private static string lastSpellName = "";
        private static bool UseAttack;
        private static bool UseTiamat;
        private static AIHeroClient _target;
        private static int lastGapClose;
        //private static Orbwalker.Orbwalker _orbwalker;

        private static AIHeroClient Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Main(string[] args)
        {
            GameEvent.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad()
        {
            Chat.Print("PORTED BY DEATHGODX");
            Botrk = new Item(ItemId.Blade_of_the_Ruined_King, 400);
            Tiamat = new Item(ItemId.Tiamat_Melee_Only, 400);
            Hydra = new Item(ItemId.Ravenous_Hydra_Melee_Only, 400);
            Bil = new Item(3144, 475f);
            Youmuu = new Item(3142, 10);
            if (Player.CharacterName != "Riven")
                return;
            var MenuRiven = new Menu("yol0 Riven", "yol0 Riven", true);
            ComboMenu = new Menu("Combo Settings", "Combo");
            ComboMenu.Add(new MenuBool("useQ", "Use Q to gapclose"));
            ComboMenu.Add(new MenuBool("useR", "Use Ultimate"));
            MenuRiven.Add(ComboMenu);
            KillStealMenu = new Menu("KillSteal Settings", "KillSteal");
            KillStealMenu.Add(new MenuSeparator("KillSteal Settings", "KillSteal Settings"));
            KillStealMenu.Add(new MenuBool("ksQ", "KS with Q"));
            KillStealMenu.Add(new MenuBool("ksW", "KS with W"));
            KillStealMenu.Add(new MenuBool("ksT", "KS with Tiamat/Hydra"));
            KillStealMenu.Add(new MenuBool("ksR", "KS with R2"));
            KillStealMenu.Add(new MenuBool("ksRA", "Activate ult for KS"));
            KillStealMenu.Add(new MenuBool("ign", "Use [Ignite] KillSteal"));
            KillStealMenu.Add(new MenuBool("Don't use R2 for KS", "noKS"));
            foreach (var enemy in GameObjects.EnemyHeroes)
            {
                KillStealMenu.Add(new MenuBool(enemy.CharacterName, enemy.CharacterName));
            }
            MenuRiven.Add(KillStealMenu);
            Misc = new Menu("Misc Settings", "Misc");
            Misc.Add(new MenuKeyBind("Flee", "Flee Mode", System.Windows.Forms.Keys.T, KeyBindType.Press));
            Misc.Add(new Menu("Auto Stun", "Stun"));
            foreach (var enemy in GameObjects.EnemyHeroes)
            {
                Misc.Add(new MenuBool(enemy.CharacterName, "Stun " + enemy.CharacterName));
            }
            Misc.Add(new MenuBool("gapclose", "Auto W Gapclosers"));
            Misc.Add(new MenuBool("interrupt", "Auto W Interruptible Spells"));
            Misc.Add(new MenuBool("keepalive", "Keep Q Alive"));
            MenuRiven.Add(Misc);
            Drawmenu.Add(new MenuBool("drawRange", "Draw Engage Range"));
            Drawmenu.Add(new MenuBool("drawTarget", "Draw Current Target"));
            Drawmenu.Add(new MenuBool("drawDamage", "Draw Damage on Healthbar"));
            MenuRiven.Add(Drawmenu);
            MenuRiven.Attach();

            //_menu.Add(new Menu("Orbwalker", "Orbwalker"));
            //_menu.Add(new Menu("Target Selector", "Target Selector"));

            //_orbwalker = new Orbwalker.Orbwalker(_menu.SubMenu("Orbwalker"));
            //TargetSelector.AddToMenu(_menu.SubMenu("Target Selector"));
            //Utility.HpBarDamageIndicator.DamageToUnit = GetDamage;

            _R.SetSkillshot(0.25f, 60f, 2200, false, SkillshotType.Cone);
            _E.SetSkillshot(0, 0, 1450, false, SkillshotType.Line);

            Game.OnUpdate += OnUpdate;
            Orbwalker.OnAction += BeforeAttack;
            Orbwalker.OnAction += AfterAttack;
            AIBaseClient.OnProcessSpellCast += OnProcessSpellCast;
            AIBaseClient.OnPlayAnimation += OnPlayAnimation;
            AttackableUnit.OnCreate += OnDamage;
            Drawing.OnDraw += OnDraw;
            Gapcloser.OnGapcloser += OnEnemyGapcloser;
            Interrupter.OnInterrupterSpell += OnInterruptableTarget;
        }

        private static void KillSecure()
        {
            foreach (var enemy in GameObjects.EnemyHeroes)
            {
                if (!enemy.IsDead && enemy.IsVisible)
                {
                    if (ultiReady && KillStealMenu["ksR"].GetValue<MenuBool>().Enabled && qCount == 2 && _Q.IsReady() &&
                        enemy.IsValidTarget(_Q.Range) && GetRDamage(enemy) + GetUltiQDamage(enemy) - 40 >= enemy.Health &&
                        !KillStealMenu[enemy.CharacterName].GetValue<MenuBool>().Enabled)
                    {
                        _R.Cast(enemy);
                    }
                    if (ultiReady && KillStealMenu["ksR"].GetValue<MenuBool>().Enabled &&
                        enemy.IsValidTarget(_R.Range - 30) && GetRDamage(enemy) - 20 >= enemy.Health &&
                        !KillStealMenu[enemy.CharacterName].GetValue<MenuBool>().Enabled)
                    {
                        _R.Cast(enemy);
                    }
                    else if (KillStealMenu["ksQ"].GetValue<MenuBool>().Enabled && _Q.IsReady() &&
                        enemy.IsValidTarget(_Q.Range) && (ultiOn ? GetUltiQDamage(enemy) : GetQDamage(enemy)) - 10 >= enemy.Health)
                    {
                        _Q.Cast(enemy.Position);
                    }
                    else if (KillStealMenu["ksW"].GetValue<MenuBool>().Enabled && _W.IsReady() &&
                             enemy.IsValidTarget(_W.Range) && GetWDamage(enemy) - 10 >= enemy.Health)
                    {
                        _Q.Cast(enemy.Position);
                    }
                    else if (KillStealMenu["ksT"].GetValue<MenuBool>().Enabled &&
                             (_Tiamat.IsReady || _Hydra.IsReady) && enemy.IsValidTarget(_Tiamat.Range))
                    {
                        if (_Tiamat.IsReady)
                            _Tiamat.Cast();
                        else if (_Hydra.IsReady)
                            _Hydra.Cast();
                    }
                    else if (!ultiReady && !ultiOn && KillStealMenu["ksR"].GetValue<MenuBool>().Enabled &&
                             KillStealMenu["ksRA"].GetValue<MenuBool>().Enabled &&
                             enemy.IsValidTarget(_R.Range - 30) && GetRDamage(enemy) - 20 >= enemy.Health &&
                             Orbwalker.ActiveMode != OrbwalkerMode.Combo)
                    {
                        _R.Cast();
                    }
                }
            }
        }

        private static void CancelAnimation()
        {
            if (WaitForMove)
                return;

            WaitForMove = true;
            var movePos = Game.CursorPosRaw;
            if (Player.Distance(_target.Position) < 600)
            {
                movePos = Player.Position.Extend(_target.Position, 100);
            }
            Player.IssueOrder(GameObjectOrder.MoveTo, movePos);
        }

        private static void AfterAttack(Object unit, OrbwalkerActionArgs target)
        {
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo && nextSpell == _Q)
            {
                _Q.Cast(_target.Position);
                nextSpell = null;
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            //Utility.HpBarDamageIndicator.Enabled = Drawmenu["drawDamage"].GetValue<MenuBool>().Enabled;
            CheckBuffs();
            KillSecure();
            if (Orbwalker.ActiveMode != OrbwalkerMode.Combo)
                AutoStun();
            if (Drawmenu["Flee"].GetValue<MenuKeyBind>().Active)
                Flee();

            if (_target == null)
                Orbwalker.MovementState = true;

            if (_target != null && _target.IsDead)
                Orbwalker.MovementState = true;

            if (Orbwalker.ActiveMode != OrbwalkerMode.Combo)
                Orbwalker.MovementState = true;

            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                if (_target == null)
                    _target = TargetSelector.GetTarget(_E.Range + _Q.Range, DamageType.Physical);

                if (_target != null &&
                    (_target.IsDead || !_target.IsVisible ||
                     !_target.IsValidTarget(_E.Range + _Q.Range + Player.AttackRange)))
                    Orbwalker.MovementState = true;

                if (_target == null)
                    Orbwalker.MovementState = true;
                else
                {
                    if (!_target.IsVisible)
                        _target = TargetSelector.GetTarget(_E.Range + _Q.Range, DamageType.Physical);

                    if (_target.IsDead)
                        _target = TargetSelector.GetTarget(_E.Range + _Q.Range, DamageType.Physical);

                    if (!_target.IsValidTarget(_E.Range + _Q.Range + Player.AttackRange))
                        _target = TargetSelector.GetTarget(_E.Range + _Q.Range, DamageType.Physical);

                    /*if (Hud.SelectedUnit != null && Hud.SelectedUnit != _target && Hud.SelectedUnit.IsVisible &&
                        Hud.SelectedUnit is AIHeroClient)
                    {
                        var unit = (AIHeroClient).ud.SelectedUnit;
                        if (unit.IsValidTarget())
                            _target = (AIHeroClient)Hud.SelectedUnit;
                    }*/

                    if (TargetSelector.SelectedTarget != null && TargetSelector.SelectedTarget != _target &&
                        TargetSelector.SelectedTarget.IsVisible &&
                        TargetSelector.SelectedTarget.IsValidTarget())
                    {
                        _target = TargetSelector.SelectedTarget;
                    }

                    if (_target != null && !_target.IsDead && _target.IsVisible)
                    {
                        GapClose(_target);
                        Combo(_target);
                    }
                    else
                    {
                        Orbwalker.MovementState = true;
                    }
                }
            }
            else
            {
                Orbwalker.MovementState = true;
                if (!Player.IsRecalling() && qCount != 0 && lastQCast + (3650 - Game.Ping / 2) < Variables.GameTimeTickCount &&
                    Drawmenu["keepalive"].GetValue<MenuBool>().Enabled)
                {
                    _Q.Cast(Game.CursorPosRaw);
                }
            }
        }

        private static void Combo(AIHeroClient target)
        {
            Orbwalker.MovementState = false;
            var noRComboDmg = DamageCalcNoR(target);
            if (_R.IsReady() && !ultiReady && noRComboDmg < target.Health &&
               ComboMenu["useR"].GetValue<MenuBool>().Enabled)
            {
                _R.Cast();
            }

            if (!(_Tiamat.IsReady || _Hydra.IsReady) && !_Q.IsReady() && _W.IsReady() &&
                target.IsValidTarget(_W.Range))
            {
                _W.Cast();
            }

            if (nextSpell == null && UseTiamat)
            {
                if (_Tiamat.IsReady)
                    _Tiamat.Cast();
                else if (_Hydra.IsReady)
                    _Hydra.Cast();

                UseTiamat = false;
                return;
            }

            if (nextSpell == null && UseAttack)
            {
                Orbwalker.LastAutoAttackTick = Variables.GameTimeTickCount + Game.Ping / 2;
                Orbwalker.MovementState = false;
                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                return;
            }

            if (nextSpell == _Q)
            {
                if (lastSpellName.Contains("Attack") && Player.IsWindingUp)
                    return;

                _Q.Cast(target.Position);
                nextSpell = null;
            }

            if (nextSpell == _W)
            {
                _W.Cast();
                nextSpell = null;
            }

            if (nextSpell == _E)
            {
                _E.Cast(target.Position);
                nextSpell = null;
            }
        }

        private static void OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (_W.IsReady() && sender.IsValidTarget(_W.Range) &&
                Drawmenu["interrupt"].GetValue<MenuBool>().Enabled)
                _W.Cast();
        }

        private static void OnEnemyGapcloser(AIHeroClient sender, Gapcloser.GapcloserArgs gapcloser)
        {
            if (_W.IsReady() && sender.IsValidTarget(_W.Range) &&
                Drawmenu["gapclose"].GetValue<MenuBool>().Enabled)
            {
                _W.Cast();
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (Drawmenu["drawRange"].GetValue<MenuBool>().Enabled)
            Render.Circle.DrawCircle(ObjectManager.Player.Position, _Q.Range, Color.Orange, 1);
            Render.Circle.DrawCircle(ObjectManager.Player.Position, _E.Range, Color.Orange, 1);
            Render.Circle.DrawCircle(ObjectManager.Player.Position, _W.Range, Color.Orange, 1);
            Render.Circle.DrawCircle(ObjectManager.Player.Position, _R.Range, Color.Orange, 1);
            if (Drawmenu["drawTarget"].GetValue<MenuBool>().Enabled && _target != null &&
                _target.IsVisible)
            {
                Render.Circle.DrawCircle(_target.Position, _target.BoundingRadius + 10, Color.Orange, 1);
                Render.Circle.DrawCircle(_target.Position, _target.BoundingRadius + 25, Color.Orange, 1);
                Render.Circle.DrawCircle(_target.Position, _target.BoundingRadius + 45, Color.Orange, 1);
            }
        }

        private static void OnDamage(GameObject sender, EventArgs args)
        {
            if (_target == null) return;
            if (lastQCast != 0 && lastQCast + 100 > Variables.GameTimeTickCount)
            {
                WaitForMove = true;
                CancelAnimation();
                Orbwalker.ResetAutoAttackTimer();
            }
            else if (lastSpellName.Contains("Attack"))
            {
                if (_Tiamat.IsReady)
                {
                    nextSpell = null;
                    UseTiamat = true;
                }
                else if (_Hydra.IsReady)
                {
                    nextSpell = null;
                    UseTiamat = true;
                }
                else if (_W.IsReady() && _target.IsValidTarget(_W.Range) && qCount != 0)
                {
                    UseTiamat = false;
                    nextSpell = _W;
                }
                else
                {
                    UseTiamat = false;
                    nextSpell = _Q;
                }
                UseAttack = false;
                Orbwalker.MovementState = true;
            }
        }

        private static void OnPlayAnimation(AIBaseClient sender, AIBaseClientPlayAnimationEventArgs args)
        {
            if (!sender.IsMe || Orbwalker.ActiveMode != OrbwalkerMode.Combo) return;

            if (args.Animation.Contains("Spell1"))
            {
                DelayAction.Add(125 + Game.Ping / 2, CancelAnimation);
            }
            if (WaitForMove && args.Animation.Contains("Run") && _target != null)
            {
                WaitForMove = false;
                Orbwalker.LastAutoAttackTick = Variables.GameTimeTickCount + Game.Ping / 2;
                Player.IssueOrder(GameObjectOrder.AttackUnit, _target);
            }
            if (WaitForMove && args.Animation.Contains("Idle") && _target != null)
            {
                WaitForMove = false;
                Orbwalker.LastAutoAttackTick = Variables.GameTimeTickCount + Game.Ping / 2;
                Player.IssueOrder(GameObjectOrder.AttackUnit, _target);
            }
        }

        private static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;
            var spellname = args.SData.Name;

            if (spellname == "RivenTriCleave")
            {
                lastQCast = Variables.GameTimeTickCount;
            }
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                lastSpellName = spellname;
                if (spellname.Contains("Attack"))
                {
                    if (_Tiamat.IsReady && _target.IsValidTarget(_Tiamat.Range))
                    {
                        nextSpell = null;
                        UseTiamat = true;
                    }
                    else if (_Hydra.IsReady && _target.IsValidTarget(_Hydra.Range))
                    {
                        nextSpell = null;
                        UseTiamat = true;
                    }
                }
                else if (spellname == "RivenTriCleave")
                {
                    var target = TargetSelector.GetTarget(600);
                    nextSpell = null;
                    DelayAction.Add(125 + Game.Ping / 2, CancelAnimation);

                    if (target.InAutoAttackRange())
                    {
                        nextSpell = null;
                        UseAttack = true;
                        return;
                    }

                    if (_W.IsReady() && _target.IsValidTarget(_W.Range))
                    {
                        nextSpell = _W;
                    }
                    else
                    {
                        nextSpell = null;
                        UseAttack = true;
                    }
                }
                else if (spellname == "RivenMartyr")
                {
                    if (_Q.IsReady())
                    {
                        nextSpell = _Q;
                        UseAttack = false;
                        UseTiamat = false;
                        //Utility.DelayAction.Add(175, delegate { nextSpell = _Q; });
                    }
                    else
                    {
                        nextSpell = null;
                        UseAttack = true;
                    }
                }
                else if (spellname == "ItemTiamatCleave")
                {
                    UseTiamat = false;
                    if (_W.IsReady() && _target.IsValidTarget(_W.Range))
                        nextSpell = _W;
                    else if (_Q.IsReady() && _target.IsValidTarget(_Q.Range))
                        nextSpell = _Q;
                }
                else if (spellname == "RivenFengShuiEngine")
                {
                    ultiOn = true;
                    if ((_Tiamat.IsReady && _target.IsValidTarget(_Tiamat.Range)) ||
                        (_Hydra.IsReady && _target.IsValidTarget(_Hydra.Range)))
                    {
                        nextSpell = null;
                        UseTiamat = true;
                    }
                    else if (_Q.IsReady() && _target.IsValidTarget(_Q.Range))
                    {
                        nextSpell = _Q;
                    }
                    else if (_E.IsReady())
                    {
                        nextSpell = _E;
                    }
                }
            }
        }

        private static void GapClose(AIHeroClient target)
        {
            var useE = _E.IsReady();
            var useQ = _Q.IsReady() && qCount < 2 &&ComboMenu["useQ"].GetValue<MenuBool>().Enabled;

            if (lastGapClose + 400 > Variables.GameTimeTickCount && lastGapClose != 0)
                return;

            lastGapClose = Variables.GameTimeTickCount;

            var aRange = Player.AttackRange + Player.BoundingRadius + target.BoundingRadius;
            var eRange = aRange + _E.Range;
            var qRange = aRange + _Q.Range;
            var eqRange = _Q.Range + _E.Range;
            var distance = Player.Distance(target.Position);
            if (distance < aRange)
                return;

            nextSpell = null;
            UseTiamat = false;
            UseAttack = true;
            if (_Ghostblade.IsReady)
                _Ghostblade.Cast();

            if (useQ && qRange > distance && !_E.IsReady())
            {
                var comboDmgNoR = DamageCalcNoR(target);
                if (_R.IsReady() && !ultiReady && comboDmgNoR < target.Health &&
                   ComboMenu["useR"].GetValue<MenuBool>().Enabled)
                {
                    _R.Cast();
                }
                _Q.Cast(target.Position);
            }
            else if (useE && eRange > distance + aRange)
            {
                var pred = _E.GetPrediction(target);
                _E.Cast(pred.CastPosition);
            }
            else if (useQ && eqRange + aRange > distance)
            {
                var pred = _E.GetPrediction(target);
                _E.Cast(pred.CastPosition);
            }
        }

        private static void BeforeAttack(Object sender, OrbwalkerActionArgs args)
        {
            var t = args.Target as AIMinionClient;
            if (t == null)
            {
                Orbwalker.MovementState = false;
            }
        }

        private static void CheckBuffs()
        {
            var ulti = false;
            var ulti2 = false;
            var q = false;

            foreach (var buff in Player.Buffs)
            {
                if (buff.Name == "rivenwindslashready")
                {
                    ulti = true;
                    ultiReady = true;
                }

                if (buff.Name == "RivenTriCleave")
                {
                    q = true;
                    qCount = buff.Count;
                }

                if (buff.Name == "RivenFengShuiEngine")
                {
                    ulti2 = true;
                    ultiOn = true;
                }
            }

            if (!q)
                qCount = 0;

            if (!ulti)
            {
                ultiReady = false;
            }

            if (!ulti2)
                ultiOn = false;
        }

        private static void AutoStun()
        {
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                return;

            foreach (var enemy in GameObjects.EnemyHeroes)
            {
                if (_W.IsReady() && enemy.IsValidTarget(_W.Range) &&
                    Misc[enemy.CharacterName].GetValue<MenuBool>().Enabled)
                {
                    _W.Cast();
                }
            }
        }

        private static void Flee()
        {
            Orbwalker.MovementState = true;
            if (_Q.IsReady())
            {
                _Q.Cast(Game.CursorPosRaw);
            }
            if (_E.IsReady())
            {
                _E.Cast(Game.CursorPosRaw);
            }
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPosRaw);
        }

        private static float GetDamage(AIHeroClient target)
        {
            if (_R.IsReady() || (ultiReady))
            {
                return (float)DamageCalcR(target);
            }
            return (float)DamageCalcNoR(target);
        }

        private static double GetRDamage(AIBaseClient target, float otherdmg = 0.0f)
        {
            if (_R.Level == 0)
                return 0.0;

            var minDmg = (80 + (40 * (_R.Level - 1))) +
                            0.6 * ((0.2 * (Player.BaseAttackDamage + Player.FlatPhysicalDamageMod)) + Player.FlatPhysicalDamageMod);

            var targetPercentHealthMissing = 100 * (1 - (target.Health - otherdmg) / target.MaxHealth);
            double dmg;
            if (targetPercentHealthMissing > 75.0f)
            {
                dmg = minDmg * 3;
            }
            else
            {
                dmg = minDmg + minDmg * (0.0267 * targetPercentHealthMissing);
            }

            var realDmg = Player.CalculateDamage(target, DamageType.Physical, dmg - 20);
            return realDmg;
        }

        private static double GetUltiQDamage(AIBaseClient target)
        {
            var dmg = 10 + ((_W.Level - 1) * 20) + 0.6 * (1.2 * (Player.BaseAttackDamage + Player.FlatPhysicalDamageMod));
            return Player.CalculateDamage(target, DamageType.Physical, dmg - 10);
        }

        private static double GetUltiWDamage(AIBaseClient target)
        {
            var totalAD = Player.FlatPhysicalDamageMod + Player.BaseAttackDamage;
            var dmg = 50 + ((_W.Level - 1) * 30) + (0.2 * totalAD + Player.FlatPhysicalDamageMod);
            return Player.CalculateDamage(target, DamageType.Physical, dmg - 10);
        }

        private static double GetQDamage(AIBaseClient target)
        {
            var totalAD = Player.FlatPhysicalDamageMod + Player.BaseAttackDamage;
            var dmg = 10 + ((_Q.Level - 1) * 20) + (0.35 + (Player.Level * 0.05)) * totalAD;
            return Player.CalculateDamage(target, DamageType.Physical, dmg - 10);
        }

        private static double GetWDamage(AIBaseClient target)
        {
            var dmg = 50 + (_W.Level * 30) + Player.FlatPhysicalDamageMod;
            return Player.CalculateDamage(target, DamageType.Physical, dmg - 10);
        }

        private static double DamageCalcNoR(AIBaseClient target)
        {
            var qDamage = GetQDamage(target);
            var wDamage = GetWDamage(target);
            var tDamage = 0.0;
            var aDamage = Player.GetAutoAttackDamage(target);
            var pDmgMultiplier = 0.2 + (0.05 * Math.Floor(Player.Level / 3.0));

            var totalAD = Player.BaseAttackDamage + Player.FlatPhysicalDamageMod;
            var pDamage = Player.CalculateDamage(target, DamageType.Physical, pDmgMultiplier * totalAD);

            if (!_Q.IsReady() && qCount == 0)
                qDamage = 0.0;

            if (!_W.IsReady())
                wDamage = 0.0;

            return wDamage + tDamage + (qDamage * (3 - qCount)) + (pDamage * (3 - qCount)) + aDamage * (3 - qCount);
        }

        public static double DamageCalcR(AIBaseClient target)
        {
            var qDamage = GetUltiQDamage(target);
            var wDamage = GetUltiWDamage(target);

            var tDamage = 0.0;
            var totalAD = Player.FlatPhysicalDamageMod + Player.BaseAttackDamage;


            var aDamage = Player.CalculateDamage(target, DamageType.Physical, 0.2 * totalAD + totalAD);
            var pDmgMultiplier = 0.2 + (0.05 * Math.Floor(Player.Level / 3.0));
            var pDamage = Player.CalculateDamage(target, DamageType.Physical,
                pDmgMultiplier * (0.2 * totalAD + totalAD));
            if (!_Q.IsReady() && qCount == 0)
                qDamage = 0.0;

            if (!_W.IsReady())
                wDamage = 0.0;


            var dmg = (pDamage * (3 - qCount)) + (aDamage * (3 - qCount)) + wDamage + tDamage +
                         (qDamage * (3 - qCount));

            var rDamage = GetRDamage(target, (float)dmg);

            if (_R.IsReady())
                rDamage = 0.0;

            return dmg + rDamage;
        }
    }
}