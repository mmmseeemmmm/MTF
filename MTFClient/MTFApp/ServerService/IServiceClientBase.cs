namespace MTFApp.ServerService
{
    interface IServiceClientBase
    {
        bool IsConnected { get; }
        void Connect();
        void Disconnect();
    }
}
