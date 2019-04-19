using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FileSplitterDef;

namespace FileSplitterImporter
{
    public class ImportedFactory
    {
        static volatile ImportedFactory instance;
        static object syncRoot = new object();
        CompositionContainer container;

        [ImportMany(typeof(IGenReader))]
        IEnumerable<Lazy<IGenReader>> sources;
        [ImportMany(typeof(IGenWriter))]
        IEnumerable<Lazy<IGenWriter>> targets;
        [ImportMany(typeof(IFileMerger))]
        IEnumerable<Lazy<IFileMerger>> mergers;
        [ImportMany(typeof(IFileSpliter))]
        IEnumerable<Lazy<IFileSpliter>> spliters;

        public static ImportedFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ImportedFactory();
                    }
                }

                return instance;
            }
        }

        public void DoImport()
        {

            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();

            //Add all the parts found in all assemblies in
            //the same directory as the executing program
            catalog.Catalogs.Add(
                new DirectoryCatalog(
                    Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location
                    )
                )
            );

            //Create the CompositionContainer with the parts in the catalog.
            container = new CompositionContainer(catalog);

            //Fill the imports of this object
            container.ComposeParts(this);

            //Debug.Print(container.Catalog)
        }

        private IEnumerable<Lazy<IGenReader>> GetReaderList(string value)
        {
            return sources?.Where(s => s.Value.Protocol == value);
        }

        private IEnumerable<Lazy<IGenWriter>> GetWriterList(string value)
        {
            return targets?.Where(s => s.Value.Protocol == value);
        }
        
        public bool ProtocolExist(string value)
        {
            return this?.GetReaderList(value)?.Count() == 1 ;
        }

        public IFileMerger GetMergerByProtocol(string value)
        {
            return (IFileMerger)mergers?.Where(s => s.Value.Protocol == value).FirstOrDefault().Value;
        }

        public IFileSpliter GetSpliterByProtocol(string value)
        {
            return (IFileSpliter)spliters?.Where(s => s.Value.Protocol == value).FirstOrDefault().Value;
        }


        public IGenWriter GetWriterByProtocol(string value)
        {
            return (IGenWriter)Activator.CreateInstance(GetWriterTypeByProtocol(value)); //container.GetExport<IGenWriter>(); //this?.GetWriterList(value).FirstOrDefault();
        }

        public IGenReader GetReaderByProtocol(string value)
        {
            return (IGenReader)Activator.CreateInstance(GetReaderTypeByProtocol(value)); //container.GetExport<IGenWriter>(); //this?.GetWriterList(value).FirstOrDefault();
        }

        public Type GetReaderTypeByProtocol(string value)
        {
            return this?.GetReaderList(value).FirstOrDefault().Value.GetType();
        }

        public Type GetWriterTypeByProtocol(string value)
        {
            return this?.GetWriterList(value).FirstOrDefault().Value.GetType();
        }

        public int AvailableNumberOfProtocol
        {
            get { return (sources?.Count() ?? 0); }
        }

        public List<string> EnumerateAllComponents()
        {
            var result = new List<string>();

            foreach (Lazy<IGenReader> com in sources)
            {
                //Console.WriteLine(com.Description);
                result.Add(com.Value.Protocol);
            }

            return result;
        }
    }
}
