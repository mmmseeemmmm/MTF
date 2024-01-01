// ****************************************************************************
// ****************************************************************************
// copyright Automotive Lighting Reutlingen GmbH, 2018 / EET
// ****************************************************************************
// History:
//
// 2019-08-02 (Jascha Breithaupt): CHANGED ImportContent(): added parameter "ImportSingleMultipleParameter" -> allows to import just single/multiple section(s) of parameter set
// 2019-08-02 (Jascha Breithaupt): CHANGED ImportContent(): added parameter "AppendImportData" -> values of existing data sets/sections are updated, new ones are appended
// 2019-08-01 (Jascha Breithaupt): CHANGED ImportContent(): added parameter "IsParameterSet" -> compares import set to existing set (must be same)
// 2019-08-01 (Jascha Breithaupt): CHANGED GetContentSeparated(): added default values for parameter "GetSpecificSection" and "Section"
// 2019-05-08 (Jascha Breithaupt): CHANGED GetContentSeparated(): added ItemsCount to output data
// 2019-05-08 (Jascha Breithaupt): CHANGED GetContentSeparated(): added choice to get content of just one specific section
// 2019-04-01 (Jascha Breithaupt): FIXED ImportContent(): find names -> DataSetItem can contain "SectionName" as Key
// 2019-03-13 (Jascha Breithaupt): FIXED Logging
// 2019-02-27 (Jascha Breithaupt): ADDED Logging
// 2019-02-26 (Jascha Breithaupt): ADDED function OrderContent()
// 2019-02-26 (Jascha Breithaupt): ADDED function AddSection()
// 2019-02-26 (Jascha Breithaupt): ADDED function AddDataSet()
// 2019-02-26 (Jascha Breithaupt): ADDED function AddDataSets()
// 2019-02-14 (Jascha Breithaupt): CHANGED: MTF function display names grouped with leading numbers
// 2019-02-14 (Jascha Breithaupt): CHANGED: return value of ExportContent() is now full export path and filename
// 2019-01-30 (Jascha Breithaupt): ADDED: function ImportContent()
// 2019-01-30 (Jascha Breithaupt): ADDED: function ExportContent()
// 2019-01-28 (Jascha Breithaupt): ADDED: function GetContent()
// 2019-01-25 (Jascha Breithaupt): ADDED: constructor to read csv configuration file
// 2018-09-06 (Jochen Langer):  FIXED: load list not working for section without datasets
// 2018-09-06 (Jochen Langer):  FIXED: conflict with old Volkontainer.dll using same namespace
// 2018-09-06 (Jochen Langer):  CHANGED: internal datastructure from array to dictionary
// 2018-08-30 (Jochen Langer):  ADDED:   class Section to group datasets
// 2018-08-30 (Jochen Langer):  ADDED:   file header
// ****************************************************************************


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutomotiveLighting.MTFCommon;
using System.IO;
using System.Diagnostics;
using AutomotiveLighting.MTF.GlobalDataContainer;

namespace General
{

    /** \enum DATA_TYPE
	* brief	The enum \p DATA_TYPE describes the supported data types.
	*/
    public enum DATA_TYPE
    {
        UNSIGNED,	    ///< unsigned integer
        SIGNED,      	///< signed integer
        FLOAT,	        ///< floating point
        STRING,      	///< string
    };

    /** \brief The class \b %Volkontainer represents a container capable to hold objects of any simple datatype.
    *	\note  currently only strings are supported.
    */
    [MTFClass(Name = "Global Data Container", Description = "Container to hold any simple data items for global access", Icon = MTFIcons.ZipFile)]
    [MTFClassCategory("Execution Control")]
    public class GlobalDataContainer
    {
        #region constructors

        /** \brief Initializes a new instance of the \p Volkontainer class.
        *	\note The Default ctor is needed for MTF.
        */
        public GlobalDataContainer()
        {
            //_content = new Dictionary<String, Dictionary<String, String>>();
            //_log = new Logging();
        }
        //----------------------------------------------------------------------------------------------------------------------------
        [MTFConstructor(Description = "Construct the Global Data Container with .csv configuration file.\n"
            + "\n"
            + "Structure of .csv file:\n"
            + "-------------------------------\n"
            + Constants.SectionName + ";[Name1]\n"
            + Constants.DataSetItem + ";[Key1];[Value]\n"
            + Constants.DataSetItem + ";[Key2];[Value]\n"
            + Constants.SectionName + ";[Name2]\n"
            + Constants.DataSetItem + ";[Key1];[Value]\n"
            + Constants.DataSetItem + ";[Key2];[Value]\n"
            + "\n"
            + "Note: Init values can not be \";\" separated!"
            )]
        public GlobalDataContainer(string componentName, string configFile)
        {
            if (string.IsNullOrEmpty(componentName)) { throw new Exception("Please fill in a componentName!"); }
            _compName = componentName;
            _log = new Logging(_compName);
            var result = ImportContent(configFile);
        }
        //----------------------------------------------------------------------------------------------------------------------------
        /** \brief Initializes a new instance of the \p Volkontainer class.
        *
        * \param [in] content       	List of Sections to populate the container.
        */
        [MTFConstructor(Description = "Construct the Global Data Container manually")]
        public GlobalDataContainer(string componentName, Section[] content)
        {
            if (string.IsNullOrEmpty(componentName)) { throw new Exception("Please fill in a componentName!"); }
            _compName = componentName;
            _log = new Logging(_compName);
            _content = new Dictionary<String, Dictionary<String, String>>();
            if (content != null)
            {
                foreach (Section section in content)
                {
                    _content.Add(section.Name, new Dictionary<string, string>());

                    if (section.DataSets != null)
                    {
                        foreach (DataSet dataset in section.DataSets)
                        {
                            _content[section.Name].Add(dataset.Key, dataset.Value);
                        }
                    }
                }
            }
        }

        #endregion

        #region 0 (Basic Get/Set)

