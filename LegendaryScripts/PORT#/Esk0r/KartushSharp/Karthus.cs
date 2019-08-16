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
using SharpDX.Direct3D9;
using static EnsoulSharp.SDK.Interrupter;

namespace KarthusSharp
{
    /*
     * LaneClear:
     * - allow AA on Tower, Ward (don't Q wards)
     * - improve somehow
     * - https://github.com/trus/L-/blob/master/TRUSt%20in%20my%20Karthus/Program.cs
     * 
     * Ult KS:
     * - don't KS anymore if enemy is recalling and would arrive base before ult went through (have to include BaseUlt functionality)
     * - It also ulted while taking hits from enemy tower.
     * 
     * Misc:
     * - add don't use spells until lvl x etc.
     * - Recode
     * - Onspellcast if q farm enabled, disable AA in beforeattack and start timer that lasts casttime
    * */

    internal class Karthus
    {
        readonly Menu _menu;

        private readonly Spell _spellQ;
        private readonly Spell _spellW;
        private readonly Spell _spellE;
        private readonly Spell _spellR;

        private const float SpellQWidth = 160f;
        private const float SpellWWidth = 160f;

        private bool _comboE;

       // private static Orbwalking.Orbwalker Orbwalker;

        public Karthus()
        {
            if (ObjectManager.Player.CharacterName != "Karthus")
                return;

            _menu = new Menu("KarthusSharp", "KarthusSharp",true);

            var targetSelectorMenu = new Menu("Target Selector", "TargetSelector");
            TargetSelector.GetTarget(targetSelectorMenu);
            _menu.Add(targetSelectorMenu);

            //Orbwalker = new Orbwalking.Orbwalker(_menu.Add(new Menu("Orbwalking", "Orbwalking")));

            var comboMenu = _menu.Add(new Menu("Combo", "Combo"));
            comboMenu.Add(new MenuBool("comboQ", "Use Q"));
            comboMenu.Add(new MenuBool("comboW", "Use W"));
            comboMenu.Add(new MenuBool("comboE", "Use E"));
            comboMenu.Add(new MenuBool("comboAA", "Use AA"));
            comboMenu.Add(new MenuSlider("comboWPercent", "Use W until Mana %",10));
            comboMenu.Add(new MenuSlider("comboEPercent", "Use E until Mana %",15));
            comboMenu.Add(new MenuBool("comboMove", "Orbwalk/Move"));

            var harassMenu = _menu.Add(new Menu("Harass", "Harass"));
            harassMenu.Add(new MenuBool("harassQ", "Use Q"));
            harassMenu.Add(new MenuSlider("harassQPercent", "Use Q until Mana %",15));
            harassMenu.Add(new MenuBool("harassQLasthit", "Prioritize Last Hit"));
            harassMenu.Add(new MenuBool("harassMove", "Orbwalk/Move"));

            var farmMenu = _menu.Add(new Menu("Farming", "Farming"));
            farmMenu.Add(new MenuList("farmQ", "Use Q", new[] { "Last Hit", "Lane Clear", "Both", "No" }){Index = 0 });
            farmMenu.Add(new MenuBool("farmE", "Use E in Lane Clear"));
            farmMenu.Add(new MenuBool("farmAA", "Use AA in Lane Clear"));
            farmMenu.Add(new MenuSlider("farmQPercent", "Use Q until Mana %",0,10,100));
            farmMenu.Add(new MenuSlider("farmEPercent", "Use E until Mana %", 0, 20, 100));
            farmMenu.Add(new MenuBool("farmMove", "Orbwalk/Move"));

            var notifyMenu = _menu.Add(new Menu("Notify on R killable enemies", "Notify"));
            notifyMenu.Add(new MenuBool("notifyR", "Text Notify"));
            notifyMenu.Add(new MenuBool("notifyPing", "Ping Notify"));

            var drawMenu = _menu.Add(new Menu("Drawing", "Drawing"));
            drawMenu.Add(new MenuBool("drawQ", "Draw Q range"));

            var miscMenu = _menu.Add(new Menu("Misc", "Misc"));
            miscMenu.Add(new MenuBool("ultKS", "Ultimate KS"));
            miscMenu.Add(new MenuBool("autoCast", "Auto Combo/LaneClear if dead"));
            miscMenu.Add(new MenuBool("packetCast", "Packet Cast"));

            _spellQ = new Spell(SpellSlot.Q, 875);
            _spellW = new Spell(SpellSlot.W, 1000);
            _spellE = new Spell(SpellSlot.E, 505);
            _spellR = new Spell(SpellSlot.R, 20000f);

            _spellQ.SetSkillshot(1f, 160, float.MaxValue, false, SkillshotType.Circle);
            _spellW.SetSkillshot(.5f, 70, float.MaxValue, false, SkillshotType.Circle);
            _spellE.SetSkillshot(1f, 505, float.MaxValue, false, SkillshotType.Circle);
            _spellR.SetSkillshot(3f, float.MaxValue, float.MaxValue, false, SkillshotType.Circle);

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            Orbwalker.OnAction += Orbwalking_BeforeAttack;

            Chat.Print("<font color=\"#1eff00\">KarthusSharp by DeathGODX</font> - <font color=\"#00BFFF\">Loaded</font>");
        }

