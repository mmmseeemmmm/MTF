using System.Collections.Generic;
using System.ServiceModel;
using MTFClientServerCommon;
using MTFClientServerCommon.Services;

namespace MTFApp.ServerService.Components
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    class ComponentsServiceCallbackHandler : IComponentsServiceCallback
    {
        public delegate void MonsterClassLoadedEventHandler();
        public event MonsterClassLoadedEventHandler MonsterClassLoaded;

        public event ComponentConfigQuestionEventHandler ComponentConfigQuestion;
        public delegate string ComponentConfigQuestionEventHandler(string header, string text, SequenceMessageType messageType, SequenceMessageDisplayType displayType, IList<string> items);

        public void OnMonsterClassLoaded()
        {
            var handler = MonsterClassLoaded;
            handler?.Invoke();
        }

        public string OnComponentConfigQuestion(string header, string text, SequenceMessageType messageType, SequenceMessageDisplayType displayType, IList<string> items)
        {
            var handler = ComponentConfigQuestion;
            return handler?.Invoke(header, text, messageType, displayType, items);
        }
    }
}
