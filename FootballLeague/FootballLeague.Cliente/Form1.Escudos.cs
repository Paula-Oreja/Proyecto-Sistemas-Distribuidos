
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace FootballLeague.Cliente
{
    public partial class Form1 : Form
    {
        private readonly Dictionary<int, Image> imagenesEscudos = new();

        private readonly Dictionary<string, int> idsEscudos =
            new(StringComparer.OrdinalIgnoreCase);

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            tabla.CellPainting += PintarEscudos;
            tabla.DataBindingComplete += (_, _) =>
            {
                AjustarColumnasEscudos();
            };

            inicio.Layout += (_, _) =>
            {
                MostrarEscudosInicio();
            };
        }

        protected override async void OnShown(EventArgs e)
        {
            try
            {
                var equipos =
                    await api.ObtenerListaAsync<JsonElement>(
                        "api/Equipos");

                foreach (var equipo in equipos)
                {
                    int id = Entero(equipo, "id");
                    string nombre = Texto(equipo, "nombre");

                    if (id > 0 && !string.IsNullOrWhiteSpace(nombre))
                    {
                        idsEscudos[nombre] = id;
                    }
                }
            }
            catch
            {
                // La aplicación seguirá funcionando
                // aunque no se puedan cargar los escudos.
            }

            base.OnShown(e);

            tabla.Invalidate();
        }

        private Image? ObtenerEscudo(string nombre)
        {
            if (!idsEscudos.TryGetValue(nombre.Trim(), out int id))
                return null;

            return ObtenerEscudoPorId(id);
        }

        private Image? ObtenerEscudoPorId(int id)
        {
            if (imagenesEscudos.TryGetValue(id, out Image? imagen))
                return imagen;

            string ruta = Path.Combine(
                AppContext.BaseDirectory,
                "Recursos",
                "Escudos",
                $"{id}.png");

            if (!File.Exists(ruta))
                return null;

            try
            {
                using Image original = Image.FromFile(ruta);

                Image copia = new Bitmap(original);

                imagenesEscudos[id] = copia;

                return copia;
            }
            catch
            {
                return null;
            }
        }

        private void AjustarColumnasEscudos()
        {
            foreach (string nombre in new[]
            {
                "Equipo",
                "Local",
                "Visitante"
            })
            {
                if (tabla.Columns.Contains(nombre))
                {
                    tabla.Columns[nombre]!.MinimumWidth = 220;
                    tabla.Columns[nombre]!.FillWeight = 220;
                }
            }

            if (tabla.Columns.Contains("Resultado"))
            {
                tabla.Columns["Resultado"]!.MinimumWidth = 115;
                tabla.Columns["Resultado"]!.FillWeight = 110;
            }

            if (tabla.Columns.Contains("Estado"))
            {
                tabla.Columns["Estado"]!.MinimumWidth = 110;
                tabla.Columns["Estado"]!.FillWeight = 110;
            }

            if (tabla.Columns.Contains("PartidoID"))
            {
                tabla.Columns["PartidoID"]!.MinimumWidth = 95;
            }

            if (tabla.Columns.Contains("Jornada"))
            {
                tabla.Columns["Jornada"]!.MinimumWidth = 85;
            }
        }

        private void PintarEscudos(
            object? sender,
            DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columna = tabla.Columns[e.ColumnIndex].Name;

            if (columna != "Equipo" &&
                columna != "Local" &&
                columna != "Visitante")
            {
                return;
            }

            string nombre = e.FormattedValue?.ToString() ?? "";

            Image? escudo = ObtenerEscudo(nombre);

            if (columna == "Equipo" &&
                seccionActual == "Equipos" &&
                tabla.Columns.Contains("ID"))
            {
                object? valor =
                    tabla.Rows[e.RowIndex].Cells["ID"].Value;

                if (int.TryParse(valor?.ToString(), out int id))
                    escudo = ObtenerEscudoPorId(id);
            }

            if (columna == "Equipo" &&
                tabla.Rows[e.RowIndex].DataBoundItem
                    is JugadorFila jugador)
            {
                escudo = ObtenerEscudoPorId(jugador.EquipoId);
            }

            if (escudo == null || e.Graphics == null)
                return;

            e.Paint(
                e.CellBounds,
                e.PaintParts &
                ~DataGridViewPaintParts.ContentForeground);

            int tamano = Math.Min(28, e.CellBounds.Height - 8);

            if (tamano <= 0)
                return;

            int x = e.CellBounds.Left + 8;

            int y = e.CellBounds.Top +
                (e.CellBounds.Height - tamano) / 2;

            DibujarEscudo(
                e.Graphics,
                escudo,
                new Rectangle(x, y, tamano, tamano));

            Rectangle espacioTexto = new(
                x + tamano + 9,
                e.CellBounds.Top,
                Math.Max(
                    0,
                    e.CellBounds.Width - tamano - 25),
                e.CellBounds.Height);

            bool seleccionado =
                (e.State & DataGridViewElementStates.Selected)
                    != 0;

            Color colorTexto = seleccionado
                ? e.CellStyle.SelectionForeColor
                : e.CellStyle.ForeColor;

            TextRenderer.DrawText(
                e.Graphics,
                nombre,
                e.CellStyle.Font ?? tabla.Font,
                espacioTexto,
                colorTexto,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix |
                TextFormatFlags.SingleLine);

            e.Handled = true;
        }

        private static void DibujarEscudo(
            Graphics graphics,
            Image imagen,
            Rectangle espacio)
        {
            float escala = Math.Min(
                (float)espacio.Width / imagen.Width,
                (float)espacio.Height / imagen.Height);

            int ancho = Math.Max(
                1, (int)(imagen.Width * escala));

            int alto = Math.Max(
                1, (int)(imagen.Height * escala));

            int x = espacio.X +
                (espacio.Width - ancho) / 2;

            int y = espacio.Y +
                (espacio.Height - alto) / 2;

            var modoAnterior = graphics.InterpolationMode;

            graphics.InterpolationMode =
                System.Drawing.Drawing2D.InterpolationMode
                    .HighQualityBicubic;

            graphics.DrawImage(
                imagen,
                new Rectangle(x, y, ancho, alto));

            graphics.InterpolationMode = modoAnterior;
        }

        private void MostrarEscudosInicio()
        {
            foreach (Control control in inicio.Controls)
            {
                if (control is not TableLayoutPanel panel)
                    continue;

                foreach (Control elemento in panel.Controls)
                {
                    if (elemento is not Label etiqueta)
                        continue;

                    if (!etiqueta.Text.StartsWith(
                        "Resultado de ejemplo de la liga:"))
                    {
                        continue;
                    }

                    if (etiqueta.Tag is string marca &&
                        marca == "EscudosConfigurados")
                    {
                        continue;
                    }

                    string texto = etiqueta.Text;

                    var nombres = idsEscudos.Keys
                        .Where(nombre => texto.Contains(
                            nombre,
                            StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(nombre => nombre.Length)
                        .Take(2)
                        .OrderBy(nombre => texto.IndexOf(
                            nombre,
                            StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (nombres.Count != 2)
                        continue;

                    Image? local = ObtenerEscudo(nombres[0]);
                    Image? visitante = ObtenerEscudo(nombres[1]);

                    if (local == null || visitante == null)
                        continue;

                    etiqueta.Tag = "EscudosConfigurados";

                    etiqueta.Padding =
                        new Padding(65, 5, 65, 5);

                    etiqueta.Paint += (_, e) =>
                    {
                        int y = (etiqueta.Height - 44) / 2;

                        DibujarEscudo(
                            e.Graphics,
                            local,
                            new Rectangle(10, y, 44, 44));

                        DibujarEscudo(
                            e.Graphics,
                            visitante,
                            new Rectangle(
                                etiqueta.Width - 54,
                                y,
                                44,
                                44));
                    };

                    etiqueta.Invalidate();
                }
            }
        }

        protected override void OnFormClosed(
            FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            foreach (Image imagen in imagenesEscudos.Values)
            {
                imagen.Dispose();
            }

            imagenesEscudos.Clear();
        }
    }
}
