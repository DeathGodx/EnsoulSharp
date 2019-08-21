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

namespace Pantheon
{
    internal class Program
    {
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Spell Ignite;
        public static Item Botrk;
        public static Item Bil;
        public static Item Youmuu;
        public static Font Thm;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Menu Menu, ComboMenu, JungleClearMenu, HarassMenu, LaneClearMenu, Misc, Items, KillSteals;

        static void Main(string[] args)
        {
            GameEvent.OnGameLoad += OnLoadingComplete;
        }

        // Menu

        private static void OnLoadingComplete()
        {
            if (!_Player.CharacterName.Contains("Pantheon")) return;
            Chat.Print("Doctor's Pantheon Loaded! PORTED by DeathGODx", Color.White);
            Bootstrap.Init(null);
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 600);
            E.SetSkillshot(250, 2000, 70, false,false, SkillshotType.Cone);
            R = new Spell(SpellSlot.R, 2000);
            R.SetSkillshot(1000, 2000, 160, false,false, SkillshotType.Circle);
            Youmuu = new Item(3142, 300);
            Botrk = new Item(ItemId.Blade_of_the_Ruined_King, 400);
            Bil = new Item(3144, 475f);
            Ignite = new Spell(ObjectManager.Player.GetSpellSlot("summonerdot"), 600);
            Thm = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 32, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            var MenuPant = new Menu("Doctor's Pantheon", "Pantheon", true);
            ComboMenu = new Menu("Combo Settings", "Combo");
            ComboMenu.Add(new MenuSeparator("Combo Settings", "Combo Settings"));
            ComboMenu.Add(new MenuBool("CQ", "Use [Q] Combo"));
            ComboMenu.Add(new MenuBool("CW", "Use [W] Combo"));
            ComboMenu.Add(new MenuBool("CE", "Use [E] Combo"));
            MenuPant.Add(ComboMenu);
            HarassMenu = new Menu("Harass Settings", "Harass");
            HarassMenu.Add(new MenuSeparator("Harass Settings","Harass Settings"));
            HarassMenu.Add(new MenuBool("HQ", "Use [Q] Harass"));
            HarassMenu.Add(new MenuBool("HW", "Use [W] Harass"));
            HarassMenu.Add(new MenuBool("HE", "Use [E] Harass"));
            HarassMenu.Add(new MenuSlider("HM", "Mana Harass", 50, 0, 100));
            HarassMenu.Add(new MenuSeparator("Auto Harass Settings", "Auto Harass Settings"));
            HarassMenu.Add(new MenuBool("AutoQ", "Auto [Q] Harass"));
            HarassMenu.Add(new MenuSlider("AutoM", "Mana Auto Harass", 60, 0, 100));
            HarassMenu.Add(new MenuSeparator("Auto [Q] On", "Auto [Q] On"));
            foreach (var target in GameObjects.EnemyHeroes)
            {
                HarassMenu.Add(new MenuBool("HarassQ" + target.CharacterName, "" + target.CharacterName));
            }
            MenuPant.Add(HarassMenu);
            LaneClearMenu = new Menu("Laneclear Settings", "Clear");
            LaneClearMenu.Add(new MenuSeparator("Laneclear Settings", "Laneclear Settings"));
            LaneClearMenu.Add(new MenuBool("LQ", "Use [Q] Laneclear"));
            LaneClearMenu.Add(new MenuBool("LW", "Use [W] Laneclear", false));
            LaneClearMenu.Add(new MenuBool("LE", "Use [E] Laneclear", false));
            LaneClearMenu.Add(new MenuSlider("ME", "Min Hit Minions Use [E] LaneClear", 3, 1, 6));
            LaneClearMenu.Add(new MenuSlider("LM", "Mana LaneClear", 60, 0, 100));
            LaneClearMenu.Add(new MenuSeparator("LastHit Settings", "LastHit Settings"));
            LaneClearMenu.Add(new MenuBool("LHQ", "Use [Q] LastHit"));
            LaneClearMenu.Add(new MenuSlider("LHM", "Mana LastHit", 60, 0, 100));
            MenuPant.Add(LaneClearMenu);
            JungleClearMenu = new Menu("JungleClear Settings", "JungleClear");
            JungleClearMenu.Add(new MenuSeparator("JungleClear Settings", "JungleClear Settings"));
            JungleClearMenu.Add(new MenuBool("JQ", "Use [Q] JungleClear"));
            JungleClearMenu.Add(new MenuBool("JW", "Use [W] JungleClear"));
            JungleClearMenu.Add(new MenuBool("JE", "Use [E] JungleClear"));
            JungleClearMenu.Add(new MenuSlider("JM", "Mana JungleClear", 20, 0, 100));
            MenuPant.Add(JungleClearMenu);
            Items = new Menu("Items Settings", "Items");
            Items.Add(new MenuSeparator("Items Settings", "Items Settings"));
            Items.Add(new MenuBool("you", "Use [Youmuu]"));
            Items.Add(new MenuBool("BOTRK", "Use [Botrk]"));
            Items.Add(new MenuSlider("ihp", "My HP Use BOTRK <=", 50));
            Items.Add(new MenuSlider("ihpp", "Enemy HP Use BOTRK <=", 50));
            MenuPant.Add(Items);
            Misc = new Menu("Misc Settings", "Draw");
            Misc.Add(new MenuSeparator("Anti Gapcloser", "Anti Gapcloser"));
            Misc.Add(new MenuBool("antiGap", "Use [W] Anti Gapcloser", false));
            Misc.Add(new MenuBool("inter", "Use [W] Interupt"));
            Misc.Add(new MenuSeparator("Drawings Settings", "Drawings Settings"));
            Misc.Add(new MenuBool("Draw_Disabled", "Disabled Drawings", false));
            Misc.Add(new MenuBool("Draw", "Draw [Q/W/E]"));
            Misc.Add(new MenuBool("Notifications", "Draw Text Can Kill With R"));
            Misc.Add(new MenuSeparator("Skins Settings", "Skins Settings"));
            Misc.Add(new MenuBool("checkSkin", "Use Skin Changer", false));
            Misc.Add(new MenuList("skin.Id", "Skin Mode", new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" }) {Index = 0 });
            MenuPant.Add(Misc);
            KillSteals = new Menu("KillSteal Changer", "KillSteal");
            KillSteals.Add(new MenuBool("Q", "Use [Q] KillSteal"));
            KillSteals.Add(new MenuBool("W", "Use [W] KillSteal"));
            KillSteals.Add(new MenuBool("ign", "Use [Ignite] KillSteal"));
            MenuPant.Add(KillSteals);
            MenuPant.Attach();
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            Interrupter.OnInterrupterSpell += Interupt;
            Orbwalker.OnAction += Orbwalker_CantLasthit;
            Spellbook.OnCastSpell += OnCastSpell;
            AIBaseClient.OnProcessSpellCast += AIHeroClient_OnProcessSpellCast;
            AIBaseClient.OnBuffLose += BuffLose;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.LaneClear))
            {
                JungleClear();
            }

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
                LaneClear();
            }

