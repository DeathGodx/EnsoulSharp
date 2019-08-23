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
using Geometry = EnsoulSharp.SDK.Geometry;

namespace T7_Blitzcrank
{
    class Program : Base
    {
        static void Main(string[] args) { GameEvent.OnGameLoad += OnLoad; }

        #region Events
        public static void OnLoad()
        {
            if (Player.Instance.CharacterName != CharacterName) return;

            Drawing.OnDraw += OnDraw;
            //AIHeroClientLevelUpEventArgs.AIHeroClientLevelUpEvent += OnLvlUp;
            AIBaseClient.OnBuffGain += OnBuffGain;
            Spellbook.OnCastSpell += OnCastSpell;
            //Orbwalker.OnAction += OnPreAttack;
            Interrupter.OnInterrupterSpell += OnInterruptableSpell;
            Gapcloser.OnGapcloser += OnGapcloser;
            Game.OnTick += OnTick;
            Game.OnUpdate += AutoE => { if (myhero.HasBuff(ESelfBuffName)) KnockupTarget(); };

            Potion = new Item((int)ItemId.Health_Potion,500);
            Biscuit = new Item((int)ItemId.Total_Biscuit_of_Rejuvenation, 500);
            RPotion = new Item((int)ItemId.Refillable_Potion, 500);

            Q = new Spell(SpellSlot.Q, 925);//, SkillShotType.Linear, 250, 1750, 70)
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 600);

            Player.LevelSpell(SpellSlot.Q);

            EnemyPlayerNames = GameObjects.EnemyHeroes.Select(x => x.CharacterName).ToString();
            EnemyADC = GetEnemyADC();

            DatMenu();
            CheckPrediction();

            Chat.Print("<font color='#0040FF'>T7</font><font color='#CDD411'> " + CharacterName + "</font> : Loaded!(v" + Version + ")");
            Chat.Print("<font color='#04B404'>By </font><font color='#3737E6'>Toyota</font><font color='#D61EBE'>7</font><font color='#FF0000'> <3 </font>");
        }



