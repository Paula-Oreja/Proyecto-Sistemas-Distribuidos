
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

        private readonly Font fuenteNegrita =
            new("Segoe UI", 10, FontStyle.Bold);

        private bool actualizandoInicio;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            tabla.CellPainting += PintarEscudos;
            tabla.CellFormatting += AplicarColores;

            tabla.DataBindingComplete += (_, _) =>
            {
                AjustarColumnasEscudos();
            };

            inicio.Layout += (_, _) =>
            {
                MostrarEscudosInicio();
            };

            MejorarFiltroJugadores();
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

                    if (id > 0 &&
                        !string.IsNullOrWhiteSpace(nombre))
                    {
                        idsEscudos[nombre] = id;
                    }
                }
            }
            catch
            {
                // La aplicación continuará funcionando
                // aunque no se puedan cargar los escudos.
            }

            if (IsDisposed)
                return;

            base.OnShown(e);

            tabla.Invalidate();
        }

        // CARGAR ESCUDOS

        private Image? ObtenerEscudo(string nombre)
        {
            if (!idsEscudos.TryGetValue(
                nombre.Trim(), out int id))
            {
                return null;
            }

            return ObtenerEscudoPorId(id);
        }

        private Image? ObtenerEscudoPorId(int id)
        {
            if (imagenesEscudos.TryGetValue(
                id, out Image? imagen))
            {
                return imagen;
            }

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

        // MEJORAR FILTRO DE JUGADORES

        private void MejorarFiltroJugadores()
        {
            foreach (Control control in barraFiltro.Controls)
            {
                if (control is Label etiqueta)
                {
                    etiqueta.Text = "Filtrar por equipo:";
                    etiqueta.Width = 185;
                    etiqueta.Height = 35;
                    etiqueta.AutoEllipsis = false;

                    etiqueta.Font = new Font(
                        "Segoe UI", 10, FontStyle.Bold);

                    etiqueta.TextAlign =
                        ContentAlignment.MiddleLeft;
                }
            }

            selectorEquipo.Width = 320;
        }

        // AJUSTAR COLUMNAS

        private void AjustarColumnasEscudos()
        {
            tabla.ColumnHeadersDefaultCellStyle.WrapMode =
                DataGridViewTriState.False;

            foreach (DataGridViewColumn columna in tabla.Columns)
            {
                columna.DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleLeft;
            }

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

            if (tabla.Columns.Contains("ID") &&
                seccionActual == "Equipos")
            {
                var columna = tabla.Columns["ID"]!;

                columna.MinimumWidth = 55;
                columna.FillWeight = 30;

                columna.DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleCenter;
            }

            if (seccionActual == "Equipos")
            {
                if (tabla.Columns.Contains("Equipo"))
                {
                    tabla.Columns["Equipo"]!.FillWeight = 180;
                }

                if (tabla.Columns.Contains("Ciudad"))
                {
                    tabla.Columns["Ciudad"]!.MinimumWidth = 140;
                    tabla.Columns["Ciudad"]!.FillWeight = 100;
                }

                if (tabla.Columns.Contains("Estadio"))
                {
                    tabla.Columns["Estadio"]!.MinimumWidth = 180;
                    tabla.Columns["Estadio"]!.FillWeight = 150;
                }
            }

            if (tabla.Columns.Contains("Resultado"))
            {
                var columna = tabla.Columns["Resultado"]!;

                columna.MinimumWidth = 115;
                columna.FillWeight = 110;

                columna.DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleCenter;
            }

            if (tabla.Columns.Contains("Estado"))
            {
                var columna = tabla.Columns["Estado"]!;

                columna.MinimumWidth = 115;
                columna.FillWeight = 110;

                columna.DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleCenter;
            }

            if (tabla.Columns.Contains("PartidoID"))
            {
                var columna = tabla.Columns["PartidoID"]!;

                columna.HeaderText = "ID Partido";
                columna.MinimumWidth = 115;
                columna.FillWeight = 85;

                columna.DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleCenter;
            }

            if (tabla.Columns.Contains("Jornada"))
            {
                var columna = tabla.Columns["Jornada"]!;

                columna.MinimumWidth = 85;
                columna.FillWeight = 65;

                columna.DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleCenter;
            }

            if (tabla.Columns.Contains("Dorsal"))
            {
                var columna = tabla.Columns["Dorsal"]!;

                columna.MinimumWidth = 80;
                columna.FillWeight = 65;

                columna.DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleCenter;
            }

            if (seccionActual == "Clasificación")
            {
                foreach (string nombre in new[]
                {
                    "Pos", "PJ", "PG", "PE", "PP",
                    "GF", "GC", "DG", "Puntos"
                })
                {
                    if (!tabla.Columns.Contains(nombre))
                        continue;

                    var columna = tabla.Columns[nombre]!;

                    columna.MinimumWidth =
                        nombre == "Puntos" ? 80 : 55;

                    columna.FillWeight =
                        nombre == "Puntos" ? 80 : 55;

                    columna.DefaultCellStyle.Alignment =
                        DataGridViewContentAlignment.MiddleCenter;

                    columna.HeaderCell.Style.Alignment =
                        DataGridViewContentAlignment.MiddleCenter;
                }
            }
        }

        // COLORES DE CLASIFICACIÓN Y RESULTADOS

        private void AplicarColores(
            object? sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columna =
                tabla.Columns[e.ColumnIndex].Name;

            if (seccionActual == "Clasificación")
            {
                if (columna == "Pos" &&
                    int.TryParse(
                        e.Value?.ToString(), out int posicion))
                {
                    e.CellStyle.Font = fuenteNegrita;

                    if (posicion >= 1 && posicion <= 4)
                    {
                        // CHAMPIONS LEAGUE

                        e.CellStyle.BackColor =
                            Color.FromArgb(219, 234, 254);

                        e.CellStyle.ForeColor =
                            Color.FromArgb(29, 78, 216);
                    }
                    else if (posicion == 5 || posicion == 6)
                    {
                        // EUROPA LEAGUE

                        e.CellStyle.BackColor =
                            Color.FromArgb(255, 237, 213);

                        e.CellStyle.ForeColor =
                            Color.FromArgb(154, 52, 18);
                    }
                    else if (posicion == 7)
                    {
                        // CONFERENCE LEAGUE

                        e.CellStyle.BackColor =
                            Color.FromArgb(220, 252, 231);

                        e.CellStyle.ForeColor =
                            Color.FromArgb(22, 101, 52);
                    }
                    else if (posicion >= 18)
                    {
                        // DESCENSO

                        e.CellStyle.BackColor =
                            Color.FromArgb(254, 226, 226);

                        e.CellStyle.ForeColor =
                            Color.FromArgb(185, 28, 28);
                    }
                    else
                    {
                        e.CellStyle.BackColor =
                            e.RowIndex % 2 == 0
                            ? Color.White
                            : Color.FromArgb(241, 245, 250);

                        e.CellStyle.ForeColor = azul;
                    }

                    e.CellStyle.SelectionBackColor =
                        e.CellStyle.BackColor;

                    e.CellStyle.SelectionForeColor =
                        e.CellStyle.ForeColor;
                }

                if (columna == "Puntos")
                {
                    e.CellStyle.Font = fuenteNegrita;

                    e.CellStyle.ForeColor = azul;

                    e.CellStyle.BackColor =
                        Color.FromArgb(226, 232, 240);
                }
            }

            if (seccionActual == "Calendario" ||
                seccionActual == "Resultados")
            {
                string valor = e.Value?.ToString() ?? "";

                if (columna == "Estado")
                {
                    e.CellStyle.Font = fuenteNegrita;

                    if (valor == "Finalizado")
                    {
                        e.CellStyle.BackColor =
                            Color.FromArgb(220, 252, 231);

                        e.CellStyle.ForeColor =
                            Color.FromArgb(22, 101, 52);
                    }
                    else if (valor == "Pendiente")
                    {
                        e.CellStyle.BackColor =
                            Color.FromArgb(255, 237, 213);

                        e.CellStyle.ForeColor =
                            Color.FromArgb(154, 52, 18);
                    }

                    e.CellStyle.SelectionBackColor =
                        e.CellStyle.BackColor;

                    e.CellStyle.SelectionForeColor =
                        e.CellStyle.ForeColor;
                }

                if (columna == "Resultado")
                {
                    if (valor == "Pendiente")
                    {
                        e.CellStyle.ForeColor =
                            Color.FromArgb(100, 116, 139);
                    }
                    else
                    {
                        e.CellStyle.Font = fuenteNegrita;
                        e.CellStyle.ForeColor = azul;
                    }
                }
            }
        }

        // MOSTRAR ESCUDOS EN LAS TABLAS

        private void PintarEscudos(
            object? sender,
            DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columna =
                tabla.Columns[e.ColumnIndex].Name;

            if (columna != "Equipo" &&
                columna != "Local" &&
                columna != "Visitante")
            {
                return;
            }

            string nombre =
                e.FormattedValue?.ToString() ?? "";

            Image? escudo = ObtenerEscudo(nombre);

            if (columna == "Equipo" &&
                seccionActual == "Equipos" &&
                tabla.Columns.Contains("ID"))
            {
                object? valor =
                    tabla.Rows[e.RowIndex].Cells["ID"].Value;

                if (int.TryParse(
                    valor?.ToString(), out int id))
                {
                    escudo = ObtenerEscudoPorId(id);
                }
            }

            if (columna == "Equipo" &&
                tabla.Rows[e.RowIndex].DataBoundItem
                    is JugadorFila jugador)
            {
                escudo =
                    ObtenerEscudoPorId(jugador.EquipoId);
            }

            if (escudo == null || e.Graphics == null)
                return;

            int tamano = Math.Min(
                28, e.CellBounds.Height - 8);

            if (tamano <= 0)
                return;

            e.Paint(
                e.CellBounds,
                e.PaintParts &
                ~DataGridViewPaintParts.ContentForeground);

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
                (e.State &
                DataGridViewElementStates.Selected) != 0;

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

        // DIBUJAR ESCUDOS SIN DEFORMARLOS

        private static void DibujarEscudo(
            Graphics graphics,
            Image imagen,
            Rectangle espacio)
        {
            if (espacio.Width <= 0 || espacio.Height <= 0)
                return;

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

        // MEJORAR RESULTADO DESTACADO DE INICIO

        private void MostrarEscudosInicio()
        {
            if (actualizandoInicio)
                return;

            actualizandoInicio = true;

            try
            {
                foreach (Control control in
                    inicio.Controls.Cast<Control>().ToArray())
                {
                    if (control is not TableLayoutPanel panel)
                        continue;

                    foreach (Control elemento in
                        panel.Controls.Cast<Control>().ToArray())
                    {
                        if (elemento is not Label etiqueta)
                            continue;

                        const string prefijo =
                            "Resultado de ejemplo de la liga:";

                        if (!etiqueta.Text.StartsWith(prefijo))
                            continue;

                        string texto = etiqueta.Text
                            .Substring(prefijo.Length)
                            .Trim();

                        var coincidencia =
                            System.Text.RegularExpressions.Regex.Match(
                                texto,
                                @"^(.+?)\s+(\d+)\s*-\s*(\d+)\s+(.+)$");

                        if (!coincidencia.Success)
                            continue;

                        string nombreLocal =
                            coincidencia.Groups[1].Value.Trim();

                        string golesLocal =
                            coincidencia.Groups[2].Value;

                        string golesVisitante =
                            coincidencia.Groups[3].Value;

                        string nombreVisitante =
                            coincidencia.Groups[4].Value.Trim();

                        Image? local =
                            ObtenerEscudo(nombreLocal);

                        Image? visitante =
                            ObtenerEscudo(nombreVisitante);

                        int fila = panel.GetRow(etiqueta);

                        panel.SuspendLayout();

                        try
                        {
                            panel.Controls.Remove(etiqueta);
                            etiqueta.Dispose();

                            panel.Height = 490;

                            panel.RowStyles[fila].Height = 160;

                            var tarjeta = new TableLayoutPanel
                            {
                                Dock = DockStyle.Fill,
                                BackColor = Color.White,
                                ColumnCount = 3,
                                RowCount = 2,
                                Margin = new Padding(5, 5, 12, 8),
                                Padding = new Padding(10)
                            };

                            tarjeta.ColumnStyles.Add(
                                new ColumnStyle(
                                    SizeType.Percent, 40));

                            tarjeta.ColumnStyles.Add(
                                new ColumnStyle(
                                    SizeType.Percent, 20));

                            tarjeta.ColumnStyles.Add(
                                new ColumnStyle(
                                    SizeType.Percent, 40));

                            tarjeta.RowStyles.Add(
                                new RowStyle(
                                    SizeType.Absolute, 28));

                            tarjeta.RowStyles.Add(
                                new RowStyle(
                                    SizeType.Percent, 100));

                            Label encabezado = new()
                            {
                                Text = "RESULTADO DESTACADO",
                                Dock = DockStyle.Fill,
                                ForeColor = Color.DimGray,
                                Font = new Font(
                                    "Segoe UI", 10,
                                    FontStyle.Bold),
                                TextAlign =
                                    ContentAlignment.MiddleCenter
                            };

                            tarjeta.Controls.Add(
                                encabezado, 0, 0);

                            tarjeta.SetColumnSpan(encabezado, 3);

                            tarjeta.Controls.Add(
                                CrearTarjetaEquipo(
                                    nombreLocal, local),
                                0, 1);

                            Label marcador = new()
                            {
                                Text =
                                    $"{golesLocal} - {golesVisitante}",

                                Dock = DockStyle.Fill,

                                Font = new Font(
                                    "Segoe UI", 30,
                                    FontStyle.Bold),

                                ForeColor = azul,

                                TextAlign =
                                    ContentAlignment.MiddleCenter
                            };

                            tarjeta.Controls.Add(
                                marcador, 1, 1);

                            tarjeta.Controls.Add(
                                CrearTarjetaEquipo(
                                    nombreVisitante, visitante),
                                2, 1);

                            panel.Controls.Add(tarjeta, 0, fila);

                            panel.SetColumnSpan(tarjeta, 2);
                        }
                        finally
                        {
                            panel.ResumeLayout(true);
                        }
                    }
                }
            }
            finally
            {
                actualizandoInicio = false;
            }
        }

        // TARJETA DE EQUIPO DEL MARCADOR

        private Control CrearTarjetaEquipo(
            string nombre,
            Image? escudo)
        {
            TableLayoutPanel equipo = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.White
            };

            equipo.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent, 100));

            equipo.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent, 100));

            equipo.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute, 30));

            PictureBox imagen = new()
            {
                Image = escudo,
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                Margin = new Padding(8)
            };

            Label etiqueta = new()
            {
                Text = nombre,
                Dock = DockStyle.Fill,

                Font = new Font(
                    "Segoe UI", 12, FontStyle.Bold),

                ForeColor = azul,

                TextAlign =
                    ContentAlignment.MiddleCenter,

                AutoEllipsis = true
            };

            equipo.Controls.Add(imagen, 0, 0);
            equipo.Controls.Add(etiqueta, 0, 1);

            return equipo;
        }

        // LIBERAR RECURSOS AL CERRAR

        protected override void OnFormClosed(
            FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            foreach (Image imagen in imagenesEscudos.Values)
            {
                imagen.Dispose();
            }

            imagenesEscudos.Clear();

            fuenteNegrita.Dispose();
        }
    }
}
