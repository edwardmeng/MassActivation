using System.Windows;
using MassActivation.Services;

namespace MassActivation.WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void Initialize(ICacheService cache)
        {
            this.labelName.Content = cache.Get("ApplicationName");
            this.labelVersion.Content = cache.Get("ApplicationVersion");
        }
    }
}
