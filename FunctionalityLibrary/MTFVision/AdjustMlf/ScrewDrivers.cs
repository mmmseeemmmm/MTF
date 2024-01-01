using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace MTFVision.AdjustMlfConfig
{
    public class ScrewsParams
    {
        public DirectsToAim directsToAim { get; set; }
        public ScrewsSettings screwsSettings { get; set; }
        public bool showInfo { get; set; }
    }

    public class DirectsToAim
    {
        public bool hor { get; set; }
        public bool vert { get; set; }
    }

    public class ScrewsSettings
    {
        public bool horChangeDirect { get; set; }
        public int horScrewNum { get; set; }
        public int horScrewAimAngle { get; set; }
        public float horMinSpeed { get; set; }
        public float horMaxSpeed { get; set; }
        public bool vertChangeDirect { get; set; }
        public int vertScrewNum { get; set; }
        public int vertScrewAimAngle { get; set; }
        public float vertMinSpeed { get; set; }
        public float vertMaxSpeed { get; set; }
        public float decelerateRatio { get; set; }
    }

}
