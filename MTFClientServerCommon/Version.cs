using System;
using System.Runtime.Serialization;
using System.Text;

namespace MTFClientServerCommon
{
    [Serializable]
    public class Version : MTFDataTransferObject, IComparable<Version>
    {
        private const char Separator = '.';

        public Version(int major, int minor, int build, int revision)
        {
            if (major < 0)
            {
                throw new ArgumentOutOfRangeException("major");
            }
            if (minor < 0)
            {
                throw new ArgumentOutOfRangeException("minor");
            }
            if (build < 0)
            {
                throw new ArgumentOutOfRangeException("build");
            }
            if (revision < 0)
            {
                throw new ArgumentOutOfRangeException("revision");
            }
            VersionMajor = major;
            VersionMinor = minor;
            VersionBuild = build;
            VersionRevision = revision;
        }

        public Version(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public Version(System.Version version)
            : this(version.Major, version.Minor, version.Build, version.Revision)
        {
        }

        public int VersionMajor
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public int VersionMinor
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public int VersionBuild
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public int VersionRevision
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }


        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(VersionMajor).Append(Separator);
            sb.Append(VersionMinor).Append(Separator);
            sb.Append(VersionBuild).Append(Separator);
            sb.Append(VersionRevision);
            return sb.ToString();
        }

        public void IncreaseRevision()
        {
            VersionRevision++;
        }



        public int CompareTo(Version value)
        {
            if (value == null)
            {
                return 1;
            }
            if (VersionMajor != value.VersionMajor)
            {
                return VersionMajor > value.VersionMajor ? 1 : -1;
            }
            if (VersionMinor != value.VersionMinor)
            {
                return VersionMinor > value.VersionMinor ? 1 : -1;
            }
            if (VersionBuild != value.VersionBuild)
            {
                return VersionBuild > value.VersionBuild ? 1 : -1;
            }
            if (VersionRevision == value.VersionRevision)
            {
                return 0;
            }
            return VersionRevision > value.VersionRevision ? 1 : -1;
        }

        public static bool operator ==(Version v1, Version v2)
        {
            if (ReferenceEquals(v1, null))
            {
                return ReferenceEquals(v2, null);
            }
            else
            {
                return v1.CompareTo(v2) == 0;
            }
        }

        public static bool operator !=(Version v1, Version v2)
        {
            return !(v1 == v2);
        }

        public static bool operator <(Version v1, Version v2)
        {
            if (v1 == null)
            {
                throw new ArgumentNullException("v1");
            }
            return v1.CompareTo(v2) < 0;
        }

        public static bool operator >(Version v1, Version v2)
        {
            return v2 < v1;
        }

        public static bool operator <=(Version v1, Version v2)
        {
            if (v1 == null)
            {
                throw new ArgumentNullException("v1");
            }
            return v1.CompareTo(v2) <= 0;
        }

        public static bool operator >=(Version v1, Version v2)
        {
            return v2 <= v1;
        }
    }
}