            KillSteal();
            Item();
            AutoHarass();
        }

        // Drawings

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_Player.IsDead) return;

            if (Misc["Draw_Disabled"].GetValue<MenuBool>().Enabled) return;

            if (Misc["Draw"].GetValue<MenuBool>().Enabled)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.Orange, 1);
            }

            if (Misc["Notifications"].GetValue<MenuBool>().Enabled)
            {
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                foreach (var target in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(2000) && RDamage(e) >= e.Health + e.AllShield))
                {
                    if (R.IsReady() && _Player.Distance(target) >= 810)
                    {
                        DrawFont(Thm, "[R] Can Killable " + target.CharacterName, (float)(ft[0] - 140), (float)(ft[1] + 80), SharpDX.Color.Red);
                    }
                }
            }
        }

        // Skin Changer

        public static int SkinId()
        {
            return Misc["skin.Id"].GetValue<MenuList>().Index;
        }

        public static bool checkSkin()
        {
            return Misc["checkSkin"].GetValue<MenuBool>().Enabled;
        }

        // EBuff

        public static bool ECasting
        {
            get { return Player.Instance.HasBuff("PantheonESound"); }
        }

        // OnCastSpell

        private static void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!sender.Owner.IsMe)
            {
                return;
            }

            if (args.Slot == SpellSlot.Q || args.Slot == SpellSlot.W)
            {
                if (ECasting)
                {
                    args.Process = false;
                }
            }
        }


        private static void AIHeroClient_OnProcessSpellCast(AIBaseClient sender, EventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (SpellSlot.E.IsReady())
            {
                Orbwalker.MovementState = true;
                Orbwalker.AttackState = true;
            }
        }

        private static void BuffLose(AIBaseClient sender, AIBaseClientBuffLoseEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (args.Buff.Name == "PantheonESound")
            {
                Orbwalker.MovementState = false;
                Orbwalker.AttackState = false;
            }
        }

        // Damage Lib

        public static double QDamage(AIBaseClient target)
        {
            return _Player.CalculateDamage(target, DamageType.Physical,
                (float)(new[] { 0, 65, 105, 145, 185, 225 }[Q.Level] + 1.4f * _Player.FlatPhysicalDamageMod));
        }

        public static double WDamage(AIBaseClient target)
        {
            return _Player.CalculateDamage(target, DamageType.Magical,
                (float)(new[] { 0, 50, 75, 100, 125, 150 }[W.Level] + 1.0f * _Player.FlatMagicDamageMod));
        }

        public static double RDamage(AIBaseClient target)
        {
            return _Player.CalculateDamage(target, DamageType.Magical,
                (float)(new[] { 0, 200, 350, 500 }[R.Level] + 0.5f * _Player.FlatMagicDamageMod));
        }

        // Interrupt

        public static void Interupt(AIBaseClient sender, Interrupter.InterruptSpellArgs i)
        {
            var Inter = Misc["inter"].GetValue<MenuBool>().Enabled;
            if (!sender.IsEnemy || !(sender is AIHeroClient) || Player.Instance.IsRecalling())
            {
                return;
            }

            if (Inter && W.IsReady() && i.DangerLevel == DangerLevel.High && _Player.Distance(sender) <= W.Range)
            {
                W.Cast(sender);
            }
        }

        // AntiGap

        private static void Gapcloser_OnGapCloser(AIBaseClient sender, Gapcloser.GapcloserArgs args)
        {
            var useW = Misc["antiGap"].GetValue<MenuBool>().Enabled;
            if (useW && W.IsReady() && sender.IsEnemy && sender.Distance(_Player) <= 325)
            {
                W.Cast(sender);
            }
        }

        //Harass Mode

        private static void Harass()
        {
            var useQ = HarassMenu["HQ"].GetValue<MenuBool>().Enabled;
            var useW = HarassMenu["HW"].GetValue<MenuBool>().Enabled;
            var useE = HarassMenu["HE"].GetValue<MenuBool>().Enabled;
            var mana = HarassMenu["HM"].GetValue<MenuSlider>().Value;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);

            if (Player.Instance.ManaPercent <= mana)
            {
                return;
            }

            if (target != null)
            {
                if (useW && W.CanCast(target) && !target.HasBuffOfType(BuffType.Stun))
                {
                    W.Cast(target);
                }

                if (useQ && Q.CanCast(target))
                {
                    Q.Cast(target);
                }

                if (useE && E.CanCast(target))
                {
                    var pred = E.GetPrediction(target);
                    if (!W.IsReady())
                    {
                        if (pred.Hitchance >= HitChance.Medium)
                        {
                            E.Cast(pred.CastPosition);
                        }
                    }

                    else if (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup))
                    {
                        E.Cast(target.Position);
                    }
                }
            }
        }

        //Combo Mode

        private static void Combo()
        {
            var useQ = ComboMenu["CQ"].GetValue<MenuBool>().Enabled;
            var useW = ComboMenu["CW"].GetValue<MenuBool>().Enabled;
            var useE = ComboMenu["CE"].GetValue<MenuBool>().Enabled;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);

            if (target != null)
            {
                if (useW && W.CanCast(target) && !target.HasBuffOfType(BuffType.Stun))
                {
                    W.Cast(target);
                }

                if (useQ)
                {
                    Q.Cast(target.Position - 400);
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }

                if (useE && E.CanCast(target))
                {
                    var pred = E.GetPrediction(target);
                    if (!Q.IsReady() && !W.IsReady())
                    {
                        if (pred.Hitchance >= HitChance.Medium)
                        {
                            E.Cast(pred.CastPosition);
                        }
                    }

                    if (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup))
                    {
                        E.Cast(target.Position);
                    }
                }
            }
        }

        //LaneClear Mode

        private static void LaneClear()
        {
            var useQ = LaneClearMenu["LQ"].GetValue<MenuBool>().Enabled;
            var useW = LaneClearMenu["LW"].GetValue<MenuBool>().Enabled;
            var useE = LaneClearMenu["LE"].GetValue<MenuBool>().Enabled;
            var MinE = LaneClearMenu["ME"].GetValue<MenuSlider>().Value;
            var mana = LaneClearMenu["LM"].GetValue<MenuSlider>().Value;
            var minionQ = GameObjects.EnemyMinions.Where(e => e.IsValidTarget(Q.Range) && e.IsMinion())
                            .Cast<AIBaseClient>().ToList();
            var qFarmLocation = E.GetLineFarmLocation(minionQ, E.Width);
            if (Player.Instance.ManaPercent <= mana)
            {
                return;
            }

            if (ECasting)
            {
                return;
            }

            foreach (var minion in minionQ)
            {
                if (useQ && Q.CanCast(minion))
                {
                    if (minion.Health <= QDamage(minion))
                    {
                        Q.Cast(minion);
                    }
                }

                if (useW && W.CanCast(minion))
                {
                    if (minion.Health <= WDamage(minion))
                    {
                        if (useQ)
                        {
                            if (!Q.IsReady())
                            {
                                W.Cast(minion);
                            }
                        }
                        else
                        {
                            W.Cast(minion);
                        }
                    }
                }

                if (useE && E.CanCast(minion))
                {
                    if (qFarmLocation.MinionsHit >= MinE)
                    {
                        if (useQ)
                        {
                            if (!Q.IsReady())
                            {
                                E.Cast(qFarmLocation.Position);
                            }
                        }
                        else
                        {
                            E.Cast(qFarmLocation.Position);
                        }
                    }
                }
            }
        }

        // LastHit Mode

        private static void Orbwalker_CantLasthit(object targeti, OrbwalkerActionArgs args)
        {
            var useQ = LaneClearMenu["LHQ"].GetValue<MenuBool>().Enabled;
            var mana = LaneClearMenu["LHM"].GetValue<MenuSlider>().Value;
            var unit = (useQ && Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.LastHit) && Player.Instance.ManaPercent >= mana);
            var minions = ObjectManager.Get<AIBaseClient>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (minion == null)
                {
                    return;
                }

                if (unit && Q.IsReady() && minion.IsValidTarget(Q.Range))
                {
                    if (QDamage(minion) >= minion.Health + minion.AllShield)
                    {
                        Q.Cast(minion);
                    }
                }
            }
        }
        // JungleClear Mode

        private static void JungleClear()
        {
            var monster = GameObjects.Jungle.OrderByDescending(j => j.Health).FirstOrDefault(j => j.IsValidTarget(Q.Range));
            var useQ = JungleClearMenu["JQ"].GetValue<MenuBool>().Enabled;
            var useW = JungleClearMenu["JW"].GetValue<MenuBool>().Enabled;
            var useE = JungleClearMenu["JE"].GetValue<MenuBool>().Enabled;
            var mana = JungleClearMenu["JM"].GetValue<MenuSlider>().Value;

            if (Player.Instance.ManaPercent <= mana)
            {
                return;
            }

            if (ECasting)
            {
                return;
            }

            if (monster != null)
            {
                if (useQ && Q.CanCast(monster))
                {
                    Q.Cast(monster.Position);
                }

                if (useW && W.CanCast(monster))
                {
                    W.Cast(monster);
                }

                if (useE && E.CanCast(monster))
                {
                    if (useQ && useW)
                    {
                    
                            E.Cast(monster.Position);
                        
                    }
                    else
                    {
                        E.Cast(monster.Position);
                    }
                }
            }
        }

        public static void DrawFont(Font vFont, string vText, float vPosX, float vPosY, ColorBGRA vColor)
        {
            vFont.DrawText(null, vText, (int)vPosX, (int)vPosY, vColor);
        }

        // KillSteal

        private static void KillSteal()
        {
            var Enemies = GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(Q.Range) && !e.HasBuff("JudicatorIntervention") && !e.HasBuff("kindredrnodeathbuff") && !e.HasBuff("Undying Rage") && !e.IsZombie);
            var useQ = KillSteals["Q"].GetValue<MenuBool>().Enabled;
            var useW = KillSteals["W"].GetValue<MenuBool>().Enabled;
            var useIG = KillSteals["ign"].GetValue<MenuBool>().Enabled;

            foreach (var target in Enemies)
            {
                if (useQ && Q.CanCast(target))
                {
                    if (target.HealthPercent > 15)
                    {
                        if (target.Health + target.AllShield <= QDamage(target))
                        {
                            Q.Cast(target);
                            
                        }
                    }
                    else
                    {
                        if (target.Health + target.AllShield <= QDamage(target) * 1.5f)
                        {
                            Q.Cast(target);
                        }
                    }
                }

                if (useW && W.CanCast(target))
                {
                    if (target.Health + target.AllShield <= WDamage(target) + _Player.GetAutoAttackDamage(target))
                    {
                        W.Cast(target);
                    }
                }

                if (useIG && Ignite != null && Ignite.IsReady())
                {
                    if (target.Health + target.AllShield <= _Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite))
                    {
                        Ignite.Cast(target);
                    }
                }
            }
        }

        // Auto Harass

        private static void AutoHarass()
        {
            var useQ = HarassMenu["AutoQ"].GetValue<MenuBool>().Enabled;
            var mana = HarassMenu["AutoM"].GetValue<MenuSlider>().Value;

            if (Player.Instance.ManaPercent <= mana)
            {
                return;
            }

            foreach (var target in GameObjects.EnemyHeroes.Where(e => e.IsValidTarget(Q.Range) && !e.IsDead && !e.IsZombie))
            {
                if (useQ && Q.CanCast(target) && (!Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.Combo) || !Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.Harass)))
                {
                    if (HarassMenu["HarassQ" + target.CharacterName].GetValue<MenuBool>().Enabled)
                    {
                        Q.Cast(target);
                    }
                }
            }
        }

        // Use Items

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

                if ((item && Botrk.IsReady && Botrk.IsOwned() && target.IsValidTarget(475)) && (Player.Instance.HealthPercent <= Minhp || target.HealthPercent < Minhpp))
                {
                    Botrk.Cast(target);
                }

                if (yous && Youmuu.IsReady && Youmuu.IsOwned() && _Player.Distance(target) <= 325 && Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.Combo))
                {
                    Youmuu.Cast();
                }
            }
        }
    }
}
