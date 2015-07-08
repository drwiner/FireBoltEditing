using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LN.Utilities;

namespace Assets.scripts
{
    public static class Extensions
    {
        /// <summary>
        /// converts comma delimited string into a vector 3
        /// </summary>
        /// <param name="s">this better have x,y,z in it</param>
        /// <returns>shiny vector3</returns>
        public static Vector3 ParseVector3(this string s)
        {
            string [] values = s.Split(new char[]{','});
            return new Vector3(float.Parse(values[0]),float.Parse(values[1]),float.Parse(values[2]));
        }


        /// <summary>
        /// converts comma delimited numeric pair into x,z coordinates
        /// </summary>
        /// <param name="s">string of format x,z </param>
        /// <returns>vector 3</returns>     
        public static bool TryParsePlanarCoords(this string s, out Vector3 vector3)
        {
            vector3 = Vector3.zero;
            string[] values = s.Split(new char[] { ',' });
            float x, z;
            if (values.Length > 1 &&
                float.TryParse(values[0], out x )&&
                float.TryParse(values[1], out z)) //we got two coords
            {
                vector3 = new Vector3(x, 0f, z);
                return true;
            }
            return false;
        }

        /// <summary>
        /// finds the angle of rotation to change orientation from "from" to "to"
        /// ignores Y values
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns>degree measure of needed rotation -180 to 180</returns>
        public static float GetXZAngleTo(this Vector3 from, Vector3 to)
        {

            Vector2 from2d = new Vector2(from.x, from.z);
            Vector2 to2d = new Vector2(to.x, to.z);
            Vector2 direction = to2d - from2d;
            float radians = Mathf.Atan2(direction.x,direction.y);
            if (radians > Mathf.PI) radians -= 2 * Mathf.PI;
            else if (radians < -Mathf.PI) radians += 2 * Mathf.PI;
            return radians * 180/Mathf.PI;
        }

        public static Vector3 ToVector3(this Impulse.v_1_336.Constants.Coordinate2D from)
        {
            return new Vector3((float)from.X, 0, (float)from.Y);
        }

        public static Vector3 ToVector3(this Impulse.v_1_336.Constants.Coordinate2D from, float domainToEngine)
        {
            return new Vector3((float)from.X, 0, (float)from.Y)*1/domainToEngine;
        }

        public static Vector3Nullable ToVector3Nullable(this Impulse.v_1_336.Constants.Coordinate2D from, float domainToEngine)
        {
            return new Vector3Nullable((float)from.X * 1 / domainToEngine, null, (float)from.Y * 1 / domainToEngine);
        }

        public static Vector3 ToVector3(this Impulse.v_1_336.Constants.Coordinate3D from, float domainToEngine)
        {
            return new Vector3((float)from.X, (float)from.Y, (float)from.Z) * 1 / domainToEngine;
        }

        public static float ToMillis(this uint tick, uint millisPerTick)
        {
            return tick * millisPerTick;
        }


        public static float convertSourceEngineToUnityRotation(this float sourceDegrees)
        {
            float unityDegrees = -sourceDegrees + 90 % 360;
            while (unityDegrees > 180)
            {
                unityDegrees -= 360;
            }
            while (unityDegrees < -180)
            {
                unityDegrees += 360;
            }
            return unityDegrees;
        }
    }
}
