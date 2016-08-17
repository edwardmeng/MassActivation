using System;
using System.Web.UI;
using MassActivation.Services;

namespace MassActivation.Web
{
    public partial class Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var cache = Startup.GetService<ICacheService>();
                labelName.Text = Convert.ToString(cache.Get("ApplicationName"));
                labelVersion.Text = Convert.ToString(cache.Get("ApplicationVersion"));
            }
        }
    }
}