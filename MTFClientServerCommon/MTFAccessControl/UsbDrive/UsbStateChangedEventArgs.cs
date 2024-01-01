using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsbDrive
{
    public delegate void UsbStateChangedEventHandler(UsbStateChangedEventArgs e);



    /// <summary>
    /// Define the arguments passed internally from the DriverWindow to the KeyManager
    /// handlers.
    /// </summary>

    public class UsbStateChangedEventArgs : EventArgs
    {

        /// <summary>
        /// Initialize a new instance with the specified state and disk.
        /// </summary>
        /// <param name="state">The state change code.</param>
        /// <param name="disk">The USB disk description.</param>

        public UsbStateChangedEventArgs(UsbStateChange state, UsbKeysCollection disks)
        {
            this.State = state;
            this.Disks = disks;
        }


        /// <summary>
        /// Gets the USB disk information.
        /// </summary>

        public UsbKeysCollection Disks
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the state change code.
        /// </summary>

        public UsbStateChange State
        {
            get;
            private set;
        }
    }

}
