using System;
using System.Linq;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Utility;
using EnsoulSharp;
using SharpDX;
using Color = System.Drawing.Color;
using static EnsoulSharp.SDK.Items;
using SharpDX.Direct3D9;
using static EnsoulSharp.SDK.Interrupter;
using EnsoulSharp.SDK.Prediction;

namespace Ezreal
{
    internal class Program
    {
        public static Menu Menu, ComboMenu, HarassMenu, Auto, LaneClearMenu, JungleClearMenu, Misc, Items, KillStealMenu, Drawings;
        public static Item Botrk;
        public static Item Bil;
        public static Font Thm;
        public static Font Thn;
        private static readonly Item Tear = new Item(ItemId.Tear_of_the_Goddess, 400);
        private static readonly Item Manamune = new Item(ItemId.Manamune , 400);
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Spell Ignite;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        static void Main(string[] args)
        {
            GameEvent.OnGameLoad += OnLoadingComplete;
        }

        static void OnLoadingComplete()
        {
            if (!_Player.CharacterName.Contains("Ezreal")) return;
            Chat.Print("Doctor's Ezreal Loaded! PORTED by DEATHGODx", Color.Orange);
            Q = new Spell(SpellSlot.Q, 1150);
            Q.SetSkillshot(250, 2000, 60, false, SkillshotType.Line);
            W = new Spell(SpellSlot.W, 1000);
            W.SetSkillshot(250, 1550, 80, false, SkillshotType.Line);
            E = new Spell(SpellSlot.E, 475);
            E.SetSkillshot(250, 2000, 100, false, SkillshotType.Line);
            R = new Spell(SpellSlot.R, 5000);
            R.SetSkillshot(1000, 2000, 160, false, SkillshotType.Line);
            Botrk = new Item(ItemId.Blade_of_the_Ruined_King, 400);
            Bil = new Item(3144, 475f);
            Thm = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 32, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            Thn = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 15, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            Ignite = new Spell(ObjectManager.Player.GetSpellSlot("summonerdot"), 600);
            var MenuEz = new Menu("Doctor's Ezreal", "Ezreal", true);
            ComboMenu = new Menu("Combo Settings", "Combo");
            ComboMenu.Add(new MenuSeparator("Combo Settings", "Combo Settings"));
            ComboMenu.Add(new MenuBool("ComboQ", "Use [Q] Combo"));
            ComboMenu.Add(new MenuList("comboMode", "Q Mode:", new[] {"Fast [Q]", "[Q] After AA"}) { Index = 0 });
            ComboMenu.Add(new MenuBool("ComboW", "Use [W] Combo"));
            ComboMenu.Add(new MenuList("WMode", "W Mode:", new[] {"Fast [W]", "[W] After AA" }) { Index = 0 });
            ComboMenu.Add(new MenuSeparator("Ultimate Settings", "Ultimate Settings"));
            ComboMenu.Add(new MenuBool("ComboR", "Use [R] AoE"));
            ComboMenu.Add(new MenuSlider("MinR", "Use [R] AoE if hit x Enemies", 2, 1, 5));
            MenuEz.Add(ComboMenu);
            HarassMenu = new Menu("Harass Settings", "Harass");
            HarassMenu.Add(new MenuSeparator("Harass Settings", "Harass Settings"));
            HarassMenu.Add(new MenuBool("HarassQ", "Use [Q] Harass"));
            HarassMenu.Add(new MenuSlider("ManaQ", "Mana Harass [Q]", 40));
            HarassMenu.Add(new MenuBool("HarassW", "Use [W] Harass", false));
            HarassMenu.Add(new MenuSlider("ManaW", "Mana Harass [W]", 40));
            HarassMenu.Add(new MenuSeparator("Harass On", "Harass On"));
            foreach (var target in GameObjects.EnemyHeroes)
            {
                HarassMenu.Add(new MenuBool("haras" + target.CharacterName, "" + target.CharacterName));
            }
            MenuEz.Add(HarassMenu);
            Auto = new Menu("Auto Harass Settings", "Auto Harass");
            Auto.Add(new MenuSeparator("Auto Harass Settings", "Auto Harass Settings"));
            Auto.Add(new MenuKeyBind("Key", "Auto Harass", System.Windows.Forms.Keys.T, KeyBindType.Toggle));
            Auto.Add(new MenuBool("AutoQ", "Use [Q]"));
            Auto.Add(new MenuSlider("AutomanaQ", "Min Mana Auto [Q]", 60));
            Auto.Add(new MenuBool("AutoW", "Use [W]", false));
            Auto.Add(new MenuSlider("AutomanaW", "Min Mana Auto [W]", 60));
            Auto.Add(new MenuSeparator("Auto Harass On", "Auto Harass On"));
            foreach (var target in GameObjects.EnemyHeroes)
            {
                Auto.Add(new MenuBool("harass" + target.CharacterName, "" + target.CharacterName));
            }
            MenuEz.Add(Auto);
            LaneClearMenu = new Menu("LaneClear Settings", "LaneClear");
            LaneClearMenu.Add(new MenuSeparator("LastHit Settings", "LastHit Settings"));
            LaneClearMenu.Add(new MenuBool("QLH", "Use [Q] LastHit"));
            LaneClearMenu.Add(new MenuList("LHMode", "LastHit Mode:", new[] { "Always [Q]", "[Q] If Orb Cant Killable" }) { Index = 0 });
            LaneClearMenu.Add(new MenuSlider("LhMana", "Mana Lasthit", 50));
            LaneClearMenu.Add(new MenuSeparator("Lane Clear Settings", "Lane Clear Settings"));
            LaneClearMenu.Add(new MenuBool("QLC", "Use [Q] LaneClear"));
            LaneClearMenu.Add(new MenuList("LCMode", "LaneClear Mode:", new[] {"Always [Q]", "[Q] If Orb Cant Killable"}) { Index = 0 });
            LaneClearMenu.Add(new MenuSlider("ManaLC", "Mana LaneClear", 50));
            MenuEz.Add(LaneClearMenu);
            JungleClearMenu = new Menu("JungleClear Settings", "JungleClear");
            JungleClearMenu.Add(new MenuSeparator("JungleClear Settings", "JungleClear Settings"));
            JungleClearMenu.Add(new MenuBool("QJungle", "Use [Q] JungleClear"));
            JungleClearMenu.Add(new MenuSlider("MnJungle", "Mana JungleClear", 20));
            MenuEz.Add(JungleClearMenu);
            Misc = new Menu("Misc Settings", "Misc");
            Misc.Add(new MenuSeparator("AntiGap Settings", "AntiGap Settings"));
            Misc.Add(new MenuBool("AntiGap", "Use [E] AntiGapcloser", false));
            Misc.Add(new MenuSeparator("Ultimate On CC Settings", "Ultimate On CC Settings"));
            Misc.Add(new MenuBool("Rstun", "Use [R] Enemies Immobile"));
            Misc.Add(new MenuSeparator("Auto Stacks Settings", "Auto Stacks Settings"));
            Misc.Add(new MenuBool("Stack", "Auto Stacks In Shop"));
            Misc.Add(new MenuBool("Stackk", "Auto Stacks If Enemies Around = 0", false));
            Misc.Add(new MenuSlider("Stackkm", "Min Mana Auto Stack", 80));
            MenuEz.Add(Misc);
            Items = new Menu("Items Settings", "Items");
            Items.Add(new MenuSeparator("Items Settings", "Items Settings"));
            Items.Add(new MenuBool("BOTRK", "Use [Botrk]"));
            Items.Add(new MenuSlider("ihp", "My HP Use BOTRK <=", 50));
            Items.Add(new MenuSlider("ihpp", "Enemy HP Use BOTRK <=", 50));
            MenuEz.Add(Items);
            KillStealMenu = new Menu("KillSteal Settings", "KillSteal");
            KillStealMenu.Add(new MenuSeparator("KillSteal Settings", "KillSteal Settings"));
            KillStealMenu.Add(new MenuBool("KsQ", "Use [Q] KillSteal"));
            KillStealMenu.Add(new MenuBool("KsW", "Use [W] KillSteal"));
            KillStealMenu.Add(new MenuBool("ign", "Use [Ignite] KillSteal"));
            KillStealMenu.Add(new MenuSeparator("Ultimate Settings", "Ultimate Settings"));
            KillStealMenu.Add(new MenuBool("KsR", "Use [R] KillSteal"));
            KillStealMenu.Add(new MenuKeyBind("RKb", "[R] KillSteal Semi Manual Key", System.Windows.Forms.Keys.R, KeyBindType.Toggle));
            MenuEz.Add(KillStealMenu);
            Drawings = new Menu("Draw Settings", "Draw");
            Drawings.Add(new MenuSeparator("Drawing Settings", "Drawing Settings"));
            Drawings.Add(new MenuBool("DrawQ", "[Q] Range"));
            Drawings.Add(new MenuBool("DrawW", "[W] Range", false));
            Drawings.Add(new MenuBool("DrawE", "[E] Range", false));
            Drawings.Add(new MenuBool("Notifications", "Alerter Can Killable [R]"));
            Drawings.Add(new MenuBool("DrawAT", "Draw Auto Harass"));
            MenuEz.Add(Drawings);
            MenuEz.Attach();
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Orbwalker.OnAction += Orbwalker_CantLasthit;
            Orbwalker.OnAction += ResetAttack;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_Player.IsDead) return;

