using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Oshmirto
{
    public class Parser
    {

        static XmlSerializer xs;
        static Parser parser;
        private Parser()
        {
            xs = new XmlSerializer(typeof(CameraPlan));
        }

        public static CameraPlan Parse(string filename)
        {
            if (parser == null)
            {
                parser = new Parser();
            }
            CameraPlan plan = null;
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                plan = (CameraPlan)xs.Deserialize(fs);
            }
            return plan;
        }

        public static void Write(string filename, CameraPlan plan)
        {
            if (parser == null)
            {
                parser = new Parser();
            }
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                xs.Serialize(fs, plan);
            }
        }
    }
}
