using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows;

namespace Plant_SCADA_Kernel_Dump_Parser
{
    internal class KernelDump : ViewModelBase
    {
        public static Regex KernelStartRegex = new Regex("Citect Diagnostics");
        public static Regex GeneralStatsRegex = new Regex("----General Statistics(.*)");
        public static Regex TaskControlBl_Regex = new Regex("-----Task Control Blocks(.*)");
        public static Regex TableStartRegex = new Regex("------Table (.*)");
        public static Regex QueueStartRegex = new Regex("-----Queue (.*)");
        public static Regex IODeviceStartRegex = new Regex("-----Unit Statistics");
        public static Regex IODeviceNameRegex = new Regex("Unit: *(.*)    IO Server: *(.*)");
        public static Regex ParametersRegex = new Regex("-----Parameters");
        public static Regex ParamSectionRegex = new Regex("(\\[.*\\])(.*)");

        string _filterTables;

        string _filterIODevices;

        string _filterQueues;

        string _filterIniParams;

        ObservableCollection<KernelItem> _iniParams;

        ObservableCollection<KernelItem> _tables;

        ObservableCollection<KernelItem> _iodevices;

        ObservableCollection<KernelItem> _queues;

        KernelItem _generalStatistics;

        KernelItem _taskControlBlocks;

        ObservableCollection<KernelItem> _filteredTables, _filteredIODevices, _filteredQueues, _filteredIniParams;



