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

namespace Kayle
{
    internal class Program
    {
        public static Menu Menu, ComboMenu, HarassMenu, Ulti, Heal, LaneClearMenu, JungleClearMenu, KillStealMenu, Misc;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        private static Font thm;
        public static Spell Ignite;

        static void Main(string[] args)
        {
            GameEvent.OnGameLoad += OnLoadingComplete;
        }

        private static void OnLoadingComplete()
        {
            if (!_Player.CharacterName.Contains("Kayle")) return;
            Chat.Print("Doctor's Kayle Loaded! Ported By DEATHGODx", Color.Orange);
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, (uint)Player.Instance.GetRealAutoAttackRange());
            R = new Spell(SpellSlot.R, 900);
            Ignite = new Spell(ObjectManager.Player.GetSpellSlot("summonerdot"), 600);
            thm = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 22, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            var MenuKay = new Menu("Doctor's Kayle", "Kayle", true);
            ComboMenu = new Menu("Combo Settings", "Combo");
            ComboMenu.Add(new MenuSeparator("Combo Settings", "Combo Settings"));
            ComboMenu.Add(new MenuBool("ComboQ", "Use [Q] Combo"));
            ComboMenu.Add(new MenuBool("ComboE", "Use [E] Combo"));
            MenuKay.Add(ComboMenu);
            Ulti = new Menu("Ultimate Settings", "Ulti");
            Ulti.Add(new MenuSeparator("Ultimate Settings", "Ultimate Settings"));
            Ulti.Add(new MenuBool("ultiR2", "Use [R]"));
            Ulti.Add(new MenuSlider("Alhp", "HP Use [R]", 25));
            Ulti.Add(new MenuSeparator("Use [R] On", "Use [R] On"));
            foreach (var target in GameObjects.AllyHeroes)
            {
                Ulti.Add(new MenuBool("useRon" + target.CharacterName, "" + target.CharacterName));
            }
            MenuKay.Add(Ulti);
            Heal = new Menu("Heal Settings", "Heal");
            Heal.Add(new MenuSeparator("Heal Settings", "Heal Settings"));
            Heal.Add(new MenuBool("healW2", "Use [W] Allies"));
            Heal.Add(new MenuSlider("ManaHeal", "Mana Use Heal", 20));
            Heal.Add(new MenuSlider("AlW", "Allies HP Use [W]", 70));
            Heal.Add(new MenuSeparator("Use [W] On", "Use [W] On"));
            foreach (var target in GameObjects.AllyHeroes)
            {
                Heal.Add(new MenuBool("useWon" + target.CharacterName, "" + target.CharacterName));
            }
            MenuKay.Add(Heal);
            HarassMenu = new Menu("Harass Settings", "Harass");
            HarassMenu.Add(new MenuSeparator("Harass Settings", "Harass Settings"));
            HarassMenu.Add(new MenuBool("HarassQ", "Use [Q] Harass"));
            HarassMenu.Add(new MenuBool("HarassE", "Use [E] Harass"));
            HarassMenu.Add(new MenuSlider("ManaHR", "Mana For Harass", 50));
            MenuKay.Add(HarassMenu);
            LaneClearMenu = new Menu("LaneClear Settings", "LaneClear");
            LaneClearMenu.Add(new MenuSeparator("Lane Clear Settings", "Lane Clear Settings"));
            LaneClearMenu.Add(new MenuBool("QLC", "Use [Q] LaneClear", false));
            LaneClearMenu.Add(new MenuBool("ELC", "Use [E] LaneClear"));
            LaneClearMenu.Add(new MenuSlider("ManaLC", "Mana For LaneClear", 50));
            LaneClearMenu.Add(new MenuSeparator("Lasthit Settings", "Lasthit Settings"));
            LaneClearMenu.Add(new MenuBool("QLH", "Use [Q] Lasthit"));
            LaneClearMenu.Add(new MenuSlider("ManaLH", "Mana For Lasthit", 50));
            MenuKay.Add(LaneClearMenu);
            JungleClearMenu = new Menu("JungleClear Settings", "JungleClear");
            JungleClearMenu.Add(new MenuSeparator("JungleClear Settings", "JungleClear Settings"));
            JungleClearMenu.Add(new MenuBool("QJungle", "Use [Q] JungleClear"));
            JungleClearMenu.Add(new MenuBool("EJungle", "Use [E] JungleClear"));
            JungleClearMenu.Add(new MenuSlider("ManaJC", "Mana For JungleClear", 30));
            MenuKay.Add(JungleClearMenu);
            KillStealMenu = new Menu("KillSteal Settings", "KillSteal");
            KillStealMenu.Add(new MenuSeparator("KillSteal Settings", "KillSteal Settings"));
            KillStealMenu.Add(new MenuBool("KsQ", "Use [Q] KillSteal"));
            KillStealMenu.Add(new MenuBool("ign", "Use [Ignite] KillSteal"));
            MenuKay.Add(KillStealMenu);
            Misc = new Menu("Misc Settings", "Misc");
            Misc.Add(new MenuSeparator("Drawing Settings", "Drawing Settings"));
            Misc.Add(new MenuBool("DrawQ", "[Q] Range"));
            Misc.Add(new MenuBool("DrawE", "[E] Range"));
            Misc.Add(new MenuBool("DrawR", "[R] - [W] Range"));
            Misc.Add(new MenuBool("DrawIE", "Status [E] Buff"));
            MenuKay.Add(Misc);
            MenuKay.Attach();
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Misc["DrawR"].GetValue<MenuBool>().Enabled && R.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, Color.Purple, 1);
            }

            if (Misc["DrawE"].GetValue<MenuBool>().Enabled && Player.Instance.HasBuff("JudicatorRighteousFury") && E.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.Purple, 1);
            }

            if (Misc["DrawQ"].GetValue<MenuBool>().Enabled && Q.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.Purple, 1);
            }

            if (Misc["DrawIE"].GetValue<MenuBool>().Enabled)
            {
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                if (Player.Instance.HasBuff("JudicatorRighteousFury"))
                {
                    DrawFont(thm, "Righteous Fury : " + ETime(Player.Instance), (float)(ft[0] - 125), (float)(ft[1] + 50), SharpDX.Color.Pink);
                }
            }
        }

        public static float ETime(AIBaseClient target)
        {
            if (target.HasBuff("JudicatorRighteousFury"))
            {
                return Math.Max(0, target.GetBuff("JudicatorRighteousFury").EndTime) - Game.Time;
            }
            return 0;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            E = new Spell(SpellSlot.E, (uint)Player.Instance.GetRealAutoAttackRange());

            if (Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.LaneClear))
            {
                LaneClear();
            }

            if (Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.LastHit))
            {
                LastHit();
            }

            if (Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.Harass))
            {
                Harass();
            }

            if (Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.LaneClear))
            {
                JungleClear();
            }
            if (Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.Combo))
            {
                Combo();
            }

            KillSteal();
            Ultimate();
            Heals();
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            var useQ = ComboMenu["ComboQ"].GetValue<MenuBool>().Enabled;
            var useE = ComboMenu["ComboE"].GetValue<MenuBool>().Enabled;
            if (target != null)
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }

                if (useE && E.IsReady() && target.IsValidTarget(550))
                {
                    E.Cast();
                }
            }
        }

        public static bool Tru(Vector3 position)
        {
            return ObjectManager.Get<AIBaseClient>().Any(turret => turret.IsValidTarget(950) && turret.IsEnemy);
        }

        private static void Ultimate()
        {
            var almin = Heal["AlW"].GetValue<MenuSlider>().Value;
            var useW2 = Heal["healW2"].GetValue<MenuBool>().Enabled;
            var mana = Heal["ManaHeal"].GetValue<MenuSlider>().Value;
            var target = GameObjects.AllyHeroes.Where(a => a.IsValidTarget() && a.Distance(_Player.Position) <= W.Range && !a.IsDead && !a.IsZombie && !a.HasBuff("kindredrnodeathbuff") && !a.HasBuff("JudicatorIntervention") && !a.HasBuff("Recall"));
            if (Player.Instance.ManaPercent <= mana) return;
            foreach (var target2 in target)
            {
                if (useW2 && W.IsReady() && !Player.Instance.InShop() && !Player.Instance.IsRecalling())
                {
                    if (Heal["useWon" + target2.CharacterName].GetValue<MenuBool>().Enabled && (target2.HealthPercent <= almin || target2.HasBuff("ZedR")))
                    {
                        W.Cast(target2);
                    }
                }
            }
        }

        private static void Heals()
        {
            var almin = Heal["AlW"].GetValue<MenuSlider>().Value;
            var useW2 = Heal["healW2"].GetValue<MenuBool>().Enabled;
            var mana = Heal["ManaHeal"].GetValue<MenuSlider>().Value;
            var target = GameObjects.EnemyHeroes.Where(a => a.IsValidTarget() && a.Distance(_Player.Position) <= W.Range && !a.IsDead && !a.IsZombie && !a.HasBuff("kindredrnodeathbuff") && !a.HasBuff("JudicatorIntervention") && !a.HasBuff("Recall"));
            if (Player.Instance.ManaPercent <= mana) return;
            foreach (var target2 in target)
            {
                if (useW2 && W.IsReady() && !Player.Instance.InShop() && !Player.Instance.IsRecalling())
                {
                    if (Heal["useWon" + target2.CharacterName].GetValue<MenuBool>().Enabled && (target2.HealthPercent <= almin || target2.HasBuff("ZedR")))
                    {
                        W.Cast(target2);
                    }
                }
            }
            if (Player.Instance.HealthPercent <= almin && W.IsReady())
            {
                W.Cast();
            }
        }

        private static void LaneClear()
        {
            var useQ = LaneClearMenu["QLC"].GetValue<MenuBool>().Enabled;
            var useE = LaneClearMenu["ELC"].GetValue<MenuBool>().Enabled;
            var mana = LaneClearMenu["ManaLC"].GetValue<MenuSlider>().Value;
            var minion = GameObjects.EnemyMinions.Where(a => a.Distance(Player.Instance) <= Q.Range).OrderBy(a => a.Health).FirstOrDefault();
            if (Player.Instance.ManaPercent < mana) return;
            if (minion != null)
            {
                if (useE && E.IsReady() && minion.IsValidTarget(550))
                {
                    E.Cast();
                }

                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && Player.Instance.GetSpellDamage(minion, SpellSlot.Q) >= minion.Health + minion.AllShield)
                {
                    Q.Cast(minion);
                }
            }
        }

        public static void DrawFont(Font vFont, string vText, float jx, float jy, ColorBGRA jc)
        {
            vFont.DrawText(null, vText, (int)jx, (int)jy, jc);
        }

        private static void LastHit()
        {
            var useQ = LaneClearMenu["QLH"].GetValue<MenuBool>().Enabled;
            var mana = LaneClearMenu["ManaLH"].GetValue<MenuSlider>().Value;
            var minion = GameObjects.EnemyMinions.Where(a => a.Distance(Player.Instance) <= Q.Range).OrderBy(a => a.Health).FirstOrDefault();
            if (Player.Instance.ManaPercent < mana) return;
            if (minion != null)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && Player.Instance.GetSpellDamage(minion, SpellSlot.Q) >= minion.Health + minion.AllShield)
                {
                    Q.Cast(minion);
                }
            }
        }

        private static void Harass()
        {
            var useQ = HarassMenu["HarassQ"].GetValue<MenuBool>().Enabled;
            var useE = HarassMenu["HarassE"].GetValue<MenuBool>().Enabled;
            var mana = HarassMenu["ManaHR"].GetValue<MenuSlider>().Value;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (Player.Instance.ManaPercent <= mana) return;
            if (target != null)
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }

                if (useE && E.IsReady() && target.IsValidTarget(550))
                {
                    E.Cast();
                }
            }
        }

        public static void JungleClear()
        {
            var useQ = JungleClearMenu["QJungle"].GetValue<MenuBool>().Enabled;
            var useE = JungleClearMenu["EJungle"].GetValue<MenuBool>().Enabled;
            var mana = JungleClearMenu["ManaJC"].GetValue<MenuSlider>().Value;
            var monster = GameObjects.Jungle.OrderByDescending(a => a.MaxHealth).FirstOrDefault(a => a.IsValidTarget(550));
            if (Player.Instance.ManaPercent <= mana) return;
            if (monster != null)
            {
                if (useQ && Q.IsReady() && monster.IsValidTarget(Q.Range))
                {
                    Q.Cast(monster);
                }

                if (useE && E.IsReady() && monster.IsValidTarget(Q.Range))
                {
                    E.Cast();
                }
            }
        }

        public static void Flee()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target != null)
            {
                if (Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }
            }

            if (W.IsReady())
            {
                W.Cast(Player.Instance);
            }
        }

        private static void KillSteal()
        {
            var KsQ = KillStealMenu["KsQ"].GetValue<MenuBool>().Enabled;
            foreach (var target in GameObjects.EnemyHeroes.Where(hero => hero.IsValidTarget(Q.Range) && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("Undying Rage") && !hero.IsDead && !hero.IsZombie))
            {
                if (KsQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    if (target.Health + target.AllShield <= Player.Instance.GetSpellDamage(target, SpellSlot.Q))
                    {
                        Q.Cast(target);
                    }
                }

                if (Ignite != null && KillStealMenu["ign"].GetValue<MenuBool>().Enabled && Ignite.IsReady())
                {
                    if (target.Health + target.AllShield <= _Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite))
                    {
                        Ignite.Cast(target);
                    }
                }
            }
        }
    }
}