        /** \brief Gets the value of the dataset in the section addressed by their names
        * 
        * \param [in] Section	group containing the dataset to read.
        * \param [in] Key	    name of the dataset to read.
        *
        * \return Value read as String.
        */
        [MTFMethod(DisplayName = "0 - Get Value")]
        [MTFAdditionalParameterInfo(ParameterName = "Section", ValueListName = "DataSetNames")]
        [MTFAdditionalParameterInfo(ParameterName = "Key", ValueListName = "DataSetNames", ValueListLevel = 1, ValueListParentName = "Section")]
        public String GetValue(String Section, String Key)
        {
            if (String.IsNullOrWhiteSpace(Section))
            {
                throw new ArgumentNullException("Section");
            }
            if (String.IsNullOrWhiteSpace(Key))
            {
                throw new ArgumentNullException("Key");
            }

            //write logging
            if (_log._loggingActive && !_excludeGetSetFromLog)
            { _log.WriteToLog("0 - Get Value", LoggingStatus.OK, String.Format("{0}{3}{1}{4}{2}", Section, Key, _content[Section][Key], Constants.Delimiter_1, Constants.Delimiter_2)); }

            return _content[Section][Key];
        }
        //----------------------------------------------------------------------------------------------------------------------------       
        /** \brief	Sets the value of the dataset in the section addressed by their names to the value as string.
        *
        * \param [in] Section	group containing the dataset to modify.
        * \param [in] Key	    name of the dataset to modify.
        * \param [in] Value 	new value of the dataset.\n
        */
        [MTFMethod(DisplayName = "0 - Set Value")]
        [MTFAdditionalParameterInfo(ParameterName = "Section", ValueListName = "DataSetNames")]
        [MTFAdditionalParameterInfo(ParameterName = "Key", ValueListName = "DataSetNames", ValueListLevel = 1, ValueListParentName = "Section")]
        public void SetValue(String Section, String Key, String Value)
        {
            if (String.IsNullOrWhiteSpace(Section))
            {
                throw new ArgumentNullException("Section");
            }
            if (String.IsNullOrWhiteSpace(Key))
            {
                throw new ArgumentNullException("Key");
            }

            _content[Section][Key] = Value;

            //write logging
            if (_log._loggingActive && !_excludeGetSetFromLog)
            { _log.WriteToLog("0 - Set Value", LoggingStatus.OK, String.Format("{0}{3}{1}{4}{2}", Section, Key, Value, Constants.Delimiter_1, Constants.Delimiter_2)); }
        }

        #endregion

        #region 1 (Get/Order/Add)

