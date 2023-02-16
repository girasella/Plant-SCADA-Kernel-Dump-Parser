using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Plant_SCADA_Kernel_Dump_Parser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        MainViewModel ViewModel;
        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new MainViewModel();

            this.DataContext = ViewModel;
            
        }

        private void ListBox_Selected(object sender, RoutedEventArgs e)
        {
            KernelItem ki = (KernelItem)e.Source;

            ViewModel.CurrentlySelectedItem = ki.Content;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                KernelItem ki = (KernelItem)e.AddedItems[0];
                ViewModel.CurrentlySelectedItem = ki.Content;
            }
        }

        private void TextBox_KeyEnterUpdate(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tBox = (TextBox)sender;
                DependencyProperty prop = TextBox.TextProperty;

                BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
                if (binding != null) { binding.UpdateSource(); }
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    ViewModel.KernelLoad(files[0],true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while opening this file: {ex.Message} - {ex.StackTrace}");
            }

        }

        private void TextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }
    }
}
