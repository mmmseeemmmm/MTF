using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsbDrive
{
    public class UsbKeysCollection : ObservableCollection<UsbKey>
    {
        public bool Contains(string name)
        {
            return this.AsQueryable<UsbKey>().Any(d => d.DriveLetter == name) == true;
        }


        /// <summary>
        /// Remove the named disk from the collection.
        /// </summary>
        /// <param name="name">The Windows name, or drive letter, of the disk to remove.</param>
        /// <returns>
        /// <b>True</b> if the item is removed; otherwise <b>false</b>.
        /// </returns>

        public bool Remove(string name)
        {
            UsbKey disk =
                (this.AsQueryable<UsbKey>()
                .Where(d => d.DriveLetter == name)
                .Select(d => d)).FirstOrDefault<UsbKey>();

            if (disk != null)
            {
                return this.Remove(disk);
            }

            return false;
        }
    }
}
