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

namespace D_Elise
{
    class Program
    {

        private const string ChampionName = "Elise";

        private static bool _human;

        private static bool _spider;

        private static Spell _humanQ, _humanW, _humanE, _r, _spiderQ, _spiderW, _spiderE;

        private static Menu Menu { get; set; }

        public static Menu comboMenu, harassMenu, itemMenu, clearMenu, miscMenu, jungleMenu, drawMenu, ksMenu, smiteMenu;

        private static SpellSlot _igniteSlot;

        private static AIHeroClient _player;

        private static readonly float[] HumanQcd = { 6, 6, 6, 6, 6 };

        private static readonly float[] HumanWcd = { 12, 12, 12, 12, 12 };

        private static readonly float[] HumanEcd = { 14, 13, 12, 11, 10 };

        private static readonly float[] SpiderQcd = { 6, 6, 6, 6, 6 };

        private static readonly float[] SpiderWcd = { 12, 12, 12, 12, 12 };

        private static readonly float[] SpiderEcd = { 26, 23, 20, 17, 14 };

        private static float _humQcd = 0, _humWcd = 0, _humEcd = 0;

        private static float _spidQcd = 0, _spidWcd = 0, _spidEcd = 0;

        private static float _humaQcd = 0, _humaWcd = 0, _humaEcd = 0;

        private static float _spideQcd = 0, _spideWcd = 0, _spideEcd = 0;

        private static Items.Item _tiamat, _hydra, _blade, _bilge, _rand, _lotis, _zhonya;

        private static SpellSlot _smiteSlot;

        private static Spell _smite;

