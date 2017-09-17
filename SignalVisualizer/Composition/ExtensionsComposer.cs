using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SignalVisualizer.Composition
{
    public class ExtensionsComposer
    {
        private readonly object syncRoot = new object();
        private readonly List<PathLookupInfo> paths = new List<PathLookupInfo>();

        public void AddPathLookup(string path)
        {
            lock (syncRoot)
                paths.Add(new PathLookupInfo(path));
        }

        public void AddPathLookup(string path, bool includeSubDirectories, params string[] searchPatterns)
        {
            lock (syncRoot)
                paths.Add(new PathLookupInfo(path, includeSubDirectories, searchPatterns));
        }

        public void AddPathLookup(PathLookupInfo path)
        {
            lock (syncRoot)
                paths.Add(path);
        }

        private CompositionContainer container;

        public void Initialize()
        {
            Initialize(null);
        }

        public void Initialize(IExceptionReporter reporter)
        {
            var catalogs = new List<ComposablePartCatalog>();

            catalogs.AddRange(GetAssemblies(reporter));

            var aggregateCatalog = new AggregateCatalog(catalogs);
            container = new CompositionContainer(aggregateCatalog);
        }

        public Task InitializeAsync()
        {
            return InitializeAsync(null);
        }

        public Task InitializeAsync(IExceptionReporter reporter)
        {
            return Task.Run(() => Initialize(reporter));
        }

        public T[] Compose<T>()
        {
            return Compose<T>(null);
        }

        public Task<T[]> ComposeAsync<T>()
        {
            return ComposeAsync<T>(null);
        }

        public Task<T[]> ComposeAsync<T>(IExceptionReporter errorReporter)
        {
            return Task.Factory.StartNew(() => Compose<T>(errorReporter));
        }

        public T[] Compose<T>(IExceptionReporter reporter)
        {
            try
            {
                return container.GetExportedValues<T>().ToArray();
            }
            catch (Exception ex)
            {
                if (reporter != null)
                {
                    reporter.Report(ex);
                    return new T[0];
                }

                throw;
            }
        }

        private IEnumerable<string> RetrieveFileList()
        {
            PathLookupInfo[] coldPaths;

            lock (syncRoot)
                coldPaths = paths.ToArray();

            return RetrieveFilesFromPathLookupInfos(coldPaths).ToArray();
        }

        private IEnumerable<AssemblyCatalog> GetAssemblies(IExceptionReporter reporter)
        {
            IEnumerable<AssemblyCatalog> assemblyCatalogs = RetrieveFileList()
                .Select(f => CreateAssemblyCatalog(f, reporter))
                .Where(ac => ac != null);

            foreach (AssemblyCatalog assemblyCatalog in assemblyCatalogs)
            {
                try
                {
                    bool dummy = assemblyCatalog.Parts.Any();
                }
                catch (Exception ex)
                {
                    if (reporter != null)
                        reporter.Report(ex);
                    continue;
                }

                yield return assemblyCatalog;
            }
        }

        private static IEnumerable<string> RetrieveFilesFromPathLookupInfos(IEnumerable<PathLookupInfo> pathLookupInfos)
        {
            return from pathInfo in pathLookupInfos
                   from ext in pathInfo.SearchPatterns
                   let opt = pathInfo.IncludeSubDirectories ?
                   SearchOption.AllDirectories :
                   SearchOption.TopDirectoryOnly
                   from file in Directory.GetFiles(pathInfo.Path, ext, opt)
                   select file;
        }

        private static AssemblyCatalog CreateAssemblyCatalog(string file, IExceptionReporter reporter)
        {
            try
            {
                return new AssemblyCatalog(file);
            }
            catch (Exception ex)
            {
                if (reporter != null)
                    reporter.Report(ex);
                return null;
            }
        }
    }
}