        private static void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args2)
        {
            if (sender.Owner.IsMe && args2.Slot.Equals(SpellSlot.E)) ;
        }

        private static void OnBuffGain(AIBaseClient sender, AIBaseClientBuffGainEventArgs args8)
        {
            if (check(combo, "CEONLY") && sender.IsEnemy && !sender.IsMinion && args8.Buff.Name == QTargetBuffName)
            {
                E.Cast();
            }
        }

        private static void OnInterruptableSpell(AIHeroClient sender, Interrupter.InterruptSpellArgs args4)
        {
            if (!sender.IsEnemy || !sender.IsValidTarget()) return;

            if (check(misc, "RINT") && R.IsReady() && R.IsInRange(sender))
            {
                R.Cast();
            }
            else if (check(misc, "QINT") && Q.CanCast(sender) && Q.GetPrediction(sender).Hitchance >= HitChance.VeryHigh)
            {
                Q.Cast(Q.GetPrediction(sender).CastPosition);
            }

        }

        private static void OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserArgs args5)
        {
            if (check(misc, "QGAP") && sender.IsEnemy && Q.IsReady() && Q.IsInRange(args5.EndPosition))
            {
                var qpred = Q.GetPrediction(sender);

                if (qpred != null && qpred.Hitchance == HitChance.VeryHigh)
                {
                    Q.Cast(sender.Position);
                }
            }
        }

        private static void OnTick(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.IsRecalling() || MenuGUI.IsChatOpen || ObjectManager.Player.IsWindingUp)
            {
                return;
            }
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LastHit:
                    break;
                case OrbwalkerMode.LaneClear:
                    Laneclear();
                    Jungleclear();
                    break;
            }
            if (Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.Combo))
            {
                Combo();
            }



            if (key(qsett, "FORCEQ"))
            {
               // Orbwalker.Move(Game.CursorPosRaw);
            }

            Misc();
        }

        private static void OnLvlUp(AIBaseClient sender, AIHeroClientLevelUpEventArgs args)
        {
            if (!sender.IsMe || !check(misc, "autolevel")) return;

            
            {
                if (myhero.Level > 1 && myhero.Level < 4)
                {
                    switch (myhero.Level)
                    {
                        case 2:
                            Player.LevelSpell(SpellSlot.E);
                            break;
                        case 3:
                            Player.LevelSpell(SpellSlot.W);
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
                    else if (myhero.Spellbook.CanSpellBeUpgraded(SpellSlot.E))
                    {
                        return;
                    }
                    else if (myhero.Spellbook.CanSpellBeUpgraded(SpellSlot.W))
                    {
                        return;
                    }
                }
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (myhero.IsDead || check(draw, "nodraw")) return;

            if (check(draw, "drawQ") && Q.Level > 0 && Q.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.Yellow, 1);
            }

            if (check(draw, "drawR") && R.Level > 0 && R.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, Color.Yellow, 1);
            }

            AIHeroClient target = GetTarget();//TargetSelector.GetTarget(Q.Range, DamageType.Magical, myhero.Position);

            if (target != null)
            {
                if (Q.IsReady())
                {
                    var qpred = Q.GetPrediction(target);

                    if (check(draw, "DRAWPRED"))
                    {
                        Geometry.Rectangle Prediction = new Geometry.Rectangle(myhero.Position, qpred.CastPosition, Q.Width);
                        Prediction.Draw(Color.Yellow, 1);
                    }

                    if (check(draw, "DRAWHIT"))
                    {
                        Drawing.DrawText(Drawing.WorldToScreen(myhero.Position).X - 50,
                                    Drawing.WorldToScreen(myhero.Position).Y + 10,
                                    Color.Yellow,
                                    "Hitchance %: ");
                        Drawing.DrawText(Drawing.WorldToScreen(myhero.Position).X + 37,
                                            Drawing.WorldToScreen(myhero.Position).Y + 10,
                                            Color.Green,
                                            qpred.Hitchance.ToString());
                    }
                }

                if (check(draw, "DRAWTARGET"))
                {
                    Render.Circle.DrawCircle(target.Position, 50, Color.Yellow, 1);
                }

                if (check(draw, "DRAWWAY") && target.Path.Any())
                {
                    for (var i = 1; target.Path.Length > i; i++)
                    {
                        if (target.Path[i - 1].IsValid() && target.Path[i].IsValid() && (target.Path[i - 1].IsOnScreen() || target.Path[i].IsOnScreen()))
                        {
                            Drawing.DrawLine(Drawing.WorldToScreen(target.Position), Drawing.WorldToScreen(target.Path[i]), 3, Color.White);
                        }
                    }
                }
            }
        }
        #endregion

        #region Modes
        private static void Combo()
        {
            var target = GetTarget();//TargetSelector.GetTarget(Q.Range, DamageType.MagicalAIMinionClient);
            var Qtarget = GetTarget();//TargetSelector.GetTarget(Q.Range, DamageType.MagicalAIMinionClient);

            if (Qtarget != null && Qtarget.IsValidTarget((int)Q.Range) && check(combo, "CQ") && Q.IsReady() && check(qsett, "QCLOSE") && check(qsett, "Q" + Qtarget.CharacterName))
            {
                Q.Cast(Q.GetPrediction(Qtarget).CastPosition);
            }

            if (target != null && target.IsValidTarget((int)Q.Range))
            {

                if (check(combo, "CW") && W.IsReady() && myhero.CountEnemyHeroesInRange(500) > 0)
                {
                    W.Cast();
                }

                if (check(combo, "CE") && E.IsReady() && myhero.CountEnemyHeroesInRange(300) > 0)
                {
                    E.Cast();
                }

                var MainChecks = check(combo, "CR") && R.IsReady() && !(R.IsReady() && E.IsReady()) && !myhero.HasBuff(ESelfBuffName);
                var EnemiesWithCC = check(combo, "CRAUTO") && GameObjects.EnemyHeroes.Any(x => (x.HasBuffOfType(BuffType.Knockup) || x.HasBuffOfType(BuffType.Stun)) && R.IsInRange(x));
                var MultipleEnemies = myhero.CountEnemyHeroesInRange(500) >= MenuSlider(combo, "CRMINE");
                var SingleTarget = R.CanCast(target) && target.Health + target.AllShield >= MenuSlider(combo, "CRMINH");
                var PreventKS = check(combo, "CAVOID") && target.Health + target.AllShield < myhero.GetSpellDamage(target, SpellSlot.R);

                if (MainChecks && (EnemiesWithCC || MultipleEnemies || SingleTarget))
                {
                    if (PreventKS) return;

                    R.Cast();
                }
            }
        }

        private static void Harass()
        {
            var target = GetTarget();//TargetSelector.GetTarget(Q.Range, DamageType.MagicalAIMinionClient);

            if (target != null && target.IsValidTarget((int)Q.Range))
            {
                if (check(harass, "HQ") && Q.IsReady() && check(qsett, "Q" + target.CharacterName))
                {
                    var Qpred = Q.GetPrediction(target);

                    if (Qpred.Hitchance >= HitChance.VeryHigh && Q.Cast(target.Position))
                        return;
                }

                if (check(harass, "HW") && W.IsReady() && myhero.CountEnemyHeroesInRange(myhero.GetRealAutoAttackRange()) >= MenuSlider(harass, "HWMIN") && W.Cast())
                    return;

                if (check(harass, "HR") && R.IsReady() && myhero.CountEnemyHeroesInRange(R.Range - 10) >= MenuSlider(harass, "HRMIN") && R.Cast())
                    return;
            }
        }

        private static void Laneclear()
        {
            var minions = GameObjects.EnemyMinions.Where(e => e.IsValidTarget(Q.Range) && e.IsMinion())
                          .Cast<AIBaseClient>().ToList();

            if (minions != null)
            {
                if (check(laneclear, "LQ") && Q.IsReady())
                {
                    foreach (AIMinionClient minion in minions.Where(x => x.Distance(myhero.Position) < Q.Range - 75))
                    {
                        var qpred = Q.GetPrediction(minion);

                        if (qpred.Hitchance >= HitChance.VeryHigh && Q.Cast(minion.Position))
                        {
                            return;
                        }
                    }
                }

                if (check(laneclear, "LW") && W.IsReady() && minions.Where(x => x.Distance(myhero.Position) < 250).Count() >= MenuSlider(jungleclear, "JWMIN") &&
                    W.Cast())
                {
                    return;
                }

                if (check(laneclear, "LE") && E.IsReady() && minions.Any(x => x.Health > 50 && x.Distance(myhero.Position) < myhero.GetRealAutoAttackRange()) &&
                    E.Cast())
                {
                    return;
                }

                if (check(laneclear, "LR") && R.IsReady() && minions.Where(x => x.Distance(myhero.Position) < R.Range - 10).Count() >= MenuSlider(jungleclear, "JRMIN") &&
                    R.Cast())
                {
                    return;
                }
            }
        }

        private static void Jungleclear()
        {
            var Monsters = GameObjects.Jungle.Where(e => e.IsValidTarget(Q.Range) && e.IsMinion())
                          .Cast<AIBaseClient>().ToList();

            if (Monsters != null)
            {
                if (check(jungleclear, "JQ") && Q.IsReady())
                {
                    foreach (AIMinionClient monster in Monsters)
                    {
                        if (comb(jungleclear, "JQMODE") == 0 && monster.Name.Contains("Mini")) continue;

                        var qpred = Q.GetPrediction(monster);

                        if (qpred.Hitchance >= HitChance.VeryHigh && Q.Cast(monster.Position))
                        {
                            return;
                        }
                    }
                }

                if (check(jungleclear, "JW") && W.IsReady() && Monsters.Where(x => x.Distance(myhero.Position) < 250).Count() >= MenuSlider(jungleclear, "JWMIN") &&
                    W.Cast())
                {
                    return;
                }

                if (check(jungleclear, "JE") && E.IsReady())
                {
                    foreach (AIMinionClient monster in Monsters.Where(x => x.Distance(myhero.Position) < myhero.GetRealAutoAttackRange()))
                    {
                        if (comb(jungleclear, "JEMODE") == 0 && monster.Name.Contains("Mini")) continue;

                        if (E.Cast())
                        {

                            Player.IssueOrder(GameObjectOrder.AttackUnit, monster);

                            
                            
                        }
                    }
                }

                if (check(jungleclear, "JR") && R.IsReady() && Monsters.Where(x => x.Distance(myhero.Position) < R.Range - 10).Count() >= MenuSlider(jungleclear, "JRMIN") &&
                    R.Cast())
                {
                    return;
                }
            }
        }

        private static void Misc()
        {
            var Qtarget = GetTarget();//TargetSelector.GetTarget(Q.Range, DamageType.MagicalAIMinionClient);

            if (Qtarget != null && Qtarget.IsValidTarget((int)Q.Range) && key(qsett, "FORCEQ") && Q.IsReady() && check(qsett, "Q" + Qtarget.CharacterName))
            {
                Q.Cast(Q.GetPrediction(Qtarget).CastPosition);
            }

            var target = TargetSelector.GetTarget(500, DamageType.Magical);

            if (target != null && target.IsValidTarget(1000) && check(misc, "KSR") && R.CanCast(target) && target.Health < myhero.GetSpellDamage(target, SpellSlot.R) &&
                target.Health + target.AllShield > 0)
            {
                R.Cast();
            }

           /* if (check(misc, "AUTOPOT") && !myhero.HasBuffOfType(BuffType.Heal) && myhero.HealthPercent <= MenuSlider(misc, "POTMIN"))
            {
                if (Item.HasItem(Potion.Id) && Item.CanUseItem(Potion.Id)) Potion.Cast();

                else if (Item.HasItem(Biscuit.Id) && Item.CanUseItem(Biscuit.Id)) Biscuit.Cast();

                else if (Item.HasItem(RPotion.Id) && Item.CanUseItem(RPotion.Id)) RPotion.Cast();
            }*/

//            if (check(misc, "skinhax")) myhero.SetSkinId((int)misc["skinID"].GetValue<MenuList>().Index);
        }

        #endregion

        #region Menu
        public static void DatMenu()
        {
            menu = new Menu("T7 Blitz", "blitz",true);
            qsett = new Menu("Q Settings", "qsettings");
            combo = new Menu("Combo", "combo");
            harass = new Menu("Harass", "harass");
            laneclear = new Menu("Laneclear", "lclear");
            jungleclear = new Menu("Jungleclear", "jclear");
            draw = new Menu("Drawings", "draw");
            misc = new Menu("Misc", "misc");


            qsett.Add(new MenuSeparator("Q Settings", "Q Settings"));
            qsett.Add(new MenuSeparator("Forced Q Casting", "Forced Q Casting"));
            qsett.Add(new MenuKeyBind("FORCEQ", "Force Q To Cast", System.Windows.Forms.Keys.B, KeyBindType.Press));
            qsett.Add(new MenuBool("QINFO", "Info About Forced Q Casting MenuKeyBind", false)); //.val +=
            if (qsett["QINFO"].GetValue<MenuBool>().Enabled)
            {
                Chat.Print("<font color='#A5A845'>Force Q Casting Info</font>:");
                Chat.Print("This Keybind Will Cast Q At The Target Champion Using The Current Q Prediction,");
                Chat.Print("Which Means That It Will Ignore Collision Checks Or Hitchance Numbers Lower Than The Ones On The Settings.");
                Chat.Print("You Can See The Current Q Prediction Using The Addon's Drawing Functions.");
                Chat.Print("I Also Wouldnt Recommend Using This Function Without The Addon's Prediction Drawings(You Wont See The Cast Position Otherwise!).");
                //sender.CurrentValue = false;
            }
            qsett.Add(new MenuSeparator("Q Hitchance %", "Q Hitchance %"));
            qsett.Add(new MenuSlider("QPRED", "Select Minimum Hitchance %", 65, 1, 100));
  
            qsett.Add(new MenuSeparator("Q Targets:", "Q Targets:"));
            foreach (AIHeroClient enemy in GameObjects.EnemyHeroes)
            {
                qsett.Add(new MenuBool("Q" + enemy.CharacterName, enemy.CharacterName));
            }
  
            qsett.Add(new MenuBool("QCLOSE", "Dont Grap If Target Is In AA Range"));
            menu.Add(qsett);
            combo.Add(new MenuSeparator("Spells", "Spells"));
            combo.Add(new MenuBool("CQ", "Use Q"));
            combo.Add(new MenuSeparator("(For Q Options Go To Q Settings Tab)", "(For Q Options Go To Q Settings Tab)"));
            combo.Add(new MenuBool("CW", "Use W"));
            combo.Add(new MenuSlider("CWMIN", "Min Enemies Nearby To Cast W", 1, 1, 5));
            combo.Add(new MenuBool("CE", "Use E"));
            combo.Add(new MenuBool("CEONLY", "Auto-E After Succesful Grab"));
            combo.Add(new MenuBool("CR", "Use R"));
            combo.Add(new MenuBool("CRAUTO", "Auto R On Knocked-Up/Stunned Targets"));
            combo.Add(new MenuSlider("CRMINE", "Min Enemies For R", 2, 1, 5));
            combo.Add(new MenuSlider("CRMINH", "Min Enemy Health % For R", 30, 1, 100));
            combo.Add(new MenuBool("CAVOID", "Prevent R Killsteals"));
            combo.Add(new MenuBool("CAVOIDAA", "Prevent AA KillSteals"));
            menu.Add(combo);
            harass.Add(new MenuSeparator("Spells", "Spells"));
            harass.Add(new MenuBool("HQ", "Use Q", false));
            harass.Add(new MenuSeparator("(For Q Options Go To Q Settings Tab)", "(For Q Options Go To Q Settings Tab)"));
            harass.Add(new MenuBool("HW", "Use W", false));
            harass.Add(new MenuSlider("HWMIN", "Min Enemies For W", 2, 1, 5));
            harass.Add(new MenuBool("HR", "Use R", false));
            harass.Add(new MenuSlider("HRMIN", "Min Enemies For R", 2, 1, 5));
            harass.Add(new MenuSlider("HMIN", "Min Mana % To Harass", 50, 1, 100));
            menu.Add(harass);
            laneclear.Add(new MenuSeparator("Spells", "Spells"));
            laneclear.Add(new MenuBool("LQ", "Use Q"));
            laneclear.Add(new MenuBool("LW", "Use W"));
            laneclear.Add(new MenuSlider("LWMIN", "Min Minions For W", 3, 1, 10));
            laneclear.Add(new MenuBool("LE", "Use E"));
            laneclear.Add(new MenuBool("LR", "Use R", false));
            laneclear.Add(new MenuSlider("LRMIN", "Min Minions For R", 4, 1, 10));
            laneclear.Add(new MenuSlider("LMIN", "Min Mana % To Laneclear", 50, 1, 100));
            menu.Add(laneclear);
            jungleclear.Add(new MenuSeparator("Spells", "Spells"));
            jungleclear.Add(new MenuBool("JQ", "Use Q"));
            jungleclear.Add(new MenuList("JQMODE", "Select Q Targets", new[] { "Big Monsters", "All Monsters" }) {Index = 0 });
            jungleclear.Add(new MenuBool("JW", "Use W"));
            jungleclear.Add(new MenuSlider("JWMIN", "Min Monsters For W", 2, 1, 4));
            jungleclear.Add(new MenuBool("JE", "Use E"));
            jungleclear.Add(new MenuList("JEMODE", "Select E Targets", new[] { "Big Monsters", "All Monsters"}) {Index = 0 });
            jungleclear.Add(new MenuBool("JR", "Use R"));
            jungleclear.Add(new MenuSlider("JRMIN", "Min Monsters For R", 3, 1, 4));
            jungleclear.Add(new MenuSlider("JMIN", "Min Mana % To Jungleclear", 50, 1, 100));
            menu.Add(jungleclear);
            draw.Add(new MenuBool("nodraw", "Disable All Drawings", false));
            draw.Add(new MenuBool("drawQ", "Draw Q Range"));
            draw.Add(new MenuBool("drawR", "Draw R Range"));
            draw.Add(new MenuBool("nodrawc", "Draw Only Ready Spells", false));
            draw.Add(new MenuSeparator("Q Drawings", "Q Drawings"));
            draw.Add(new MenuBool("DRAWPRED", "Draw Q Prediction", false));
            draw.Add(new MenuBool("DRAWTARGET", "Draw Q Target", false));
            draw.Add(new MenuBool("DRAWHIT", "Draw Q Hitchance", false));
            draw.Add(new MenuBool("DRAWWAY", "Draw Targets Waypoint", false));
            menu.Add(draw);
            misc.Add(new MenuSeparator("Focusing Settings", "Focusing Settings"));
            misc.Add(new MenuList("FOCUS", "Focus On: ", new[] { "Enemy ADC", "All Champs(TS)", "Custom Champion" }) { Index = 0 });
            
            misc.Add(new MenuList("CFOCUS", "Which Champion To Focus On? ", new string[] { EnemyPlayerNames }));
            misc.Add(new MenuSeparator("Other Settings", "Other Settings"));
            misc.Add(new MenuBool("KSR", "Killsteal with R"));
            misc.Add(new MenuBool("WFLEE", "Use W To Flee"));
            misc.Add(new MenuBool("QGAP", "Use Q On Gapclosers", false));
            misc.Add(new MenuSeparator("Interrupting", "Interrupting"));
            misc.Add(new MenuBool("RINT", "Use R To Interrupt"));
            misc.Add(new MenuBool("QINT", "Use Q To Interrupt"));
            misc.Add(new MenuSeparator("Auto Potion", "Auto Potion"));
            misc.Add(new MenuBool("AUTOPOT", "Activate Auto Potion"));
            misc.Add(new MenuSlider("POTMIN", "Min Health % To Active Potion", 25, 1, 100));
            misc.Add(new MenuSeparator("Auto Level Up Spells", "Auto Level Up Spells"));
            misc.Add(new MenuBool("autolevel", "Activate Auto Level Up Spells"));
            misc.Add(new MenuSeparator("Skin Hack", "Skin Hack"));
            misc.Add(new MenuBool("skinhax", "Activate Skin hack"));
            misc.Add(new MenuList("skinID", "Skin Hack", new[]
            {
                "Default",
                "Rusty",
                "Goalkeeper",
                "Boom Boom",
                "Piltover Customs",
                "Definitely Not",
                "iBlitz",
                "Riot",
                "Chroma Red",
                "Chroma Blue",
                "Chroma Gray",
                "Battle Boss"
            })
            {Index = 0 });
            menu.Add(misc);
            menu.Attach();
        }
        #endregion
    }

}