        static void Main(string[] args)
        {
            GameEvent.OnGameLoad += Game_OnGameLoad;

        }
        public static void Game_OnGameLoad()
        {

            _player = ObjectManager.Player;
            if (_player.CharacterName != ChampionName) return;

            _humanQ = new Spell(SpellSlot.Q, 625f);
            _humanW = new Spell(SpellSlot.W, 950f);
            _humanE = new Spell(SpellSlot.E, 1075f);
            _spiderQ = new Spell(SpellSlot.Q, 475f);
            _spiderW = new Spell(SpellSlot.W, 0);
            _spiderE = new Spell(SpellSlot.E, 750f);
            _r = new Spell(SpellSlot.R, 0);

            _humanW.SetSkillshot(0.75f, 100f, 5000, true, true, SkillshotType.Line);
            _humanE.SetSkillshot(0.5f, 55f, 1450, true, true, SkillshotType.Line);

            _bilge = new Items.Item(3144, 475f);
            _blade = new Items.Item(3153, 425f);
            _hydra = new Items.Item(3074, 250f);
            _tiamat = new Items.Item(3077, 250f);
            _rand = new Items.Item(3143, 490f);
            _lotis = new Items.Item(3190, 590f);
            _zhonya = new Items.Item(3157, 10);


            _smite = new Spell(SpellSlot.Summoner1, 570f);
            _smiteSlot = SpellSlot.Summoner1;
            _smite = new Spell(SpellSlot.Summoner2, 570f);
            _smiteSlot = SpellSlot.Summoner2;

            _igniteSlot = _player.GetSpellSlot("SummonerDot");

            Menu = new Menu("D-Elise", "D-Elise", true);

            comboMenu = new Menu("Combo", "Combo");
            comboMenu.Add(new MenuBool("UseHumanQ", "Human Q"));
            comboMenu.Add(new MenuBool("UseHumanW", "Human W"));
            comboMenu.Add(new MenuBool("UseHumanE", "Human E"));
            comboMenu.Add(new MenuBool("UseRCombo", "Auto use R"));
            comboMenu.Add(new MenuBool("UseSpiderQ", "Spider Q"));
            comboMenu.Add(new MenuBool("UseSpiderW", "Spider W"));
            comboMenu.Add(new MenuBool("UseSpiderE", "Spider E"));
            Menu.Add(comboMenu);
            harassMenu = new Menu("Harass", "Harass");
            harassMenu.Add(new MenuBool("UseQHarass", "Human Q"));
            harassMenu.Add(new MenuBool("UseWHarass", "Human W"));
            harassMenu.Add(new MenuSlider("Harrasmana", "Minimum Mana", 60, 1, 100));
            Menu.Add(harassMenu);
            itemMenu = new Menu("Items", "items");
            itemMenu.Add(new MenuBool("Youmuu", "Use Youmuu's"));
            itemMenu.Add(new MenuBool("Bilge", "Use Bilge"));
            itemMenu.Add(new MenuSlider("BilgeEnemyhp", "If Enemy Hp <", 85, 1, 100));
            itemMenu.Add(new MenuSlider("Bilgemyhp", "Or Your Hp <", 85, 1, 100));
            itemMenu.Add(new MenuBool("Blade", "Use Bork"));
            itemMenu.Add(new MenuSlider("BladeEnemyhp", "If Enemy Hp <", 85, 1, 100));
            itemMenu.Add(new MenuSlider("Blademyhp", "Or Your Hp <", 85, 1, 100));
            itemMenu.Add(new MenuBool("Hextech", "Hextech Gunblade"));
            itemMenu.Add(new MenuSlider("HextechEnemyhp", "If Enemy Hp <", 85, 1, 100));
            itemMenu.Add(new MenuSlider("Hextechmyhp", "Or Your Hp <", 85, 1, 100));
            itemMenu.Add(new MenuSeparator("Deffensive Items", "Deffensive Items"));
            itemMenu.Add(new MenuBool("Omen", "Use Randuin Omen"));
            itemMenu.Add(new MenuSlider("Omenenemys", "Randuin if enemys>", 2, 1, 5));
            itemMenu.Add(new MenuBool("Zhonyas", "Use Zhonya's"));
            itemMenu.Add(new MenuSlider("Zhonyashp", "Use Zhonya's if HP%<", 20, 1, 100));
            itemMenu.Add(new MenuBool("useqss", "Use QSS/Mercurial Scimitar/Dervish Blade"));
            itemMenu.Add(new MenuBool("blind", "Blind"));
            itemMenu.Add(new MenuBool("charm", "Charm"));
            itemMenu.Add(new MenuBool("fear", "Fear"));
            itemMenu.Add(new MenuBool("flee", "Flee"));
            itemMenu.Add(new MenuBool("taunt", "Taunt"));
            itemMenu.Add(new MenuBool("snare", "Snare"));
            itemMenu.Add(new MenuBool("suppression", "Suppression"));
            itemMenu.Add(new MenuBool("stun", "Stun"));
            itemMenu.Add(new MenuBool("polymorph", "Polymorph"));
            itemMenu.Add(new MenuBool("silence", "Silence"));
            itemMenu.Add(new MenuList("Cleansemode", "Use Cleanse", new[] { "Always", "In Combo" }) { Index = 0 });
            itemMenu.Add(new MenuSeparator("Potions", "Potions"));
            itemMenu.Add(new MenuBool("usehppotions", "Use Healt potion/Refillable/Hunters/Corrupting/Biscuit"));
            itemMenu.Add(new MenuSlider("usepotionhp", "If Health % <", 35, 1, 100));
            itemMenu.Add(new MenuBool("usemppotions", "Use Hunters/Corrupting/Biscuit"));
            itemMenu.Add(new MenuSlider("usepotionmp", "If Mana % <", 35, 1, 100));
            Menu.Add(itemMenu);

            clearMenu = new Menu("Farm", "Farm");
            clearMenu.Add(new MenuBool("HumanQFarm", "Human Q"));
            clearMenu.Add(new MenuBool("HumanWFarm", "Human W"));
            clearMenu.Add(new MenuBool("SpiderQFarm", "Spider Q", false));
            clearMenu.Add(new MenuBool("SpiderWFarm", "Spider W"));
            clearMenu.Add(new MenuKeyBind("Farm_R", "Auto Switch (toggle)", System.Windows.Forms.Keys.L, KeyBindType.Toggle));
            clearMenu.Add(new MenuSlider("Lanemana", "Minimum Mana", 60, 1, 100));
            Menu.Add(clearMenu);
            jungleMenu = new Menu("Jungle", "Jungle");
            jungleMenu.Add(new MenuBool("HumanQFarmJ", "Human Q"));
            jungleMenu.Add(new MenuBool("HumanWFarmJ", "Human W"));
            jungleMenu.Add(new MenuBool("SpiderQFarmJ", "Spider Q"));
            jungleMenu.Add(new MenuBool("SpiderWFarmJ", "Spider W"));
            jungleMenu.Add(new MenuSlider("Junglemana", "Minimum Mana", 60, 1, 100));
            Menu.Add(jungleMenu);
            smiteMenu = new Menu("Smite", "Smite");
            smiteMenu.Add(new MenuKeyBind("Usesmite", "Use Smite(toggle)", System.Windows.Forms.Keys.H, KeyBindType.Toggle));
            smiteMenu.Add(new MenuBool("Useblue", "Smite Blue Early"));
            smiteMenu.Add(new MenuSlider("manaJ", "Smite Blue Early if MP% <", 35, 1, 100));
            smiteMenu.Add(new MenuBool("Usered", "Smite Red Early"));
            smiteMenu.Add(new MenuSlider("healthJ", "Smite Red Early if HP% <", 35, 1, 100));
            smiteMenu.Add(new MenuBool("smitecombo", "Use Smite in target"));
            smiteMenu.Add(new MenuBool("Smiteeee", "Smite Minion in HumanE path", false));
            Menu.Add(smiteMenu);
            miscMenu = new Menu("Misc", "Misc");
            miscMenu.Add(new MenuBool("Spidergapcloser", "SpiderE to GapCloser"));
            miscMenu.Add(new MenuBool("Humangapcloser", "HumanE to GapCloser"));
            miscMenu.Add(new MenuBool("UseEInt", "HumanE to Interrupt"));
            miscMenu.Add(new MenuKeyBind("autoE", "HUmanE with VeryHigh Chance", System.Windows.Forms.Keys.T, KeyBindType.Press));
            miscMenu.Add(new MenuList("Echange", "E Hit Combo", new[] { "Low", "Medium", "High", "Very High" }) { Index = 0 });
            Menu.Add(miscMenu);
            ksMenu = new Menu("KillSteal", "Ks");
            ksMenu.Add(new MenuBool("ActiveKs", "Use KillSteal"));
            ksMenu.Add(new MenuBool("HumanQKs", "Human Q"));
            ksMenu.Add(new MenuBool("HumanWKs", "Human W"));
            ksMenu.Add(new MenuBool("SpiderQKs", "Spider Q"));
            ksMenu.Add(new MenuBool("UseIgnite", "Use Ignite"));
            Menu.Add(ksMenu);
            drawMenu = new Menu("Drawings", "Drawings");
            drawMenu.Add(new MenuBool("DrawQ", "Human Q", false));
            drawMenu.Add(new MenuBool("DrawW", "Human W", false));
            drawMenu.Add(new MenuBool("DrawE", "Human E", false));
            drawMenu.Add(new MenuBool("SpiderDrawQ", "Spider Q", false));
            drawMenu.Add(new MenuBool("SpiderDrawE", "Spider E", false));
            drawMenu.Add(new MenuBool("Drawsmite", "Draw Smite", true));
            drawMenu.Add(new MenuBool("drawmode", "Draw Smite Mode", true));
            drawMenu.Add(new MenuBool("DrawCooldown", "Draw Cooldown", false));
            drawMenu.Add(new MenuBool("Drawharass", "Draw AutoHarass", true));
            Menu.Add(drawMenu);
            Menu.Attach();
            Gapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnInterrupterSpell += Interrupter2_OnInterruptableTarget;
            AIBaseClient.OnProcessSpellCast += OnProcessSpellCast;
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Chat.Print("<font color='#881df2'>D-Elise by Diabaths by DEATHGODx</font> Loaded.");
            Chat.Print(
                "<font color='#f2f21d'>If You like my work and want to support me,  plz donate via paypal in </font> <font color='#00e6ff'>ssssssssssmith@hotmail.com</font> (10) S");

        }

