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
        
        static XmlSerializer xs;
        static Parser parser;
        private Parser()
        {
            xs = new XmlSerializer(typeof(CinematicModel));
        }

        public static CinematicModel Parse(string filename)
        {
            if (parser == null)
            {
                parser = new Parser();
            }
            FileStream fs = new FileStream(filename, FileMode.Open);
            CinematicModel model = (CinematicModel)xs.Deserialize(fs);
            return model; //going to need a more easily traversable data structure...something keyed on actor and action names
        }
    }
}
