
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FF6exped.CustomControls.CustomComboBox
{
    /// <summary>
    /// Interaction logic for CustomComboBox.xaml
    /// </summary>
    public partial class CustomComboBox : UserControl
    {
        public ObservableCollection<ComboBoxInfo> Items { get; private set; }

        public CustomComboBox()
        {
            InitializeComponent();                      
        }

        public ObservableCollection<ComboBoxInfo> ItemSource
        {
            get
            {
                return (ObservableCollection<ComboBoxInfo>)cmb.ItemsSource;
            }
            set
            {
                cmb.ItemsSource = value;
            }
        }

        public ComboBoxInfo SelectedItem
        {
            get
            {
                return (ComboBoxInfo)cmb.SelectedItem;
            }
            set
            {
                if (value != null)
                    cmb.SelectedItem = value;
            }
        }

        public int SelectedIndex
        {
            get
            {
                return cmb.SelectedIndex;
            }
            set
            {
                cmb.SelectedIndex = value;
            }
        }

        private void cmb_Loaded(object sender, RoutedEventArgs e)
        {
            /*ComboBox comboBox = sender as ComboBox;
            ToggleButton toggleButton = GetVisualChild<ToggleButton>(comboBox);
            ButtonChrome chrome = toggleButton.Template.FindName("Chrome", toggleButton) as ButtonChrome;
            chrome.RenderMouseOver = false;
            chrome.RenderPressed = false;
            chrome.RenderDefaulted = false;
            chrome.Background = Brushes.Transparent;
            Grid MainGrid = comboBox.Template.FindName("MainGrid", comboBox) as Grid;
            Binding backgroundBinding = new Binding("Background");
            backgroundBinding.Source = comboBox;
            MainGrid.SetBinding(BackgroundProperty, backgroundBinding);*/
        }

        private static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
    }
}
