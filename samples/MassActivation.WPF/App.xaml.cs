using System;
using System.Windows;
using MassActivation.Services;

namespace MassActivation.WPF
{
    public partial class App : Application
    {
        private ICacheService _cache;

        public void Configuration(ICacheService cache)
        {
            _cache = cache;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            ((MainWindow)MainWindow).Initialize(_cache);
        }
    }
}
