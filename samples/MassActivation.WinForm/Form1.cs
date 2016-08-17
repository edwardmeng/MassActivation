using System;
using System.Windows.Forms;
using MassActivation.Services;

namespace MassActivation.WinForm
{
    public partial class Form1 : Form
    {
        public Form1(ICacheService cache)
        {
            InitializeComponent();
            labelName.Text = Convert.ToString(cache.Get("ApplicationName"));
            labelVersion.Text = Convert.ToString(cache.Get("ApplicationVersion"));
        }
    }
}
