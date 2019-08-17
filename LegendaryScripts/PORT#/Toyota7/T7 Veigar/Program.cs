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
using SPrediction;

namespace T7_Veigar_V2
{
    class Program
    {
        #region Declarations
        static void Main(string[] args) { GameEvent.OnGameLoad+= OnLoad; }
        public static AIHeroClient myhero { get { return ObjectManager.Player; } }
        private static Menu menu, combo, harass, laneclear, jungleclear, misc, draw, pred;

        public static Prediction.Input QDATA = new Prediction.Input
        {
            SpellDelay = 0.25f,
            //Radius = DemSpells.Q.Width,
            SpellRange= DemSpells.Q.Range - 30,
            SpellMissileSpeed = DemSpells.Q.Speed,
            SpellSkillShotType = SkillshotType.Line,
        };

        public static Prediction.Input WDATA = new Prediction.Input
        {
            SpellDelay = 1.25f,
            //Radius = DemSpells.W.Width,
            SpellRange = DemSpells.W.Range - 5,
            SpellMissileSpeed = DemSpells.W.Speed,
            SpellSkillShotType = SkillshotType.Circle
        };

        public static Prediction.Input EDATA = new Prediction.Input
        {
            SpellDelay= 0.5f,
            //Radius = DemSpells.E.Width,
            SpellRange= DemSpells.E.Range,
            SpellMissileSpeed= DemSpells.E.Speed,
            SpellSkillShotType= SkillshotType.Circle
        };
        private static Spell Q;
        private static Spell W;
        private static Spell E;
        private static Spell R;
        private static Spell Ignite { get; set; }

        static readonly string CharacterName = "Veigar";
        static readonly string Version = "2.0";
        static readonly string Date = "5/9/16";

        public static Item Potion { get; private set; }
        public static Item Biscuit { get; private set; }
        public static Item RPotion { get; private set; }
        #endregion

        #region Events
        private static void OnLoad()
        {
            if (Player.Instance.CharacterName != CharacterName) { return; }

            Chat.Print("<font color='#0040FF'>T7</font><font color='#FF0505'> Veigar</font><font color='#CC3939'>:R </font> : Loaded!(v" + Version + ")");
            Chat.Print("<font color='#7752FF'>By </font><font color='#0FA348'>Toyota</font><font color='#7752FF'>7</font><font color='#FF0000'> <3 </font>");
            Chat.Print("<font color='#7752FF'>By </font><font color='#0FA348'>DEATHGOD PORTED</font><font color='#7752FF'>7</font><font color='#FF0000'> <3 </font>");

            Drawing.OnDraw += OnDraw;
            AIBaseClient.OnCreate += OnLvlUp;
            Game.OnTick += OnTick;
            Gapcloser.OnGapcloser += delegate (AIHeroClient sender, Gapcloser.GapcloserArgs args2)
            {
                if (DemSpells.E.CanCast(sender) && sender.IsEnemy && comb(misc, "gapmode") != 0 && sender != null)
                {
                    EDATA.Target = sender;
                    var targetE = TargetSelector.GetTarget(E.Range, DamageType.Magical);
                    var Epred = E.GetPrediction(targetE);

                    if (comb(misc, "gapmode") == 1 && !sender.IsFleeing && sender.IsFacing(myhero) && DemSpells.E.Cast(myhero.Position))
                        return;

                    else if (comb(misc, "gapmode") == 2 && Epred.Hitchance >= HitChance.VeryHigh && DemSpells.E.Cast(Epred.CastPosition))
                        return;
                }
            };

            Potion = new Item((int)ItemId.Health_Potion ,500);
            Biscuit = new Item((int)ItemId.Total_Biscuit_of_Rejuvenation, 500);
            RPotion = new Item((int)ItemId.Refillable_Potion, 500);

            Player.LevelSpell(SpellSlot.Q);


                Ignite = new Spell(myhero.GetSpellSlot("summonerdot"), 600);

            DatMenu();
        }

