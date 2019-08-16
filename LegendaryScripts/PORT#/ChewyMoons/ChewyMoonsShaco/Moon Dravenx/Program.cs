namespace MoonDraven
{
    using System;

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
    using static EnsoulSharp.SDK.Items;
    using SharpDX.Direct3D9;
    using static EnsoulSharp.SDK.Interrupter;

    internal class Program
    {
        #region Methods

        private static void GameOnOnGameLoad()
        {
            if (ObjectManager.Player.Name == "Draven")
            {
                new MoonDraven().Load();
            }
        }

        private static void Main(string[] args)
        {
            GameEvent.OnGameLoad += GameOnOnGameLoad;
        }

        #endregion
    }
}