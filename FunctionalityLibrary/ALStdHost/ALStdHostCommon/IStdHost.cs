using System.ServiceModel;

namespace ALStdHostCommon
{
    [ServiceContract]
    public interface IStdHost
    {
        [OperationContract]
        void CreateInstance(string alStdAssembly, string workingDirectory);

        [OperationContract]
        void GetVideoModuleDriver();

        [OperationContract]
        byte VideoModuleDriverGetNamedValue(string parentName, string parameterName);

        [OperationContract]
        void VideoModuleDriverSetNamedValue(string parentName, string parameterName, byte value);

        [OperationContract]
        void VideoModuleDriverSetPicture(ALStdHostCommon.VmdPictures pictureName, ALStdHostCommon.HsvlMode hsvlMode);

        [OperationContract]
        void DestroyInstance();

        [OperationContract]
        string Ping();
    }
}