            if (Drawings["DrawQ"].GetValue<MenuBool>().Enabled && Q.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.Orange, 1);
            }

            if (Drawings["DrawW"].GetValue<MenuBool>().Enabled && W.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, Color.Orange, 1);
            }

            if (Drawings["DrawE"].GetValue<MenuBool>().Enabled && E.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.Orange, 1);
            }

            if (Drawings["Notifications"].GetValue<MenuBool>().Enabled && R.IsReady())
            {
                var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                if (target.IsValidTarget(1600) && Player.Instance.GetSpellDamage(target, SpellSlot.R) > target.Health + target.PhysicalShield)
                {
                    DrawFont(Thm, "R Can Killable " + target.CharacterName, (float)(ft[0] - 140), (float)(ft[1] + 80), SharpDX.Color.Red);
                }
            }

            if (Drawings["DrawAT"].GetValue<MenuBool>().Enabled)
            {
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                if (Auto["Key"].GetValue<MenuKeyBind>().Active)
                {
                    DrawFont(Thn, "Auto Harass : Enable", (float)(ft[0] - 60), (float)(ft[1] + 20), SharpDX.Color.White);
                }
            }
        }

        public static void DrawFont(Font vFont, string vText, float vPosX, float vPosY, ColorBGRA vColor)
        {
            vFont.DrawText(null, vText, (int)vPosX, (int)vPosY, vColor);
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.Combo))
            {
                Combo();
            }

            if (Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.Harass))
            {
                Harass();
            }

            if (Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.LaneClear))
            {
                JungleClear();
            }

            if (Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.LaneClear))
            {
                LaneClear();
            }

            if (Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.LastHit))
            {
                LastHit();
            }
            KillSteal();
            Stacks();
            AutoHarass();
            RStun();
            Item();
        }

        public static void Item()
        {
            var item = Items["BOTRK"].GetValue<MenuBool>().Enabled;
            var Minhp = Items["ihp"].GetValue<MenuSlider>().Value;
            var Minhpp = Items["ihpp"].GetValue<MenuSlider>().Value;
            foreach (var target in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(475) && !e.IsDead))
            {
                if (item && Bil.IsReady && Bil.IsOwned() && Bil.IsInRange(target))
                {
                    Bil.Cast(target);
                }

                if ((item && Botrk.IsReady && Botrk.IsOwned() && target.IsValidTarget(475)) && (Player.Instance.HealthPercent <= Minhp || target.HealthPercent < Minhpp))
                {
                    Botrk.Cast(target);
                }
            }
        }

        public static void ResetAttack(object e, OrbwalkerActionArgs args)
        {
            if (!(e is AIHeroClient)) return;
            var useQ = ComboMenu["ComboQ"].GetValue<MenuBool>().Enabled;
            var useW = ComboMenu["ComboW"].GetValue<MenuBool>().Enabled;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var champ = (AIHeroClient)e;
            if (champ == null || champ.Type != GameObjectType.AIHeroClient || !champ.IsValid) return;
            if (target != null)
            {
                if (ComboMenu["comboMode"].GetValue<MenuList>().Index == 1)
                {
                    if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && _Player.Position.Distance(target) < Player.Instance.GetRealAutoAttackRange(target) && Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.Combo))
                    {
                        var Pred = Q.GetPrediction(target);
                        if (Pred.Hitchance >= HitChance.High)
                        {
                            Q.Cast(Pred.CastPosition);
                        }
                    }
                }

                if (ComboMenu["WMode"].GetValue<MenuList>().Index == 1)
                {
                    if (useW && W.IsReady() && target.IsValidTarget(W.Range) && _Player.Position.Distance(target) < Player.Instance.GetRealAutoAttackRange(target) && Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.Combo))
                    {
                        var WPred = W.GetPrediction(target);
                        if (WPred.Hitchance >= HitChance.High)
                        {
                            W.Cast(WPred.CastPosition);
                        }
                    }
                }
            }
        }

        public static void Combo()
        {
            var useQ = ComboMenu["ComboQ"].GetValue<MenuBool>().Enabled;
            var useW = ComboMenu["ComboW"].GetValue<MenuBool>().Enabled;
            var useR = ComboMenu["ComboR"].GetValue<MenuBool>().Enabled;
            var MinR = ComboMenu["MinR"].GetValue<MenuSlider>().Value;
            foreach (var target in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(2000) && !e.IsDead))
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    if (ComboMenu["comboMode"].GetValue<MenuList>().Index == 0)
                    {
                        var predQ = Q.GetPrediction(target);
                        if (predQ.Hitchance >= HitChance.High)
                        {
                            Q.Cast(predQ.CastPosition);
                        }
                    }
                    else
                    {
                        if (_Player.Position.Distance(target) > Player.Instance.GetRealAutoAttackRange(target))
                        {
                            var Qpred = Q.GetPrediction(target);
                            if (Qpred.Hitchance >= HitChance.Medium)
                            {
                                Q.Cast(Qpred.CastPosition);
                            }
                        }
                    }
                }

                if (useW && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    if (ComboMenu["WMode"].GetValue<MenuList>().Index == 0)
                    {
                        var Wpred = W.GetPrediction(target);
                        if (Wpred.Hitchance >= HitChance.High)
                        {
                            W.Cast(Wpred.CastPosition);
                        }
                    }
                    else
                    {
                        if (_Player.Position.Distance(target) > Player.Instance.GetRealAutoAttackRange(target))
                        {
                            var predW = W.GetPrediction(target);
                            if (predW.Hitchance >= HitChance.High)
                            {
                                W.Cast(predW.CastPosition);
                            }
                        }
                    }
                }

                if (useR && R.IsReady() && target.IsValidTarget(2000) && !target.IsValidTarget(800))
                {
                    var pred = R.GetPrediction(target);
                    if (pred.CastPosition.CountEnemyHeroesInRange(R.Width) >= MinR && pred.Hitchance >= HitChance.Medium)
                    {
                        R.Cast(pred.CastPosition);
                    }
                }
            }
        }

        public static void JungleClear()
        {
            var useQ = JungleClearMenu["QJungle"].GetValue<MenuBool>().Enabled;
            var mana = JungleClearMenu["MnJungle"].GetValue<MenuSlider>().Value;
            var monster = GameObjects.Jungle.Where(j => j.IsValidTarget(700)).FirstOrDefault(j => j.IsValidTarget(Q.Range));
            if (monster != null)
            {
                if (useQ && Q.IsReady() && Player.Instance.ManaPercent >= mana)
                {
                    Q.Cast(monster);
                }
            }
        }

        private static void Flee()
        {
            if (E.IsReady())
            {
                E.Cast();
            }
        }

        public static void LaneClear()
        {
            var useQ = LaneClearMenu["QLC"].GetValue<MenuBool>().Enabled;
            var mana = LaneClearMenu["ManaLC"].GetValue<MenuSlider>().Value;
            var minions = GameObjects.Jungle.Where(m => m.IsValidTarget(Q.Range)).OrderBy(m => m.Health).FirstOrDefault();
            if (minions != null)
            {
                if (LaneClearMenu["LCMode"].GetValue<MenuList>().Index == 0)
                {
                    if (useQ && Player.Instance.ManaPercent >= mana)
                    {
                        if (Q.IsReady() && minions.Health + minions.AllShield <= Player.Instance.GetSpellDamage(minions, SpellSlot.Q))
                        {
                            Q.Cast(minions);
                        }
                    }
                }
            }
        }

        private static void Orbwalker_CantLasthit(object targeti, OrbwalkerActionArgs args)
        {
            var useQ = LaneClearMenu["QLC"].GetValue<MenuBool>().Enabled;
            var useQ2 = LaneClearMenu["QLH"].GetValue<MenuBool>().Enabled;
            var mana = LaneClearMenu["ManaLC"].GetValue<MenuSlider>().Value;
            var manaa = LaneClearMenu["LhMana"].GetValue<MenuSlider>().Value;
            var unit = (LaneClearMenu["LCMode"].GetValue<MenuList>().Index == 1 && Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.LaneClear) && useQ && Player.Instance.ManaPercent >= mana)
            || (LaneClearMenu["LHMode"].GetValue<MenuList>().Index == 1 && Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.LastHit) && useQ2 && Player.Instance.ManaPercent >= manaa);
            var minions = ObjectManager.Get<AIBaseClient>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (minion == null) return;
                if (unit && Q.IsReady() && minion.IsValidTarget(Q.Range))
                {
                    if (Player.Instance.GetSpellDamage(minion, SpellSlot.Q) >= minion.Health + minion.AllShield)
                    {
                        Q.Cast(minion);
                    }
                }
            }
        }

        public static void Harass()
        {
            var useQ = HarassMenu["HarassQ"].GetValue<MenuBool>().Enabled;
            var useW = HarassMenu["HarassW"].GetValue<MenuBool>().Enabled;
            var ManaQ = HarassMenu["ManaQ"].GetValue<MenuSlider>().Value;
            var ManaW = HarassMenu["ManaW"].GetValue<MenuSlider>().Value;
            foreach (var Selector in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(W.Range) && !e.IsDead))
            {
                if (useQ && Player.Instance.ManaPercent >= ManaQ && Q.IsReady() && Selector.IsValidTarget(Q.Range))
                {
                    var Qpred = Q.GetPrediction(Selector);
                    if (HarassMenu["haras" + Selector.CharacterName].GetValue<MenuBool>().Enabled && Qpred.Hitchance >= HitChance.VeryHigh)
                    {
                        Q.Cast(Qpred.CastPosition);
                    }
                }

                if (useW && Player.Instance.ManaPercent >= ManaW && W.IsReady() && Selector.IsValidTarget(W.Range))
                {
                    var Wpred = W.GetPrediction(Selector);
                    if (HarassMenu["haras" + Selector.CharacterName].GetValue<MenuBool>().Enabled && Wpred.Hitchance >= HitChance.VeryHigh)
                    {
                        W.Cast(Wpred.CastPosition);
                    }
                }
            }
        }

        public static void LastHit()
        {
            var useQ = LaneClearMenu["QLH"].GetValue<MenuBool>().Enabled;
            var mana = LaneClearMenu["LhMana"].GetValue<MenuSlider>().Value;
            var minion = GameObjects.EnemyMinions.Where(m => m.IsValidTarget(Q.Range)).OrderBy(m => m.Health).FirstOrDefault();
            if (Player.Instance.ManaPercent < mana)
            {
                return;
            }

            if (minion != null)
            {
                if (LaneClearMenu["LHMode"].GetValue<MenuList>().Index == 0)
                {
                    if (useQ && Q.IsReady() && minion.Health + minion.AllShield <= Player.Instance.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        Q.Cast(minion);
                    }
                }
            }
        }

        public static void AutoHarass()
        {
            var useQ = Auto["AutoQ"].GetValue<MenuBool>().Enabled;
            var useW = Auto["AutoW"].GetValue<MenuBool>().Enabled;
            var key = Auto["Key"].GetValue<MenuKeyBind>().Active;
            var automana = Auto["AutomanaQ"].GetValue<MenuSlider>().Value;
            var automanaw = Auto["AutomanaW"].GetValue<MenuSlider>().Value;
            foreach (var Selector in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(W.Range) && !e.IsDead))
            {
                if (key && Selector.IsValidTarget(W.Range) && !Tru(_Player.Position) && !Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.Combo) && !Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.Harass))
                {
                    if (useQ && Q.IsReady() && automana <= Player.Instance.ManaPercent)
                    {
                        var predQ = Q.GetPrediction(Selector);
                        if (Auto["harass" + Selector.CharacterName].GetValue<MenuBool>().Enabled && predQ.Hitchance >= HitChance.VeryHigh)
                        {
                            Q.Cast(predQ.CastPosition);
                        }
                    }

                    if (useW && W.IsReady() && automanaw <= Player.Instance.ManaPercent)
                    {
                        var predW = W.GetPrediction(Selector);
                        if (Auto["harass" + Selector.CharacterName].GetValue<MenuBool>().Enabled && predW.Hitchance >= HitChance.VeryHigh)
                        {
                            W.Cast(predW.CastPosition);
                        }
                    }
                }
            }
        }

        // Thanks MarioGK has allowed me to use some his logic

        public static void RStun()
        {
            var Rstun = Misc["Rstun"].GetValue<MenuBool>().Enabled;
            if (Rstun && R.IsReady())
            {
                var target = TargetSelector.GetTarget(1600, DamageType.Physical);
                if (target != null)
                {
                    if ((!target.IsValidTarget(800)) && (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup)))
                    {
                        R.Cast(target.Position);
                    }
                }
            }
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserArgs e)
        {
            if (Misc["AntiGap"].GetValue<MenuBool>().Enabled && E.IsReady() && sender.IsEnemy && sender.IsVisible && sender.IsValidTarget(E.Range))
            {
                E.Cast((sender));
            }
        }

        public static bool Tru(Vector3 position)
        {
            return ObjectManager.Get<AITurretClient>().Any(turret => turret.IsValidTarget(950) && turret.IsEnemy);
        }

        public static void KillSteal()
        {
            var KsQ = KillStealMenu["KsQ"].GetValue<MenuBool>().Enabled;
            var KsW = KillStealMenu["KsW"].GetValue<MenuBool>().Enabled;
            foreach (var target in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(2500) && !e.HasBuff("JudicatorIntervention") && !e.HasBuff("kindredrnodeathbuff") && !e.HasBuff("Undying Rage") && !e.IsDead && !e.IsZombie))
            {
                if (KsQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    if (target.Health + target.PhysicalShield <= Player.Instance.GetSpellDamage(target, SpellSlot.Q))
                    {
                        var Qpred = Q.GetPrediction(target);
                        if (Qpred.Hitchance >= HitChance.VeryHigh)
                        {
                            Q.Cast(Qpred.CastPosition);
                        }
                    }
                }

                if (KsW && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    if (target.Health + target.PhysicalShield <= Player.Instance.GetSpellDamage(target, SpellSlot.W))
                    {
                        var Wpred = W.GetPrediction(target);
                        if (Wpred.Hitchance >= HitChance.VeryHigh)
                        {
                            W.Cast(Wpred.CastPosition);
                        }
                    }
                }

                var KsR = KillStealMenu["KsR"].GetValue<MenuBool>().Enabled;
                if (KsR && R.IsReady())
                {
                    if (target.Health + target.PhysicalShield <= Player.Instance.GetSpellDamage(target, SpellSlot.R) && target.IsValidTarget(2500) && !target.IsValidTarget(900))
                    {
                        var pred = R.GetPrediction(target);
                        if (pred.Hitchance >= HitChance.VeryHigh)
                        {
                            R.Cast(pred.CastPosition);
                        }
                    }
                }

                if (R.IsReady() && KillStealMenu["RKb"].GetValue<MenuKeyBind>().Active)
                {
                    if (target.Health + target.PhysicalShield <= Player.Instance.GetSpellDamage(target, SpellSlot.R))
                    {
                        var pred = R.GetPrediction(target);
                        if (pred.Hitchance >= HitChance.VeryHigh)
                        {
                            R.Cast(pred.CastPosition);
                        }
                    }
                }

                if (Ignite != null && KillStealMenu["ign"].GetValue<MenuBool>().Enabled && Ignite.IsReady())
                {
                    if (target.Health <= _Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite))
                    {
                        Ignite.Cast(target);
                    }
                }
            }
        }

        public static void Stacks()
        {
            //var castPos = Player.Instance.Position.Distance(Game.CursorPosCenter) <= Q.Range ? Game.CursorPosCenter : Player.Instance.Position.Extend(Game.CursorPosCenter, Q.Range).To3D();
            if (Misc["Stack"].GetValue<MenuBool>().Enabled && Q.IsReady() &&
            (Player.Instance.InShop()) && (Tear.IsOwned() || Manamune.IsOwned()))
            {
                Q.Cast();
            }

            var mana = Misc["Stackkm"].GetValue<MenuSlider>().Value;
            if (Misc["Stackk"].GetValue<MenuBool>().Enabled && Q.IsReady() &&
            (!Player.Instance.InShop() && _Player.Position.CountEnemyHeroesInRange(2500) < 1 && !_Player.IsRecalling() && Player.Instance.ManaPercent >= mana && !GameObjects.AllyMinions.Any(x => x.IsValidTarget(2000))
            && (Tear.IsOwned() || Manamune.IsOwned())))
            {
                if (!Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.LaneClear) && !Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.LastHit) && !Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.Harass) && !Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.Combo)
                     && !Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.LaneClear))
                {
                    Q.Cast();
                }
            }
        }
    }
}