        public ObservableCollection<KernelItem> Tables
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FilterTables))
                    return _tables;
                else
                    return _filteredTables;


            }
            set { _tables = value; OnPropertyChanged(); }
        }
        public ObservableCollection<KernelItem> Iodevices
        {

            get
            {
                if (string.IsNullOrWhiteSpace(FilterIODevices))
                    return _iodevices;
                else
                    return _filteredIODevices;

            }
            set { _iodevices = value; OnPropertyChanged(); }
        }
        public ObservableCollection<KernelItem> Queues
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FilterQueues))
                    return _queues;
                else
                    return _filteredQueues;

            }
            set { _queues = value; OnPropertyChanged(); }
        }

        public ObservableCollection<KernelItem> IniParams
        {
            get
            {

                if (string.IsNullOrWhiteSpace(FilterIniParams))
                    return _iniParams;
                else
                    return _filteredIniParams;

            }
            set
            {
                _iniParams = value;
                OnPropertyChanged();
            }
        }
        public string FilterIniParams
        {
            get => _filterIniParams;
            set
            {
                _filterIniParams = value;
                var filtered = _iniParams.Where(x => x.Title.ToLower().Contains(_filterIniParams.ToLower()));

                _filteredIniParams.Clear();

                foreach (var item in filtered)
                {
                    _filteredIniParams.Add(item);
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(IniParams));

            }

        }
        public KernelItem GeneralStatistics
        {
            get => _generalStatistics;
            set { _generalStatistics = value; OnPropertyChanged(); }
        }
        public KernelItem TaskControlBlocks
        {
            get => _taskControlBlocks;
            set { _taskControlBlocks = value; OnPropertyChanged(); }
        }

        public string FilterTables
        {
            get => _filterTables;
            set
            {
                _filterTables = value;
                var filtered = _tables.Where(x => x.Title.ToLower().Contains(_filterTables.ToLower()));

                _filteredTables.Clear();

                foreach (var item in filtered)
                {
                    _filteredTables.Add(item);
                }


                OnPropertyChanged();
                OnPropertyChanged("Tables");
            }
        }
        public string FilterIODevices
        {
            get => _filterIODevices;
            set
            {
                _filterIODevices = value;
                var filtered = _iodevices.Where(x => x.Title.ToLower().Contains(_filterIODevices.ToLower()));

                _filteredIODevices.Clear();

                foreach (var item in filtered)
                {
                    _filteredIODevices.Add(item);
                }


                OnPropertyChanged();
                OnPropertyChanged("Iodevices");
            }
        }

        public string FilterQueues
        {
            get => _filterQueues;
            set {
                _filterQueues = value;
                var filtered = _queues.Where(x => x.Title.ToLower().Contains(_filterQueues.ToLower()));

                _filteredQueues.Clear();

                foreach (var item in filtered)
                {
                    _filteredQueues.Add(item);
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(Queues));
            }
        }



        public KernelDump()
        {
            _tables = new ObservableCollection<KernelItem>();
            _filteredTables = new ObservableCollection<KernelItem>();

            _iodevices = new ObservableCollection<KernelItem>();
            _filteredIODevices = new ObservableCollection<KernelItem>();

            _queues = new ObservableCollection<KernelItem>();
            _filteredQueues = new ObservableCollection<KernelItem>();

            _iniParams = new ObservableCollection<KernelItem>();
            _filteredIniParams = new ObservableCollection<KernelItem>();
        }

        public static bool IsValidKernelDump(FileInfo dumpFile)
        {

            KernelDump kd = new KernelDump();
            bool retValue = false;
            try
            {
                FileStream kernelFileStream = new FileStream(dumpFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader kernelFileReader = new StreamReader(kernelFileStream);
                
                for (int i = 0; i < 100; i++)
                {
                    string line = kernelFileReader.ReadLine();
                    if (kd.IsKernelStart(line))
                    {
                        retValue = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return retValue;
        }

        public static KernelDump Parse(FileInfo dumpFile, KernelDump kDump = null)
        {
            if (kDump == null)
            {
                kDump = new KernelDump();
            }

            try
            {

                FileStream kernelFileStream = new FileStream(dumpFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader kernelFileReader = new StreamReader(kernelFileStream);

                while (!kernelFileReader.EndOfStream)
                {
                    string line = kernelFileReader.ReadLine();

                    if (kDump.IsKernelStart(line))
                    {
                        kDump.ClearAllData();
                    }
                    if (kDump.ParseBlock(line, kernelFileReader, GeneralStatsRegex, kDump.Tables)) continue;
                    if (kDump.ParseBlock(line, kernelFileReader, TaskControlBl_Regex, kDump.Tables)) continue;
                    if (kDump.ParseBlock(line, kernelFileReader, TableStartRegex, kDump.Tables)) continue;
                    if (kDump.ParseBlock(line, kernelFileReader, QueueStartRegex, kDump.Queues)) continue;
                    if (kDump.ParseIoDevice(line, kernelFileReader)) continue;
                    kDump.ParseIniParams(line, kernelFileReader);


                }
                kernelFileReader.Close();
                kernelFileStream.Close();
            }

            catch (Exception ex)
            {
                Debug.Print($"Exception in parsing kernel dump:{ex.Message} - {ex.StackTrace}");
                MessageBox.Show($"An error occurred while parsing the kernel dump file: {ex.Message}","Error",MessageBoxButton.OK,MessageBoxImage.Error);
            }
            return kDump;

        }


        bool ParseBlock(string line, StreamReader sr, Regex regex, ObservableCollection<KernelItem> items)
        {
            var match = regex.Match(line);

            if (!match.Success)
                return false;

            if (!match.Groups[1].Success)
                return false;

            if (match.Groups[1].Value.ToLower().Contains("not used"))
                return false;

            KernelItem item = new KernelItem();
            item.Title = match.Groups[1].Value;

            while (!String.IsNullOrWhiteSpace(line))
            {
                item.AddContentLine(line);
                line = sr.ReadLine();
            }
            items.Add(item);
            if (regex == GeneralStatsRegex)
            {
                item.Title = "General statistics";
                GeneralStatistics = item;
            }
            if (regex == TaskControlBl_Regex)
            {
                item.Title = "Task Control Blocks";
                TaskControlBlocks = item;
            }

            return true;
        }

        bool ParseIniParams(string line, StreamReader sr)
        {
            var match = ParametersRegex.Match(line);

            if (!match.Success)
                return false;

            Dictionary<string, KernelItem> sections = new Dictionary<string, KernelItem>();
            KernelItem item = null;
            line = sr.ReadLine();
            while (!String.IsNullOrWhiteSpace(line))
            {
                match = ParamSectionRegex.Match(line);
                if (match.Success)
                {
                    if (match.Groups[1].Success)
                    {
                        string sectionName = match.Groups[1].Value;

                        if (!sections.ContainsKey(sectionName))
                        {
                            item = new KernelItem();
                            sections.Add(sectionName, item);
                            item.Title = sectionName;
                            item.AddContentLine(sectionName);
                            IniParams.Add(item);
                        }
                        else 
                            item = sections[sectionName];

                        if (match.Groups[2].Success)
                            item.AddContentLine(match.Groups[2].Value.Trim()) ;                    
                    }
                }
                
                line = sr.ReadLine();
            }
            return true;
        }

        bool ParseIoDevice(string line, StreamReader sr)
        {
            var match = IODeviceStartRegex.Match(line);
            if (!match.Success)
                return false;

            line = sr.ReadLine();

            match = IODeviceNameRegex.Match(line);

            if (!match.Success) return false;

            if (!match.Groups[1].Success) return false;

            KernelItem item = new KernelItem();
            item.Title = match.Groups[1].Value;

            if (match.Groups[2].Success)
            {
                item.Title = match.Groups[2].Value + " - " + match.Groups[1].Value;
            }

            while (!String.IsNullOrWhiteSpace(line))
            {
                item.AddContentLine(line);
                line = sr.ReadLine();
            }
            Iodevices.Add(item);

            return true;
        }

        Boolean IsKernelStart(string line)
        {
            return KernelStartRegex.IsMatch(line);
        }

        internal void ClearAllData()
        {
            Tables.Clear();
            _filteredTables.Clear();
            Queues.Clear();
            _filteredQueues.Clear();
            Iodevices.Clear();
            _filteredIODevices.Clear();
            IniParams.Clear();
            _filteredIniParams.Clear();
            GeneralStatistics = null;
            TaskControlBlocks = null;
        }



    }


}