        void Game_OnUpdate(EventArgs args)
        {
            if (_menu["ultKS"].GetValue<MenuBool>())
                UltKs();
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Orbwalker.AttackState =(_menu["comboAA"].GetValue<MenuBool>() || ObjectManager.Player.Mana < 100); //if no mana, allow auto attacks!
                    Orbwalker.MovementState =(_menu["comboMove"].GetValue<MenuBool>());
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Orbwalker.AttackState =(true);
                    Orbwalker.MovementState =(_menu["harassMove"].GetValue<MenuBool>());
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    Orbwalker.AttackState =(_menu["farmAA"].GetValue<MenuBool>() || ObjectManager.Player.Mana < 100);
                    Orbwalker.MovementState =(_menu["farmMove"].GetValue<MenuBool>());
                    LaneClear();
                    break;
                case OrbwalkerMode.LastHit:
                    Orbwalker.AttackState =(true);
                    Orbwalker.MovementState =(_menu["farmMove"].GetValue<MenuBool>());
                    LastHit();
                    break;
                default:
                    Orbwalker.AttackState =(true);
                    Orbwalker.MovementState =(true);
                    RegulateEState();

                    if (_menu["autoCast"].GetValue<MenuBool>())
                        if (IsInPassiveForm())
                            if (!Combo())
                                LaneClear(true);

                    break;
            }
        }