        private static void OnTick(EventArgs args)
        {
            if (myhero.IsDead) return;

            var flags = Orbwalker.ActiveMode;

            if (flags.HasFlag(OrbwalkerMode.Combo)) Combo();

            if (flags.HasFlag(OrbwalkerMode.Harass) || check(harass, "AUTOH") && myhero.ManaPercent > MenuSlider(harass, "HMIN")) Harass();

            if (flags.HasFlag(OrbwalkerMode.LaneClear) || check(laneclear, "AUTOL") && myhero.ManaPercent > MenuSlider(laneclear, "LMIN")) Laneclear();

            if (flags.HasFlag(OrbwalkerMode.LaneClear) && myhero.ManaPercent > MenuSlider(jungleclear, "JMIN")) Jungleclear();

            if (key(laneclear, "QSTACK") && MenuSlider(laneclear, "LMIN") <= myhero.ManaPercent) QStack();

            Misc();
            CheckPrediction();
        }

        private static void OnLvlUp(GameObject sender, EventArgs args)
        {
            if (!sender.IsMe || !check(misc, "autolevel")) return;

           
            {
                if (myhero.Level > 1 && myhero.Level < 4)
                {
                    switch (myhero.Level)
                    {
                        case 2:
                            Player.LevelSpell(SpellSlot.W);
                            break;
                        case 3:
                            Player.LevelSpell(SpellSlot.E);
                            break;
                    }
                }
                else if (myhero.Level >= 4)
                {
                    if (myhero.Spellbook.CanSpellBeUpgraded(SpellSlot.R))
                    {
                        return;
                    }
                    else if (myhero.Spellbook.CanSpellBeUpgraded(SpellSlot.Q))
                    {
                        return;
                    }
                    else if (myhero.Spellbook.CanSpellBeUpgraded(comb(misc, "LEVELMODE") == 1 ? SpellSlot.E : SpellSlot.W))
                    {
                        return;
                    }
                    else if (myhero.Spellbook.CanSpellBeUpgraded(comb(misc, "LEVELMODE") == 1 ? SpellSlot.W : SpellSlot.E))
                    {
                        return;
                    }
                }
            } new Random().Next(300, 600);
        }

        private static void OnDraw(EventArgs args)
        {
            if (myhero.IsDead || check(draw, "nodraw")) return;

            if (check(draw, "drawQ") && DemSpells.Q.Level > 0)
            {

                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.Fuchsia, 1);
            }

            if (check(draw, "drawW") && DemSpells.W.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, Color.Fuchsia, 1);
            }

