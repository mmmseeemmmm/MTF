using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace ALNATraceability
{
    //Attribute01 - 25   0-not tested, 1-OK, 2-NOK - something like part present, just test value OK/NOK
    //NumericResult01 - test result 0-not tested,, 1-OK, 2-NOK
    //NumericMin01 - min value
    //NUmericMax01 - max value
    //NumericValue01 - measured value
    //
    [MTFClass(Name = "ALNA Traceability", Icon = MTFIcons.DataCollection, Description = "Driver for ALNA traceability")]
    [MTFClassCategory("Report")]
    public class ALNATraceability : IDisposable
    {
        private const string tableFromPrefix = "TM_From_";
        private const string tableToPrefix = "TM_To_";
        private const int resultCount = 25;
        private readonly string machineName;
        //private readonly SqlConnection connection;
        private readonly string connectionString;
        private List<TestResult> resultsToFlush = new List<TestResult>();
        private List<AttributeResult> attributeResultsToFlush = new List<AttributeResult>();
        private string actualSerialNumber;

        [MTFConstructor]
        [MTFAdditionalParameterInfo(ParameterName = "dbConnection", DisplayName = "Database connection", Description = "Connection string for connection to the database")]
        [MTFAdditionalParameterInfo(ParameterName = "machineName", DisplayName = "Machine name", Description = "Machine name is used for gnerating treacibility table names TM_From_xxx and TM_To_xxx. These tables are used for communication with Traceability Manager.")]
        public ALNATraceability(string dbConnection, string machineName)
        {
            connectionString = dbConnection;
            this.machineName = machineName;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();                
                watchdoqInProgress = true;
                resetTraceability();
            }
            startWatchDog();
        }
        
        [MTFMethod(DisplayName = "Start cycle")]
        public void StartCycle(string serialNumber)
        {
            actualSerialNumber = serialNumber;
            resultsToFlush.Clear();
            attributeResultsToFlush.Clear();
            checkWatchDog();
            checkTMReady();
            startTMCycle(serialNumber);
            waitTMStartCycle();
            //resetStationTable();
            resetTraceability();
        }

        [MTFMethod(DisplayName =  "Add component")]
        public void AddComponent(string componentName, string componentSerialNumbeer)
        {
            checkWatchDog();
            checkTMReady();
            addComponent(actualSerialNumber, componentName, componentSerialNumbeer);
            waitTMAddComponent();
            resetTraceability();
        }

        [MTFMethod(DisplayName = "End cycle")]
        public void EndCycle(bool ok, string failureReason)
        {
            if (string.IsNullOrEmpty(actualSerialNumber))
            {
                return;
            }

            if (resultsToFlush != null && resultsToFlush.Exists(r => r.Index > resultCount))
            {
                throw new Exception(string.Format("The method can not store as many result with index bigger than {0}. Please check the sequence.", resultCount));
            }
            if (attributeResultsToFlush != null && attributeResultsToFlush.Exists(r => r.Index > resultCount))
            {
                throw new Exception(string.Format("The method can not store as many attribute results with index bigger than {0}. Please check the sequence.", resultCount));
            }
            checkWatchDog();
            checkTMReady();
            try
            {
                saveResult(actualSerialNumber, ok, failureReason);
            }
            finally
            {
                actualSerialNumber = string.Empty;
            }
            
            waitTMEndCycle();
            //resetResutlTable();
            resetTraceability();
        }

        [MTFMethod(DisplayName = "Start cycle master part")]
        public void StartCycleMasterPart(string serialNumber)
        {
            actualSerialNumber = serialNumber;
            resultsToFlush.Clear();
            attributeResultsToFlush.Clear();
            checkWatchDog();
            checkTMReady();
            startMasterCycle(serialNumber);
            waitMasterStartCycle();
            //resetStationMasterTable();
            resetTraceability();
        }

        [MTFMethod(DisplayName = "End cycle master part")]
        public void EndCycleMasterPart(bool ok, string failureReason)
        {
            if (string.IsNullOrEmpty(actualSerialNumber))
            {
                return;
            }

            if (resultsToFlush != null && resultsToFlush.Exists(r => r.Index > resultCount))
            {
                throw new Exception(string.Format("The method can not store as many result with index bigger than {0}. Please check the sequence.", resultCount));
            }
            if (attributeResultsToFlush != null && attributeResultsToFlush.Exists(r => r.Index > resultCount))
            {
                throw new Exception(string.Format("The method can not store as many attribute results with index bigger than {0}. Please check the sequence.", resultCount));
            }
            checkWatchDog();
            checkTMReady();
            try
            {
                saveMasterResult(actualSerialNumber, ok, failureReason);
            }
            finally
            {
                actualSerialNumber = string.Empty;
            }
            
            waitMasterEndCycle();
            //resetMasterResutlTable();           
            resetTraceability();
        }
        
        [MTFMethod(DisplayName = "Add test results")]
        public void AddTestResults(List<TestResult> results)
        {
            int[] indexToRemove = resultsToFlush.Where(r => results.Any(res => res.Index == r.Index)).Select(r => r.Index).ToArray();
            foreach (int index in indexToRemove)
            {
                resultsToFlush.Remove(resultsToFlush.First(r => r.Index == index));
            }
            resultsToFlush.AddRange(results);
        }

        [MTFMethod(DisplayName = "Add attribute result")]
        public void AddAttributeTest(List<AttributeResult> results)
        {
            int[] indexToRemove = attributeResultsToFlush.Where(r => results.Any(res => res.Index == r.Index)).Select(r => r.Index).ToArray();
            foreach (int index in indexToRemove)
            {
                attributeResultsToFlush.Remove(attributeResultsToFlush.First(r => r.Index == index));
            }
            attributeResultsToFlush.AddRange(results);
        }

        private string fromTable
        { get { return string.Format("{0}{1}", tableFromPrefix, machineName); } }
        
        private string toTable
        { get { return string.Format("{0}{1}", tableToPrefix, machineName); } }

        public void Dispose()
        {
            stopWatchDog();
        }

        private void startWatchDog()
        {
            Task.Run(() => { watchdog(); });
        }

        private void stopWatchDog()
        {
            while (watchdoqInProgress)
            {
                Thread.Sleep(100);
            }

            runWatchdog = false;            
        }

        private int prevWatchdog;
        private int wrongWatchdogs;
        private bool runWatchdog;
        private bool watchdoqInProgress;
        private void watchdog()
        {
            runWatchdog = true;
            int watchdog = 0;
            string valueName = "Watchdog";
            while (runWatchdog)
            {
                try
                {
                    using (SqlConnection watchdogConnection = new SqlConnection(connectionString))
                    {
                        watchdogConnection.Open();
                        watchdoqInProgress = true;

                        using (SqlCommand commandWatchdog = new SqlCommand(string.Format("Select [{0}] from [{1}]", valueName, fromTable), watchdogConnection))
                        {
                            var val = commandWatchdog.ExecuteScalar();
                            watchdog = val == DBNull.Value ? 0 : (int) val;
                        }

                        using (SqlCommand command = new SqlCommand(string.Format("Update [{0}] Set [Watchdog]=@Watchdog", fromTable), watchdogConnection))
                        {
                            command.Parameters.AddWithValue("@Watchdog", 1 - watchdog);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                finally
                {
                    //TM didn't update watchdog, count it, if is too much not updated, TM is offline
                    wrongWatchdogs = prevWatchdog == watchdog ? wrongWatchdogs + 1 : 0;
                    prevWatchdog = watchdog;

                    watchdoqInProgress = false;                    
                }
                Thread.Sleep(2000);
            }
        }

        private void checkWatchDog()
        {
            if (wrongWatchdogs > 5)
            {
                throw new Exception("Watchdog error - TM is offline.");
            }
        }

        private T getDbValue<T>(string valueName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(string.Format("Select [{0}] from [{1}]", valueName, toTable), connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var value = reader[valueName];
                            if (value == DBNull.Value)
                            {
                                return default(T);
                            }

                            return (T) reader[valueName];
                        }
                    }
                }
            }

            return default(T);
        }

        private void checkTMReady()
        {
            for (int i = 0; i < 20; i++)
            {
                var isReady = getDbValue<int?>("Ready"); //0=Busy, 1=Ready
                if (isReady != 0)
                {
                    return;
                }
                Thread.Sleep(200);
            }

            stopWatchDog();
            watchdoqInProgress = true;
            resetTraceability();
            
            startWatchDog();
            for (int i = 0; i < 20; i++)
            {
                var isReady = getDbValue<int?>("Ready"); //0=Busy, 1=Ready
                if (isReady != 0)
                {
                    return;
                }
                Thread.Sleep(200);
            }


            throw new Exception("Traceability Manager is Busy"); 
        }

        private void startTMCycle(string serialNumber)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(string.Format("Update [{0}] Set [StartCycle]=1,[SerialNumber]=@serialNumber", fromTable), connection))
                { 
                    command.Parameters.AddWithValue("@serialNumber", serialNumber);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void addComponent(string serialNumber, string componentName, string componentSerialNumber)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(string.Format("Update [{0}] Set [Component]=1,[SerialNumber]=@serialNumber,[ComponentName]=@componentName,[ComponentSN]=@componentSN", fromTable), connection))
                {
                    command.Parameters.AddWithValue("@serialNumber", serialNumber);
                    command.Parameters.AddWithValue("@componentName", componentName);
                    command.Parameters.AddWithValue("@componentSN", componentSerialNumber);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void startMasterCycle(string serialNumber)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(string.Format("Update [{0}] Set [MasterInStation]=1,[SerialNumber]=@serialNumber", fromTable), connection))
                { 
                    command.Parameters.AddWithValue("@serialNumber", serialNumber);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void waitTMStartCycle()
        {
            for (int i = 0; i < 50; i++)
            {
                if (getDbValue<int?>("StartGranted") == 1)
                {
                    return;
                }
                if (getDbValue<int?>("StartDenied") == 1)
                {
                    var errormessage = getDbValue<string>("Message");
                    resetTraceability();
                    throw new Exception(string.Format("Start cycle isn't possible. {0}", errormessage));
                }
                Thread.Sleep(200);
            }
            resetTraceability();
            throw new Exception("Wait for TM start cycle - timeout");
        }   
        
        private void waitTMAddComponent()
        {
            for (int i = 0; i < 50; i++)
            {
                if (getDbValue<int?>("ComponentGranted") == 1)
                {
                    return;
                }
                if (getDbValue<int?>("ComponentDenied") == 1)
                {
                    var errormessage = getDbValue<string>("Message");
                    resetTraceability();
                    throw new Exception(string.Format("Add component isn't possible. {0}", errormessage));
                }
                Thread.Sleep(200);
            }
            resetTraceability();
            throw new Exception("Wait for TM add component - timeout");
        }
        
        private void waitMasterStartCycle()
        {
            for (int i = 0; i < 50; i++)
            {
                if (getDbValue<int?>("MasterAcknowledge") == 1)
                {
                    return;
                }
                Thread.Sleep(200);
            }
            resetTraceability();
            throw new Exception("Wait for Master start cycle - timeout");
        }

        private void waitTMEndCycle()
        {
            for (int i = 0; i < 50; i++)
            {
                if (getDbValue<int?>("ResultStored") == 1)
                {
                    return;
                }
                if (getDbValue<int?>("Error") == 1)
                {
                    var errormessage = getDbValue<string>("Message");
                    throw new Exception(string.Format("Store result error. {0}", errormessage));
                }
                Thread.Sleep(200);
            }

            resetTraceability();
            throw new Exception("Wait for TM end cycle - timeout");            
        }        
        
        private void waitMasterEndCycle()
        {
            for (int i = 0; i < 50; i++)
            {
                if (getDbValue<int?>("MasterResultStored") == 1)
                {
                    return;
                }
                if (getDbValue<int?>("Error") == 1)
                {
                    var errormessage = getDbValue<string>("Message");
                    resetTraceability();
                    throw new Exception(string.Format("Store resutlt error. {0}", errormessage));
                }
                Thread.Sleep(200);
            }

            resetTraceability();
            throw new Exception("Wait for Master end cycle - timeout");            
        }

        private void resetTraceability()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                resetStationTable(connection);
                resetStationMasterTable(connection);
                resetResutlTable(connection);
                resetMasterResutlTable(connection);
                resetComponentTable(connection);
            }
        }

        private void resetStationTable(SqlConnection connection)
        {
            using (SqlCommand command = new SqlCommand(string.Format("Update [{0}] Set [StartCycle]=0,[SerialNumber]=''", fromTable), connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private void resetStationMasterTable(SqlConnection connection)
        {
            using (SqlCommand command = new SqlCommand(string.Format("Update [{0}] Set [MasterInStation]=0,[SerialNumber]=''", fromTable), connection))
            { 
                command.ExecuteNonQuery();
            }
        }

        private void resetComponentTable(SqlConnection connection)
        {
            using (SqlCommand command = new SqlCommand(string.Format("Update [{0}] Set [Component]=0,[SerialNumber]='',[ComponentName]='',[ComponentSN]=''", fromTable), connection))
            { 
                command.ExecuteNonQuery();
            }
        }

        private void resetResutlTable(SqlConnection connection)
        {
            using (SqlCommand command = generateSaveResultCommand("PartOK", "PartNOK", connection))
            {
                command.Parameters.AddWithValue("@PartOK", 0);
                command.Parameters.AddWithValue("@PartNOK", 0);
                command.Parameters.AddWithValue("@FailureReason", string.Empty);

                command.Parameters.AddWithValue("@SerialNumber", string.Empty);
                fillResultData(command, null);
                fillResultAttributes(command, null);

                command.ExecuteNonQuery();
            }
        }

        private void resetMasterResutlTable(SqlConnection connection)
        {
            using (SqlCommand command = generateSaveResultCommand("MasterSuccessful", "MasterNotSuccessful", connection))
            {
                command.Parameters.AddWithValue("@MasterSuccessful", 0);
                command.Parameters.AddWithValue("@MasterNotSuccessful", 0);
                command.Parameters.AddWithValue("@FailureReason", string.Empty);

                command.Parameters.AddWithValue("@SerialNumber", string.Empty);
                fillResultData(command, null);
                fillResultAttributes(command, null);

                command.ExecuteNonQuery();
            }
        }

        private void appendSetProperty(StringBuilder sb, string name)
        {
            sb.Append("[").Append(name).Append("]=@").Append(name);
        }

        private SqlCommand generateSaveResultCommand(string okFieldName, string nokFieldName, SqlConnection connection)
        {
            var sb = new StringBuilder("Update [").Append(fromTable).Append("] Set ");
            appendSetProperty(sb, okFieldName);
            sb.Append(",");
            appendSetProperty(sb, nokFieldName);
            sb.Append(",");
            appendSetProperty(sb, "SerialNumber");
            sb.Append(",");
            appendSetProperty(sb, "FailureReason");
            for (int i = 1; i <= resultCount; i++)
            {
                sb.Append(",");
                appendSetProperty(sb, string.Format("NumericTest{0:00}", i));
                sb.Append(",");
                appendSetProperty(sb, string.Format("NumericMin{0:00}", i));
                sb.Append(",");
                appendSetProperty(sb, string.Format("NumericMax{0:00}", i));
                sb.Append(",");
                appendSetProperty(sb, string.Format("NumericValue{0:00}", i));
                sb.Append(",");
                appendSetProperty(sb, string.Format("AttributeTest{0:00}", i));
            }

            return new SqlCommand(sb.ToString(), connection);
        }

        private void fillResultData(SqlCommand command, List<TestResult> results)
        {
            for (int i = 1; i <= resultCount; i++)
            {
                if (results != null && results.Exists(r => r.Index == i))
                {
                    var result = results.First(r => r.Index == i);
                    command.Parameters.AddWithValue(string.Format("@NumericTest{0:00}", i), result.Result ? 1 : 2);
                    command.Parameters.AddWithValue(string.Format("@NumericMin{0:00}", i), result.Min);
                    command.Parameters.AddWithValue(string.Format("@NumericMax{0:00}", i), result.Max);
                    command.Parameters.AddWithValue(string.Format("@NumericValue{0:00}", i), result.Value);
                }
                else
                {
                    command.Parameters.AddWithValue(string.Format("@NumericTest{0:00}", i), 0);
                    command.Parameters.AddWithValue(string.Format("@NumericMin{0:00}", i), 0);
                    command.Parameters.AddWithValue(string.Format("@NumericMax{0:00}", i), 0);
                    command.Parameters.AddWithValue(string.Format("@NumericValue{0:00}", i), 0);
                }
            }
        }

        private void fillResultAttributes(SqlCommand command, List<AttributeResult> results)
        {
            for (int i = 1; i <= resultCount; i++)
            {
                if (results != null && results.Exists(r => r.Index == i))
                {
                    var result = results.First(r => r.Index == i);
                    command.Parameters.AddWithValue(string.Format("@AttributeTest{0:00}", i), result.Result ? 1 : 2);
                }
                else
                {
                    command.Parameters.AddWithValue(string.Format("@AttributeTest{0:00}", i), 0);
                }
            }            
        }

        private void saveResult(string serialNumber, bool ok, string failureReason)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                failureReason = failureReason.Length > 40 ? failureReason.Substring(0, 40) : failureReason;
                using (SqlCommand command = generateSaveResultCommand("PartOK", "PartNOK", connection))
                {
                    if (ok)
                    {
                        command.Parameters.AddWithValue("@PartOK", 1);
                        command.Parameters.AddWithValue("@PartNOK", 0);
                        command.Parameters.AddWithValue("@FailureReason", string.Empty);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@PartOK", 0);
                        command.Parameters.AddWithValue("@PartNOK", 1);
                        command.Parameters.AddWithValue("@FailureReason", failureReason);
                    }
                    command.Parameters.AddWithValue("@SerialNumber", serialNumber);
                    fillResultData(command, resultsToFlush);
                    fillResultAttributes(command, attributeResultsToFlush);

                    command.ExecuteNonQuery();
                }
            }
        }       
        
        private void saveMasterResult(string serialNumber, bool ok, string failureReason)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                failureReason = failureReason.Length > 40 ? failureReason.Substring(0, 40) : failureReason;
                using (SqlCommand command = generateSaveResultCommand("MasterSuccessful", "MasterNotSuccessful", connection))
                {
                    if (ok)
                    {
                        command.Parameters.AddWithValue("@MasterSuccessful", 1);
                        command.Parameters.AddWithValue("@MasterNotSuccessful", 0);
                        command.Parameters.AddWithValue("@FailureReason", string.Empty);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@MasterSuccessful", 0);
                        command.Parameters.AddWithValue("@MasterNotSuccessful", 1);
                        command.Parameters.AddWithValue("@FailureReason", failureReason);
                    }
                    command.Parameters.AddWithValue("@SerialNumber", serialNumber);
                    fillResultData(command, resultsToFlush);
                    fillResultAttributes(command, attributeResultsToFlush);

                    command.ExecuteNonQuery();
                }
            }
        }
    }

    [MTFKnownClass]
    public class TestResult
    {
        public bool Result { get; set; }
        public int Index { get; set; }
        public float Min { get; set; }
        public float Max { get; set; }
        public float Value { get; set; }
    }

    [MTFKnownClass]
    public class AttributeResult
    {
        public bool Result { get; set; }
        public int Index { get; set; }
    }
}
