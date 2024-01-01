using MTFApp.UIHelpers;
using MTFClientServerCommon;
using MTFClientServerCommon.Mathematics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MTFApp.ServerService;
using MTFApp.ServerService.Components;
using MTFClientServerCommon.Constants;

namespace MTFApp.EditorTest
{
    class EditorTestPresenter : PresenterBase
    {
        public EditorTestPresenter()
        {
            //termValue = new AddTerm { Value1 = new SubtractTerm { Value1 = new ConstantTerm { Value = 100 }, Value2 = new ConstantTerm { Value = 32 } }, Value2 = new ConstantTerm { Value = 58 } };

            termValue = new AddTerm() { Value1 = new ConstantTerm(typeof(Int32)) { Value = 10 }, Value2 = new ConstantTerm(typeof(int)) { Value = 20 } };
            GenericClassInfo gci = ServiceClientsContainer.Get<ComponentsClient>().GetClassInfo("MTFTestingLibrary.TestSettings");
            GenericClassInfo gci2 = ServiceClientsContainer.Get<ComponentsClient>().GetClassInfo("MTFTestingLibrary.SubSettings");
            termValue2 = new ActivityResultTerm()
            {
                PropertyPath = new MTFObservableCollection<MTFClientServerCommon.GenericPropertyInfo>()
                {
                    gci.Properties[7], new GenericPropertyIndexer(gci.Properties[7]){ Index = 6}, gci2.Properties[1]
                },
                Value = new MTFSequenceActivity() { ReturnType = "MTFTestingLibrary.TestSettings" }
            };
            stringListValue = new List<string> { "string 1", "STRING 2", "Long string string string string string string" };
            //intListValue = new List<int>() { 1 };
            byteListValue = new List<byte>() { 1 };
            boolListValue = new List<bool>() { true };
            boolArrayValue = new bool[] { true };
            intValue = 5;
            dataTable = GetDataTable();
            //createBugTestingSequence();
        }

        private void createBugTestingSequence()
        {
            var sequenceEditorPresenter = new SequenceEditor.SequenceEditorPresenter();
            var sequence = createSequence("GeneratedSequence", sequenceEditorPresenter);

            var bugfixTestingClass = addClassInfo("MTFTestingLibrary.BugFixTesting", sequenceEditorPresenter);
            string bugfixTestingClassAlias = sequenceEditorPresenter.SelectedSequenceClassInfo.Alias;

            sequence.MTFSequenceActivities.Add(createActivity(bugfixTestingClass, "ConsumeEnumTest", bugfixTestingClassAlias, "Enum Test"));
            sequence.MTFSequenceActivities.Add(createActivity(bugfixTestingClass.Methods[2], bugfixTestingClassAlias, "act2"));
            sequence.MTFSequenceActivities.Add(createActivity(bugfixTestingClass.Methods[3], bugfixTestingClassAlias, "act3"));

            saveSequence(sequence);
        }

        private MTFSequence createSequence(string sequenceName, SequenceEditor.SequenceEditorPresenter sequenceEditorPresenter)
        {
            var sequence = sequenceEditorPresenter.Sequence;
            sequence.Name = sequenceName;
            sequence.MTFSequenceClassInfos = new MTFObservableCollection<MTFSequenceClassInfo>();
            sequence.MTFSequenceActivities = new MTFObservableCollection<MTFSequenceActivity>();
            sequence.ActivitiesByCall = new MTFObservableCollection<MTFSequenceActivity>();
            sequence.MTFVariables = new MTFObservableCollection<MTFVariable>();

            return sequenceEditorPresenter.Sequence;
        }

        private MTFClassInfo addClassInfo(string fullName, SequenceEditor.SequenceEditorPresenter sequenceEditorPresenter)
        {
            foreach (var category in sequenceEditorPresenter.ClassCategories)
            {
                var classInfo = category.Classes.FirstOrDefault(ci => ci.FullName == fullName);
                if (classInfo != null)
                {
                    sequenceEditorPresenter.AddClassToSequence.Execute(classInfo);
                    //map mtf class to instance
                    sequenceEditorPresenter.SelectedSequenceClassInfo = sequenceEditorPresenter.Sequence.MTFSequenceClassInfos[0];
                    sequenceEditorPresenter.SelectedSequenceClassInfo.MTFClassInstanceConfiguration = sequenceEditorPresenter.ClassInstanceConfiurations[0];

                    return classInfo;
                }
            }

            return null;
        }

