using AutomotiveLighting.MTFCommon;
using System;

namespace AutomotiveLighting.MTF.GlobalDataContainer
{
    /** \class Section
*	\brief The class \b %Section is used to group multiple datasets in the container.
*/
    [MTFKnownClassAttribute]
    public class Section
    {
        /** \brief Initializes a new instance of the \p Section class.
        *
        * \param [in] name			    The name of the section
        * \param [in] dataSets			an array of datasets in the section
        */
        public Section(String name, DataSet[] dataSets)
        {
            _name = name;
            _dataSets = dataSets;
        }

        /** \brief Initializes a new instance of the \p Section class.
        */
        public Section() { }


        public String Name
        {
            get { return this._name; }
            set { this._name = value; }
        }

        public DataSet[] DataSets
        {
            get { return this._dataSets; }
            set { this._dataSets = value; }
        }

        private String _name;
        private DataSet[] _dataSets;
    };
}
