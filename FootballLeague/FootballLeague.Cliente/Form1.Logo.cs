
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FootballLeague.Cliente
{
    public partial class Form1 : Form
    {
        private Image? imagenLogoFootballLeague;

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (imagenLogoFootballLeague == null)
            {
                IncorporarLogoFootballLeague();
            }
        }

        private void IncorporarLogoFootballLeague()
        {
            string ruta = Path.Combine(
                AppContext.BaseDirectory,
                "Recursos",
                "Logo",
                "logo.png");

            if (!File.Exists(ruta))
                return;

            Label? encabezado =
                BuscarEncabezadoFootballLeague(this);

            if (encabezado == null ||
                encabezado.Parent == null)
            {
                return;
            }

            try
            {
                using Image original = Image.FromFile(ruta);

                imagenLogoFootballLeague =
                    new Bitmap(original);

                Control menu = encabezado.Parent;

                menu.SuspendLayout();

                try
                {
                    int alturaAnterior = encabezado.Height;
                    int nuevaAltura = 155;

                    int diferencia =
                        nuevaAltura - alturaAnterior;

                    int limiteAnterior = encabezado.Bottom;

                    encabezado.Text = "";
                    encabezado.Image = null;
                    encabezado.AutoSize = false;

                    if (encabezado.Dock == DockStyle.None &&
                        menu is not FlowLayoutPanel &&
                        menu is not TableLayoutPanel)
                    {
                        foreach (Control control in menu.Controls)
                        {
                            if (control != encabezado &&
                                control.Top >= limiteAnterior &&
                                control.Dock == DockStyle.None)
                            {
                                control.Top +=
                                    Math.Max(0, diferencia);
                            }
                        }

                        encabezado.Left = 5;

                        encabezado.Width =
                            Math.Max(1, menu.ClientSize.Width - 10);

                        encabezado.Anchor =
                            AnchorStyles.Top |
                            AnchorStyles.Left |
                            AnchorStyles.Right;
                    }

                    if (menu is TableLayoutPanel panel)
                    {
                        int fila = panel.GetRow(encabezado);

                        if (fila >= 0 &&
                            fila < panel.RowStyles.Count)
                        {
                            panel.RowStyles[fila].SizeType =
                                SizeType.Absolute;

                            panel.RowStyles[fila].Height =
                                nuevaAltura;
                        }
                    }

                    encabezado.Height = nuevaAltura;

                    encabezado.MinimumSize =
                        new Size(0, nuevaAltura);

                    encabezado.Padding = new Padding(0);

                    encabezado.Paint +=
                        DibujarLogoFootballLeague;

                    encabezado.Invalidate();
                }
                finally
                {
                    menu.ResumeLayout(true);
                }

                Disposed += (_, _) =>
                {
                    imagenLogoFootballLeague?.Dispose();
                    imagenLogoFootballLeague = null;
                };
            }
            catch
            {
                encabezado.Text = "FootballLeague";

                imagenLogoFootballLeague?.Dispose();
                imagenLogoFootballLeague = null;
            }
        }

        private Label? BuscarEncabezadoFootballLeague(
            Control contenedor)
        {
            foreach (Control control in contenedor.Controls)
            {
                if (control is Label etiqueta &&
                    etiqueta.Text.Trim().Equals(
                        "FootballLeague",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return etiqueta;
                }

                Label? encontrado =
                    BuscarEncabezadoFootballLeague(control);

                if (encontrado != null)
                    return encontrado;
            }

            return null;
        }

        private void DibujarLogoFootballLeague(
            object? sender,
            PaintEventArgs e)
        {
            if (sender is not Label etiqueta ||
                imagenLogoFootballLeague == null)
            {
                return;
            }

            int anchoDisponible = etiqueta.Width - 8;
            int altoDisponible = etiqueta.Height - 8;

            if (anchoDisponible <= 0 ||
                altoDisponible <= 0)
            {
                return;
            }

            float escala = Math.Min(
                (float)anchoDisponible /
                    imagenLogoFootballLeague.Width,

                (float)altoDisponible /
                    imagenLogoFootballLeague.Height);

            int ancho = Math.Max(
                1,
                (int)(imagenLogoFootballLeague.Width * escala));

            int alto = Math.Max(
                1,
                (int)(imagenLogoFootballLeague.Height * escala));

            int x = (etiqueta.Width - ancho) / 2;
            int y = (etiqueta.Height - alto) / 2;

            var modoAnterior =
                e.Graphics.InterpolationMode;

            e.Graphics.InterpolationMode =
                System.Drawing.Drawing2D.InterpolationMode
                    .HighQualityBicubic;

            e.Graphics.DrawImage(
                imagenLogoFootballLeague,
                new Rectangle(x, y, ancho, alto));

            e.Graphics.InterpolationMode = modoAnterior;
        }
    }
}