        [MTFMethod(Description = "Get all content of container or section\n",
                    DisplayName = "1 - Get Content Separated")]
        [MTFAdditionalParameterInfo(ParameterName = "GetSpecificSection", Description = "Get content of one specific section")]
        [MTFAdditionalParameterInfo(ParameterName = "Section", ValueListName = "DataSetNames", Description = "Section to get content")]
        [MTFAdditionalParameterInfo(ParameterName = "ExcludeContentFromLogging", Description = "Do not write content to logfile")]
        public OutputData GetContentSeparated(bool GetSpecificSection = false, String Section = "", bool ExcludeContentFromLogging = false)
        {
            //sanity check
            if (GetSpecificSection == true && String.IsNullOrWhiteSpace(Section)) { throw new ArgumentNullException("Section"); }

            //get number of total items
            var _numItems = 0;
            var _numSections = 0;
            switch (GetSpecificSection)
            {
                //all sections
                case false:
                default:
                    foreach (var item in _content)
                    {
                        _numItems += item.Value.Count;
                        _numSections++;
                    }
                    //write logging 
                    if (_log._loggingActive) { _log.WriteToLog("1 - Get Content Separated", LoggingStatus.OK, String.Format("Get {0} -> {1}s: {2} | {3}s: {4}", _compName, Constants.SectionName, _numSections, Constants.DataSetItem, _numItems)); }
                    break;
                //specific section
                case true:
                    _numSections = 1;
                    foreach (var item in _content)
                    {
                        if (item.Key == Section)
                        { _numItems = item.Value.Count; break; }
                    }
                    //write logging 
                    if (_log._loggingActive) { _log.WriteToLog("1 - Get Content Separated", LoggingStatus.OK, String.Format("Get {0} -> {1}: {2} | {3}s: {4}", _compName, Constants.SectionName, Section, Constants.DataSetItem, _numItems)); }
                    break;
            }                     
           
            //create arrays
            OutputData _outContent = new OutputData();
            _outContent.ItemsCount = _numItems;
            _outContent.Section = new string[_numItems];
            _outContent.Key = new string[_numItems];
            _outContent.Data = new string[_numItems];
            string[] content = new string[_numItems];

            //get all items
            int cnt = 0;

            //write logging
            if (_log._loggingActive && !ExcludeContentFromLogging)
            {
                _log.WriteToLog("1 - Get Content Separated", LoggingStatus.OK, "#Index | " + Constants.SectionName + " | " + Constants.DataSetItem + ".Key | " + Constants.DataSetItem + ".Value");
            }

            //get content of sections(s)
            switch (GetSpecificSection)
            {
                //all sections
                case false:
                default:
                    foreach (var name in _content)
                    {
                        var dict = _content[name.Key];
                        foreach (var item in dict)
                        {
                            _outContent.Section[cnt] = name.Key;
                            _outContent.Key[cnt] = item.Key;
                            _outContent.Data[cnt] = item.Value;
                            //write logging
                            if (_log._loggingActive && !ExcludeContentFromLogging)
                            {
                                //"[name] -> [key] = [value]"
                                content[cnt] = name.Key + " | " + item.Key + " | " + item.Value;
                                _log.WriteToLog("1 - Get Content Separated", LoggingStatus.OK, "#" + cnt + " | " + content[cnt]);
                            }
                            cnt++;
                        }
                    }
                    break;
                //specific section
                case true:
                    var secDict = _content[Section];
                    foreach (var item in secDict)
                    {
                        _outContent.Section[cnt] = Section;
                        _outContent.Key[cnt] = item.Key;
                        _outContent.Data[cnt] = item.Value;
                        //write logging
                        if (_log._loggingActive && !ExcludeContentFromLogging)
                        {
                            //"[name] -> [key] = [value]"
                            content[cnt] = Section + " | " + item.Key + " | " + item.Value;
                            _log.WriteToLog("1 - Get Content Separated", LoggingStatus.OK, "#" + cnt + " | " + content[cnt]);
                        }
                        cnt++;
                    }
                    break;
            }

            return _outContent;
        }
        //----------------------------------------------------------------------------------------------------------------------------
        [MTFMethod(Description = "Get all content of container in this way:\n"
           + "[" + Constants.SectionName + "]" + Constants.Delimiter_1 + "[" + Constants.DataSetItem + ".Key]" + Constants.Delimiter_2 + "[" + Constants.DataSetItem + ".Value ]\n",
            DisplayName = "1 - Get Content")]
        [MTFAdditionalParameterInfo(ParameterName = "FirstElemIsAll", Description = "First element of output array contains all elements, separated by '\\n'")]
        [MTFAdditionalParameterInfo(ParameterName = "ExcludeContentFromLogging", Description = "Do not write content to logfile")]
        public string[] GetContent(bool FirstElemIsAll = false, bool ExcludeContentFromLogging = false)
        {
            //get number of total items
            var _numItems = 0;
            var _numSections = 0;
            foreach (var item in _content)
            {
                _numItems += item.Value.Count;
                _numSections++;
            }

            //write logging
            if (_log._loggingActive) { _log.WriteToLog("1 - Get Content", LoggingStatus.OK, String.Format("Get {0} -> {1}s: {2} | {3}s: {4}", _compName, Constants.SectionName, _numSections, Constants.DataSetItem, _numItems)); }

            if (FirstElemIsAll) { _numItems++; }

            //create array
            string[] content = new string[_numItems];

            //get all items
            int cnt = 0;
            if (FirstElemIsAll) { cnt++; }
            foreach (var name in _content)
            {
                var dict = _content[name.Key];
                foreach (var item in dict)
                {
                    //"[name] -> [key] = [value]"
                    content[cnt] = name.Key + Constants.Delimiter_1 + item.Key + Constants.Delimiter_2 + item.Value;
                    //write logging
                    if (_log._loggingActive && !ExcludeContentFromLogging) { _log.WriteToLog("1 - Get Content", LoggingStatus.OK, content[cnt]); }
                    cnt++;
                }
            }

            if (FirstElemIsAll)
            {
                for (int i = 1; i < _numItems; i++)
                {
                    content[0] += content[i] + "\n";
                }
            }

            return content;
        }
        //----------------------------------------------------------------------------------------------------------------------------
        [MTFMethod(Description = "Orders container content by Section and DataSet-Key",
                    DisplayName = "1 - Order Content")]
        [MTFAllowedParameterValue("SortOrder", "Ascending", "Ascending")]
        [MTFAllowedParameterValue("SortOrder", "Descending", "Descending")]
        public string OrderContent(string SortOrder = "Ascending")
        {
            Dictionary<String, Dictionary<String, String>> _contentPreSorted = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<String, Dictionary<String, String>> _contentSorted = new Dictionary<string, Dictionary<string, string>>();
            string _outputString = "Unknown SortOrder";
            switch (SortOrder)
            {
                case "Ascending":
                    _contentPreSorted = _content.Keys.OrderBy(k => k).ToDictionary(k => k, k => _content[k]);
                    _contentSorted = _contentPreSorted;
                    foreach (var item in _content)
                    {
                        _contentSorted[item.Key] = _contentPreSorted[item.Key].Keys.OrderBy(k => k).ToDictionary(k => k, k => _content[item.Key][k]);
                    }
                    _outputString = "Ascending";
                    break;
                case "Descending":
                    _contentPreSorted = _content.Keys.OrderByDescending(k => k).ToDictionary(k => k, k => _content[k]);
                    _contentSorted = _contentPreSorted;
                    foreach (var item in _content)
                    {
                        _contentSorted[item.Key] = _contentPreSorted[item.Key].Keys.OrderByDescending(k => k).ToDictionary(k => k, k => _content[item.Key][k]);
                    }
                    _outputString = "Descending";
                    break;
            }

            //write logging
            if (_log._loggingActive) { _log.WriteToLog("1 - Order Content", LoggingStatus.OK, SortOrder); }

            _content = _contentSorted;
            return _outputString;
        }
        //----------------------------------------------------------------------------------------------------------------------------       
        [MTFMethod(Description = "Adds a new Section to container",
            DisplayName = "1 - Add Section")]
        public string AddSection(String NewSection)
        {
            //sanity check
            if (String.IsNullOrWhiteSpace(NewSection)) { throw new ArgumentNullException("NewSection"); }

            //check if section exists
            if (!_content.ContainsKey(NewSection))
            {
                _content.Add(NewSection, new Dictionary<string, string>());
                //write logging
                if (_log._loggingActive) { _log.WriteToLog("1 - Add Section", LoggingStatus.OK, NewSection); }
                return NewSection;
            }
            else
            {
                var _errMsg = String.Format("Section ({0}) does already exist!", NewSection);
                //write logging
                if (_log._loggingActive) { _log.WriteToLog("1 - Add Section", LoggingStatus.ERROR, _errMsg); }
                throw new Exception(_errMsg);
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------
        [MTFMethod(Description = "Add DataSet (Key-Value pair) to existing Section",
            DisplayName = "1 - Add DataSet")]
        [MTFAdditionalParameterInfo(ParameterName = "Section", ValueListName = "DataSetNames")]
        [MTFAdditionalParameterInfo(ParameterName = "SetValueKeyExists", Description = "Sets the value if the key already exists")]
        public string AddDataSet(String Section, String Key, String Value, bool SetValueKeyExists = false)
        {
            //sanity check
            if (String.IsNullOrWhiteSpace(Section)) { throw new ArgumentNullException("Section"); }
            if (String.IsNullOrWhiteSpace(Key)) { throw new ArgumentNullException("Key"); }
            if (String.IsNullOrWhiteSpace(Value)) { throw new ArgumentNullException("Value"); }

            string outString = "";

            //check if section exists
            if (_content.ContainsKey(Section))
            {
                //check if key already exists
                if (!_content[Section].ContainsKey(Key))
                {
                    _content[Section].Add(Key, Value);

                    outString = String.Format("{0}{3}{1}{4}{2}", Section, Key, Value, Constants.Delimiter_1, Constants.Delimiter_2);
                    //write logging
                    if (_log._loggingActive) { _log.WriteToLog("1 - Add DataSet", LoggingStatus.OK, outString); }
                }
                else
                {
                    if (SetValueKeyExists)
                    {
                        //write logging
                        if (_log._loggingActive) { _log.WriteToLog("1 - Add DataSet", LoggingStatus.OK, String.Format("Key ({1}) already exists: {0}{3}{1}{4}{2} | Set to new value ({0}{3}{1}{4}{5})", Section, Key, _content[Section][Key], Constants.Delimiter_1, Constants.Delimiter_2, Value)); }
                        _content[Section][Key] = Value;
                        outString = String.Format("Update {0}{3}{1}{4}{2}", Section, Key, Value, Constants.Delimiter_1, Constants.Delimiter_2);
                    }
                    else
                    {
                        string _errMsg = String.Format("Key ({0}) already exists!", Key);
                        //write logging
                        if (_log._loggingActive) { _log.WriteToLog("1 - Add DataSet", LoggingStatus.ERROR, _errMsg); }
                        throw new Exception(_errMsg);
                    }
                }
                return outString;
            }
            else
            {
                string _errMsg = String.Format("Section ({0}) does not exist!", Section);
                //write logging
                if (_log._loggingActive) { _log.WriteToLog("1 - Add DataSet", LoggingStatus.ERROR, _errMsg); }
                throw new Exception(_errMsg);
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------
        [MTFMethod(Description = "Add multiple DataSets (Key-Value pairs) to existing Section",
            DisplayName = "1 - Add DataSets")]
        [MTFAdditionalParameterInfo(ParameterName = "Section", ValueListName = "DataSetNames")]
        [MTFAdditionalParameterInfo(ParameterName = "SetValueKeyExists", Description = "Sets the value if the key already exits")]
        public string[] AddDataSets(String Section, DataSet[] DataSets, bool SetValueKeyExists = false)
        {
            //sanity check
            if (String.IsNullOrWhiteSpace(Section)) { throw new ArgumentNullException("Section"); }
            if (DataSets == null) { throw new ArgumentNullException("DataSets"); }
            for (int i = 0; i < DataSets.Count(); i++)
            {
                if (String.IsNullOrWhiteSpace(DataSets[i].Value)) { throw new ArgumentNullException("Value (#" + i + ")"); }
                if (String.IsNullOrWhiteSpace(DataSets[i].Key)) { throw new ArgumentNullException("Key (#" + i + ")"); }
            }

            List<string> output = new List<string>();

            //check if section exists
            if (_content.ContainsKey(Section))
            {
                foreach (DataSet dataset in DataSets)
                {
                    //check if key already exists
                    if (!_content[Section].ContainsKey(dataset.Key))
                    {
                        _content[Section].Add(dataset.Key, dataset.Value);
                        var _s = String.Format("{0}{3}{1}{4}{2}", Section, dataset.Key, dataset.Value, Constants.Delimiter_1, Constants.Delimiter_2);
                        output.Add(_s);
                        //write logging
                        if (_log._loggingActive) { _log.WriteToLog("1 - Add DataSets", LoggingStatus.OK, _s); }
                    }
                    else
                    {
                        if (SetValueKeyExists)
                        {
                            //write logging
                            if (_log._loggingActive) { _log.WriteToLog("1 - Add DataSets", LoggingStatus.OK, String.Format("Key ({1}) already exists: {0}{3}{1}{4}{2} | Set to new value ({0}{3}{1}{4}{5})", Section, dataset.Key, _content[Section][dataset.Key], Constants.Delimiter_1, Constants.Delimiter_2, dataset.Value, dataset.Key)); }
                            _content[Section][dataset.Key] = dataset.Value;
                        }
                        else
                        {

                            string _errMsg = String.Format("Key ({0}) already exists!", dataset.Key);
                            //write logging
                            if (_log._loggingActive) { _log.WriteToLog("1 - Add DataSets", LoggingStatus.ERROR, _errMsg); }
                            throw new Exception(_errMsg);
                        }
                    }
                }
            }
            else
            {
                string _errMsg = String.Format("Section ({0}) does not exist!", Section);
                //write logging
                if (_log._loggingActive) { _log.WriteToLog("1 - Add DataSets", LoggingStatus.ERROR, _errMsg); }
                throw new Exception(_errMsg);
            }

            return output.ToArray();
        }

        #endregion

        #region 2 (Import/Export)

        [MTFMethod(Description = "Saves content of container in file.\n"
            + "Textfile (.txt):\n"
            + "[" + Constants.SectionName + "]" + Constants.Delimiter_1 + "[" + Constants.DataSetItem + ".Key]" + Constants.Delimiter_2 + "[" + Constants.DataSetItem + ".Value ]\n"
            + "\n"
            + "Semicolon Separated Values (.csv):\n"
            + Constants.SectionName + ";[Name1]\n"
            + Constants.DataSetItem + ";[Key1];[Value]\n"
            + Constants.DataSetItem + ";[Key2];[Value]\n"
            + Constants.SectionName + ";[Name2]\n"
            + Constants.DataSetItem + ";[Key1];[Value]\n"
            + Constants.DataSetItem + ";[Key2];[Value]\n"
            + "\n"
            + "Output value is export\n"
            + "[0] Directory Name\n"
            + "[1] File Name\n"
            + "[2] Full Path (Directory + File Name)",
           DisplayName = "2 - Export Content")]
        [MTFAdditionalParameterInfo(ParameterName = "ExportPath", Description = "Path to save output file", DefaultValue = @".\_gdcexport\")]
        [MTFAdditionalParameterInfo(ParameterName = "AppendToFileName", Description = "String is appended to file name with leading '_'")]
        //[MTFAdditionalParameterInfo(ParameterName = "CreateSourceFile", Description = "Creates .csv file which can be used to initialize the container")]
        [MTFAdditionalParameterInfo(ParameterName = "OutputFormat", Description = "Creates .csv file which can be used to initialize the container")]
        [MTFAllowedParameterValue("OutputFormat", ".txt", ".txt")]
        [MTFAllowedParameterValue("OutputFormat", ".csv (importable)", ".csv")]
        [MTFAdditionalParameterInfo(ParameterName = "ExcludeContentFromLogging", Description = "Do not write content to logfile")]
        public string[] ExportContent(string ExportPath = @".\_gdcexport\", string AppendToFileName = "", string OutputFormat = ".txt", bool ExcludeContentFromLogging = false)
        {
            //check sanity
            if (ExportPath == null) { ExportPath = @".\gdcexport\"; }
            if (!Directory.Exists(ExportPath)) { Directory.CreateDirectory(ExportPath); }

            // Create new stopwatch.
            Stopwatch stopwatch = new Stopwatch();

            // Begin timing.
            stopwatch.Start();

            //
            if (AppendToFileName != "")
            {
                if (AppendToFileName.Substring(0, 1) != "_") { AppendToFileName = "_" + AppendToFileName; }
            }

            //get container content
            _log.DeactivateLogging(false);//_log._loggingActive = false;
            var _contentOut = GetContent(false, true);
            _log.ActivateLogging(false, false);//_log._loggingActive = true;

            //create filename and extension            
            var _fileName = "_" + _compName + "_Content";
            var _extension = ".txt";

            //create source file
            if (OutputFormat == ".csv")
            {
                //update filename and extension
                _fileName = "_" + _compName + "_Source";
                _extension = ".csv";

                //create source content
                List<string> _contentTemp = new List<string>();
                List<string[]> _contentSplit = new List<string[]>();
                string[] _contentSplitActual = new string[3];
                string _sectionNameTemp = null;

                for (int i = 0; i < _contentOut.Length; i++)
                {
                    _contentSplit.Add(_contentOut[i].Split(new string[] { Constants.Delimiter_1, Constants.Delimiter_2 }, StringSplitOptions.None));
                    _contentSplitActual = _contentSplit.ElementAt(i).ToArray();
                    if (i != 0)
                    {
                        if (_contentSplitActual[0] != _sectionNameTemp)
                        {
                            _sectionNameTemp = _contentSplitActual[0];
                            _contentTemp.Add(Constants.SectionName + ";" + _sectionNameTemp);
                            _contentTemp.Add(Constants.DataSetItem + ";" + _contentSplitActual[1] + ";" + _contentSplitActual[2]);
                        }
                        else
                        {
                            _contentTemp.Add(Constants.DataSetItem + ";" + _contentSplitActual[1] + ";" + _contentSplitActual[2]);
                        }
                    }
                    else
                    {
                        _sectionNameTemp = _contentSplitActual[0];
                        _contentTemp.Add(Constants.SectionName + ";" + _sectionNameTemp);
                        _contentTemp.Add(Constants.DataSetItem + ";" + _contentSplitActual[1] + ";" + _contentSplitActual[2]);
                    }
                }

                //copy temp to out
                _contentOut = _contentTemp.ToArray();
            }

            //get current date and time
            var _dateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss_ffff");

            //write output file
            try
            {
                var outPathFile = System.IO.Path.Combine(ExportPath + "/" + _dateTime + _fileName + AppendToFileName + _extension);
                outPathFile = System.IO.Path.GetFullPath(outPathFile);
                File.WriteAllLines(outPathFile, _contentOut);
                //write logging
                if (_log._loggingActive)
                {
                    //output/export path and file name
                    _log.WriteToLog("2 - Export Content", LoggingStatus.OK, String.Format("Export data from {0} to {1}", _compName, outPathFile));
                    //number of items
                    var _numItems = 0;
                    var _numSections = 0;
                    foreach (var item in _content)
                    {
                        // _numValues += _content[item.Key].Values.Count;
                        _numItems += item.Value.Count;
                        _numSections++;
                    }
                    _log.WriteToLog("2 - Export Content", LoggingStatus.OK, String.Format("{0}s: {1} | {2}s: {3}", Constants.SectionName, _numSections, Constants.DataSetItem, _numItems));
                    //_log.WriteToLog("2 - Export Content", "OK", "Items: " + _numItems.ToString());
                    //items/content
                    if (!ExcludeContentFromLogging)
                    {
                        for (int i = 0; i < _contentOut.Length; i++)
                        {
                            _log.WriteToLog("2 - Export Content", LoggingStatus.OK, _contentOut[i]);
                        }
                    }
                }
                // Stop timing.
                stopwatch.Stop();
                //write logging
                if (_log._loggingActive) { _log.WriteToLog("2 - Export Content", LoggingStatus.OK, String.Format("Export done -> Duration: {0}", stopwatch.Elapsed)); }
                return new string[3] { Path.GetDirectoryName(outPathFile), Path.GetFileName(outPathFile), outPathFile };
            }
            catch (Exception)
            {
                var _errMsg = "Could not write data to file";
                if (!Directory.Exists(ExportPath)) { _errMsg += ": directory does not exist!"; }
                //write logging
                if (_log._loggingActive) { _log.WriteToLog("2 - Export Content", LoggingStatus.ERROR, _errMsg); }
                // Stop timing.
                stopwatch.Stop();
                //write logging
                if (_log._loggingActive) { _log.WriteToLog("2 - Export Content", LoggingStatus.ERROR, String.Format("Export done -> Duration: {0}", stopwatch.Elapsed)); }
                throw new Exception(_errMsg);
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------
        [MTFMethod(Description = "Import content from file.",
           DisplayName = "2 - Import Content")]
        [MTFAdditionalParameterInfo(ParameterName = "configFile", Description = "Path to configuration file")]
        [MTFAdditionalParameterInfo(ParameterName = "ExcludeContentFromLogging", Description = "Do not write imported content to logfile")]
        [MTFAdditionalParameterInfo(ParameterName = "IsParameterSet", Description = "Checks if imported data sections and keys match the existing ones.\nIgnored if \"AppendImportData\" is checked!")]
        [MTFAdditionalParameterInfo(ParameterName = "AppendImportData", Description = "Appends import data to existing data. Values for existing keys are updated\nParameter \"IsParameterSet\" is ignored")]
        [MTFAdditionalParameterInfo(ParameterName = "ImportSingleMultipleParameter", Description = "Allows to import just single/multiple section(s) of parameter set.\nIntegrity of containing DataSets is checked")]
        public bool ImportContent(string configFile, bool AppendImportData = false, bool IsParameterSet = false, bool ImportSingleMultipleParameter = false, bool ExcludeContentFromLogging = false)
        {
            //variable for exception text
            string _exceptionText = "";

            //check file extension
            if (Path.GetExtension(configFile) != ".csv") throw new Exception("configuration file is not of type .csv!");

            //check if file exists
            if (File.Exists(configFile))
            {
                // Create new stopwatch.
                Stopwatch stopwatch = new Stopwatch();
                // Begin timing.
                stopwatch.Start();
                //get number of lines
                var _lines = File.ReadAllLines(configFile).Count();
                //read all lines
                string[] _fileData = new string[_lines];
                _fileData = File.ReadAllLines(configFile);

                //find names
                var _names = Array.FindAll(_fileData, x => x.Split(';')[0].Contains(Constants.SectionName));
                var _namesPos = new int[_names.Count()];

                for (int i = 0; i < _names.Count(); i++)
                {
                    _namesPos[i] = Array.FindIndex(_fileData, x => x.Contains(_names[i]));
                }

                //find datasets
                var _datasets = Array.FindAll(_fileData, x => x.Contains(Constants.DataSetItem));
                var _datasetsPos = new int[_datasets.Count()];

                //write logging
                if (_log._loggingActive)
                { _log.WriteToLog("2 - Import Content", LoggingStatus.OK, String.Format("Import data from {4} to {5} -> {0}s: {1} | {2}s: {3}", Constants.SectionName, _names.Count(), Constants.DataSetItem, _datasets.Count(), Path.GetFullPath(configFile), _compName)); }

                for (int i = 0; i < _datasets.Count(); i++)
                {
                    for (int j = 0; j < _fileData.Count(); j++)
                    {
                        if (_fileData[j] == _datasets[i])
                        {
                            if (i != 0)
                            {
                                if (j > _datasetsPos[i - 1])
                                {
                                    _datasetsPos[i] = j;
                                    break;
                                }
                            }
                            else
                            {
                                _datasetsPos[i] = j;
                                break;
                            }
                        }
                    }

                }

                //check if data is found in file
                if (_names.Count() == 0)
                {
                    _exceptionText = String.Format("No data were found in \"{0}\" to import!", configFile);
                    if (_log._loggingActive && !ExcludeContentFromLogging) { _log.WriteToLog("2 - Import Content", LoggingStatus.ERROR, _exceptionText); }
                    throw new Exception(_exceptionText);
                    //return false;
                }

                //create objects to hold data
                Section[] importSections = new Section[_names.Count()];
                DataSet[] importDataSets;

                //fill
                for (int i = 0; i < _names.Count(); i++)
                {
                    //get line with name
                    var _lineContentName = _fileData[_namesPos[i]].Split(';');

                    //make sure line contains name
                    if (_lineContentName[0] == Constants.SectionName)
                    {
                        //get number of datasets for this name
                        int _numDatasets = 0;
                        if (i != _names.Count() - 1)
                        {
                            _numDatasets = _datasetsPos.Count(x => x < _namesPos[i + 1]
                              && _namesPos[i] < x
                              );
                        }
                        else //last name item
                        { _numDatasets = _datasetsPos.Count(x => x > _namesPos[i]); }

                        //create object to hold data
                        importDataSets = new DataSet[_numDatasets];

                        //write logging
                        if (_log._loggingActive)
                        { _log.WriteToLog("2 - Import Content", LoggingStatus.OK, String.Format("{0}: {1}", _lineContentName[1], _numDatasets)); }

                        //fill dataSet
                        int _dataSetNo = 0;
                        for (int j = 0; j < _datasetsPos.Count(); j++)
                        {
                            if (_datasetsPos[j] > _namesPos[i])
                            {
                                //get line with dataSet
                                var _lineContentDataSet = _fileData[_datasetsPos[j]].Split(';');
                                importDataSets[_dataSetNo] = new DataSet();
                                importDataSets[_dataSetNo].Key = _lineContentDataSet[1];
                                importDataSets[_dataSetNo].Value = _lineContentDataSet[2];
                                _dataSetNo++;
                            }

                            if (_dataSetNo == _numDatasets)
                            { break; }
                        }

                        //write data to content
                        importSections[i] = new Section(_lineContentName[1], importDataSets);
                    }
                }

                //append import data
                #region append_import_data
                if (AppendImportData)
                {
                    //write to log
                    if (_log._loggingActive && !ExcludeContentFromLogging) { _log.WriteToLog("2 - Import Content", LoggingStatus.OK, String.Format("Append import data to/update existing data")); }

                    foreach (Section _sectionItem in importSections)
                    {
                        //check if section already exists
                        if (!_content.ContainsKey(_sectionItem.Name))
                        {
                            //create section and
                            _content.Add(_sectionItem.Name, new Dictionary<string, string>());
                            if (_log._loggingActive && !ExcludeContentFromLogging) { _log.WriteToLog("2 - Import Content", LoggingStatus.OK, String.Format("Added new {0}: \"{1}\"", Constants.SectionName, _sectionItem.Name)); }
                            //add all new keys/values
                            foreach (DataSet _dataSetItem in _sectionItem.DataSets)
                            {
                                _content[_sectionItem.Name].Add(_dataSetItem.Key, _dataSetItem.Value);
                                if (_log._loggingActive && !ExcludeContentFromLogging) { _log.WriteToLog("2 - Import Content", LoggingStatus.OK, String.Format("Added new {5}: {0}{3}{1}{4}{2}", _sectionItem.Name, _dataSetItem.Key, _dataSetItem.Value, Constants.Delimiter_1, Constants.Delimiter_2, Constants.DataSetItem)); }
                            }
                        }
                        else
                        {
                            //append key or update value
                            foreach (DataSet _dataSetItem in _sectionItem.DataSets)
                            {
                                if (_content[_sectionItem.Name].ContainsKey(_dataSetItem.Key))
                                {
                                    //update value
                                    _content[_sectionItem.Name][_dataSetItem.Key] = _dataSetItem.Value;
                                    if (_log._loggingActive && !ExcludeContentFromLogging) { _log.WriteToLog("2 - Import Content", LoggingStatus.OK, String.Format("{5} updated: {0}{3}{1}{4}{2}", _sectionItem.Name, _dataSetItem.Key, _dataSetItem.Value, Constants.Delimiter_1, Constants.Delimiter_2, Constants.DataSetItem)); }
                                }
                                else
                                {
                                    //append key/value
                                    _content[_sectionItem.Name].Add(_dataSetItem.Key, _dataSetItem.Value);
                                    if (_log._loggingActive && !ExcludeContentFromLogging) { _log.WriteToLog("2 - Import Content", LoggingStatus.OK, String.Format("Added new {5}: {0}{3}{1}{4}{2}", _sectionItem.Name, _dataSetItem.Key, _dataSetItem.Value, Constants.Delimiter_1, Constants.Delimiter_2, Constants.DataSetItem)); }
                                }
                            }
                        }
                    }
                }
                #endregion
                //parameter set
                #region parameter_set
                else if (IsParameterSet || ImportSingleMultipleParameter)
                {
                    _exceptionText = "";

                    //write to log
                    if (_log._loggingActive && !ExcludeContentFromLogging) { _log.WriteToLog("2 - Import Content", LoggingStatus.OK, String.Format("Import data are parameter set -> check validity")); }

                    //check number of sections
                    #region check no sections
                    //if this is disabled   -> single parts/sections can be updated
                    //if this is enabled    -> all paramters must be in import file
                    if (importSections.Count() != _content.Count() && ImportSingleMultipleParameter == false)
                    {
                        _exceptionText = String.Format("Size of imported parameter set ({0}) does not fit current set size ({1})! Mismatches", importSections.Count(), _content.Count());
                        //find which sections mismatch
                        #region mismatches
                        var _sectionKeysCurrent = _content.Keys.ToArray();
                        var _sectionKeysImport = importSections.Select(x => x.Name).ToArray();
                        var _sectionKeysMismatches = _sectionKeysCurrent.Except(_sectionKeysImport).Concat(_sectionKeysImport.Except(_sectionKeysCurrent)).ToArray();
                        _exceptionText += String.Format(" ({0}):",_sectionKeysMismatches.Count());
                        for (int i = 0; i < _sectionKeysMismatches.Length; i++)
                        {
                            _exceptionText += String.Format(" \"{0}\"", _sectionKeysMismatches[i]);
                        }
                        #endregion                        
                        if (_log._loggingActive && !ExcludeContentFromLogging) { _log.WriteToLog("2 - Import Content", LoggingStatus.ERROR, _exceptionText); }
                        throw new Exception(_exceptionText);
                    }
                    #endregion

                    foreach (Section _sectionItem in importSections)
                    {
                        //check names of sections
                        if (!_content.ContainsKey(_sectionItem.Name))
                        {
                            _exceptionText = String.Format("Current set does not contain section \"{0}\"!", _sectionItem.Name);
                            if (_log._loggingActive && !ExcludeContentFromLogging) { _log.WriteToLog("2 - Import Content", LoggingStatus.ERROR, _exceptionText); }
                            throw new Exception(_exceptionText);
                        }

                        //check number of keys in every section
                        if (_sectionItem.DataSets.Count() != _content[_sectionItem.Name].Count)
                        {
                            _exceptionText = String.Format("Number of data set elements ({0}) in section \"{1}\" of imported parameter set does not fit current section size ({2})! Mismatches", _sectionItem.DataSets.Count(), _sectionItem.Name, _content[_sectionItem.Name].Count);
                            //find which sections mismatch
                            #region mismatches
                            var _dataKeysCurrent = _content[_sectionItem.Name].Keys.ToArray();
                            List<string> _dataKeysImportList = new List<string>();
                            foreach (DataSet _dataSetItem in _sectionItem.DataSets)
                            { _dataKeysImportList.Add(_dataSetItem.Key); }
                            var _dataKeysImport = _dataKeysImportList.ToArray();

                            var _dataKeysMismatches = _dataKeysCurrent.Except(_dataKeysImport).Concat(_dataKeysImport.Except(_dataKeysCurrent)).ToArray();
                            _exceptionText += String.Format(" ({0}):", _dataKeysMismatches.Count());
                            for (int i = 0; i < _dataKeysMismatches.Length; i++)
                            {
                                _exceptionText += String.Format(" \"{0}\"", _dataKeysMismatches[i]);
                            }
                            #endregion 
                            if (_log._loggingActive && !ExcludeContentFromLogging) { _log.WriteToLog("2 - Import Content", LoggingStatus.ERROR, _exceptionText); }
                            throw new Exception(_exceptionText);
                        }
                        //check names of keys
                        else
                        {
                            foreach (DataSet _dataSetItem in _sectionItem.DataSets)
                            {
                                if (!_content[_sectionItem.Name].ContainsKey(_dataSetItem.Key))
                                {
                                    _exceptionText = String.Format("Current section \"{0}\" does not contain data set element \"{1}\"!", _sectionItem.Name, _dataSetItem.Key);
                                    if (_log._loggingActive && !ExcludeContentFromLogging) { _log.WriteToLog("2 - Import Content", LoggingStatus.ERROR, _exceptionText); }
                                    throw new Exception(_exceptionText);
                                }
                            }
                        }
                    }
                    //write to log
                    if (_log._loggingActive && !ExcludeContentFromLogging) { _log.WriteToLog("2 - Import Content", LoggingStatus.OK, String.Format("Import data parameter set is valid")); }
                }
                #endregion

                if (!AppendImportData)
                {
                    //fill data to vars
                    _content = new Dictionary<String, Dictionary<String, String>>();
                    if (importSections != null)
                    {
                        foreach (Section section in importSections)
                        {
                            _content.Add(section.Name, new Dictionary<string, string>());

                            if (section.DataSets != null)
                            {
                                foreach (DataSet dataset in section.DataSets)
                                {
                                    _content[section.Name].Add(dataset.Key, dataset.Value);
                                    //write logging
                                    if (_log._loggingActive && !ExcludeContentFromLogging) { _log.WriteToLog("2 - Import Content", LoggingStatus.OK, String.Format("{0}{3}{1}{4}{2}", section.Name, dataset.Key, dataset.Value, Constants.Delimiter_1, Constants.Delimiter_2)); }
                                }
                            }
                        }
                    }
                }
                // Stop timing.
                stopwatch.Stop();
                //write logging
                if (_log._loggingActive) { _log.WriteToLog("2 - Import Content", LoggingStatus.OK, String.Format("Import done -> Duration: {0}", stopwatch.Elapsed)); }
                return true;
            }
            else
            { throw new Exception("File " + configFile + " does not exist!"); }
        }

        #endregion

        #region 9 (Logging)

        [MTFMethod(Description = "Logging for Global Data Container.\n"
                                + "Settings (except DeleteCompressed) are taken over in \"Activate\" option.",
                    DisplayName = "9 - Logging")]
        [MTFAdditionalParameterInfo(ParameterName = "Option", Description = "Logging options for Global Data Container")]
        [MTFAllowedParameterValue("Option", "Activate", "Activate")]
        [MTFAllowedParameterValue("Option", "Deactivate", "Deactivate")]
        [MTFAllowedParameterValue("Option", "Compress", "Compress")]
        [MTFAdditionalParameterInfo(ParameterName = "LogFilePath", Description = "Path to save logfile(s)", DefaultValue = @".\_gdclog\")]
        [MTFAdditionalParameterInfo(ParameterName = "MaxLogFiles", Description = "Max. number of logfiles for this logging session.\nOldest will be deleted if max number is reached!\n0: no limit", DefaultValue = "10")]
        [MTFAdditionalParameterInfo(ParameterName = "MaxLogFileSizeMB", Description = "Max. size in MegaBytes per logfile for this logging session.\n0: no limit", DefaultValue = "10")]
        [MTFAdditionalParameterInfo(ParameterName = "DeleteCompressed", Description = "Delete compressed log files")]
        [MTFAdditionalParameterInfo(ParameterName = "ExcludeGetSetFromLogging", Description = "\"Get Value\" and \"Set Value\" are not logged")]
        public string GDCLogging(string Option, string LogFilePath = @".\_gdclog\", int MaxLogFiles = 0, int MaxLogFileSizeMB = 0, bool DeleteCompressed = false, bool ExcludeGetSetFromLogging = false)
        {
            string _outputString = "Unknown Option";
            switch (Option)
            {
                case "Activate":
                    _log._maxLogFiles = MaxLogFiles;
                    _log._maxLogFileSizeMB = MaxLogFileSizeMB;
                    _excludeGetSetFromLog = ExcludeGetSetFromLogging;
                    if (!string.IsNullOrEmpty(LogFilePath)) { _log._logPath = LogFilePath; }
                    _log.ActivateLogging(true, true);
                    //write logging
                    if (_log._loggingActive) { _log.WriteToLog("9 - Logging", LoggingStatus.OK, String.Format("Logging activated -> MaxLogFiles: {0} | MaxLogFileSizeMB: {1} | ExcludeGetSetFromLogging: {2}", _log._maxLogFiles, _log._maxLogFileSizeMB, _excludeGetSetFromLog)); }
                    _outputString = "Activate";
                    break;
                case "Deactivate":
                    //write logging
                    _log._logStopWatch.Stop();
                    _log._logStopTime = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff");
                    if (_log._loggingActive) { _log.WriteToLog("9 - Logging", LoggingStatus.OK, String.Format("Logging deactivated (Start: {0} Stop: {1} Duration: {2})", _log._logStartTime, _log._logStopTime, _log._logStopWatch.Elapsed)); }
                    _log.DeactivateLogging(true);
                    _outputString = "Deactivate";
                    break;
                case "Compress":
                    //write logging
                    if (_log._loggingActive) { _log.WriteToLog("9 - Logging", LoggingStatus.OK, "Compressing logfiles to: " + Path.GetFullPath(_log._logPath + _log._logFilePrefix + _compName + _log._logFileExtension) + ".zip"); }
                    string compOut = _log.CompressLogfiles(DeleteCompressed);
                    //write logging
                    if (_log._loggingActive) { _log.WriteToLog("9 - Logging", LoggingStatus.OK, "Logfile(s) compressed to " + compOut); }
                    _outputString = "Compress";
                    break;
            }

            return _outputString;
        }

        #endregion

        /** \brief	Retrieves a list of all names of the defined sections and its datasets
        *	\note	Intended for usage by MTF to populate drop-down lists of set / get activities.
        */
        [MTFValueListGetterMethod(SubListSeparator = ".")]
        public List<Tuple<String, Object>> DataSetNames()
        {
            var dataSetNames = new List<Tuple<String, Object>>();
            StringBuilder sb = new StringBuilder();

            // iterate over the sections
            foreach (KeyValuePair<String, Dictionary<String, String>> section in _content)
            {

                if (section.Value.Count > 0)
                {
                    // iterate over the datasets within the  section
                    foreach (KeyValuePair<String, String> dataSet in section.Value)
                    {
                        sb.Clear();
                        sb.Append(section.Key);
                        sb.Append(".");
                        sb.Append(dataSet.Key);

                        String[] dataSetName = new String[2];
                        dataSetName[0] = section.Key;
                        dataSetName[1] = dataSet.Key;

                        dataSetNames.Add(new Tuple<String, Object>(sb.ToString(), dataSetName));
                    }
                }
                else
                {
                    sb.Clear();
                    sb.Append(section.Key);
                    sb.Append(".");
                    sb.Append(string.Empty);

                    String[] dataSetName = new String[2];
                    dataSetName[0] = section.Key;
                    dataSetName[1] = string.Empty;

                    dataSetNames.Add(new Tuple<String, Object>(sb.ToString(), dataSetName));
                }
            }

            return dataSetNames;
        }

        private Dictionary<String, Dictionary<String, String>> _content;
        private Logging _log;
        private string _compName;
        private bool _excludeGetSetFromLog = false;
    };


}
