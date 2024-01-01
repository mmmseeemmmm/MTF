using AL_Dat_Client;
using AutomotiveLighting.MTFCommon;

namespace BMWCoding
{
    [MTFKnownClass]
    public class MTFProject
    {
        public static MTFProject GetProject(Project project)
        {
            return project == null
                ? null
                : new MTFProject
                  {
                      Name = project.name,
                      CdZipName = project.cd_zipName,
                      Customer = project.customer,
                      DataPath = project.data_path,
                      BusName = project.bus_name,
                      VehicleFamily = project.vehicle_family,
                  };
        }

        public string Name { get; set; }
        public string CdZipName { get; set; }
        public string Customer { get; set; }
        public string DataPath { get; set; }
        public string BusName { get; set; }
        public string VehicleFamily { get; set; }
    }
}