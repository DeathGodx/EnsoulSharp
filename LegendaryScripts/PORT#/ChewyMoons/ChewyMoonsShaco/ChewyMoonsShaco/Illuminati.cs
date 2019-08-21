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

namespace ChewyMoonsShaco
{
    internal class Illuminati
    {
        private const float BoxSafeDistance = 100;
        public static float TriangleLegDistance = ChewyMoonShaco.illuminatiMenu["BoxDistance"].GetValue<MenuSlider>().Value;
        private static readonly List<AIMinionClient> Boxes = new List<AIMinionClient>();
        private static Vector3 _extendPoint;

        public static void Init()
        {
            // Add all existing boxes
            Boxes.AddRange(ObjectManager.Get<AIMinionClient>().Where(x => x.Name == "shacobox"));

            GameObject.OnCreate += (sender, args) =>
            {
                if (!(sender is AIMinionClient))
                {
                    return;
                }

                var @object = (AIMinionClient)sender;

                if (@object.Name == "shacobox")
                {
                    Boxes.Add(@object);
                }
            };

            GameObject.OnDelete += (sender, args) =>
            {
                if (!(sender is AIMinionClient))
                {
                    return;
                }

                var @object = (AIMinionClient)sender;

                if (@object.Name == "shacobox")
                {
                    Boxes.RemoveAll(x => x.NetworkId == @object.NetworkId);
                }
            };

            Drawing.OnDraw += DrawingOnOnDraw;

            _extendPoint = ObjectManager.Player.Position;

            Game.OnUpdate += GameOnOnUpdate;
        }

        private static void GameOnOnUpdate(EventArgs args)
        {
            if (!ChewyMoonShaco.illuminatiMenu["RepairTriangle"].GetValue<MenuBool>().Enabled)
            {
                return;
            }

            foreach (var shacoBox in Boxes)
            {
                var angle = 120;

                var point = RotateAroundPoint(
                    angle, shacoBox.Position.ToVector2(), shacoBox.Position.Extend(_extendPoint, TriangleLegDistance));

                if (!Boxes.Any(x => x.Distance(point) < BoxSafeDistance) && ChewyMoonShaco.W.IsInRange(point))
                {
                    ChewyMoonShaco.W.Cast(point);
                }
                else
                {
                    angle = -120;

                    point = RotateAroundPoint(
                        angle, shacoBox.Position.ToVector2(), shacoBox.Position.Extend(_extendPoint, TriangleLegDistance));

                    if (!Boxes.Any(x => x.Distance(point) < BoxSafeDistance) && ChewyMoonShaco.W.IsInRange(point))
                    {
                        ChewyMoonShaco.W.Cast(point);
                    }
                    else
                    {
                        angle = 240;

                        point = RotateAroundPoint(
                            angle, shacoBox.Position.ToVector2(), shacoBox.Position.Extend(_extendPoint, TriangleLegDistance));

                        if (!Boxes.Any(x => x.Distance(point) < BoxSafeDistance) && ChewyMoonShaco.W.IsInRange(point))
                        {
                            ChewyMoonShaco.W.Cast(point);
                        }
                    }
                }
            }
        }

        private static Vector2 RotateAroundPoint(double angleDegree, Vector2 center, Vector3 point)
        {
            var angle = angleDegree * Math.PI / 180;

            var rotatedX = Math.Cos(angle) * (point.X - center.X) - Math.Sin(angle) * (point.Y - center.Y) + center.X;
            var rotatedY = Math.Sin(angle) * (point.X - center.X) + Math.Cos(angle) * (point.Y - center.Y) + center.Y;

            return new Vector2((float)rotatedX, (float)rotatedY);
        }

        public static void PlaceInitialBox()
        {
            ChewyMoonShaco.W.Cast(ObjectManager.Player.Position.Extend(Game.CursorPosRaw, ChewyMoonShaco.W.Range));
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            foreach (var shacoBox in Boxes)
            {
                var angle = 0;

                var point = RotateAroundPoint(
                    angle, shacoBox.Position.ToVector2(), shacoBox.Position.Extend(_extendPoint, TriangleLegDistance));

                if (!Boxes.Any(x => x.Distance(point) < BoxSafeDistance))
                {
                    Drawing.DrawCircle(point.ToVector3(), BoxSafeDistance, Color.Aqua);
                }

                angle = 120;

                point = RotateAroundPoint(
                    angle, shacoBox.Position.ToVector2(), shacoBox.Position.Extend(_extendPoint, TriangleLegDistance));

                if (!Boxes.Any(x => x.Distance(point) < shacoBox.BoundingRadius))
                {
                    Drawing.DrawCircle(point.ToVector3(), BoxSafeDistance, Color.Aqua);
                }

                angle = 240;

                point = RotateAroundPoint(
                    angle, shacoBox.Position.ToVector2(), shacoBox.Position.Extend(_extendPoint, TriangleLegDistance));

                if (!Boxes.Any(x => x.Distance(point) < shacoBox.BoundingRadius))
                {
                    Drawing.DrawCircle(point.ToVector3(), BoxSafeDistance, Color.Aqua);
                }

                angle = 360;

                point = RotateAroundPoint(angle, shacoBox.Position.ToVector2(), shacoBox.Position.Extend(_extendPoint, TriangleLegDistance));

                if (!Boxes.Any(x => x.Distance(point) < BoxSafeDistance))
                {
                    Drawing.DrawCircle(point.ToVector3(), BoxSafeDistance, Color.Aqua);
                }
            }
        }
    }
}
