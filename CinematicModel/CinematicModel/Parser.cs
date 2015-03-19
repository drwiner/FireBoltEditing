using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    public class Parser
    {
        
        XmlSerializer xs;
        public Parser()
        {
            xs = new XmlSerializer(typeof(CinematicModel));
        }

        public CinematicModel Parse(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open);
            CinematicModel model = (CinematicModel)xs.Deserialize(fs);
            return model;
        }
    }
}