        private void Orbwalking_BeforeAttack(Object sender, OrbwalkerActionArgs args)
        {
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                args.Process = !_spellQ.IsReady();
            }
            else if (Orbwalker.ActiveMode == OrbwalkerMode.LastHit)
            {
                bool farmQ = _menu["farmQ"].GetValue<MenuList>().Index == 0 || _menu["farmQ"].GetValue<MenuList>().Index == 2;
                args.Process = !(farmQ && _spellQ.IsReady() && GetManaPercent() >= _menu["farmQPercent"].GetValue<MenuSlider>().Value);
            }
        }

        public float GetManaPercent()
        {
            return (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana) * 100f;
        }

        public bool PacketsNoLel()
        {
            return _menu["packetCast"].GetValue<MenuBool>();
        }

        bool Combo()
        {
            bool anyQTarget = false;

            if (_menu["comboW"].GetValue<MenuBool>())
                CastW(TargetSelector.GetTarget(_spellW.Range, DamageType.Magical), _menu["comboWPercent"].GetValue<MenuSlider>().Value);

            if (_menu["comboE"].GetValue<MenuBool>() && _spellE.IsReady() && !IsInPassiveForm())
            {
                var target = TargetSelector.GetTarget(_spellE.Range, DamageType.Magical);

                if (target != null)
                {
                    var enoughMana = GetManaPercent() >= _menu["comboEPercent"].GetValue<MenuSlider>().Value;

                    if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ToggleState == 1)
                    {
                        if (ObjectManager.Player.Distance(target.Position) <= _spellE.Range && enoughMana)
                        {
                            _comboE = true;
                            _spellE.Cast();
                        }
                    }
                    else if (!enoughMana)
                        RegulateEState(true);
                }
                else
                    RegulateEState();
            }

            if (_menu["comboQ"].GetValue<MenuBool>() && _spellQ.IsReady())
            {
                var target = TargetSelector.GetTarget(_spellQ.Range, DamageType.Magical);

                if (target != null)
                {
                    anyQTarget = true;
                    CastQ(target);
                }
            }

            return anyQTarget;
        }

        void Harass()
        {
            if (_menu["harassQLasthit"].GetValue<MenuBool>())
                LastHit();

            if (_menu["harassQ"].GetValue<MenuBool>())
                CastQ(TargetSelector.GetTarget(_spellQ.Range, DamageType.Magical), _menu["harassQPercent"].GetValue<MenuSlider>().Value);
        }

        void LaneClear(bool ignoreConfig = false)
        {
            var farmQ = ignoreConfig || _menu["farmQ"].GetValue<MenuList>().Index == 1 || _menu["farmQ"].GetValue<MenuList>().Index == 2;
            var farmE = ignoreConfig || _menu["farmE"].GetValue<MenuBool>();

            List<AIBaseClient> minions;

            bool jungleMobs;
            if (farmQ && _spellQ.IsReady())
            {
                minions = GameObjects.Jungle.Where(e => e.IsValidTarget(_spellQ.Range) && e.IsMinion())
                           .Cast<AIBaseClient>().ToList();
                minions.RemoveAll(x => x.MaxHealth <= 5); //filter wards the ghetto method lel

                jungleMobs = minions.Any(x => x.Team == GameObjectTeam.Neutral);

                _spellQ.Width = SpellQWidth;
                var farmInfo = _spellQ.GetCircularFarmLocation(minions, _spellQ.Width);

                if (farmInfo.MinionsHit >= 1)
                    CastQ(farmInfo.Position, jungleMobs ? 0 : _menu["farmQPercent"].GetValue<MenuSlider>().Value);
            }

            if (!farmE || !_spellE.IsReady() || IsInPassiveForm())
                return;
            _comboE = false;
            minions = GameObjects.Jungle.Where(e => e.IsValidTarget(_spellE.Range) && e.IsMinion())
                           .Cast<AIBaseClient>().ToList();
            minions.RemoveAll(x => x.MaxHealth <= 5); //filter wards the ghetto method lel

            jungleMobs = minions.Any(x => x.Team == GameObjectTeam.Neutral);

            var enoughMana = GetManaPercent() > _menu["farmEPercent"].GetValue<MenuSlider>().Value;

            if (enoughMana && ((minions.Count >= 3 || jungleMobs) && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ToggleState == 1))
                _spellE.CastOnUnit(ObjectManager.Player);
            else if (!enoughMana || ((minions.Count <= 2 && !jungleMobs) && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ToggleState == 2))
                RegulateEState(!enoughMana);
        }

        void LastHit()
        {
            var farmQ = _menu["farmQ"].GetValue<MenuList>().Index == 0 || _menu["farmQ"].GetValue<MenuList>().Index == 2;

            if (!farmQ || !_spellQ.IsReady())
                return;
            var minions = GameObjects.EnemyMinions.Where(e => e.IsValidTarget(_spellQ.Range) && e.IsMinion())
                           .Cast<AIBaseClient>().ToList();
var miniond = GameObjects.EnemyMinions.Where(m => m.IsValidTarget(_spellQ.Range)).OrderBy(m => m.Health).FirstOrDefault();
            minions.RemoveAll(x => x.MaxHealth <= 5); //filter wards the ghetto method lel

            /*foreach (var minion in minions.Where(x => ObjectManager.Player.GetSpellDamage(x, SpellSlot.Q, 1) >= //FirstDamage = multitarget hit, differentiate! (check radius around mob predicted pos)
                                                      HealthPrediction.HealthPredictionType.Simulated))*/
            {
                CastQ(miniond, _menu["farmQPercent"].GetValue<MenuSlider>().Value);
            }
        }

        void UltKs()
        {
            if (!_spellR.IsReady())
                return;
            //var time = Utils.TickCount;

            List<AIHeroClient> ultTargets = new List<AIHeroClient>();

            foreach (var target in Program.Helper.EnemyInfo.Where(x => //need to check if recently recalled (for cases when no mana for baseult)
                x.Player.IsValid &&
                !x.Player.IsDead &&
                x.Player.IsEnemy &&
                //!(x.RecallInfo.Recall.Status == Packet.S2C.Recall.RecallStatus.RecallStarted && x.RecallInfo.GetRecallCountdown() < 3100) && //let BaseUlt handle this one
                ((!x.Player.IsVisible && x.LastSeen < 10000) || (x.Player.IsVisible && x.Player.IsValidTarget())) &&
                ObjectManager.Player.GetSpellDamage(x.Player, SpellSlot.R) >= Program.Helper.GetTargetHealth(x, (int)(_spellR.Delay * 1000f))))
            {
                if (target.Player.IsVisible || (!target.Player.IsVisible && target.LastSeen < 2750)) //allies still attacking target? prevent overkill
                    if (Program.Helper.OwnTeam.Any(x => !x.IsMe && x.Distance(target.Player) < 1600))
                        continue;

                if (IsInPassiveForm() || !Program.Helper.EnemyTeam.Any(x => x.IsValid && !x.IsDead && (x.IsVisible || (!x.IsVisible && Program.Helper.GetPlayerInfo(x).LastSeen < 2750)) && ObjectManager.Player.Distance(x) < 1600)) //any other enemies around? dont ult unless in passive form
                    ultTargets.Add(target.Player);
            }

            int targets = ultTargets.Count();

            if (targets > 0)
            {
                //dont ult if Zilean is nearby the target/is the target and his ult is up
                var zilean = Program.Helper.EnemyTeam.FirstOrDefault(x => x.CharacterName == "Zilean" && (x.IsVisible || (!x.IsVisible && Program.Helper.GetPlayerInfo(x).LastSeen < 3000)) && (x.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Ready ||
                            (x.Spellbook.GetSpell(SpellSlot.R).Level > 0 &&
                            x.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.CooldownOrSealed &&
                            x.Mana >= x.Spellbook.GetSpell(SpellSlot.R).ManaCost)));

                if (zilean != null)
                {
                    int inZileanRange = ultTargets.Count(x => x.Distance(zilean) < 2500); //if multiple, shoot regardless

                    if (inZileanRange > 0)
                        targets--; //remove one target, because zilean can save one
                }

                if (targets > 0)
                    _spellR.Cast();
            }
        }

        void RegulateEState(bool ignoreTargetChecks = false)
        {
            if (!_spellE.IsReady() || IsInPassiveForm() ||
                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ToggleState != 2)
                return;
            var target = TargetSelector.GetTarget(_spellE.Range, DamageType.Magical);
            var minions = GameObjects.EnemyMinions.Where(e => e.IsValidTarget(_spellE.Range) && e.IsMinion())
                           .Cast<AIBaseClient>().ToList();

            if (!ignoreTargetChecks && (target != null || (!_comboE && minions.Count != 0)))
                return;
            _spellE.CastOnUnit(ObjectManager.Player);
            _comboE = false;
        }

        void CastQ(AIBaseClient target, int minManaPercent = 0)
        {
            if (!_spellQ.IsReady() || !(GetManaPercent() >= minManaPercent))
                return;
            if (target == null)
                return;
            _spellQ.Width = GetDynamicQWidth(target);
            _spellQ.Cast(target);
        }

        void CastQ(Vector2 pos, int minManaPercent = 0)
        {
            if (!_spellQ.IsReady())
                return;
            if (GetManaPercent() >= minManaPercent)
                _spellQ.Cast(pos);
        }

        void CastW(AIBaseClient target, int minManaPercent = 0)
        {
            if (!_spellW.IsReady() || !(GetManaPercent() >= minManaPercent))
                return;
            if (target == null)
                return;
            _spellW.Width = GetDynamicWWidth(target);
            _spellW.Cast(target);
        }

        float GetDynamicWWidth(AIBaseClient target)
        {
            return Math.Max(70, (1f - (ObjectManager.Player.Distance(target) / _spellW.Range)) * SpellWWidth);
        }

        float GetDynamicQWidth(AIBaseClient target)
        {
            return Math.Max(30, (1f - (ObjectManager.Player.Distance(target) / _spellQ.Range)) * SpellQWidth);
        }

        static bool IsInPassiveForm()
        {
            return ObjectManager.Player.IsZombie; //!ObjectManager.Player.IsHPBarRendered;
        }

        void Drawing_OnDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                var drawQ = _menu["drawQ"].GetValue<MenuBool>().Enabled;

                if (drawQ)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _spellQ.Range, Color.DarkBlue);
            }

            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Level > 0)
            {
                var victims = "";

                //var time = Utils.TickCount;
                var targetii = TargetSelector.GetTarget(1000);
                foreach (EnemyInfo target in Program.Helper.EnemyInfo.Where(x =>
                    x.Player.IsValid &&
                    !x.Player.IsDead &&
                    x.Player.IsEnemy &&
                    ((!x.Player.IsVisible && x.LastSeen < 10000) || (x.Player.IsVisible && targetii.IsValidTarget(x.LastSeen))) &&
                    ObjectManager.Player.GetSpellDamage(x.Player, SpellSlot.R) >= Program.Helper.GetTargetHealth(x, (int)(_spellR.Delay * 1000f))))
                {
                    victims += target.Player.CharacterName + " ";

                    /*if (!_menu["notifyPing"].GetValue<MenuBool>() ||
                        (target.LastPinged != 0 && Utils.TickCount - target.LastPinged <= 11000))
                        continue;
                    if (!(ObjectManager.Player.Distance(target.Player) > 1800) ||
                        (!target.Player.IsVisible && time - target.LastSeen <= 2750))
                        continue;
                    Program.Helper.Ping(target.Player.Position);
                    target.LastPinged = Utils.TickCount;*/
                }

                if (victims != "" && _menu["notifyR"].GetValue<MenuBool>())
                {
                    Drawing.DrawText(Drawing.Width * 0.44f, Drawing.Height * 0.7f, System.Drawing.Color.GreenYellow, "Ult can kill: " + victims);

                    //use when pos works
                    //new Render.Text((int)(Drawing.Width * 0.44f), (int)(Drawing.Height * 0.7f), "Ult can kill: " + victims, 30, SharpDX.Color.Red); //.Add()
                }
            }
        }
    }
}