        private object GetDataTable()
        {
            //DataTable dt = new DataTable();
            //dt.Columns.Add("IntColumn", typeof(int));
            //dt.Columns.Add("BoolColumn", typeof(bool));
            //dt.Columns.Add("StringColumn", typeof(string));

            //dt.Rows.Add(1, true, "first");
            //dt.Rows.Add(2, false, "second");
            //dt.Rows.Add(3, true, "third");
            //return dt;
            List<List<object>> table = new List<List<object>>();
            for (int i = 0; i < 5; i++)
            {
                var row = new List<object>() { 1, 2, 3 };
                table.Add(row);
            }
            return table;
        }

        private void saveSequence(MTFSequence sequence)
        {
            try
            {
                MTFClient.SaveSequence(sequence, sequence.Name + BaseConstants.SequenceExtension, false, EnvironmentHelper.UserName);
            }
            catch (Exception ex)
            {
                MTFMessageBox.Show("MTF Error", ex.Message, MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
            }
        }

        private MTFSequenceActivity createActivity(MTFClassInfo classInfo, string methodName, string classAlias, string activityName)
        {
            return createActivity(classInfo.Methods.First(m => m.Name == methodName), classAlias, activityName);
        }

        private MTFSequenceActivity createActivity(MTFMethodInfo mtfMethod, string classAlias, string activityName)
        {
            var mtfmethodParameters = new ObservableCollection<MTFParameterValue>();
            foreach (MTFParameterInfo paramInfo in mtfMethod.Parameters)
            {
                mtfmethodParameters.Add(new MTFParameterValue(paramInfo));
            }

            return new MTFSequenceActivity
            {
                IsActive = true,
                ActivityName = activityName,
                MTFClassAlias = classAlias,

                MTFMethodName = mtfMethod.Name,
                MTFMethodDisplayName = mtfMethod.DisplayName,
                MTFMethodDescription = mtfMethod.Description,
                ReturnType = mtfMethod.ReturnType,
                MTFParameters = mtfmethodParameters,
            };
        }

        private object dataTable;

        public object DataTable
        {
            get { return dataTable; }
            set { dataTable = value; }
        }

        //public string DataTableType { get { return typeof(MTFCommon.MTFTable).FullName; } }


        private string stringValue;
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; NotifyPropertyChanged(); }
        }

