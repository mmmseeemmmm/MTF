using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;
using MTFCore.Services;
using MTFCore.Services.Components;

namespace MTFCore.Managers.Components
{
    class ComponentsHandler
    {
        private static readonly string disabledDriverSuffix = "disabled";
        private static readonly List<string> dontLoadAsmFromFileLoadItFormDomain = new List<string> { "MTFCommon.dll" };
        private readonly Dictionary<string, Assembly> loadedAssemblies;
        private List<MTFClassCategory> classCategories = new List<MTFClassCategory>();

        public ComponentsHandler()
        {
            AvailableClasses = new List<GenericClassInfo>();
            AvailableMonsterClasses = new List<MTFClassInfo>();
            loadedAssemblies = new Dictionary<string, Assembly>();
        }

        public List<MTFClassInfo> AvailableMonsterClasses { get; }
        public List<GenericClassInfo> AvailableClasses { get; }

        public IEnumerable<MTFClassCategory> ClassCategories => classCategories;

        public bool IsAssemblyLoaded(string asmName) => loadedAssemblies.ContainsKey(asmName);
        public Assembly GetAssembly(string asmName) => loadedAssemblies[asmName];

        public void AddAssemblyFromFile(string asmFullName)
        {
            var loadedAsm = LoadAssemblyFromFile(asmFullName, true);
            var dependencies = loadedAsm.GetReferencedAssemblies();
            foreach (var dependency in dependencies)
            {
                if (!loadedAssemblies.ContainsKey(dependency.Name + ".dll"))
                {
                    var name = Path.Combine(Path.GetDirectoryName(asmFullName), dependency.Name + ".dll");
                    if (File.Exists(name))
                    {
                        AddAssemblyFromFile(name);
                    }
                    else
                    {
                        var asm = Assembly.Load(dependency.FullName);
                        AddAssembly(asm);
                    }
                }
            }
            AddAssembly(loadedAsm);
        }

        public void ReadAssemblies()
        {
            CreateAssemblyParsingDomain();

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                AddAssemblyClassInfos(asm);
            }
            ReadAssemblies(BaseConstants.AssembliesPath);

            UnloadAssemblyParsingDomain();

            OrderClassCategories();

            ServiceContainer.Get<ComponentsService>().RaiseOnMonsterClassLoaded();
        }

        private void ReadAssemblies(string asmPath)
        {
            if (Directory.Exists(asmPath))
            {
                foreach (string file in Directory.GetFiles(asmPath, "*.dll"))
                {
                    try
                    {
                        AddAssemblyClassInfos(file);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error during loading assembly " + file + " Exception: " + e.Message);
                    }
                }

                foreach (string directory in Directory.GetDirectories(asmPath))
                {
                    if (!directory.EndsWith(disabledDriverSuffix, StringComparison.InvariantCultureIgnoreCase))
                    {
                        ReadAssemblies(directory);
                    }
                }
            }
        }

        private static Assembly LoadAssemblyFromFile(string asmFullName, bool loadDependencies)
        {
            if (dontLoadAsmFromFileLoadItFormDomain.Contains(Path.GetFileName(asmFullName)))
            {
                return AppDomain.CurrentDomain.GetAssemblies().First(a => a.ManifestModule.Name == Path.GetFileName(asmFullName));
            }

            return loadDependencies ? Assembly.LoadFrom(asmFullName) : Assembly.LoadFile(asmFullName);
        }

        private static AppDomain assemblyParsingDomain;

        private void AddAssemblyClassInfos(string asmFullName)
        {
            Type assemblyWalkerType = typeof(AssemblyWalker);
            var assemblyWalker = (AssemblyWalker)assemblyParsingDomain.CreateInstanceAndUnwrap(assemblyWalkerType.Assembly.FullName, assemblyWalkerType.FullName);

            assemblyWalker.ProcessAssembly(asmFullName);
            AddAssemblyWalkerResult(assemblyWalker);
        }

        private static void UnloadAssemblyParsingDomain() => AppDomain.Unload(assemblyParsingDomain);
        private static void CreateAssemblyParsingDomain() => assemblyParsingDomain = AppDomain.CreateDomain("AssemblyParsingDomain");

        private void AddAssemblyClassInfos(Assembly assembly)
        {
            var assemblyWalker = new AssemblyWalker();
            assemblyWalker.ProcessAssembly(assembly);
            AddAssemblyWalkerResult(assemblyWalker);
        }

        private static readonly object addAssemblyWalkerResultLock = new object();
        private void AddAssemblyWalkerResult(AssemblyWalker assemblyWalker)
        {
            lock (addAssemblyWalkerResultLock)
            {
                AvailableClasses.AddRange(assemblyWalker.AvailableClasses);
                var newMonsterClasses = assemblyWalker.AvailableMonsterClasses;
                AvailableMonsterClasses.AddRange(newMonsterClasses);

                foreach (var classInfo in newMonsterClasses)
                {
                    foreach (var categoryName in classInfo.Categories)
                    {
                        var category = classCategories.FirstOrDefault(x => x.Name == categoryName);
                        if (category != null)
                        {
                            category.Classes.Add(classInfo);
                        }
                        else
                        {
                            classCategories.Add(new MTFClassCategory
                            {
                                Name = categoryName,
                                Classes = new MTFObservableCollection<MTFClassInfo> { classInfo }
                            });
                        }
                    }
                }
            }
        }

        private void OrderClassCategories()
        {
            classCategories = classCategories.OrderBy(c => c.Name).ToList();
            foreach (var category in classCategories)
            {
                category.Classes = new ObservableCollection<MTFClassInfo>(category.Classes.OrderBy(c => c.Name));
            }
        }

        private void AddAssembly(Assembly assembly) => loadedAssemblies[assembly.ManifestModule.Name] = assembly;
    }
}
