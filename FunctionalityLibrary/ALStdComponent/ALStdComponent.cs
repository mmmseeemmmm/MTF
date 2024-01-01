using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ALStdHostCommon;
using AutomotiveLighting.MTFCommon;

namespace ALStdComponent
{
    [MTFClass(Name = "ALSTD", Description = "Driver for ALSTD", ThreadSafeLevel = ThreadSafeLevel.Instance, Icon = MTFIcons.AL)]
    [MTFClassCategory("Control & Measurement")]
    class ALStdComponent : IDisposable
    {
        private ALStdControl alStdControl;

        [MTFConstructor]
        public ALStdComponent()
        {
        }

        [MTFMethod(DisplayName = "Start ALSTD")]
        public void Start()
        {
            if (this.alStdControl == null)
            {
                this.alStdControl = new ALStdControl();
            }
        }

        [MTFMethod(DisplayName = "Stop ALSTD")]
        public void Stop()
        {
            if (this.alStdControl != null)
            {
                this.alStdControl.Stop();
                this.alStdControl.Dispose();
                this.alStdControl = null;
            }
        }

        [MTFMethod(DisplayName = "Set Picture HSVL")]
        [MTFAllowedParameterValue("picture", "AL", "0")]
        [MTFAllowedParameterValue("picture", "None", "1")]
        [MTFAllowedParameterValue("picture", "White", "2")]
        [MTFAllowedParameterValue("picture", "Black", "3")]
        [MTFAllowedParameterValue("picture", "BlackWhite", "4")]
        [MTFAllowedParameterValue("picture", "WhiteBlack", "5")]
        [MTFAllowedParameterValue("picture", "LowBeam", "6")]
        [MTFAllowedParameterValue("picture", "Chessboard01", "7")]
        [MTFAllowedParameterValue("picture", "Chessboard02", "8")]
        [MTFAllowedParameterValue("picture", "Chessboard03", "9")]
        [MTFAllowedParameterValue("picture", "Error", "10")]
        [MTFAllowedParameterValue("picture", "Edge", "11")]
        [MTFAllowedParameterValue("picture", "NotFound", "12")]
        [MTFAllowedParameterValue("picture", "ShadeOfGreys", "13")]
        [MTFAllowedParameterValue("picture", "DlpAdjustmentRhs", "14")]
        [MTFAllowedParameterValue("picture", "DlpAdjustmentLhs", "15")]
        [MTFAllowedParameterValue("hsvlMode", "Par1Bit", "0")]
        [MTFAllowedParameterValue("hsvlMode", "Crc6Bit", "1")]
        public void SetPicture(int picture, int hsvlMode)
        {
            if (this.alStdControl != null)
            {
                this.alStdControl.SetPicture((VmdPictures)picture, (HsvlMode)hsvlMode);
            }
            else
            {
                throw new Exception("ALSTD not started!");
            }
        }

        public void Dispose()
        {
            if (this.alStdControl != null)
            {
                this.alStdControl.Dispose();
            }
        }
    }
}
