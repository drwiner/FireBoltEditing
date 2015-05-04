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
            CinematicModel model = null;
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                model = (CinematicModel)xs.Deserialize(fs);
            }
            return model; 
        }

        public static void Write(string filename, CinematicModel model)
        {
            if (parser == null)
            {
                parser = new Parser();
            }
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                xs.Serialize(fs, model);
            }
        }
    }
}
