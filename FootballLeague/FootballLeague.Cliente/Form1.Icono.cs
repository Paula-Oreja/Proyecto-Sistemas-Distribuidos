
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FootballLeague.Cliente
{
    public partial class Form1 : Form
    {
        private Icon? iconoFootballLeague;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            ConfigurarIconoFootballLeague();
        }

        private void ConfigurarIconoFootballLeague()
        {
            if (iconoFootballLeague != null)
                return;

            string ruta = Path.Combine(
                AppContext.BaseDirectory,
                "Recursos",
                "Logo",
                "FootballLeague.ico");

            if (!File.Exists(ruta))
                return;

            try
            {
                iconoFootballLeague = new Icon(ruta);

                Icon = iconoFootballLeague;

                ShowIcon = true;

                Disposed += (_, _) =>
                {
                    iconoFootballLeague?.Dispose();
                    iconoFootballLeague = null;
                };
            }
            catch
            {
                iconoFootballLeague?.Dispose();
                iconoFootballLeague = null;
            }
        }
    }
}
