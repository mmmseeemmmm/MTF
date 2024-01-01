using AlDaqis.DaqisService;

namespace AlDaqis
{
    static class ErrorMessage
    {
        internal const string CouldNotConnect = "Could not connect to the Daqis.";
        internal const string WrongResult = "Wrong result has been sent from Daqis";

        internal static string GetDaqisError(dataError type, string msg)
        {
            return $"Daqis error: {type} - {msg}.";
        }
    }
}
