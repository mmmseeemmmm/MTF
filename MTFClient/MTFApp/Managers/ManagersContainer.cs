using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTFApp.Managers.Components;
using MTFApp.Managers.Sequence;

namespace MTFApp.Managers
{
    static class ManagersContainer
    {
        private static List<ManagerBase> managers = null;

        public static void Init()
        {
            managers = new List<ManagerBase>
            {
                new ComponentsManager(),
                //new SequenceManager(),
            };

            List<Task> initTasks = new List<Task>();
            foreach (var manager in managers)
            {
                initTasks.Add(Task.Run(() =>
                {
                    manager.Init();
                }));
            }

            Task.WaitAll(initTasks.ToArray());
        }

        public static T Get<T>() where T : class => managers.FirstOrDefault(m => m.GetType() == typeof(T)) as T;
    }
}
