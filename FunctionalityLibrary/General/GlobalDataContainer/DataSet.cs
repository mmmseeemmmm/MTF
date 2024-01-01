using AutomotiveLighting.MTFCommon;
using General;
using System;

namespace AutomotiveLighting.MTF.GlobalDataContainer
{
    /** \class DataSet
*	\brief The class \b %DataSet represents an individual data item that can be stored in the container.
*/
    [MTFKnownClassAttribute]
    public class DataSet
    {

        /** \brief Initializes a new instance of the \p DataSet class.
        *
        * \param [in] key			    The name of the dataset
        * \param [in] dataType			The datatype of the dataset's value
        * \param [in] initialValue		The value of the dataset
        */
        public DataSet(String key, DATA_TYPE dataType, String initialValue)
        {
            _key = key;
            _dataType = dataType;
            _value = initialValue;
        }

        /** \brief Initializes a new instance of the \p DataSet class.
        */
        public DataSet() { }

        public String Key
        {
            get { return this._key; }
            set { this._key = value; }
        }

        /*public DATA_TYPE DataType
        {
            get { return this._dataType; }
            set { this._dataType = value; }
        }*/

        public String Value
        {
            get { return this._value; }
            set { this._value = value; }
        }

        private String _key;
        private DATA_TYPE _dataType;
        private String _value;

    };
}
