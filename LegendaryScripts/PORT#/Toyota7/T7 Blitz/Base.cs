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

namespace T7_Blitzcrank
{
    class Base
    {
        #region Declerations

        public static AIHeroClient myhero { get { return ObjectManager.Player; } }
        public static AIHeroClient EnemyADC { get; set; }

        public static Menu menu, combo, harass, laneclear, jungleclear, misc, draw, qsett;

        public static readonly string CharacterName = "Blitzcrank", Version = "1.1", Date = "29/7/17", QTargetBuffName = "rocketgrab2", ESelfBuffName = "PowerFist";
        public static readonly string[] ADCNames = new string[] { "Ashe","Caitlyn","Corki","Draven","Ezreal","Graves","Jhin","Jinx","Kalista","Kog'Maw","Lucian",
                                                                  "Miss Fortune","Quinn","Sivir","Tristana","Twitch","Urgot","Varus","Vayne" };
        public static string EnemyPlayerNames;

        public static Item Potion { get; set; }
        public static Item Biscuit { get; set; }
        public static Item RPotion { get; set; }

        public static Spell Q { get; set; }
        public static Spell W { get; set; }
        public static Spell E { get; set; }
        public static Spell R { get; set; }

        #endregion

        #region Methods

        public static void CheckPrediction()
        {
            string CorrectPrediction = "SDK Prediction";
        }

        public static void KnockupTarget()
        {
            var target = GameObjects.EnemyHeroes.Where(x => x.Distance(myhero.Position) < 300).FirstOrDefault();

            if (target == null) return;


        }

        public static AIHeroClient GetEnemyADC()
        {
            foreach (var name in GameObjects.EnemyHeroes.Select(x => x.CharacterName))
            {
                if (ADCNames.Contains(name)) return GameObjects.EnemyHeroes.FirstOrDefault(x => x.CharacterName == name);
            }

            return null;
        }

        public static AIHeroClient GetTarget()
        {
            var selection = comb(misc, "FOCUS");


            switch (selection)
            {
                case 0:
                    if (EnemyADC != null && EnemyADC.IsValidTarget((int)Q.Range + 250))
                    {
                        return EnemyADC;
                    }
                    else return TargetSelector.GetTarget(Q.Range + 100, DamageType.Magical);
                case 1:
                    return TargetSelector.GetTarget(Q.Range + 100, DamageType.Magical);
                case 2:
                    var target = GameObjects.EnemyHeroes.FirstOrDefault(x => x.CharacterName == EnemyPlayerNames);

                    if (target != null && target.IsValidTarget((int)Q.Range + 250))
                    {
                        return target;
                    }
                    else return TargetSelector.GetTarget(Q.Range + 100, DamageType.Magical);
            }

            return null;
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
}