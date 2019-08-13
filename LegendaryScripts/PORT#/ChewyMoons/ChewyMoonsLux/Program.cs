#region

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
#endregion

namespace ChewyMoonsLux
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            GameEvent.OnGameLoad += ChewyMoonsLux.OnGameLoad;
        }
    }
}