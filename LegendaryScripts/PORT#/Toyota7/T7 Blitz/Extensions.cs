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

using Geometry = EnsoulSharp.SDK.Geometry;
using SPrediction;

namespace T7_Blitzcrank
{
    static class Extensions
    {
        public static bool ValidTarget(this AIHeroClient hero, int range)
        {
            return !hero.HasBuff("UndyingRage") && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("ChronoShift") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("bansheesveil") && !hero.HasBuff("fioraw") &&
                   !hero.IsInvulnerable && !hero.IsDead && hero.IsValidTarget(range) && !hero.IsZombie &&
                   !hero.HasBuffOfType(BuffType.Invulnerability) && !hero.HasBuffOfType(BuffType.SpellImmunity) && !hero.HasBuffOfType(BuffType.SpellShield);
        }

        public static int CountEnemies(this AIHeroClient hero, int range)
        {
            return GameObjects.EnemyHeroes.Where(x => x.Distance(hero.Position) < range).Count();
        }

        public static int CountAllies(this AIBaseClient hero, int range)
        {
            return GameObjects.AllyHeroes.Where(x => x.Distance(hero.Position) < range).Count();
        }

        public static bool HasPowerFist(this AIHeroClient hero)
        {
            return hero.HasBuff(Base.ESelfBuffName);
        }

        public static Vector3 GetPositionAfter(this AIBaseClient target, int milliseconds = 250)
        {
            return Prediction.GetFastUnitPosition(target, milliseconds).ToVector3();
        }
    }
}