        private bool boolValue;
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; NotifyPropertyChanged(); }
        }

        private object intValue;
        public object IntValue
        {
            get { return intValue; }
            set { intValue = value; NotifyPropertyChanged(); }
        }


        private uint uintValue;
        public uint UIntValue
        {
            get { return uintValue; }
            set { uintValue = value; NotifyPropertyChanged(); }
        }

        private float floatValue;
        public float FloatValue
        {
            get { return floatValue; }
            set { floatValue = value; NotifyPropertyChanged(); }
        }

        private sbyte sbyteValue;
        public sbyte SByteValue
        {
            get { return sbyteValue; }
            set { sbyteValue = value; NotifyPropertyChanged(); }
        }

        private byte byteValue;
        public byte ByteValue
        {
            get { return byteValue; }
            set { byteValue = value; NotifyPropertyChanged(); }
        }

        private Int16 int16Value;
        public Int16 Int16Value
        {
            get { return int16Value; }
            set { int16Value = value; NotifyPropertyChanged(); }
        }

        private UInt16 uint16Value;
        public UInt16 UInt16Value
        {
            get { return uint16Value; }
            set { uint16Value = value; NotifyPropertyChanged(); }
        }

        private Int64 int64Value;
        public Int64 Int64Value
        {
            get { return int64Value; }
            set { int64Value = value; NotifyPropertyChanged(); }
        }

        private UInt64 uint64Value;
        public UInt64 UInt64Value
        {
            get { return uint64Value; }
            set { uint64Value = value; NotifyPropertyChanged(); }
        }

        private double doubleValue;
        public double DoubleValue
        {
            get { return doubleValue; }
            set { doubleValue = value; NotifyPropertyChanged(); }
        }

        private decimal decimalValue;
        public decimal DecimalValue
        {
            get { return decimalValue; }
            set { decimalValue = value; NotifyPropertyChanged(); }
        }

        private Term termValue;
        public Term TermValue
        {
            get { return termValue; }
            set { termValue = value; NotifyPropertyChanged(); }
        }
        private Term termValue2;
        public Term TermValue2
        {
            get { return termValue2; }
            set { termValue2 = value; NotifyPropertyChanged(); }
        }
        private Term termValue3;
        public Term TermValue3
        {
            get { return termValue3; }
            set { termValue3 = value; NotifyPropertyChanged(); }
        }

        private Term termValue4;
        public Term TermValue4
        {
            get { return termValue4; }
            set { termValue4 = value; NotifyPropertyChanged(); }
        }

        private object stringListValue;
        public object StringListValue
        {
            get { return stringListValue; }
            set { stringListValue = value; NotifyPropertyChanged(); }
        }
        public string StringListType { get { return typeof(List<string>).FullName; } }

        private object intListValue;
        public object IntListValue
        {
            get { return intListValue; }
            set { intListValue = value; NotifyPropertyChanged(); }
        }
        public string IntListType { get { return typeof(List<int>).FullName; } }

        private object byteListValue;
        public object ByteListValue
        {
            get { return byteListValue; }
            set { byteListValue = value; NotifyPropertyChanged(); }
        }
        public string ByteListType { get { return typeof(List<byte>).FullName; } }

        private object boolListValue;
        public object BoolListValue
        {
            get { return boolListValue; }
            set { boolListValue = value; NotifyPropertyChanged(); }
        }
        public string BoolListType { get { return typeof(List<bool>).FullName; } }

        private object knowObjectListValue;
        public object KnownObjectListValue
        {
            get { return knowObjectListValue; }
            set { knowObjectListValue = value; NotifyPropertyChanged(); }
        }

        public string KnownObjectListValueListType { get { return "System.Collections.Generic.List`1[[MTFTestingLibrary.SubSettings2, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]"; } }
        //public string KnownObjectListValueListType { get { return "System.Collections.Generic.List`1[[System.Collections.Generic.List`1[[MTFTestingLibrary.SubSettings2, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]"; } }

        private object unknowObjectListValue;
        public object UnKnownObjectListValue
        {
            get { return unknowObjectListValue; }
            set { unknowObjectListValue = value; NotifyPropertyChanged(); }
        }
        public string UnKnownObjectListValueListType { get { return "System.Collections.Generic.List`1[[MTFTestingLibrary.SubSettings, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]"; } }

        private object boolArrayValue;
        public object BoolArrayValue
        {
            get { return boolArrayValue; }
            set { boolArrayValue = value; NotifyPropertyChanged(); }
        }
        public string BoolArrayListType { get { return typeof(bool[]).FullName; } }

        private MTFStringFormat stringFormat;

        public MTFStringFormat StringFormat
        {
            get { return stringFormat; }
            set { stringFormat = value; }
        }


        private object listList1;
        public object ListList1
        {
            get { return listList1; }
            set
            {
                listList1 = value;
                NotifyPropertyChanged();
            }
        }
        public string ListList1Type { get { return typeof(List<List<int>>).FullName; } }

        private object listList2;
        public object ListList2
        {
            get { return listList2; }
            set
            {
                listList2 = value;
                NotifyPropertyChanged();
            }
        }
        public string ListList2Type { get { return typeof(List<List<List<bool>>>).FullName; } }

        private object listList3;
        public object ListList3
        {
            get { return listList3; }

            set
            {
                listList3 = value;
                NotifyPropertyChanged();
            }
        }
        public string ListList3Type { get { return "System.Collections.Generic.List`1[[System.Collections.Generic.List`1[[MTFTestingLibrary.SubSettings2, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]"; } }

        private object arrayArray1;
        public object ArrayArray1
        {
            get { return arrayArray1; }
            set
            {
                arrayArray1 = value;
                NotifyPropertyChanged();
            }
        }
        public string ArrayArray1Type { get { return typeof(int[][][]).FullName; } }

        private object arrayArray2;
        public object ArrayArray2
        {
            get { return arrayArray2; }
            set
            {
                arrayArray2 = value;
                NotifyPropertyChanged();
            }
        }
        public string ArrayArray2Type { get { return typeof(bool[][]).FullName; } }

        private object arrayArray3;
        public object ArrayArray3
        {
            get { return arrayArray3; }
            set
            {
                arrayArray3 = value;
                NotifyPropertyChanged();
            }
        }
        public string ArrayArray3Type { get { return "MTFTestingLibrary.SubSettings[][][]"; } }

        private object listArray;
        public object ListArray
        {
            get { return listArray; }
            set
            {
                listArray = value;
                NotifyPropertyChanged();
            }
        }
        public string ListArrayType { get { return typeof(List<int[]>).FullName; } }

        private object arrayList;
        public object ArrayList
        {
            get { return arrayList; }
            set
            {
                arrayList = value;
                NotifyPropertyChanged();
            }
        }
        public string ArrayListType { get { return typeof(List<int>[]).FullName; } }


        private object obj;

        public object Obj
        {
            get { return obj; }
            set { obj = value; NotifyPropertyChanged(); }
        }

        public string ObjTypeName { get { return "MTFTestingLibrary.SubSettings2"; } }

        private object unknownobj;

        public object UnknownObj
        {
            get { return unknownobj; }
            set { unknownobj = value; NotifyPropertyChanged(); }
        }

        public string UnknownObjTypeName { get { return "MTFTestingLibrary.SubSettings"; } }

        private object knownobj;

        public object KnownObj
        {
            get { return knownobj; }
            set { knownobj = value; NotifyPropertyChanged(); }
        }

        public string KnownObjTypeName { get { return "MTFTestingLibrary.SubSettings3"; } }

        private object bitmap;

        public object Bitmap
        {
            get { return bitmap; }
            set { bitmap = value; NotifyPropertyChanged(); }
        }

        public string BitmapTypeName { get { return typeof(System.Drawing.Bitmap).FullName; } }


        private object commonObj;

        public object CommonObj
        {
            get { return commonObj; }
            set { commonObj = value; NotifyPropertyChanged(); }
        }

        public string CommonObjTypeName { get { return typeof(AutomotiveLighting.MTFCommon.Types.MeasuredData).FullName; } }

        private readonly LocalizedTestPresenter localizedTestPresenter = new LocalizedTestPresenter();

        public LocalizedTestPresenter LocalizedTestPresenter
        {
            get { return localizedTestPresenter; }
        }

        private readonly  IconsPresenter iconsPresenter = new IconsPresenter();
        public IconsPresenter IconsPresenter => iconsPresenter;

        private Term emptyTerm = new EmptyTerm();

        private Term constantTerm = new ConstantTerm(typeof(int)) { Value = 123456 };

        private Term variableTerm = new VariableTerm() { MTFVariable = new MTFVariable() { Name = "MyVariable", TypeName = "System.Boolean" } };

        private Term activityResultTerm = new ActivityResultTerm() { Value = new MTFSubSequenceActivity() { ActivityName = "MySubSequence" } };

        private Term termWrapper = new TermWrapper() { Value = new ConstantTerm(typeof(int)) { Value = 123456 } };

        private Term binaryTerm = new AddTerm() { Value1 = new ConstantTerm(typeof(int)) { Value = 1 }, Value2 = new ConstantTerm(typeof(int)) { Value = 2 } };

        private Term unaryTerm = new NotTerm() { Value = new AddTerm() { Value1 = new ConstantTerm(typeof(int)) { Value = 1 }, Value2 = new ConstantTerm(typeof(int)) { Value = 2 } } };

        private Term binaryTerm2 = new LessThanTerm() { Value1 = new EmptyTerm(), Value2 = new EmptyTerm() };

        private Term binaryTerm3 = new AddTerm() { Value1 = new EmptyTerm(), Value2 = new AddTerm() { Value1 = new ConstantTerm(typeof(int)) { Value = 1 }, Value2 = new EmptyTerm() } };

        //private Term newTerm;

        public Term NewTerm
        {
            get { return emptyTerm; }
            set { emptyTerm = value; }
        }

        private List<string> tagList;

        public List<string> TagList
        {
            get { return tagList; }
            set
            {
                tagList = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand AddWarningCommand
        {
            get { return new Command(AddWarning); }
        }

        public ICommand RemoveWarningCommand
        {
            get { return new Command(RemoveWarning); }
        }

        private Queue<WarningMessage> warnings = new Queue<WarningMessage>();
        private void AddWarning()
        {
            var warning = new WarningMessage { Message = "warning asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf asjf aofis aodi fdi odif asoif difa diofa dsfiosd fiosdf" + warnings.Count };
            warnings.Enqueue(warning);
            Warning.Add(warning);
        }

        private void RemoveWarning()
        {
            if (warnings.Count > 0)
            {
                var warning = warnings.Dequeue();
                Warning.Remove(warning.Id);
            }
        }

    }
    public class SubSettings
    {
        //[MTFAllowedPropertyValue("sub item 1", 1)]
        //[MTFAllowedPropertyValue("sub item 2", 2)]
        public int subI1 { get; set; }
        public int subI2 { get; set; }

        public string subS1 { get; set; }
    }
}
