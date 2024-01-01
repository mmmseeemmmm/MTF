using System;
using System.Collections.Generic;
using AutomotiveLighting.MTFCommon;
using MTFClientServerCommon;
using MTFClientServerCommon.Mathematics;
using MTFActivityResult = MTFClientServerCommon.MTFActivityResult;

namespace MTFCore.Managers.Components
{
    internal class ComponentsManager : ManagerBase
    {
        private readonly ComponentsHandler componentsHandler = new ComponentsHandler();
        private Invoker invoker;
        
        public ComponentsManager()
        {
            invoker = new Invoker(componentsHandler);
        }

        public override void Init()
        {
            base.Init();
            ReadAssemblies();
            //Task.Run(() => ReadAssemblies());
        }

        #region Components hanler calls
        private void ReadAssemblies() => componentsHandler.ReadAssemblies();

        public List<GenericClassInfo> AvailableClasses => componentsHandler.AvailableClasses;

        public List<MTFClassInfo> AvailableMonsterClasses => componentsHandler.AvailableMonsterClasses;

        public IEnumerable<MTFClassCategory> ClassCategories => componentsHandler.ClassCategories;


        #endregion Components hanler calls

        #region Invoker calls
        public object CreateInstance(MTFClassInstanceConfiguration classInstanceConfiguration) => invoker.CreateInstance(classInstanceConfiguration);

        public Type GetType(MTFClassInfo classInfo) => invoker.GetType(classInfo);

        public object Cast(IParameterValue value, MTFSequenceActivity currentActivity, MTFActivityResult activityResult) => invoker.Cast(value, currentActivity, activityResult);

        public object Cast(IParameterValue value, MTFSequenceActivity currentActivity, MTFActivityResult activityResult, Func<Term, MTFSequenceActivity, MTFActivityResult, ScopeData, object> evaluateTerm, ScopeData scope) =>
            invoker.Cast(value, currentActivity, activityResult, evaluateTerm, scope);

        public IParameterHelperClass CreateHelperClassInstance(string helperClassName, string assemblyFullName) => invoker.CreateHelperClassInstance(helperClassName, assemblyFullName);

        #endregion Invoker calls

    }
}
