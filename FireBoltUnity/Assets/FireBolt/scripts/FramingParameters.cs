using Oshmirto;
using System.Collections.Generic;
namespace Assets.scripts
{
    public class FramingParameters
    {
        public static Dictionary<FramingType, FramingParameters> FramingTable = new Dictionary<FramingType,FramingParameters>()
        {
            {FramingType.Full,new FramingParameters(){MaxPercent=1.0f,MinPercent=0.9f,TargetPercent=0.95f}},
            {FramingType.ExtremeLong, new FramingParameters(){MaxPercent=0.25f, MinPercent=0.01f, TargetPercent=0.2f}},
            {FramingType.CloseUp, new FramingParameters(){MaxPercent=10.25f, MinPercent=8.01f,TargetPercent=9.0f}}
            
        };

        public float MaxPercent { get; set; }
        public float MinPercent { get; set; }
        public float TargetPercent { get; set; }
    }
}