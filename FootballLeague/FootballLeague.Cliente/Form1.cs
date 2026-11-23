
using FootballLeague.Cliente.Services;
using System.Drawing;
using System.Text.Json;

namespace FootballLeague.Cliente
{
    public partial class Form1 : Form
    {
        private readonly ApiService api = new ApiService();

        private readonly Color azul = Color.FromArgb(17, 35, 67);
        private readonly Color azulClaro = Color.FromArgb(33, 65, 112);
        private readonly Color fondo = Color.FromArgb(245, 247, 251);

        private DataGridView tabla = null!;
        private ComboBox selectorLiga = null!;
        private Label titulo = null!;
        private Label estado = null!;

        private string seccionActual = "Clasificación";
        private bool iniciando = true;

        private class LigaOpcion
        {
            public int Id { get; set; }
            public string Nombre { get; set; } = "";

            public override string ToString()
            {
                return $"{Nombre} (ID {Id})";
            }
        }

        private class PartidoFila
        {
            public int Jornada { get; set; }
            public int PartidoID { get; set; }
            public string Local { get; set; } = "";
            public string Visitante { get; set; } = "";
            public string Resultado { get; set; } = "";
        }

        public Form1()
        {
            InitializeComponent();
            CrearInterfaz();

            Shown += async (_, _) =>
            {
                await CargarSelectorLigas();
                iniciando = false;
                await MostrarSeccion("Clasificación");
            };
        }

        private void CrearInterfaz()
        {
            SuspendLayout();
            Controls.Clear();

            Text = "FootballLeague - Gestor de Fútbol";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1350, 800);
            MinimumSize = new Size(1000, 600);
            BackColor = fondo;

            // MENÚ LATERAL
            Panel menu = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = azul
            };

            Controls.Add(menu);

            Label logo = new Label
            {
                Text = "FootballLeague",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(12, 25),
                Size = new Size(205, 55)
            };

            menu.Controls.Add(logo);

            string[] secciones =
            {
                "Ligas",
                "Equipos",
                "Jugadores",
                "Calendario",
                "Clasificación"
            };

            int y = 110;

            foreach (string seccion in secciones)
            {
                string nombre = seccion;

                Button boton = new Button
                {
                    Text = nombre,
                    Location = new Point(10, y),
                    Size = new Size(200, 48),
                    BackColor = azulClaro,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 11),
                    Cursor = Cursors.Hand
                };

                boton.FlatAppearance.BorderSize = 0;

                boton.Click += async (_, _) =>
                    await MostrarSeccion(nombre);

