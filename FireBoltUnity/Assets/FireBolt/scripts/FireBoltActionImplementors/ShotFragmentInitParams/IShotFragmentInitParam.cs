using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.scripts
{
    interface IShotFragmentInitParam
    {
        /// <summary>
        /// is this parameter specified for the fragment
        /// </summary>
        public bool IsSpecified();

    }
}
