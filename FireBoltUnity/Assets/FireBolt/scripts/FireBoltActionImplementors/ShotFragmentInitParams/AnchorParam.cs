using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oshmirto;

namespace Assets.scripts 
{
    /// <summary>
    /// may be over-building the system...we'll hold off on abstractifying all the parameters for the moment and see how the data flows
    /// not currently used
    /// </summary>
    public class AnchorParam : IShotFragmentInitParam
    {
        private bool isSpecified = false;
        private bool requiresGrounding = false;

        public AnchorParam(string anchor)
        {
            if (anchor != null)
            {
                parseAnchor(anchor);
            }
        }

        void parseAnchor(string anchor)
        {

        }
        

        public bool IsSpecified()
        {
            return isSpecified;
        }

        public bool RequiresGrounding { get { return requiresGrounding; } }
    }
}
