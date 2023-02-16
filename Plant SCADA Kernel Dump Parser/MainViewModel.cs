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

namespace Plant_SCADA_Kernel_Dump_Parser
{
    internal class MainViewModel : ViewModelBase
    {
        ObservableCollection<KernelItem> _tables;

        ObservableCollection<KernelItem> _iodevices;

        ObservableCollection<KernelItem> _queues;

        string _selectedFile;

        string _currentlySelectedItem;

        KernelDump _dump;

        DateTime _lastModificationFile;

        ICommand _loadFile;

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

        public MainViewModel()
        {
            Dump = new KernelDump();
            LoadFile = new DelegateCommand(() => LoadKernel());

        }


        internal void LoadKernel(string fileName ="")
        {

            if (string.IsNullOrEmpty(fileName))
            {
                if (!FileOpen()) return;
            }
            else
                SelectedFile = fileName;
            

            if (!String.IsNullOrWhiteSpace(SelectedFile))
            {
                Dump.ClearAllData();
                Dump = null;
                CurrentlySelectedItem = "";

            }

            FileInfo file = new FileInfo(SelectedFile);

            if (!file.Exists)
            {
                MessageBox.Show("This file does not exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Dump = KernelDump.Parse(file, Dump);

            LastModificationFile = file.LastWriteTime;

            if (Dump.GeneralStatistics != null)
            {
                CurrentlySelectedItem = Dump.GeneralStatistics.Content;
            }

        }


        bool FileOpen()
        {

            var fileDialog = new OpenFileDialog();

            bool? fileDialogResult = fileDialog.ShowDialog();

            if (fileDialogResult == null || fileDialogResult == false)
                return false;

            SelectedFile = fileDialog.FileName;

            return true;

        }



    }
}
