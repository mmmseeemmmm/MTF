using System;
using System.ServiceModel;
using AlDaqis.DaqisService;
using AutomotiveLighting.MTFCommon;

namespace AlDaqis
{
    [MTFClass(Icon = MTFIcons.DataCollection, Name = "Daqis Web Services")]
    [MTFClassCategory("Report")]
    public class Daqis : IDisposable
    {
        private readonly DAWebServiceClient client;

        [MTFConstructor]
        [MTFAdditionalParameterInfo(ParameterName = "ipAddress", DisplayName = "IP address")]
        [MTFAdditionalParameterInfo(ParameterName = "port", DisplayName = "Port")]
        [MTFAdditionalParameterInfo(ParameterName = "path", DisplayName = "DaqisPath", DefaultValue = "daqis/DAWebService")]
        public Daqis(string ipAddress, int port, string path)
        {
            var endPoint = $"http://{ipAddress}:{port}/{path}";
            client = new DAWebServiceClient(new BasicHttpBinding(), new EndpointAddress(endPoint));
        }

        [MTFConstructor]
        [MTFAdditionalParameterInfo(ParameterName = "ipAddress", DisplayName = "IP address")]
        [MTFAdditionalParameterInfo(ParameterName = "path", DisplayName = "DaqisPath", DefaultValue = "daqis/DAWebService")]
        public Daqis(string ipAddress, string path)
        {
            var endPoint = $"http://{ipAddress}/{path}";
            client = new DAWebServiceClient(new BasicHttpBinding(), new EndpointAddress(endPoint));
        }

        [MTFMethod(DisplayName = "Check presentation on the Working place",
            Description =
                "It returns True when the piece with the specified serial number has been identified in the specified station. Otherwise False.")]
        [MTFAdditionalParameterInfo(ParameterName = "wpCode", DisplayName = "Working place code")]
        [MTFAdditionalParameterInfo(ParameterName = "sn", DisplayName = "Serial number")]
        public bool CheckPresentationOnTheWorkingPlace(string wpCode, string sn)
        {
            if (client != null)
            {
                //wpCode = "WS-ALCZ-DQ3410";
                //sn = "0301115201002507190064";
                var data = new productResultData {lineCode = wpCode, serialNumber = sn};
                var result = client.getLastProductionResultForDA(data);
                if (result != null)
                {
                    if (result.errorData == null)
                    {
                        return true;
                    }

                    if (result.errorData.dataError == dataError.MISSING_RESULT)
                    {
                        return false;
                    }

                    throw new Exception(ErrorMessage.GetDaqisError(result.errorData.dataError, result.errorData.errorMessage));
                }

                throw new Exception(ErrorMessage.WrongResult);
            }

            throw new Exception(ErrorMessage.CouldNotConnect);
        }

        [MTFMethod]
        public ValidationResultData TestGetValidation(string workPlaceCode, string partVersion, string partNumber, string serialNumber)
        {
            var now = DateTime.Now;
            if (client != null)
            {
                var data = new operationData
                           {
                               initial = true,

                               message = new message
                                         {
                                             clientCode = workPlaceCode,
                                             //timestamp = now,
                                             version = "1.0",
                                             timestampSpecified = false
                                         },
                    product = new product
                    {
                        partNumber = partNumber,
                        partVersion = partVersion,
                        //productionTimestamp = now,
                        serialNumber = serialNumber,
                    }
                };

                var result = client.getValidation(data);

                if (result != null)
                {
                    return new ValidationResultData
                           {
                               ValidationResult = result.validationResult,
                               Message = result.additionalMessage,
                               ValidationResultSpecified = result.validationResultSpecified,
                               ValidationsFinished = result.validationsFinished,
                               ValidationFinishedSpecified = result.validationResultSpecified,
                               HasProduct = result.product !=null,
                           };
                }

                throw new Exception(ErrorMessage.WrongResult);
            }

            throw new Exception(ErrorMessage.CouldNotConnect);
        }

        public void Dispose()
        {
            client?.Close();
        }
    }

    [MTFKnownClass]
    public class ValidationResultData
    {
        public int ValidationResult { get; set; }
        public string Message { get; set; }
        public bool ValidationResultSpecified { get; set; }
        public bool ValidationsFinished { get; set; }
        public bool ValidationFinishedSpecified { get; set; }
        public bool HasProduct { get; set; }
    }
}