        public static bool getMenuBoolItem(Menu m, string item)
        {
            return m[item].GetValue<MenuBool>().Enabled;
        }

        public static int getMenuSliderItem(Menu m, string item)
        {
            return m[item].GetValue<MenuSlider>().Value;
        }

        public static bool getMenuKeyBindItem(Menu m, string item)
        {
            return m[item].GetValue<MenuKeyBind>().Active;
        }

        public static int getBoxItem(Menu m, string item)
        {
            return m[item].GetValue<MenuList>().Index;
        }


        private static void Game_OnGameUpdate(EventArgs args)
        {
            Cooldowns();

            _player = ObjectManager.Player;



            CheckSpells();
            if (getMenuKeyBindItem(smiteMenu, "Usesmite"))
            {
                Smiteuse();
            }
            if (Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.LastHit)
                || Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.LaneClear))
                FarmLane();

            if (Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.LaneClear))
            {
                JungleFarm();
            }
            Usepotion();
            if (Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.Combo))
            {
                Combo();
            }
            if (Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.Harass))
            {
                Harass();

            }
            if (getMenuBoolItem(ksMenu, "ActiveKs"))
            {
                KillSteal();
            }
            if (getMenuKeyBindItem(miscMenu, "autoE"))
            {
                AutoE();
            }
        }

        private static void Smiteontarget()
        {
            if (_smite == null) return;
            var hero = GameObjects.EnemyHeroes.FirstOrDefault(x => x.IsValidTarget(570));
            var smiteDmg = _player.GetSummonerSpellDamage(hero, SummonerSpell.Smite);
            var usesmite = getMenuBoolItem(smiteMenu, "smitecombo");
            if (usesmite
                && ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) == SpellState.Ready)
            {
                if (!hero.HasBuffOfType(BuffType.Stun) || !hero.HasBuffOfType(BuffType.Slow))
                {
                    ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, hero);
                }
                else if (smiteDmg >= hero.Health)
                {
                    ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, hero);
                }
            }
            if (usesmite
                && ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) == SpellState.Ready && hero.IsValidTarget(570))
            {
                ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, hero);
            }
        }

        private static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!getMenuBoolItem(drawMenu, "DrawCooldown")) return;
            if (sender.IsMe)
                //Game.PrintChat("Spell name: " + args.SData.Name.ToString());
                GetCDs(args);
        }

        private static void Usepotion()
        {
            var mobs = MinionManager.GetMinions(
                _player.Position,
                600);
            var iusehppotion = getMenuBoolItem(itemMenu, "usehppotions");
            var iusepotionhp = _player.Health
                               <= (_player.MaxHealth * (getMenuSliderItem(itemMenu, "usepotionhp")) / 100);
            var iusemppotion = getMenuBoolItem(itemMenu, "usemppotions");
            var iusepotionmp = _player.Mana
                               <= (_player.MaxMana * (getMenuSliderItem(itemMenu, "usepotionmp")) / 100);
            if (_player.InFountain() || ObjectManager.Player.HasBuff("Recall")) return;

            if (ObjectManager.Player.CountEnemyHeroesInRange(800) > 0
                || (mobs.Count > 0 && Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.LaneClear) && _smite != null))
            {
                if (iusepotionhp && iusehppotion
                    && !(ObjectManager.Player.HasBuff("RegenerationPotion")
                         || ObjectManager.Player.HasBuff("ItemMiniRegenPotion")
                         || ObjectManager.Player.HasBuff("ItemCrystalFlask")
                         || ObjectManager.Player.HasBuff("ItemCrystalFlaskJungle")
                         || ObjectManager.Player.HasBuff("ItemDarkCrystalFlask")))
                {

                    /* if (Items.HasItem(2010) && Items.CanUseItem(2010))
                     {
                         Items.UseItem(2010);
                     }
                     if (Items.HasItem(2003) && Items.CanUseItem(2003))
                     {
                         Items.UseItem(2003);
                     }
                     if (Items.HasItem(2031) && Items.CanUseItem(2031))
                     {
                         Items.UseItem(2031);
                     }
                     if (Items.HasItem(2032) && Items.CanUseItem(2032))
                     {
                         Items.UseItem(2032);
                     }
                     if (Items.HasItem(2033) && Items.CanUseItem(2033))
                     {
                         Items.UseItem(2033);
                     }*/
                }
                if (iusepotionmp && iusemppotion
                    && !(ObjectManager.Player.HasBuff("ItemDarkCrystalFlask")
                         || ObjectManager.Player.HasBuff("ItemMiniRegenPotion")
                         || ObjectManager.Player.HasBuff("ItemCrystalFlaskJungle")
                         || ObjectManager.Player.HasBuff("ItemCrystalFlask")))
                {
                    /*if (Items.HasItem(2041) && Items.CanUseItem(2041))
                    {
                        Items.UseItem(2041);
                    }
                    if (Items.HasItem(2010) && Items.CanUseItem(2010))
                    {
                        Items.UseItem(2010);
                    }
                    if (Items.HasItem(2032) && Items.CanUseItem(2032))
                    {
                        Items.UseItem(2032);
                    }
                    if (Items.HasItem(2033) && Items.CanUseItem(2033))
                    {
                        Items.UseItem(2033);
                    }*/
                }
            }
        }

        private static void UseItemes()
        {
            foreach (var hero in ObjectManager.Get<AIHeroClient>().Where(hero => hero.IsEnemy))
            {
                var iBilge = getMenuBoolItem(itemMenu, "Bilge");
                var iBilgeEnemyhp = hero.Health
                                    <= (hero.MaxHealth * (getMenuSliderItem(itemMenu, "BilgeEnemyhp")) / 100);
                var iBilgemyhp = _player.Health
                                 <= (_player.MaxHealth * (getMenuSliderItem(itemMenu, "Bilgemyhp")) / 100);
                var iBlade = getMenuBoolItem(itemMenu, "Blade");
                var iBladeEnemyhp = hero.Health
                                    <= (hero.MaxHealth * (getMenuSliderItem(itemMenu, "BladeEnemyhp")) / 100);
                var iBlademyhp = _player.Health
                                 <= (_player.MaxHealth * (getMenuSliderItem(itemMenu, "Blademyhp")) / 100);
                var iYoumuu = getMenuBoolItem(itemMenu, "Youmuu");
                var iHextech = getMenuBoolItem(itemMenu, "Hextech");
                var iHextechEnemyhp = hero.Health <=
                                      (hero.MaxHealth * (getMenuSliderItem(itemMenu, "HextechEnemyhp")) / 100);
                var iHextechmyhp = _player.Health <=
                                   (_player.MaxHealth * (getMenuSliderItem(itemMenu, "Hextechmyhp")) / 100);
                var iOmen = getMenuBoolItem(itemMenu, "Omen");
                var iOmenenemys = hero.CountEnemyHeroesInRange(450) >= getMenuSliderItem(itemMenu, "Omenenemys");
                var iZhonyas = getMenuBoolItem(itemMenu, "Zhonyas");
                var iZhonyashp = _player.Health
                                 <= (_player.MaxHealth * (getMenuSliderItem(itemMenu, "Zhonyashp")) / 100);
                if (hero.IsValidTarget(450) && iBilge && (iBilgeEnemyhp || iBilgemyhp) && _bilge.IsReady)
                {
                    _bilge.Cast(hero);

                }
                if (hero.IsValidTarget(450) && iBlade && (iBladeEnemyhp || iBlademyhp) && _blade.IsReady)
                {
                    _blade.Cast(hero);

                }
                if (iOmenenemys && iOmen && _rand.IsReady && hero.IsValidTarget(450))
                {
                    _rand.Cast();
                }
                if (iZhonyas && iZhonyashp && ObjectManager.Player.CountEnemyHeroesInRange(1000) >= 1)
                {
                    _zhonya.Cast(_player);

                }
            }
        }

        private static void Combo()
        {
            foreach (var hero in ObjectManager.Get<AIHeroClient>().Where(hero => hero.IsEnemy))
            {
                var target = TargetSelector.GetTarget(_humanW.Range, DamageType.Magical);
                var sReady = (_smiteSlot != SpellSlot.Unknown
                              && ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) == SpellState.Ready);
                var qdmg = _player.GetSpellDamage(hero, SpellSlot.Q);
                var wdmg = _player.GetSpellDamage(hero, SpellSlot.W);
                //if (target == null) return; //buffelisecocoon
                Smiteontarget();
                if (_human)
                {
                    if (target.IsValidTarget(_humanE.Range) && getMenuBoolItem(comboMenu, "UseHumanE")
                        && _humanE.IsReady())
                    {
                        if (sReady && getMenuBoolItem(smiteMenu, "Smiteeee")
                            && _humanE.GetPrediction(target).CollisionObjects.Count == 1)
                        {
                            CheckingCollision(target);
                            _humanE.Cast(hero);
                        }
                        else if (_humanE.GetPrediction(target).Hitchance >= Echange())
                        {
                            _humanE.Cast(target);
                        }
                    }

                    if (target.IsValidTarget(_humanQ.Range) && getMenuBoolItem(comboMenu, "UseHumanQ")
                        && _humanQ.IsReady() && _humanW.IsReady() == false)
                    {
                        _humanQ.Cast(target);
                    }
                    if (target.IsValidTarget(_humanW.Range) && getMenuBoolItem(comboMenu, "UseHumanW")
                        && _humanW.IsReady() && _humanE.IsReady() == false)
                    {
                        _humanW.Cast(target);
                    }
                    if (!_humanQ.IsReady() && !_humanW.IsReady() && !_humanE.IsReady()
                        && getMenuBoolItem(comboMenu, "UseRCombo") && _r.IsReady())
                    {
                        _r.Cast();
                    }
                    if (!_humanQ.IsReady() && !_humanW.IsReady() && hero.IsValidTarget(_spiderQ.Range)
                        && getMenuBoolItem(comboMenu, "UseRCombo") && _r.IsReady())
                    {
                        _r.Cast();
                    }
                }
                if (!_spider) return;
                if (hero.IsValidTarget(_spiderQ.Range) && getMenuBoolItem(comboMenu, "UseSpiderQ")
                    && _spiderQ.IsReady())
                {
                    _spiderQ.Cast(hero);
                }
                if (hero.IsValidTarget(200) && getMenuBoolItem(comboMenu, "UseSpiderW") && _spiderW.IsReady())
                {
                    _spiderW.Cast();
                }
                if (hero.IsValidTarget(_spiderE.Range) && _player.Distance(target) > _spiderQ.Range
                    && getMenuBoolItem(comboMenu, "UseSpiderE") && _spiderE.IsReady() && !_spiderQ.IsReady())
                {
                    _spiderE.Cast(hero);
                }
                if (!hero.IsValidTarget(_spiderQ.Range) && !_spiderE.IsReady() && _r.IsReady() && !_spiderQ.IsReady()
                    && getMenuBoolItem(comboMenu, "UseRCombo"))
                {
                    _r.Cast();
                }
                if (_humanQ.IsReady() && _humanW.IsReady() && _r.IsReady() && getMenuBoolItem(comboMenu, "UseRCombo"))
                {
                    _r.Cast();
                }
                if (_humanQ.IsReady() && _humanW.IsReady() && _r.IsReady() && getMenuBoolItem(comboMenu, "UseRCombo"))
                {
                    _r.Cast();
                }
                if ((_humanQ.IsReady() && qdmg >= hero.Health || _humanW.IsReady() && wdmg >= hero.Health)
                    && getMenuBoolItem(comboMenu, "UseRCombo"))
                {
                    _r.Cast();
                }

                UseItemes();
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(_humanQ.Range, DamageType.Magical);

            if (_human && target.IsValidTarget(_humanQ.Range) && getMenuBoolItem(harassMenu, "UseQHarass")
                && _humanQ.IsReady())
            {
                _humanQ.Cast(target);
            }

            if (_human && target.IsValidTarget(_humanW.Range) && getMenuBoolItem(harassMenu, "UseWHarass")
                && _humanW.IsReady())
            {
                _humanW.Cast(target, false, true);
            }
        }

        private static void JungleFarm()
        {
            var jungleQ = (getMenuBoolItem(jungleMenu, "HumanQFarmJ")
                           && (100 * (_player.Mana / _player.MaxMana))
                           > getMenuSliderItem(jungleMenu, "Junglemana"));
            var jungleW = (getMenuBoolItem(jungleMenu, "HumanQFarmJ")
                           && (100 * (_player.Mana / _player.MaxMana))
                           > getMenuSliderItem(jungleMenu, "Junglemana"));
            var spiderjungleQ = getMenuBoolItem(jungleMenu, "SpiderQFarmJ");
            var spiderjungleW = getMenuBoolItem(jungleMenu, "SpiderWFarmJ");
            var switchR = (100 * (_player.Mana / _player.MaxMana)) < getMenuSliderItem(jungleMenu, "Junglemana");
            var mobs = GameObjects.Jungle.Where(e => e.IsValidTarget(_humanQ.Range))
               .Cast<AIBaseClient>().ToList();
            if (mobs.Count > 0)
            {
                foreach (var minion in mobs)
                    if (_human)
                    {
                        if (_humanE.IsReady() && minion.IsValidTarget(_humanE.Range) && _player.Distance(minion) <= _humanW.Range && _humanQ.IsReady() == false)
                        {
                            _humanE.Cast(minion);
                        }
                        if (jungleW && _humanW.IsReady() && !_humanQ.IsReady() && minion.IsValidTarget()
                            && _player.Distance(minion) <= _humanW.Range && _humanE.IsReady() == false)
                        {
                            _humanW.Cast(minion);
                        }

                        if (jungleQ && _humanQ.IsReady() && minion.IsValidTarget()
                            && _player.Distance(minion) <= _humanQ.Range)
                        {
                            _humanQ.Cast(minion);
                        }
                        if ((!_humanQ.IsReady() && !_humanW.IsReady()) || switchR)
                        {
                            _r.Cast();
                        }
                    }
                foreach (var minion in mobs)
                {
                    if (_spider)
                    {
                        if (spiderjungleQ && _spiderQ.IsReady() && minion.IsValidTarget()
                            && _player.Distance(minion) <= _spiderQ.Range)
                        {
                            _spiderQ.Cast(minion);
                        }
                        if (spiderjungleW && _spiderW.IsReady() && minion.IsValidTarget()
                            && _player.Distance(minion) <= 150)
                        {

                            _spiderW.Cast();
                        }
                        if (_r.IsReady() && _humanQ.IsReady() && _spider && !switchR)
                        {
                            _r.Cast();
                        }
                    }
                }
            }
        }

        private static void FarmLane()
        {
            var ManaUse = (100 * (_player.Mana / _player.MaxMana)) > getMenuSliderItem(clearMenu, "Lanemana");
            var useR = getMenuKeyBindItem(clearMenu, "Farm_R");
            var useHumQ = (getMenuBoolItem(clearMenu, "HumanQFarm")
                           && (100 * (_player.Mana / _player.MaxMana))
                           > getMenuSliderItem(clearMenu, "Lanemana"));
            var useHumW = (getMenuBoolItem(clearMenu, "HumanWFarm")
                           && (100 * (_player.Mana / _player.MaxMana))
                           > getMenuSliderItem(clearMenu, "Lanemana"));
            var useSpiQFarm = (_spiderQ.IsReady() && getMenuBoolItem(clearMenu, "SpiderQFarm"));
            var useSpiWFarm = (_spiderW.IsReady() && getMenuBoolItem(clearMenu, "SpiderWFarm"));
            var allminions = MinionManager.GetMinions(
                _player.Position,
                _humanQ.Range);
            {
                if (Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.LaneClear))
                {
                    foreach (var minion in allminions)
                        if (_human)
                        {
                            if (useHumQ && _humanQ.IsReady() && minion.IsValidTarget()
                                && _player.Distance(minion) <= _humanQ.Range)
                            {
                                _humanQ.Cast(minion);
                            }
                            if (useHumW && _humanW.IsReady() && minion.IsValidTarget()
                                && _player.Distance(minion) <= _humanW.Range)
                            {
                                _humanW.Cast(minion);
                            }
                            if (useR && _r.IsReady())
                            {
                                _r.Cast();
                            }
                        }
                    foreach (var minion in allminions)
                        if (_spider)
                        {
                            if (useSpiQFarm && _spiderQ.IsReady() && minion.IsValidTarget()
                                && _player.Distance(minion) <= _spiderQ.Range)
                            {
                                _spiderQ.Cast(minion);
                            }
                            if (useSpiWFarm && _spiderW.IsReady() && minion.IsValidTarget()
                                && _player.Distance(minion) <= 125)
                            {
                                _spiderW.Cast();
                            }
                        }
                }
                if (Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.LastHit))
                {
                    foreach (var minion in allminions)
                        if (_human)
                        {
                            if (useHumQ && _player.GetSpellDamage(minion, SpellSlot.Q) > minion.Health
                                && _humanQ.IsReady() && minion.IsValidTarget()
                                && _player.Distance(minion) <= _humanQ.Range)
                            {
                                _humanQ.Cast(minion);
                            }
                            if (useHumW && _player.GetSpellDamage(minion, SpellSlot.W) > minion.Health
                                && _humanW.IsReady() && minion.IsValidTarget()
                                && _player.Distance(minion) <= _humanW.Range)
                            {
                                _humanW.Cast(minion);
                            }
                            if (useR && _r.IsReady())
                            {
                                _r.Cast();
                            }
                        }
                    foreach (var minion in allminions)
                        if (_spider)
                        {
                            if (useSpiQFarm && _spiderQ.IsReady()
                                && _player.GetSpellDamage(minion, SpellSlot.Q) > minion.Health && _spiderQ.IsReady()
                                && minion.IsValidTarget() && _player.Distance(minion) <= _spiderQ.Range)
                            {
                                _spiderQ.Cast(minion);
                            }
                            if (useSpiQFarm && _spiderW.IsReady() && minion.IsValidTarget()
                                && _player.Distance(minion) <= 125)
                            {
                                _spiderW.Cast();
                            }
                        }
                }
            }
        }

        public static readonly string[] Smitetype =
            {
                "s5_summonersmiteplayerganker", "s5_summonersmiteduel",
                "s5_summonersmitequick", "itemsmiteaoe", "summonersmite"
            };

        private static int GetSmiteDmg()
        {
            int level = _player.Level;
            int index = _player.Level / 5;
            float[] dmgs = { 370 + 20 * level, 330 + 30 * level, 240 + 40 * level, 100 + 50 * level };
            return (int)dmgs[index];
        }


        //New map Monsters Name By SKO
        private static void Smiteuse()
        {
            var jungle = Orbwalker.ActiveMode.HasFlag(OrbwalkerMode.LaneClear);
            if (ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) != SpellState.Ready) return;
            var useblue = getMenuBoolItem(smiteMenu, "Useblue");
            var usered = getMenuBoolItem(smiteMenu, "Usered");
            var health = (100 * (_player.Health / _player.MaxHealth)) < getMenuSliderItem(smiteMenu, "healthJ");
            var mana = (100 * (_player.Mana / _player.MaxMana)) < getMenuSliderItem(smiteMenu, "manaJ");
            string[] jungleMinions;

            jungleMinions = new string[] { "TT_Spiderboss", "TT_NWraith", "TT_NGolem", "TT_NWolf" };
            jungleMinions = new string[]
                                {
                                        "SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_RiftHerald",
                                        "SRU_Red", "SRU_Krug", "SRU_Dragon_Air", "SRU_Dragon_Water", "SRU_Dragon_Fire",
                                        "SRU_Dragon_Elder", "SRU_Baron"
                                };

            var minions = MinionManager.GetMinions(_player.Position, 1000);
            if (minions.Count() > 0)
            {
                int smiteDmg = GetSmiteDmg();

                foreach (AIBaseClient minion in minions)
                {
                    if (minion.Health <= smiteDmg
                        && jungleMinions.Any(name => minion.Name.Substring(0, minion.Name.Length - 5).Equals(name)))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                    if (minion.Health <= smiteDmg && jungleMinions.Any(name => minion.Name.StartsWith(name))
                        && !jungleMinions.Any(name => minion.Name.Contains("Mini")))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                    else if (jungle && useblue && mana && minion.Health >= smiteDmg
                             && jungleMinions.Any(name => minion.Name.StartsWith("SRU_Blue"))
                             && !jungleMinions.Any(name => minion.Name.Contains("Mini")))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                    else if (jungle && usered && health && minion.Health >= smiteDmg
                             && jungleMinions.Any(name => minion.Name.StartsWith("SRU_Red"))
                             && !jungleMinions.Any(name => minion.Name.Contains("Mini")))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                }
            }
        }

        private static void AutoE()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPosRaw);
            var target = TargetSelector.GetTarget(_humanE.Range, DamageType.Magical);

            if (_human && target.IsValidTarget(_humanE.Range) && _humanE.IsReady()
                && _humanE.GetPrediction(target).Hitchance >= HitChance.VeryHigh)
            {
                _humanE.Cast(target);
            }
        }

        /*private static bool Packets()
        {
            return _config.Item("usePackets").GetValue<bool>();
        }*/

        private static void Interrupter2_OnInterruptableTarget(
            AIHeroClient unit, Interrupter.InterruptSpellArgs args)
        {
            if (!getMenuBoolItem(miscMenu, "UseEInt")) return;
            if (unit.IsValidTarget(_humanE.Range) && _humanE.GetPrediction(unit).Hitchance >= HitChance.Low)
            {
                _humanE.Cast(unit);
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, Gapcloser.GapcloserArgs gapcloser)
        {
            if (_spiderE.IsReady() && _spider && sender.IsValidTarget(_spiderE.Range)
                && getMenuBoolItem(miscMenu, "Spidergapcloser"))
            {
                _spiderE.Cast(sender);
            }
            if (_humanE.IsReady() && _human && sender.IsValidTarget(_humanE.Range)
                && getMenuBoolItem(miscMenu, "Humangapcloser"))
            {
                _humanE.Cast(sender);
            }
        }

        private static float CalculateCd(float time)
        {
            return time + (time * _player.PercentCooldownMod);
        }

        private static void Cooldowns()
        {
            _humaQcd = ((_humQcd - Game.Time) > 0) ? (_humQcd - Game.Time) : 0;
            _humaWcd = ((_humWcd - Game.Time) > 0) ? (_humWcd - Game.Time) : 0;
            _humaEcd = ((_humEcd - Game.Time) > 0) ? (_humEcd - Game.Time) : 0;
            _spideQcd = ((_spidQcd - Game.Time) > 0) ? (_spidQcd - Game.Time) : 0;
            _spideWcd = ((_spidWcd - Game.Time) > 0) ? (_spidWcd - Game.Time) : 0;
            _spideEcd = ((_spidEcd - Game.Time) > 0) ? (_spidEcd - Game.Time) : 0;
        }

        private static void GetCDs(AIBaseClientProcessSpellCastEventArgs spell)
        {
            if (_human)
            {
                if (spell.SData.Name == "EliseHumanQ") _humQcd = Game.Time + CalculateCd(HumanQcd[_humanQ.Level]);
                if (spell.SData.Name == "EliseHumanW") _humWcd = Game.Time + CalculateCd(HumanWcd[_humanW.Level]);
                if (spell.SData.Name == "EliseHumanE") _humEcd = Game.Time + CalculateCd(HumanEcd[_humanE.Level]);
            }
            else
            {
                if (spell.SData.Name == "EliseSpiderQCast") _spidQcd = Game.Time + CalculateCd(SpiderQcd[_spiderQ.Level]);
                if (spell.SData.Name == "EliseSpiderW") _spidWcd = Game.Time + CalculateCd(SpiderWcd[_spiderW.Level]);
                if (spell.SData.Name == "EliseSpiderEInitial") _spidEcd = Game.Time + CalculateCd(SpiderEcd[_spiderE.Level]);
            }
        }

        private static HitChance Echange()
        {
            switch (getBoxItem(miscMenu, "Echange"))
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        // Credits to Brain0305
        private static bool CheckingCollision(AIHeroClient target)
        {
            foreach (var col in MinionManager.GetMinions(_player.Position, 1500))
            {
                //var segment = Geometry.Circle(col.Position.ToVector2(),_player.Position.ToVector2(),col.Position.ToVector2());

                if (col.Distance(_player.Position) < _smite.Range
                    && col.Health < _player.GetSummonerSpellDamage(col, SummonerSpell.Smite))
                {
                    _player.Spellbook.CastSpell(_smiteSlot, col);
                    return true;
                }
            }
            return false;
        }

        // Credits to Brain0305
        static float GetHitBox(AIBaseClient minion)
        {
            var nameMinion = minion.Name.ToLower();
            if (nameMinion.Contains("mech")) return 65;
            if (nameMinion.Contains("wizard") || nameMinion.Contains("basic")) return 48;
            if (nameMinion.Contains("wolf") || nameMinion.Contains("wraith")) return 50;
            if (nameMinion.Contains("golem") || nameMinion.Contains("lizard")) return 80;
            if (nameMinion.Contains("dragon") || nameMinion.Contains("worm")) return 100;
            return 50;
        }

        private static void KillSteal()
        {
            foreach (var hero in ObjectManager.Get<AIHeroClient>().Where(hero => hero.IsEnemy))
            {
                var igniteDmg = _player.GetSummonerSpellDamage(hero, SummonerSpell.Ignite);
                var qhDmg = _player.GetSpellDamage(hero, SpellSlot.Q);
                var wDmg = _player.GetSpellDamage(hero, SpellSlot.W);

                if (hero.IsValidTarget(600) && getMenuBoolItem(ksMenu, "UseIgnite")
                    && _igniteSlot != SpellSlot.Unknown
                    && _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
                {
                    if (igniteDmg > hero.Health)
                    {
                        _player.Spellbook.CastSpell(_igniteSlot, hero);
                    }
                }
                if (_human)
                {
                    if (_humanQ.IsReady() && hero.IsValidTarget(_humanQ.Range)
                        && getMenuBoolItem(ksMenu, "HumanQKs"))
                    {
                        if (hero.Health <= qhDmg)
                        {
                            _humanQ.Cast(hero);
                        }
                    }
                    if (_humanW.IsReady() && hero.IsValidTarget(_humanW.Range)
                        && getMenuBoolItem(ksMenu, "HumanWKs"))
                    {
                        if (hero.Health <= wDmg)
                        {
                            _humanW.Cast(hero);
                        }
                    }
                }
                if (_spider && _spiderQ.IsReady() && hero.IsValidTarget(_spiderQ.Range)
                    && getMenuBoolItem(ksMenu, "SpiderQKs"))
                {
                    if (hero.Health <= qhDmg)
                    {
                        _spiderQ.Cast(hero);
                    }
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var elise = Drawing.WorldToScreen(_player.Position);

            if (getMenuBoolItem(drawMenu, "drawmode") && _smite != null)
            {
                if (getMenuBoolItem(smiteMenu, "smitecombo"))
                {
                    Drawing.DrawText(
                        Drawing.Width * 0.02f,
                        Drawing.Height * 0.90f,
                        System.Drawing.Color.GreenYellow,
                        "Smite Tagret");
                }
                else
                    Drawing.DrawText(
                        Drawing.Width * 0.02f,
                        Drawing.Height * 0.90f,
                        System.Drawing.Color.GreenYellow,
                        "Smite minion in Human E Path");
            }

            if (getMenuBoolItem(drawMenu, "Drawsmite") && _smite != null)
            {
                if (getMenuKeyBindItem(smiteMenu, "Usesmite"))
                {
                    Drawing.DrawText(
                        Drawing.Width * 0.02f,
                        Drawing.Height * 0.88f,
                        System.Drawing.Color.GreenYellow,
                        "Smite Jungle On");
                }
                else
                    Drawing.DrawText(
                        Drawing.Width * 0.02f,
                        Drawing.Height * 0.88f,
                        System.Drawing.Color.DarkRed,
                        "Smite Jungle On");
            }
            if (_human && getMenuBoolItem(drawMenu, "DrawQ"))
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _humanQ.Range, System.Drawing.Color.GreenYellow);
            }
            if (_human && getMenuBoolItem(drawMenu, "DrawW"))
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _humanW.Range, System.Drawing.Color.GreenYellow);
            }
            if (_human && getMenuBoolItem(drawMenu, "DrawE"))
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _humanE.Range, System.Drawing.Color.GreenYellow);
            }
            if (_spider && getMenuBoolItem(drawMenu, "SpiderDrawQ"))
            {
                Render.Circle.DrawCircle(
                    ObjectManager.Player.Position,
                    _spiderQ.Range,
                    System.Drawing.Color.GreenYellow);
            }
            if (_spider && getMenuBoolItem(drawMenu, "SpiderDrawE"))
            {
                Render.Circle.DrawCircle(
                    ObjectManager.Player.Position,
                    _spiderE.Range,
                    System.Drawing.Color.GreenYellow);
            }

            if (!getMenuBoolItem(drawMenu, "DrawCooldown")) return;
            if (!_spider)
            {
                if (_spideQcd == 0) Drawing.DrawText(elise[0] - 60, elise[1], Color.White, "SQ Rdy");
                else Drawing.DrawText(elise[0] - 60, elise[1], Color.Orange, "SQ: " + _spideQcd.ToString("0.0"));
                if (_spideWcd == 0) Drawing.DrawText(elise[0] - 20, elise[1] + 30, Color.White, "SW Rdy");
                else Drawing.DrawText(elise[0] - 20, elise[1] + 30, Color.Orange, "SW: " + _spideWcd.ToString("0.0"));
                if (_spideEcd == 0) Drawing.DrawText(elise[0], elise[1], Color.White, "SE Rdy");
                else Drawing.DrawText(elise[0], elise[1], Color.Orange, "SE: " + _spideEcd.ToString("0.0"));
            }
            else
            {
                if (_humaQcd == 0) Drawing.DrawText(elise[0] - 60, elise[1], Color.White, "HQ Rdy");
                else Drawing.DrawText(elise[0] - 60, elise[1], Color.Orange, "HQ: " + _humaQcd.ToString("0.0"));
                if (_humaWcd == 0) Drawing.DrawText(elise[0] - 20, elise[1] + 30, Color.White, "HW Rdy");
                else Drawing.DrawText(elise[0] - 20, elise[1] + 30, Color.Orange, "HW: " + _humaWcd.ToString("0.0"));
                if (_humaEcd == 0) Drawing.DrawText(elise[0], elise[1], Color.White, "HE Rdy");
                else Drawing.DrawText(elise[0], elise[1], Color.Orange, "HE: " + _humaEcd.ToString("0.0"));
            }
        }

        private static void CheckSpells()
        {
            if (_player.Spellbook.GetSpell(SpellSlot.Q).Name == "EliseHumanQ"
                || _player.Spellbook.GetSpell(SpellSlot.W).Name == "EliseHumanW"
                || _player.Spellbook.GetSpell(SpellSlot.E).Name == "EliseHumanE")
            {
                _human = true;
                _spider = false;
            }

            if (_player.Spellbook.GetSpell(SpellSlot.Q).Name == "EliseSpiderQCast"
                || _player.Spellbook.GetSpell(SpellSlot.W).Name == "EliseSpiderW"
                || _player.Spellbook.GetSpell(SpellSlot.E).Name == "EliseSpiderEInitial")
            {
                _human = false;
                _spider = true;
            }
        }
    }
}
