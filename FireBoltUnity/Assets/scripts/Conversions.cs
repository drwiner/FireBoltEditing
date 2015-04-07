using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.scripts
{
    public class Conversions
    {
        /// <summary>
        /// converts comma delimited string into a vector 3
        /// </summary>
        /// <param name="s">this better have x,y,z in it</param>
        /// <returns>shiny vector3</returns>
        public static Vector3 ParseVector3(string s)
        {
            string [] values = s.Split(new char[]{','});
            return new Vector3(float.Parse(values[0]),float.Parse(values[1]),float.Parse(values[2]));
        }
    }
}
