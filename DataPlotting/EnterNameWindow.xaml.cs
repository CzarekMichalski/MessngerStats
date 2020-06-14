using System.Windows;

namespace DataPlotting
{
    public partial class EnterNameWindow : Window
    {
        private UsernameWrapper _nameWrapper { get; set; }

        public EnterNameWindow(UsernameWrapper wrapper)
        {
            _nameWrapper = wrapper;
            InitializeComponent();
        }

        private void SubmitName(object sender, RoutedEventArgs e)
        {
            if (NameTextBox.Text != "") _nameWrapper.UserName = NameTextBox.Text;

            Close();
        }
    }
}