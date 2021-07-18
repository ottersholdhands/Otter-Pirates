using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace OtterPirates
{
    public class JobDriver_Hoop : JobDriver_WatchBuilding
    {
        private static int ballThrowInterval = 600; // probably not worth exposing to XML. Not using const just in case someone needs to change.
        private static float ballSpeed = 2f;

        // Potential optimization: cache pawn's tick offset on Notify_JobStarting. Probably not worth the hassle.

        protected override void WatchTickAction()
        {
            if (base.pawn.IsHashIntervalTick(ballThrowInterval))
            {
                Throwball(pawn, TargetA.Cell);
            }
            base.WatchTickAction();
        }

        public static void Throwball(Pawn thrower, IntVec3 target)
        {
            // if the pawn can't throw or the map is saturated with motes, do nothing
            if (!thrower.Position.ShouldSpawnMotesAt(thrower.Map) || thrower.Map.moteCounter.SaturatedLowPriority) return;
            Vector3 preciseTarget = target.ToVector3Shifted();
            // Give each player a unique ball. Assumes the relevant defs exist and that at most three players play, otherwise everyone else will default to green.
            ThingDef ball = OtterDefOf.Otter_Mote_GreenBall;
            if (target.x == thrower.Position.x - 1 || target.z == thrower.Position.z - 1) ball = OtterDefOf.Otter_Mote_RedBall;
            else if (target.x == thrower.Position.x + 1 || target.z == thrower.Position.z + 1) ball = OtterDefOf.Otter_Mote_BlueBall;
            // Transforms from the ball. Copied directly from the original.
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ball, null);
            moteThrown.Scale = 1f;
            moteThrown.rotationRate = 0f;
            moteThrown.exactPosition = thrower.DrawPos;
            moteThrown.exactRotation = (preciseTarget - moteThrown.exactPosition).AngleFlat();
            moteThrown.SetVelocity((preciseTarget - moteThrown.exactPosition).AngleFlat(), ballSpeed);
            moteThrown.MoveAngle = (preciseTarget - moteThrown.exactPosition).AngleFlat();
            moteThrown.airTimeLeft = (moteThrown.exactPosition - preciseTarget).MagnitudeHorizontal() / ballSpeed;

            // Throw the ball
            GenSpawn.Spawn(moteThrown, thrower.Position, thrower.Map);
        }
    }
    [DefOf]
    public static class OtterDefOf
    {
        public static ThingDef Otter_Mote_RedBall, Otter_Mote_GreenBall, Otter_Mote_BlueBall;

        static OtterDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(OtterDefOf));
        }
    }
}