            if (check(draw, "drawE") && DemSpells.E.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.Fuchsia, 1);
            }

            if (check(draw, "drawR") && DemSpells.R.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, Color.Fuchsia, 1);
            }

            if (check(draw, "drawk"))
            {
                foreach (var enemy in GameObjects.EnemyHeroes)
                {
                    if (enemy.IsVisibleOnScreen && ComboDamage(enemy) > enemy.Health)
                    {
                        Drawing.DrawText(Drawing.WorldToScreen(enemy.Position).X, Drawing.WorldToScreen(enemy.Position).Y - 30,
                                            Color.Green, "Killable With Combo");
                    }
                    else if (enemy.IsVisibleOnScreen && ComboDamage(enemy) + myhero.GetSummonerSpellDamage(enemy, SummonerSpell.Ignite) > enemy.Health)
                    {
                        Drawing.DrawText(Drawing.WorldToScreen(enemy.Position).X, Drawing.WorldToScreen(enemy.Position).Y - 30,
                                            Color.Green, "Combo + Ignite");
                    }
                }
            }

            if (check(draw, "drawStacks"))
            {
                Drawing.DrawText(Drawing.WorldToScreen(myhero.Position).X - 50, Drawing.WorldToScreen(myhero.Position).Y + 10,
                                 Color.Red, key(laneclear, "QSTACK") ? "Auto Stacking: ON" : "Auto Stacking: OFF");
            }

            if (check(draw, "drawStackCount") && myhero.HasBuff("veigarphenomenalevilpower"))
            {
                Drawing.DrawText(Drawing.WorldToScreen(myhero.Position).X - 25, Drawing.WorldToScreen(myhero.Position).Y + 25,
                                 Color.Red, "Count: " + myhero.GetBuffCount("veigarphenomenalevilpower").ToString());
            }
        }
        #endregion

        #region Modes
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(950, DamageType.Physical);

            if (target != null && target.IsValidTarget(950))
            {
                if (DemSpells.E.CanCast(target) && check(combo, "CE"))
                {
                    if (check(combo, "EIMMO") && target.HasBuffOfType(BuffType.Stun)) return;

                    EDATA.Target = target;
                    var targetE = TargetSelector.GetTarget(E.Range);
                    var Epred = E.GetPrediction(targetE);

                    switch (comb(combo, "CEMODE"))
                    {
                        case 0:
                            if (Epred.Hitchance >= HitChance.VeryHigh) DemSpells.E.Cast(Epred.CastPosition);
                            break;
                        case 1:
                            if (Epred.CastPosition.Distance(myhero.Position) < DemSpells.E.Range - 5)
                            {
                                switch (target.IsFleeing)
                                {
                                    case true:
                                        DemSpells.E.Cast(/*Epred.CastPosition.Distance.(myhero.Position, target.IsMoving ? 200 : 190)*/);
                                        break;
                                    case false:
                                        DemSpells.E.Cast(/*Epred.PredictedPosition.Extend(myhero.Position, target.IsMoving ? 200 : 190).To3D()*/);
                                        break;
                                }
                            }
                            break;
                        case 2:
                            
                            if (DemSpells.E.Cast()) return;
                            break;
                    }
                }

                if (DemSpells.Q.CanCast(target) && combo["CQ"].GetValue<MenuBool>().Enabled)
                {
                    QDATA.Target = target;
                    var targetQ = TargetSelector.GetTarget(Q.Range);
                    var Qpred = Q.GetPrediction(targetQ);

                    if (Qpred.CollisionObjects.Count() < 2 && Qpred.Hitchance >= HitChance.VeryHigh && DemSpells.Q.Cast(Qpred.CastPosition))
                    {
                        return;
                    }
                }

                if (DemSpells.W.CanCast(target) && check(combo, "CW"))
                {
                    WDATA.Target = target;
                    var targetW = TargetSelector.GetTarget(W.Range);
                    var Wpred = W.GetPrediction(targetW);

                    switch (comb(combo, "CWMODE"))
                    {
                        case 0:
                            if (Wpred.Hitchance >= HitChance.VeryHigh || Wpred.Hitchance == HitChance.Immobile ||
                               (target.HasBuffOfType(BuffType.Slow) && Wpred.Hitchance == HitChance.High))
                            {
                                DemSpells.W.Cast(Wpred.CastPosition);
                            }
                            break;
                        case 1:
                            DemSpells.W.Cast(target.Position);
                            break;
                        case 2:
                            if (target.HasBuffOfType(BuffType.Stun) || Wpred.Hitchance == HitChance.Immobile)
                            {
                                DemSpells.W.Cast(Wpred.CastPosition);
                            }
                            break;
                    }
                }

                if (check(combo, "CR") && DemSpells.R.IsReady() &&
                    DemSpells.R.IsInRange(target.Position) && ComboDamage(target) > target.Health &&
                    RDamage(target) > target.Health && !target.HasBuff("bansheesveil") && !target.HasBuff("fioraw"))
                {
                    if ((ComboDamage(target) - RDamage(target)) > target.Health) return;
                    DemSpells.R.Cast(target);
                }

                if (check(combo, "IgniteC") && Ignite.IsReady() && ComboDamage(target) < target.Health &&
                    Ignite.IsInRange(target.Position) && myhero.GetSummonerSpellDamage(target, SummonerSpell.Ignite) > target.Health &&
                    !check(misc, "autoign"))
                    Ignite.Cast(target);
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(950, DamageType.Physical);

            if (target != null && target.IsValidTarget(950))
            {
                if (DemSpells.Q.CanCast(target) && check(harass, "HQ"))
                {
                    QDATA.Target = target;
                    var targetQ = TargetSelector.GetTarget(Q.Range);
                    var Qpred = Q.GetPrediction(targetQ);

                    if (Qpred.CollisionObjects.Count() < 2 && Qpred.Hitchance >= HitChance.VeryHigh && DemSpells.Q.Cast(Qpred.CastPosition)) return;
                }

                if (DemSpells.W.CanCast(target) && check(harass, "HW"))
                {
                    WDATA.Target = target;
                    var targetW = TargetSelector.GetTarget(W.Range);
                    var Wpred = W.GetPrediction(targetW);

                    switch (comb(harass, "HWMODE"))
                    {
                        case 0:
                            if (Wpred.Hitchance >= HitChance.VeryHigh || Wpred.Hitchance == HitChance.Immobile ||
                               (target.HasBuffOfType(BuffType.Slow) && Wpred.Hitchance == HitChance.High))
                            {
                                DemSpells.W.Cast(Wpred.CastPosition);
                            }
                            break;
                        case 1:
                            DemSpells.W.Cast(target.Position);
                            break;
                        case 2:
                            if (target.HasBuffOfType(BuffType.Stun) || Wpred.Hitchance == HitChance.Immobile)
                            {
                                DemSpells.W.Cast(Wpred.CastPosition);
                            }
                            break;
                    }
                }
            }
        }

        private static void Laneclear()
        {
            var Minions= GameObjects.EnemyMinions.Where(e => e.IsValidTarget(950) && e.IsMinion())
                            .Cast<AIBaseClient>().ToList();

            if (Minions != null)
            {
                if (!key(laneclear, "QSTACK") && DemSpells.Q.IsReady() && check(laneclear, "LQ"))
                {
                    Minions.ToList().ForEach(x =>
                    {
                        QDATA.Target = x;
                        var targetQ = TargetSelector.GetTarget(Q.Range);
                        DemSpells.Q.Cast(Q.GetPrediction(targetQ).CastPosition);
                    });
                }

                if (DemSpells.W.IsReady() && check(laneclear, "LW") && DemSpells.W.Cast()) return;
            }
        }

        private static void Jungleclear()
        {
            
            var Monsters = GameObjects.Jungle.Where(e => e.IsValidTarget(950));
            if (Monsters != null)
            {
                if (check(jungleclear, "JQ") && DemSpells.Q.IsReady())
                {
                    foreach (AIMinionClient monster in Monsters.Where(x => DemSpells.Q.CanCast(x)))
                    {
                        if (comb(jungleclear, "JQMODE") == 0 && monster.Name.Contains("Mini")) return;

                        QDATA.Target = monster;
                        var targetQ = TargetSelector.GetTarget(Q.Range);
                        var Qpred = Q.GetPrediction(targetQ);

                        if (Qpred.CollisionObjects.Count() < 2 && DemSpells.Q.Cast(Qpred.CastPosition)) return;
                    }
                }

                if (check(jungleclear, "JW") && DemSpells.W.IsReady()) return;

                if (check(jungleclear, "JE") && DemSpells.E.IsReady()) return;
            }
        }

        private static void Misc()
        {
            var target = TargetSelector.GetTarget(1000, DamageType.Magical);

            if (target != null && target.IsValidTarget(1000))
            {

                if (check(misc, "KSQ") && DemSpells.Q.CanCast(target) && myhero.GetSpellDamage(target, SpellSlot.Q) > target.Health)
                {
                    QDATA.Target = target;
                    var targetQ = TargetSelector.GetTarget(Q.Range);
                    var Qpred = Q.GetPrediction(targetQ);

                    if (Qpred.Hitchance >= HitChance.VeryHigh && DemSpells.Q.Cast(Qpred.CastPosition)) return;
                }

                if (check(misc, "KSW") && DemSpells.W.CanCast(target) && myhero.GetSpellDamage(target, SpellSlot.W) > target.Health)
                {
                    WDATA.Target = target;
                    var targetW = TargetSelector.GetTarget(W.Range);
                    var Wpred = W.GetPrediction(targetW);

                    if (Wpred.Hitchance >= HitChance.VeryHigh && DemSpells.Q.Cast(Wpred.CastPosition)) return;
                }

                if (check(misc, "KSR") && DemSpells.R.CanCast(target) && RDamage(target) > target.Health && !target.HasBuff("bansheesveil") && !target.HasBuff("fioraw") && DemSpells.R.Cast(target))
                {
                    return;
                }

                if (Ignite != null && check(misc, "autoign") && Ignite.IsReady() && target.IsValidTarget(Ignite.Range) &&
                    myhero.GetSummonerSpellDamage(target, SummonerSpell.Ignite) > target.Health)
                {
                    Ignite.Cast(target);
                }
            }

            if (check(misc, "KSJ") && DemSpells.W.IsReady() && DemSpells.W.IsReady() && GameObjects.Jungle.Count(x => x.IsValidTarget(DemSpells.W.Range)) > 0)
            {
                foreach (var monster in GameObjects.Jungle.Where(x => !x.Name.ToLower().Contains("mini") && !x.IsDead &&
                                                                                                   x.Health > 200 && x.IsValidTarget(Q.Range)&&
                                                                                                   (x.Name.ToLower().Contains("dragon") ||
                                                                                                    x.Name.ToLower().Contains("baron") ||
                                                                                                    x.Name.ToLower().Contains("herald"))))
                {
                    WDATA.Target = monster;
                    var targetW = TargetSelector.GetTarget(W.Range);
                    var Wpred = W.GetPrediction(targetW);

                    if (monster.Name.ToLower().Contains("herald") && Wpred.Hitchance == HitChance.High && DemSpells.W.Cast(Wpred.CastPosition)) return;

                    else if (!monster.Name.ToLower().Contains("herald") && DemSpells.W.Cast(monster.Position)) return;
                }
            }

            if (check(misc, "AUTOPOT") && (!myhero.HasBuff("RegenerationPotion") && !myhero.HasBuff("ItemMiniRegenPotion") && !myhero.HasBuff("ItemCrystalFlask")) &&
                myhero.HealthPercent <= MenuSlider(misc, "POTMIN"))
            {
                if (Potion.Cast());

                else if (Biscuit.Cast());

                else if (RPotion.Cast());
            }

           
        }
        #endregion

        #region Menu
        public static void DatMenu()
        {
            menu = new Menu("T7 Veigar:R", "veigarxd");
            combo = new Menu("Combo", "combo");
            harass = new Menu("Harass", "harass");
            laneclear = new Menu("Laneclear", "lclear");
            jungleclear = new Menu("Jungleclear", "jclear");
            misc = new Menu("Misc", "misc");
            draw = new Menu("Drawings", "draw");
            pred = new Menu("Prediction", "pred");

            menu.Add(new MenuSeparator("Welcome to T7 Veigar:R And Thank You For Using!", "Welcome to T7 Veigar:R And Thank You For Using!"));
            menu.Add(new MenuSeparator("Version " + Version + " " + Date, "Version " + Version + " " + Date));
            menu.Add(new MenuSeparator("Author: Toyota7", "Author: Toyota7"));
            
            combo.Add(new MenuSeparator("Spells", "Spells"));
            combo.Add(new MenuBool("CQ", "Use Q"));
            combo.Add(new MenuBool("CW", "Use W"));
            combo.Add(new MenuBool("CE", "Use E"));
            combo.Add(new MenuBool("CR", "Use R"));
            if (Ignite != null) combo.Add(new MenuBool("IgniteC", "Use Ignite", false));
            combo.Add(new MenuSeparator("W Mode:", "W Mode:"));
            combo.Add(new MenuList("CWMODE", "Select Mode", new[] { "With Prediciton", "Without Prediction", "Only On Stunned Enemies" }) {Index = 0 });
            combo.Add(new MenuSeparator("E Options:", "E Options:"));
            combo.Add(new MenuList("CEMODE", "E Mode: ", new[] { "Target On The Center", "Target On The Edge(stun)", "AOE" }) { Index = 0 });
            combo.Add(new MenuSlider("CEAOE", "Min Champs For AOE Function", 2, 1, 5));
            combo.Add(new MenuBool("EIMMO", "Dont Use E On Immobile Enemies"));
            menu.Add(combo);
            harass.Add(new MenuSeparator("Spells", "Spells"));
            harass.Add(new MenuBool("HQ", "Use Q", false));
            harass.Add(new MenuBool("HW", "Use W", false));
            harass.Add(new MenuList("HWMODE", "Select Mode", new[] { "With Prediciton", "Without Prediction(Not Recommended)", "Only On Stunned Enemies" }) {Index = 0 });
            harass.Add(new MenuBool("AUTOH", "Auto harass", false));
            harass.Add(new MenuSlider("HMIN", "Min Mana % To Harass", 40, 0, 100));
            menu.Add(harass);
            laneclear.Add(new MenuSeparator("Spells", "Spells"));
            laneclear.Add(new MenuBool("LQ", "Use Q"));
            laneclear.Add(new MenuSeparator("Q Stacking", "Q Stacking"));
            laneclear.Add(new MenuKeyBind("QSTACK", "Auto Stacking", System.Windows.Forms.Keys.H, KeyBindType.Toggle));
            laneclear.Add(new MenuList("QSTACKMODE", "Select Mode", new[] { "LastHit 1 Minion", "LastHit 2 Minions" }) { Index = 0 });
            laneclear.Add(new MenuBool("LW", "Use W", false));
            laneclear.Add(new MenuSlider("LWMIN", "Min Minions For W", 2, 1, 6));
            laneclear.Add(new MenuBool("AUTOL", "Auto Laneclear", false));
            laneclear.Add(new MenuSlider("LMIN", "Min Mana % To Laneclear", 50, 0, 100));
            menu.Add(laneclear);
            jungleclear.Add(new MenuSeparator("Spells", "Spells"));
            jungleclear.Add(new MenuBool("JQ", "Use Q", false));
            jungleclear.Add(new MenuList("JQMODE", "Q Mode", new[] { "All Monsters", "Big Monsters" }) {Index = 0 });
            jungleclear.Add(new MenuBool("JW", "Use W", false));
            jungleclear.Add(new MenuBool("JE", "Use E", false));
            jungleclear.Add(new MenuSlider("JMIN", "Min Mana % To Jungleclear", 10, 0, 100));
            menu.Add(jungleclear);
            draw.Add(new MenuBool("nodraw", "Disable All Drawings", false));
            draw.Add(new MenuBool("drawQ", "Draw Q Range"));
            draw.Add(new MenuBool("drawW", "Draw W Range"));
            draw.Add(new MenuBool("drawE", "Draw E Range"));
            draw.Add(new MenuBool("drawR", "Draw R Range"));
            draw.Add(new MenuBool("drawk", "Draw Killable Enemies", false));
            draw.Add(new MenuBool("nodrawc", "Draw Only Ready Spells", false));
            draw.Add(new MenuBool("drawStacks", "Draw Auto Stack Mode"));
            draw.Add(new MenuBool("drawStackCount", "Draw Stack Count", false));
            menu.Add(draw);
            misc.Add(new MenuSeparator("Killsteal", "Killsteal"));
            misc.Add(new MenuBool("KSQ", "Killsteal with Q", false));
            misc.Add(new MenuBool("KSW", "Killsteal with W", false));
            misc.Add(new MenuBool("KSR", "Killsteal with R", false));
            if (Ignite != null) misc.Add(new MenuBool("autoign", "Auto Ignite If Killable"));
            misc.Add(new MenuBool("KSJ", "Steal Dragon/Baron/Rift Herald With W", false));
            misc.Add(new MenuBool("AUTOPOT", "Auto Potion"));
            misc.Add(new MenuSlider("POTMIN", "Min Health % To Activate Potion", 50, 1, 100));
            misc.Add(new MenuSeparator("Gapcloser", "Gapcloser"));
            misc.Add(new MenuList("gapmode", "Use E On Mode:", new[] { "Off", "Self", "Enemy(Pred)" }) {Index = 0 });
            misc.Add(new MenuSeparator("Auto Level Up Spells", "Auto Level Up Spells"));
            misc.Add(new MenuBool("autolevel", "Activate Auto Level Up Spells"));
            misc.Add(new MenuList("LEVELMODE", "Select Sequence", new[] { "Q>E>W", "Q>W>E" }) {Index = 0 });
            misc.Add(new MenuSeparator("Skin Hack", "Skin Hack"));
            misc.Add(new MenuBool("skinhax", "Activate Skin hack"));
            misc.Add(new MenuList("skinID", "Skin Hack", new[] { "Default", "White Mage", "Curling", "Veigar Greybeard", "Leprechaun", "Baron Von", "Superb Villain", "Bad Santa", "Final Boss" }) {Index = 0 });
            menu.Add(misc);
            pred.Add(new MenuSeparator("Q HitChance", "Q HitChance"));
            pred.Add(new MenuSlider("QPred", "% Hitchance", 85, 0, 100));
            pred.Add(new MenuSeparator("W HitChance", "W HitChance"));
            pred.Add(new MenuSlider("WPred", "% Hitchance", 85, 0, 100));
            pred.Add(new MenuSeparator("E HitChance", "E HitChance"));
            pred.Add(new MenuSlider("EPred", "% Hitchance", 85, 0, 100));
            menu.Add(pred);
            menu.Attach();
        }
        #endregion

        #region Methods
        private static void CheckPrediction()
        {
            string CorrectPrediction = "SDK Beta Prediction";

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.LaneClear:
                    CorrectPrediction = "SDK Prediction";
                    break;
                default:
                    CorrectPrediction = "SDK Beta Prediction";
                    break;
            }
        }

        private static float ComboDamage(AIHeroClient target)
        {
            if (target != null)
            {
                float TotalDamage = 0;

                if (DemSpells.Q.IsReady()); 

                if (DemSpells.W.IsReady()); 

                if (DemSpells.R.IsReady()); 

                return TotalDamage;
            }
            return 0;
        }

        private static void QStack()
        {
            if (!DemSpells.Q.IsReady() || Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.Combo) || myhero.IsRecalling()) return;
            var minions = GameObjects.EnemyMinions.Where(e => e.IsValidTarget(Q.Range) && e.IsMinion())
                 .Cast<AIBaseClient>().ToList();


            if (minions != null)
            {
                foreach (var minion in minions.Where(x => !x.IsDead && x.IsValidTarget(DemSpells.Q.Range - 10) && x.Health < QDamage(x) - 10))
                {
                    var Qpred = DemSpells.Q.GetPrediction(minion);

                    var collisions = Qpred.CollisionObjects.ToList();

                    switch (comb(laneclear, "QSTACKMODE"))
                    {
                        case 0:
                            if (collisions.Count() <= 1)
                            {
                                DemSpells.Q.Cast(Qpred.CastPosition);

                            }
                            else
                            {
                                DemSpells.Q.Cast(Qpred.CastPosition);
                            }
                            break;
                        case 1:
                            if ((collisions.Count() == 1 &&
                                collisions.FirstOrDefault().Health < QDamage(collisions.FirstOrDefault()) - 10))
                            {
                                DemSpells.Q.Cast(Qpred.CastPosition);
                            }
                            else if (collisions.Count() == 2 && collisions[0].Health < QDamage(collisions[0]) - 10 &&
                                                               collisions[1].Health < QDamage(collisions[1]) - 10)
                            {
                                DemSpells.Q.Cast(Qpred.CastPosition);
                            }
                            break;
                    }
                }
            }
        }

        private static double QDamage(AIBaseClient target)
        {
            var index = DemSpells.Q.Level - 1;

            var QDamage = new[] { 70, 110, 150, 190, 230 }[index] +
                          (0.6f * myhero.FlatMagicDamageMod);

            return myhero.CalculateDamage(target, DamageType.Magical, (float)QDamage);
        }

        private static double RDamage(AIHeroClient target)
        {
            var level = DemSpells.R.Level;

            var damage = new float[] { 0, 175, 250, 325 }[level] + (((100 - target.HealthPercent) * 1.5) / 100) * new float[] { 0, 175, 250, 325 }[level] +
                0.75 * myhero.FlatMagicDamageMod;
            return myhero.CalculateDamage(target, DamageType.Magical, (float)damage);
        }

        public static bool check(Menu submenu, string sig)
        {
            return submenu[sig].GetValue<MenuBool>().Enabled;
        }

        public static int MenuSlider(Menu submenu, string sig)
        {
            return submenu[sig].GetValue<MenuSlider>().Value;
        }

        public static int comb(Menu submenu, string sig)
        {
            return submenu[sig].GetValue<MenuList>().Index;
        }

        public static bool key(Menu submenu, string sig)
        {
            return submenu[sig].GetValue<MenuKeyBind>().Active;
        }
        #endregion
    }

    public static class DemSpells
    {
        public static Spell Q { get; private set; }
        public static Spell W { get; private set; }
        public static Spell E { get; private set; }
        public static Spell R { get; private set; }

        static DemSpells()
        {
            Q = new Spell(SpellSlot.Q, 950);//, SkillShotType.Linear, 250, 2000, 70);
            W = new Spell(SpellSlot.W, 900);//, SkillShotType.Circular, 1250, int.MaxValue, 225)
            E = new Spell(SpellSlot.E, 700);//, SkillShotType.Circular, 500, int.MaxValue, 380)
            R = new Spell(SpellSlot.R, 650);
        }


    }

   /* public static class Extensions
    {
        public static bool CastOnAOELocation(this Spellspell, bool JungleMode = false)
        {
            var targets = JungleMode ? EntityManager.MinionsAndMonsters.GetJungleMonsters(Program.myhero.Position, (float)DemSpells.Q.Range).ToArray<AIBaseClient>() :
                                        EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Program.myhero.Position, (float)DemSpells.Q.Range).ToArray<AIBaseClient>();

            var AOE = Prediction.Position.GetPredictionAoe(targets, new Prediction.Position.PredictionData(
                                                Prediction.Position.PredictionData.PredictionType.Circular,
                                                (int)spell.Range,
                                                spell.Width,
                                                0,
                                                spell.Delay,
                                                spell.Speed,
                                                spell.AllowedCollisionCount,
                                                Player.Instance.Position))
                                                .OrderByDescending(x => x.GetCollisionObjects<AIMinionClient>().Count())
                                                .FirstOrDefault();

            if (AOE != null && spell.Cast(AOE.CastPosition)) return true;

            return false;
        }

        public static bool ValidTarget(this AIHeroClient hero, int range)
        {
            return !hero.HasBuff("UndyingRage") && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("ChronoShift") && !hero.HasBuff("kindredrnodeathbuff") &&
                   !hero.IsInvulnerable && !hero.IsDead && hero.IsValidTarget(range) && !hero.IsZombie &&
                   !hero.HasBuffOfType(BuffType.Invulnerability) && !hero.HasBuffOfType(BuffType.SpellImmunity) && !hero.HasBuffOfType(BuffType.SpellShield);

        }
    }*/
}