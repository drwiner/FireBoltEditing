using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public static float ToMillis(this uint tick, uint millisPerTick)
        {
            return tick * millisPerTick;
        }
    }
}
