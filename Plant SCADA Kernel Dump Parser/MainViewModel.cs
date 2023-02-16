using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.ComponentModel;

namespace Plant_SCADA_Kernel_Dump_Parser
{
    internal class MainViewModel : ViewModelBase
    {
        ObservableCollection<KernelItem> _tables;

        ObservableCollection<KernelItem> _iodevices;

        ObservableCollection<KernelItem> _queues;

        int autoReloadInterval = 5;

        string _selectedFile;

        string _currentlySelectedItem;

        KernelDump _dump;

        DateTime _lastModificationFile;

        ICommand _loadFile;

        bool _autoReload;

        BackgroundWorker ReloadWorker;

        public ICommand LoadFile
        {
            get => _loadFile;
            set { _loadFile = value; OnPropertyChanged(); }
        }
        public string SelectedFile
        {
            get => _selectedFile;
            set { _selectedFile = value;
                OnPropertyChanged();
            
            }
        }
        public string CurrentlySelectedItem
        {
            get => _currentlySelectedItem;
            set { _currentlySelectedItem = value; OnPropertyChanged(); }
        }
        public DateTime LastModificationFile
        {
            get => _lastModificationFile;
            set { _lastModificationFile = value;
                OnPropertyChanged(nameof(LastEditFile)); 
                OnPropertyChanged(); }
        }

        public string LastEditFile
        {
            get
            {
                return String.IsNullOrWhiteSpace(SelectedFile) ? "" : "Date modification: " + LastModificationFile.ToString();
            }
        }
        public KernelDump Dump
        {
            get => _dump;
            set { _dump = value; OnPropertyChanged(); }
        }

        public bool AutoReload { 
            get => _autoReload;
            set {
                _autoReload = value;
                OnPropertyChanged();
                if (_autoReload)
                {
                    ReloadWorker.RunWorkerAsync();
                }                        
            }
        }

        public MainViewModel()
        {
            Dump = new KernelDump();
            LoadFile = new DelegateCommand(() => ExecuteOpenCommand());

            ReloadWorker = new BackgroundWorker();

            ReloadWorker.DoWork += ReloadWorker_DoWork;        
        
        }

        private void ReloadWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            int counter = autoReloadInterval;
            App.Current.Dispatcher.BeginInvoke((System.Action)delegate
            {
                KernelLoad(SelectedFile, false);                
            });
            while (AutoReload)
            {
                if (e.Cancel) return;
                counter--;
                if (counter == 0)
                {
                    counter = autoReloadInterval;
                    App.Current.Dispatcher.BeginInvoke((System.Action)delegate
                    {
                        KernelLoad(SelectedFile, false);
;                   });
                }                
                System.Threading.Thread.Sleep(1000);
            }
        }


        internal void ExecuteOpenCommand()
        {

            string file = FileOpen();

            if (string.IsNullOrEmpty(file))
                return;

            AutoReload = false;

            KernelLoad(file, true);
        }

        public void KernelLoad (string fileName, bool openingNewFile)
        {
            if (!ValidateFile(fileName, openingNewFile)) return;
            
            SelectedFile = fileName;
            FileInfo file = new FileInfo(SelectedFile);
            LastModificationFile = file.LastWriteTime;
            //Dump.ClearAllData();
            KernelDump.Parse(file, Dump);
            CurrentlySelectedItem = "";
            if (openingNewFile && Dump.GeneralStatistics != null)
            {
                CurrentlySelectedItem = Dump.GeneralStatistics.Content;
            }

        }

        string FileOpen()
        {

            var fileDialog = new OpenFileDialog();

            bool? fileDialogResult = fileDialog.ShowDialog();

            if (fileDialogResult == null || fileDialogResult == false)
                return "";

            SelectedFile = fileDialog.FileName;

            return SelectedFile;

        }

        bool ValidateFile(string fileName, bool isOpeningNewFile)
        {
            FileInfo file = new FileInfo(fileName);
            if (!file.Exists)
            {
                if (isOpeningNewFile) MessageBox.Show("This file does not exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (!KernelDump.IsValidKernelDump(file))
            {
                if (isOpeningNewFile) MessageBox.Show("The selected file is not a valid Citect/Plant SCADA Kernel Dump.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;

        }



    }
}