                menu.Controls.Add(boton);
                y += 58;
            }

            // CONTENIDO PRINCIPAL
            Panel contenido = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = fondo
            };

            Controls.Add(contenido);
            contenido.BringToFront();

            // Distribución fija:
            // Selector arriba, título debajo,
            // tabla en el centro y estado abajo.
            TableLayoutPanel distribucion = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = fondo,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            distribucion.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            distribucion.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 60));

            distribucion.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 70));

            distribucion.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            distribucion.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 35));

            contenido.Controls.Add(distribucion);

            // SELECTOR DE LIGA
            FlowLayoutPanel barra = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 8, 0, 0)
            };

            selectorLiga = new ComboBox
            {
                Width = 330,
                Height = 35,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11),
                Margin = new Padding(0, 0, 15, 0)
            };

            selectorLiga.SelectedIndexChanged +=
                async (_, _) =>
                {
                    if (!iniciando)
                        await MostrarSeccion(seccionActual);
                };

            barra.Controls.Add(selectorLiga);

            Button actualizar = new Button
            {
                Text = "Actualizar",
                Size = new Size(125, 35),
                BackColor = azulClaro,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            actualizar.FlatAppearance.BorderSize = 0;

            actualizar.Click += async (_, _) =>
            {
                await MostrarSeccion(seccionActual);
            };

            barra.Controls.Add(actualizar);

            distribucion.Controls.Add(barra, 0, 0);

            // TÍTULO
            titulo = new Label
            {
                Text = "Clasificación",
                Dock = DockStyle.Fill,
                Font = new Font(
                    "Segoe UI", 24, FontStyle.Bold),
                ForeColor = azul,
                TextAlign = ContentAlignment.MiddleLeft
            };

            distribucion.Controls.Add(titulo, 0, 1);

            // TABLA
            tabla = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                MultiSelect = false,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,

                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill,

                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10),
                ColumnHeadersHeight = 45,
                ColumnHeadersHeightSizeMode =
                    DataGridViewColumnHeadersHeightSizeMode.DisableResizing,

                ScrollBars = ScrollBars.Both,
                AutoGenerateColumns = true,
                EnableHeadersVisualStyles = false
            };

            tabla.RowTemplate.Height = 38;

            tabla.ColumnHeadersDefaultCellStyle.BackColor = azul;
            tabla.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            tabla.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 10, FontStyle.Bold);

            tabla.ColumnHeadersDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            tabla.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(215, 229, 250);

            tabla.DefaultCellStyle.SelectionForeColor =
                Color.Black;

            tabla.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(241, 245, 250);

            tabla.DataBindingComplete += (_, _) =>
            {
                AjustarColumnas();

                if (tabla.Rows.Count > 0)
                {
                    tabla.ClearSelection();

                    // Siempre empezamos por la primera fila.
                    tabla.FirstDisplayedScrollingRowIndex = 0;
                }
            };

            distribucion.Controls.Add(tabla, 0, 2);

            // ESTADO
            estado = new Label
            {
                Text = "Preparando conexión...",
                Dock = DockStyle.Fill,
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft
            };

            distribucion.Controls.Add(estado, 0, 3);

            ResumeLayout(true);
        }

        private void AjustarColumnas()
        {
            if (tabla.Columns.Count == 0)
                return;

            foreach (DataGridViewColumn columna in tabla.Columns)
            {
                columna.MinimumWidth = 55;
                columna.FillWeight = 65;
            }

            if (tabla.Columns.Contains("Equipo"))
            {
                tabla.Columns["Equipo"]!.MinimumWidth = 200;
                tabla.Columns["Equipo"]!.FillWeight = 250;
            }

            if (tabla.Columns.Contains("Nombre"))
            {
                tabla.Columns["Nombre"]!.MinimumWidth = 180;
                tabla.Columns["Nombre"]!.FillWeight = 180;
            }

            if (tabla.Columns.Contains("Local"))
            {
                tabla.Columns["Local"]!.MinimumWidth = 170;
                tabla.Columns["Local"]!.FillWeight = 180;
            }

            if (tabla.Columns.Contains("Visitante"))
            {
                tabla.Columns["Visitante"]!.MinimumWidth = 170;
                tabla.Columns["Visitante"]!.FillWeight = 180;
            }

            if (tabla.Columns.Contains("Puntos"))
            {
                tabla.Columns["Puntos"]!.FillWeight = 85;
            }
        }

        private async Task CargarSelectorLigas()
        {
            try
            {
                var datos =
                    await api.ObtenerListaAsync<JsonElement>(
                        "api/Ligas");

                var opciones = datos.Select(x => new LigaOpcion
                {
                    Id = Entero(x, "id"),
                    Nombre = Texto(x, "nombre")
                }).ToList();

                selectorLiga.DataSource = opciones;

                var principal =
                    opciones.FirstOrDefault(x => x.Id == 2);

                if (principal != null)
                    selectorLiga.SelectedItem = principal;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo conectar con la API.\n" +
                    "Comprueba que está ejecutándose.\n\n" +
                    ex.Message,
                    "Error de conexión",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private int LigaSeleccionada()
        {
            return (selectorLiga.SelectedItem
                as LigaOpcion)?.Id ?? 2;
        }

        private async Task MostrarSeccion(string seccion)
        {
            seccionActual = seccion;
            titulo.Text = seccion;
            estado.Text = "Cargando datos...";
            tabla.DataSource = null;

            try
            {
                int ligaId = LigaSeleccionada();

                switch (seccion)
                {
                    case "Ligas":
                        await CargarLigas();
                        break;

                    case "Equipos":
                        await CargarEquipos();
                        break;

                    case "Jugadores":
                        await CargarJugadores();
                        break;

                    case "Calendario":
                        await CargarCalendario(ligaId);
                        break;

                    case "Clasificación":
                        await CargarClasificacion(ligaId);
                        break;
                }

                estado.Text =
                    $"Datos cargados correctamente. " +
                    $"Registros: {tabla.Rows.Count}";
            }
            catch (Exception ex)
            {
                estado.Text = "Error al cargar datos.";

                MessageBox.Show(
                    ex.Message,
                    "FootballLeague",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task CargarLigas()
        {
            var datos =
                await api.ObtenerListaAsync<JsonElement>(
                    "api/Ligas");

            tabla.DataSource = datos.Select(x => new
            {
                ID = Entero(x, "id"),
                Nombre = Texto(x, "nombre"),
                Temporada = Texto(x, "temporada"),
                Activa = Booleano(x, "activa") ? "Sí" : "No"
            }).ToList();
        }

        private async Task CargarEquipos()
        {
            var datos =
                await api.ObtenerListaAsync<JsonElement>(
                    "api/Equipos");

            tabla.DataSource = datos.Select(x => new
            {
                ID = Entero(x, "id"),
                Equipo = Texto(x, "nombre"),
                Ciudad = Texto(x, "ciudad"),
                Estadio = Texto(x, "estadio")
            }).ToList();
        }

        private async Task CargarJugadores()
        {
            var datos =
                await api.ObtenerListaAsync<JsonElement>(
                    "api/Jugadores");

            tabla.DataSource = datos.Select(x => new
            {
                ID = Entero(x, "id"),
                Nombre = Texto(x, "nombre"),
                Dorsal = Entero(x, "dorsal"),
                Posicion = Texto(x, "posicion"),
                EquipoID = Entero(x, "equipoId")
            }).ToList();
        }

        private async Task CargarCalendario(int ligaId)
        {
            var jornadas =
                await api.ObtenerListaAsync<JsonElement>(
                    $"api/Calendario/liga/{ligaId}");

            var filas = new List<PartidoFila>();

            foreach (var jornada in jornadas)
            {
                int numero = Entero(jornada, "numero");

                if (!jornada.TryGetProperty(
                    "partidos", out var partidos) ||
                    partidos.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var partido in partidos.EnumerateArray())
                {
                    filas.Add(new PartidoFila
                    {
                        Jornada = numero,
                        PartidoID = Entero(partido, "id"),
                        Local = Texto(partido, "equipoLocal"),
                        Visitante = Texto(partido, "equipoVisitante"),
                        Resultado = Booleano(partido, "finalizado")
                            ? Texto(partido, "golesLocal") +
                              " - " +
                              Texto(partido, "golesVisitante")
                            : "Pendiente"
                    });
                }
            }

            tabla.DataSource = filas;
        }

        private async Task CargarClasificacion(int ligaId)
        {
            var datos =
                await api.ObtenerListaAsync<JsonElement>(
                    $"api/Clasificacion/liga/{ligaId}");

            tabla.DataSource = datos.Select(x => new
            {
                Pos = Entero(x, "posicion"),
                Equipo = Texto(x, "equipo"),
                PJ = Entero(x, "partidosJugados"),
                PG = Entero(x, "partidosGanados"),
                PE = Entero(x, "partidosEmpatados"),
                PP = Entero(x, "partidosPerdidos"),
                GF = Entero(x, "golesFavor"),
                GC = Entero(x, "golesContra"),
                DG = Entero(x, "diferenciaGoles"),
                Puntos = Entero(x, "puntos")
            }).ToList();
        }

        private static string Texto(
            JsonElement objeto, string propiedad)
        {
            if (objeto.ValueKind == JsonValueKind.Object &&
                objeto.TryGetProperty(propiedad, out var valor) &&
                valor.ValueKind != JsonValueKind.Null)
            {
                return valor.ValueKind == JsonValueKind.String
                    ? valor.GetString() ?? ""
                    : valor.ToString();
            }

            return "";
        }

        private static int Entero(
            JsonElement objeto, string propiedad)
        {
            return int.TryParse(
                Texto(objeto, propiedad), out int n) ? n : 0;
        }

        private static bool Booleano(
            JsonElement objeto, string propiedad)
        {
            return bool.TryParse(
                Texto(objeto, propiedad), out bool b) && b;
        }
    }
}
