using System.Windows;
using System.Windows.Input;

namespace Sample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void What_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            await What.BeginAnimationAsync();
            What.Visibility = Visibility.Collapsed;
        }
    }
}
