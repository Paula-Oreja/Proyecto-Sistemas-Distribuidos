using FootballLeague.Cliente.Services;
using FootballLeague.Cliente.Forms;
using System.Drawing;
using System.Text.Json;

namespace FootballLeague.Cliente
{
    public partial class Form1 : Form
    {
        private readonly ApiService api = new();

        private readonly Color azul = Color.FromArgb(15, 23, 42);
        private readonly Color azulClaro = Color.FromArgb(37, 99, 235);
        private readonly Color fondo = Color.FromArgb(241, 245, 249);

        private DataGridView tabla = null!;
        private ComboBox selectorLiga = null!;
        private ComboBox selectorJornada = null!;
        private ComboBox selectorEquipo = null!;

        private Label titulo = null!;
        private Label estado = null!;
        private Label etiquetaJornada = null!;

        private FlowLayoutPanel barraAcciones = null!;
        private FlowLayoutPanel barraFiltro = null!;
        private TableLayoutPanel distribucion = null!;

        private Button botonCrearEquipo = null!;
        private Button botonEditarEquipo = null!;
        private Button botonEliminarEquipo = null!;

        private Button botonCrearJugador = null!;
        private Button botonEditarJugador = null!;
        private Button botonEliminarJugador = null!;
        private Button botonResultado = null!;

        private readonly List<PartidoFila> partidos = new();
        private readonly List<JugadorFila> jugadores = new();

        private string seccionActual = "Inicio";
        private Panel inicio = null!;
        private readonly Dictionary<string, Button> menuBotones = new();

        private bool iniciando = true;
        private bool cargandoJornadas;
        private bool cargandoSeccion;
        private bool cargandoEquipos;
        private bool operacionEnCurso;

        private class LigaOpcion
        {
            public int Id { get; set; }
            public string Nombre { get; set; } = "";

            public override string ToString()
            {
                return $"{Nombre} (ID {Id})";
            }
        }

        private class EquipoOpcion
        {
            public int Id { get; set; }
            public string Nombre { get; set; } = "";

            public override string ToString()
            {
                return Nombre;
            }
        }

        private class JugadorFila
        {
            public int ID { get; set; }
            public string Nombre { get; set; } = "";
            public int Dorsal { get; set; }
            public string Posicion { get; set; } = "";
            public string Equipo { get; set; } = "";
            public int EquipoId { get; set; }
        }

        private class PartidoFila
        {
            public int Jornada { get; set; }
            public int PartidoID { get; set; }
            public string Local { get; set; } = "";
            public string Visitante { get; set; } = "";
            public string Resultado { get; set; } = "Pendiente";
            public string Estado { get; set; } = "Pendiente";
        }

        public Form1()
        {
            InitializeComponent();
            CrearInterfaz();

            Shown += async (_, _) =>
            {
                await CargarSelectorLigas();
                iniciando = false;
                await MostrarSeccion("Inicio");
            };
        }

        private void CrearInterfaz()
        {
            SuspendLayout();
            Controls.Clear();

            Text = "FootballLeague - Gestor de Fútbol";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1400, 820);
            MinimumSize = new Size(1100, 650);
            BackColor = fondo;

            // ESTRUCTURA PRINCIPAL

            TableLayoutPanel estructura = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            estructura.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 220));

            estructura.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            estructura.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            Controls.Add(estructura);

            // MENÚ LATERAL

            Panel menu = new()
            {
                Dock = DockStyle.Fill,
                BackColor = azul
            };

            estructura.Controls.Add(menu, 0, 0);

            menu.Controls.Add(new Label
            {
                Text = "FootballLeague",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(12, 25),
                Size = new Size(200, 55)
            });

            string[] secciones =
            {
                "Inicio",
                "Ligas",
                "Equipos",
                "Jugadores",
                "Calendario",
                "Resultados",
                "Clasificación"
            };

            int y = 105;

            foreach (string seccion in secciones)
            {
                string nombre = seccion;

                Button boton = new()
                {
                    Text = (nombre switch
                    {
                        "Inicio" => "⌂",
                        "Ligas" => "★",
                        "Equipos" => "◆",
                        "Jugadores" => "●",
                        "Calendario" => "▦",
                        "Resultados" => "✓",
                        _ => "▥"
                    }) + "   " + nombre,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(14, 0, 0, 0),
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
                menuBotones[nombre] = boton;
                y += 57;
            }

            // CONTENIDO PRINCIPAL

            Panel contenido = new()
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = fondo
            };

            estructura.Controls.Add(contenido, 1, 0);

            distribucion = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            distribucion.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            distribucion.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 55));

            distribucion.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 70));

            distribucion.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 55));

            distribucion.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 0));

            distribucion.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            distribucion.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 35));

            contenido.Controls.Add(distribucion);

            // SELECTOR DE LIGA

            FlowLayoutPanel barraSuperior = new()
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                Padding = new Padding(0, 8, 0, 0)
            };

            selectorLiga = new ComboBox
            {
                Width = 350,
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

            barraSuperior.Controls.Add(selectorLiga);

            Button actualizar = CrearBoton("Actualizar", 125);

            actualizar.Click += async (_, _) =>
                await MostrarSeccion(seccionActual);

            barraSuperior.Controls.Add(actualizar);

            distribucion.Controls.Add(barraSuperior, 0, 0);

            // TÍTULO

            titulo = new Label
            {
                Text = "Inicio",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = azul,
                TextAlign = ContentAlignment.MiddleLeft
            };

            distribucion.Controls.Add(titulo, 0, 1);

            // BARRA DE ACCIONES

            barraAcciones = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                Padding = new Padding(0, 5, 0, 0)
            };

            etiquetaJornada = new Label
            {
                Text = "Jornada:",
                Width = 85,
                Height = 35,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 11)
            };

            barraAcciones.Controls.Add(etiquetaJornada);

            selectorJornada = new ComboBox
            {
                Width = 130,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11)
            };

            selectorJornada.SelectedIndexChanged += (_, _) =>
            {
                if (!cargandoJornadas && !operacionEnCurso)
                    MostrarPartidosJornada();
            };

            barraAcciones.Controls.Add(selectorJornada);

            // BOTÓN REGISTRAR RESULTADO

            botonResultado =
                CrearBoton("Registrar resultado", 190);

            botonResultado.Margin =
                new Padding(20, 0, 0, 0);

            botonResultado.Click += async (_, _) =>
                await RegistrarResultado();

            barraAcciones.Controls.Add(botonResultado);

            // BOTONES DE EQUIPOS

            botonCrearEquipo =
                CrearBoton("+ Crear equipo", 160);

            botonCrearEquipo.Click += async (_, _) =>
                await AbrirCrearEquipo();

            barraAcciones.Controls.Add(botonCrearEquipo);

            botonEditarEquipo =
                CrearBoton("Editar equipo", 160);

            botonEditarEquipo.Margin =
                new Padding(12, 0, 0, 0);

            botonEditarEquipo.Click += async (_, _) =>
                await AbrirEditarEquipo();

            barraAcciones.Controls.Add(botonEditarEquipo);

            // NUEVO BOTÓN ELIMINAR EQUIPO

            botonEliminarEquipo =
                CrearBoton("Eliminar equipo", 170);

            botonEliminarEquipo.BackColor =
                Color.FromArgb(170, 45, 45);

            botonEliminarEquipo.Margin =
                new Padding(12, 0, 0, 0);

            botonEliminarEquipo.Click += async (_, _) =>
                await EliminarEquipo();

            barraAcciones.Controls.Add(botonEliminarEquipo);

            // BOTONES DE JUGADORES

            botonCrearJugador =
                CrearBoton("+ Crear jugador", 160);

            botonCrearJugador.Click += async (_, _) =>
                await AbrirCrearJugador();

            barraAcciones.Controls.Add(botonCrearJugador);

            botonEditarJugador =
                CrearBoton("Editar jugador", 160);

            botonEditarJugador.Margin =
                new Padding(12, 0, 0, 0);

            botonEditarJugador.Click += async (_, _) =>
                await AbrirEditarJugador();

            barraAcciones.Controls.Add(botonEditarJugador);

            botonEliminarJugador =
                CrearBoton("Eliminar jugador", 170);

            botonEliminarJugador.BackColor =
                Color.FromArgb(170, 45, 45);

            botonEliminarJugador.Margin =
                new Padding(12, 0, 0, 0);

            botonEliminarJugador.Click += async (_, _) =>
                await EliminarJugador();

            barraAcciones.Controls.Add(botonEliminarJugador);

            distribucion.Controls.Add(barraAcciones, 0, 2);

            // FILTRO DE JUGADORES

            barraFiltro = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                Padding = new Padding(0, 5, 0, 0)
            };

            Label etiquetaEquipo = new()
            {
                Text = "Filtrar por equipo:",
                Width = 155,
                Height = 35,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = azul,
                TextAlign = ContentAlignment.MiddleLeft
            };

            barraFiltro.Controls.Add(etiquetaEquipo);

            selectorEquipo = new ComboBox
            {
                Width = 350,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11),
                Margin = new Padding(0, 3, 15, 0),
                MaxDropDownItems = 15
            };

            selectorEquipo.SelectedIndexChanged += (_, _) =>
            {
                if (!cargandoEquipos &&
                    !cargandoSeccion &&
                    !operacionEnCurso &&
                    seccionActual == "Jugadores")
                {
                    MostrarJugadoresFiltrados();
                }
            };

            barraFiltro.Controls.Add(selectorEquipo);

            Button limpiarFiltro =
                CrearBoton("Todos los equipos", 170);

            limpiarFiltro.Click += (_, _) =>
            {
                if (selectorEquipo.Items.Count > 0)
                    selectorEquipo.SelectedIndex = 0;
            };

            barraFiltro.Controls.Add(limpiarFiltro);

            distribucion.Controls.Add(barraFiltro, 0, 3);

            // TABLA PRINCIPAL

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
                RowTemplate = { Height = 38 },
                ColumnHeadersHeight = 45,
                ColumnHeadersHeightSizeMode =
                    DataGridViewColumnHeadersHeightSizeMode
                        .DisableResizing,
                ScrollBars = ScrollBars.Both,
                EnableHeadersVisualStyles = false
            };

            tabla.ColumnHeadersDefaultCellStyle.BackColor = azul;

            tabla.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            tabla.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 10, FontStyle.Bold);

            tabla.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(215, 229, 250);

            tabla.DefaultCellStyle.SelectionForeColor =
                Color.Black;

            tabla.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(241, 245, 250);

            tabla.DataBindingComplete += (_, _) =>
            {
                AjustarColumnas();
                tabla.ClearSelection();

                if (tabla.Rows.Count > 0)
                {
                    try
                    {
                        tabla.FirstDisplayedScrollingRowIndex = 0;
                    }
                    catch
                    {
                    }
                }
            };

            var areaPrincipal = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            distribucion.Controls.Add(areaPrincipal, 0, 4);
            areaPrincipal.Controls.Add(tabla);
            inicio = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = fondo };
            areaPrincipal.Controls.Add(inicio);
            tabla.CellFormatting += (_, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                if (seccionActual == "Clasificación" && tabla.Columns[e.ColumnIndex].Name == "Pos" && e.Value is int pos)
                {
                    e.CellStyle.BackColor = pos <= 4 ? Color.FromArgb(219, 234, 254) :
                        pos >= 18 ? Color.FromArgb(254, 226, 226) : Color.White;
                    e.CellStyle.ForeColor = pos >= 18 ? Color.DarkRed : azul;
                }
            };
            tabla.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            tabla.GridColor = Color.FromArgb(226, 232, 240);
            tabla.DefaultCellStyle.Padding = new Padding(5, 0, 5, 0);

            // ESTADO INFERIOR

            estado = new Label
            {
                Text = "Preparando conexión...",
                Dock = DockStyle.Fill,
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft
            };

            distribucion.Controls.Add(estado, 0, 5);

            ActualizarBotones();

            ResumeLayout(true);
        }

        // CREAR BOTONES

        private Button CrearBoton(string texto, int ancho)
        {
            Button boton = new()
            {
                Text = texto,
                Size = new Size(ancho, 35),
                BackColor = azulClaro,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            boton.FlatAppearance.BorderSize = 0;

            return boton;
        }

        // MOSTRAR BOTONES SEGÚN LA SECCIÓN

        private void ActualizarBotones()
        {
            bool esCalendario = seccionActual == "Calendario";
            bool esResultados = seccionActual == "Resultados";
            bool esEquipos = seccionActual == "Equipos";
            bool esJugadores = seccionActual == "Jugadores";

            barraAcciones.Visible = esCalendario || esResultados || esEquipos || esJugadores;
            distribucion.RowStyles[2].Height = (esCalendario || esResultados || esEquipos || esJugadores) ? 55 : 0;
            bool esInicio = seccionActual == "Inicio";
            tabla.Visible = !esInicio;
            inicio.Visible = esInicio;
            if (esInicio) inicio.BringToFront(); else tabla.BringToFront();
            foreach (var par in menuBotones)
                par.Value.BackColor = par.Key == seccionActual ? azulClaro : azul;

            etiquetaJornada.Visible =
                esCalendario || esResultados;

            selectorJornada.Visible =
                esCalendario || esResultados;

            botonResultado.Visible = esResultados;

            botonCrearEquipo.Visible = esEquipos;
            botonEditarEquipo.Visible = esEquipos;
            botonEliminarEquipo.Visible = esEquipos;

            botonCrearJugador.Visible = esJugadores;
            botonEditarJugador.Visible = esJugadores;
            botonEliminarJugador.Visible = esJugadores;

            barraFiltro.Visible = esJugadores;

            distribucion.RowStyles[3].Height =
                esJugadores ? 50 : 0;
        }

        // AJUSTAR COLUMNAS

        private void AjustarColumnas()
        {
            foreach (DataGridViewColumn columna in tabla.Columns)
            {
                columna.MinimumWidth = 55;
                columna.FillWeight = 65;
            }

            foreach (string nombre in new[]
            {
                "Equipo", "Nombre", "Local", "Visitante"
            })
            {
                if (tabla.Columns.Contains(nombre))
                {
                    tabla.Columns[nombre]!.MinimumWidth = 180;
                    tabla.Columns[nombre]!.FillWeight = 200;
                }
            }

            if (tabla.Columns.Contains("PartidoID"))
            {
                tabla.Columns["PartidoID"]!.HeaderText =
                    "ID Partido";
            }

            if (tabla.Columns.Contains("Posicion"))
            {
                tabla.Columns["Posicion"]!.HeaderText = "Posición";
                tabla.Columns["Posicion"]!.MinimumWidth = 130;
                tabla.Columns["Posicion"]!.FillWeight = 110;
            }

            if (tabla.Columns.Contains("ID"))
            {
                tabla.Columns["ID"]!.Visible =
                    seccionActual != "Jugadores";
            }

            if (tabla.Columns.Contains("EquipoId"))
            {
                tabla.Columns["EquipoId"]!.Visible = false;
            }
        }

        // CARGAR LIGAS EN EL SELECTOR

        private async Task CargarSelectorLigas()
        {
            try
            {
                var datos =
                    await api.ObtenerListaAsync<JsonElement>(
                        "api/Ligas");

                var opciones = datos.Select(x =>
                    new LigaOpcion
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
            return
                (selectorLiga.SelectedItem as LigaOpcion)
                ?.Id ?? 2;
        }

        // CARGAR EQUIPOS EN EL SELECTOR

        private async Task CargarSelectorEquipos()
        {
            int equipoAnterior =
                (selectorEquipo.SelectedItem as EquipoOpcion)
                ?.Id ?? 0;

            cargandoEquipos = true;

            try
            {
                var datos =
                    await api.ObtenerListaAsync<JsonElement>(
                        "api/Equipos");

                var opciones = new List<EquipoOpcion>
                {
                    new EquipoOpcion
                    {
                        Id = 0,
                        Nombre = "Todos los equipos"
                    }
                };

                opciones.AddRange(
                    datos.Select(x => new EquipoOpcion
                    {
                        Id = Entero(x, "id"),
                        Nombre = Texto(x, "nombre")
                    })
                    .OrderBy(x => x.Nombre));

                selectorEquipo.DataSource = null;
                selectorEquipo.DataSource = opciones;

                var anterior = opciones.FirstOrDefault(
                    x => x.Id == equipoAnterior);

                selectorEquipo.SelectedItem =
                    anterior ?? opciones[0];
            }
            finally
            {
                cargandoEquipos = false;
            }
        }

        // MOSTRAR SECCIÓN

        private async Task MostrarSeccion(string seccion)
        {
            if (cargandoSeccion || operacionEnCurso)
                return;

            cargandoSeccion = true;
            seccionActual = seccion;

            titulo.Text = seccion;
            estado.Text = "Cargando datos...";

            ActualizarBotones();

            barraAcciones.Enabled = false;
            barraFiltro.Enabled = false;
            selectorLiga.Enabled = false;

            tabla.DataSource = null;

            try
            {
                int ligaId = LigaSeleccionada();

                switch (seccion)
                {
                    case "Inicio":
                        await CargarInicio(ligaId);
                        break;
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
                    case "Resultados":
                        await CargarCalendario(ligaId);
                        break;

                    case "Clasificación":
                        await CargarClasificacion(ligaId);
                        break;
                }

                if (seccion != "Inicio" && seccion != "Jugadores" &&
                    seccion != "Calendario" &&
                    seccion != "Resultados")
                {
                    estado.Text =
                        "Datos cargados correctamente. " +
                        $"Registros visibles: {tabla.Rows.Count}";
                }
            }
            catch (Exception ex)
            {
                estado.Text = "Error al cargar los datos.";

                MessageBox.Show(
                    ex.Message,
                    "FootballLeague",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                cargandoSeccion = false;

                barraAcciones.Enabled = true;
                barraFiltro.Enabled = true;
                selectorLiga.Enabled = true;
            }
        }

        private async Task CargarInicio(int ligaId)
        {
            foreach (Control control in inicio.Controls.Cast<Control>().ToArray()) control.Dispose();
            var equipos = await api.ObtenerListaAsync<JsonElement>("api/Clasificacion/liga/" + ligaId);
            var todosJugadores = await api.ObtenerListaAsync<JsonElement>("api/Jugadores");
            var jornadas = await api.ObtenerListaAsync<JsonElement>("api/Calendario/liga/" + ligaId);
            var encuentros = jornadas.SelectMany(j => j.TryGetProperty("partidos", out var p) && p.ValueKind == JsonValueKind.Array ? p.EnumerateArray().ToArray() : Array.Empty<JsonElement>()).ToList();
            var terminados = encuentros.Where(p => Booleano(p, "finalizado")).ToList();
            var ids = equipos.Select(e => Entero(e, "equipoId")).ToHashSet();
            int totalJugadores = (ids.Contains(0) || ids.Count == 0) ? todosJugadores.Count : todosJugadores.Count(j => ids.Contains(Entero(j, "equipoId")));
            var panel = new TableLayoutPanel { Dock = DockStyle.Top, Height = 420, ColumnCount = 2, RowCount = 4, BackColor = fondo };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            for (int i = 0; i < 4; i++) panel.RowStyles.Add(new RowStyle(SizeType.Absolute, i == 3 ? 90 : 110));
            void Tarjeta(string nombre, string valor, int col, int fila)
            {
                var tarjeta = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(5, 5, 12, 8), Padding = new Padding(16) };
                tarjeta.Controls.Add(new Label { Text = nombre, Dock = DockStyle.Top, Height = 27, ForeColor = Color.DimGray, Font = new Font("Segoe UI", 11) });
                var cifra = new Label { Text = valor, Dock = DockStyle.Bottom, Height = 45, ForeColor = azul, Font = new Font("Segoe UI", 20, FontStyle.Bold), AutoEllipsis = true };
                tarjeta.Controls.Add(cifra);
                panel.Controls.Add(tarjeta, col, fila);
            }
            Tarjeta("Equipos inscritos", equipos.Count.ToString(), 0, 0);
            Tarjeta((ids.Contains(0) || ids.Count == 0) ? "Jugadores registrados (total)" : "Jugadores de la liga", totalJugadores.ToString(), 1, 0);
            Tarjeta("Jornadas", jornadas.Count.ToString(), 0, 1);
            Tarjeta("Partidos programados", encuentros.Count.ToString(), 1, 1);
            Tarjeta("Partidos finalizados", terminados.Count.ToString(), 0, 2);
            Tarjeta("Partidos pendientes", (encuentros.Count - terminados.Count).ToString(), 1, 2);
            var resumen = new Label { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(16), Margin = new Padding(5, 5, 12, 8), Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = azul, TextAlign = ContentAlignment.MiddleLeft };
            var partido = terminados.LastOrDefault();
            resumen.Text = terminados.Count == 0 ? "Todavía no hay resultados registrados." : $"Resultado de ejemplo de la liga:  {NombreEquipo(partido, "equipoLocal")}   {Texto(partido, "golesLocal")} - {Texto(partido, "golesVisitante")}   {NombreEquipo(partido, "equipoVisitante")}";
            panel.Controls.Add(resumen, 0, 3);
            panel.SetColumnSpan(resumen, 2);
            inicio.Controls.Add(panel);
            estado.Text = "Resumen de la liga actualizado correctamente.";
        }

        // CREAR EQUIPO

        private async Task AbrirCrearEquipo()
        {
            using CrearEquipoForm ventana = new();

            if (ventana.ShowDialog(this) == DialogResult.OK)
            {
                await MostrarSeccion("Equipos");
            }
        }

        // OBTENER EQUIPO SELECCIONADO

        private int? ObtenerEquipoSeleccionado()
        {
            if (seccionActual != "Equipos" ||
                tabla.SelectedRows.Count == 0 ||
                !tabla.Columns.Contains("ID"))
            {
                MessageBox.Show(
                    "Selecciona primero un equipo de la tabla.",
                    "FootballLeague",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return null;
            }

            object? valor =
                tabla.SelectedRows[0].Cells["ID"].Value;

            if (valor == null ||
                !int.TryParse(valor.ToString(), out int equipoId))
            {
                MessageBox.Show(
                    "No se pudo identificar el equipo.");

                return null;
            }

            return equipoId;
        }

        // EDITAR EQUIPO

        private async Task AbrirEditarEquipo()
        {
            int? equipoId = ObtenerEquipoSeleccionado();

            if (equipoId == null)
                return;

            using EditarEquipoForm ventana =
                new(equipoId.Value);

            if (ventana.ShowDialog(this) == DialogResult.OK)
            {
                await MostrarSeccion("Equipos");
            }
        }

        // ELIMINAR EQUIPO

        private async Task EliminarEquipo()
        {
            int? equipoId = ObtenerEquipoSeleccionado();

            if (equipoId == null)
                return;

            string nombre =
                tabla.SelectedRows[0]
                .Cells["Equipo"].Value?.ToString()
                ?? "el equipo seleccionado";

            DialogResult confirmacion = MessageBox.Show(
                $"¿Seguro que deseas eliminar a {nombre}?\n\n" +
                "Esta acción no se puede deshacer.",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (confirmacion != DialogResult.Yes)
                return;

            bool eliminado = false;

            operacionEnCurso = true;
            barraAcciones.Enabled = false;
            selectorLiga.Enabled = false;
            tabla.Enabled = false;

            try
            {
                using var respuesta =
                    await api.EliminarAsync(
                        $"api/Equipos/{equipoId.Value}");

                if (respuesta.IsSuccessStatusCode)
                {
                    eliminado = true;

                    MessageBox.Show(
                        "Equipo eliminado correctamente.",
                        "FootballLeague",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else if ((int)respuesta.StatusCode == 409)
                {
                    MessageBox.Show(
                        $"No se puede eliminar a {nombre}.\n\n" +
                        "El equipo tiene inscripciones, jugadores " +
                        "o partidos asociados.",
                        "Eliminación no permitida",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                else if ((int)respuesta.StatusCode == 404)
                {
                    MessageBox.Show(
                        "El equipo ya no existe.",
                        "Equipo no encontrado",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    eliminado = true;
                }
                else
                {
                    string detalle =
                        await respuesta.Content.ReadAsStringAsync();

                    MessageBox.Show(
                        "No se pudo eliminar el equipo.\n\n" +
                        detalle,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error al eliminar equipo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                operacionEnCurso = false;
                barraAcciones.Enabled = true;
                selectorLiga.Enabled = true;
                tabla.Enabled = true;
            }

            if (eliminado)
                await MostrarSeccion("Equipos");
        }

        // CREAR JUGADOR

        private async Task AbrirCrearJugador()
        {
            using CrearJugadorForm ventana = new();

            if (ventana.ShowDialog(this) == DialogResult.OK)
            {
                await MostrarSeccion("Jugadores");
            }
        }

        // OBTENER JUGADOR SELECCIONADO

        private int? ObtenerJugadorSeleccionado()
        {
            if (seccionActual != "Jugadores" ||
                tabla.SelectedRows.Count == 0 ||
                !tabla.Columns.Contains("ID"))
            {
                MessageBox.Show(
                    "Selecciona primero un jugador de la tabla.",
                    "FootballLeague",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return null;
            }

            object? valor =
                tabla.SelectedRows[0].Cells["ID"].Value;

            if (valor == null ||
                !int.TryParse(valor.ToString(), out int jugadorId))
            {
                MessageBox.Show(
                    "No se pudo identificar el jugador.");

                return null;
            }

            return jugadorId;
        }

        // EDITAR JUGADOR

        private async Task AbrirEditarJugador()
        {
            int? jugadorId = ObtenerJugadorSeleccionado();

            if (jugadorId == null)
                return;

            using EditarJugadorForm ventana =
                new(jugadorId.Value);

            if (ventana.ShowDialog(this) == DialogResult.OK)
            {
                await MostrarSeccion("Jugadores");
            }
        }

        // ELIMINAR JUGADOR

        private async Task EliminarJugador()
        {
            int? jugadorId = ObtenerJugadorSeleccionado();

            if (jugadorId == null)
                return;

            string nombre =
                tabla.SelectedRows[0]
                .Cells["Nombre"].Value?.ToString()
                ?? "el jugador seleccionado";

            DialogResult confirmacion = MessageBox.Show(
                $"¿Seguro que deseas eliminar a {nombre}?\n\n" +
                "Esta acción no se puede deshacer.",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (confirmacion != DialogResult.Yes)
                return;

            bool eliminado = false;

            operacionEnCurso = true;
            barraAcciones.Enabled = false;
            barraFiltro.Enabled = false;
            selectorLiga.Enabled = false;
            tabla.Enabled = false;

            try
            {
                using var respuesta =
                    await api.EliminarAsync(
                        $"api/Jugadores/{jugadorId.Value}");

                if (respuesta.IsSuccessStatusCode)
                {
                    eliminado = true;

                    MessageBox.Show(
                        "Jugador eliminado correctamente.",
                        "FootballLeague",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    string detalle =
                        await respuesta.Content.ReadAsStringAsync();

                    MessageBox.Show(
                        "No se pudo eliminar el jugador.\n\n" +
                        detalle,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error al eliminar jugador",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                operacionEnCurso = false;
                barraAcciones.Enabled = true;
                barraFiltro.Enabled = true;
                selectorLiga.Enabled = true;
                tabla.Enabled = true;
            }

            if (eliminado)
                await MostrarSeccion("Jugadores");
        }

        // CARGAR LIGAS

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

        // CARGAR EQUIPOS

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

        // CARGAR JUGADORES

        private async Task CargarJugadores()
        {
            jugadores.Clear();

            await CargarSelectorEquipos();

            var datos =
                await api.ObtenerListaAsync<JsonElement>(
                    "api/Jugadores");

            jugadores.AddRange(
                datos.Select(x => new JugadorFila
                {
                    ID = Entero(x, "id"),
                    Nombre = Texto(x, "nombre"),
                    Dorsal = Entero(x, "dorsal"),
                    Posicion = Texto(x, "posicion"),
                    Equipo = Texto(x, "nombreEquipo"),
                    EquipoId = Entero(x, "equipoId")
                }));

            MostrarJugadoresFiltrados();
        }

        // FILTRAR JUGADORES

        private void MostrarJugadoresFiltrados()
        {
            if (seccionActual != "Jugadores")
                return;

            var equipoSeleccionado =
                selectorEquipo.SelectedItem as EquipoOpcion;

            int equipoId = equipoSeleccionado?.Id ?? 0;

            string nombreEquipo =
                equipoSeleccionado?.Nombre ??
                "Todos los equipos";

            IEnumerable<JugadorFila> resultado = jugadores;

            if (equipoId != 0)
            {
                resultado = resultado.Where(
                    j => j.EquipoId == equipoId);
            }

            var lista = resultado
                .OrderBy(j => j.Equipo)
                .ThenBy(j => j.Dorsal)
                .ToList();

            tabla.DataSource = null;
            tabla.DataSource = lista;
            tabla.ClearSelection();

            if (tabla.Rows.Count > 0)
            {
                tabla.FirstDisplayedScrollingRowIndex = 0;
            }

            estado.Text =
                $"{nombreEquipo}: {lista.Count} jugadores. " +
                $"Total registrado: {jugadores.Count}.";
        }

        // CARGAR CALENDARIO

        private async Task CargarCalendario(int ligaId)
        {
            int jornadaAnterior = 1;

            if (selectorJornada.SelectedItem is int anterior)
                jornadaAnterior = anterior;

            var jornadas =
                await api.ObtenerListaAsync<JsonElement>(
                    $"api/Calendario/liga/{ligaId}");

            partidos.Clear();

            foreach (var jornada in jornadas)
            {
                int numero = Entero(jornada, "numero");

                if (!jornada.TryGetProperty(
                        "partidos", out var lista) ||
                    lista.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var partido in lista.EnumerateArray())
                {
                    bool finalizado =
                        Booleano(partido, "finalizado");

                    partidos.Add(new PartidoFila
                    {
                        Jornada = numero,
                        PartidoID = Entero(partido, "id"),

                        Local =
                            NombreEquipo(partido, "equipoLocal"),

                        Visitante =
                            NombreEquipo(partido, "equipoVisitante"),

                        Resultado = finalizado
                            ? $"{Texto(partido, "golesLocal")} - " +
                              $"{Texto(partido, "golesVisitante")}"
                            : "Pendiente",

                        Estado = finalizado
                            ? "Finalizado"
                            : "Pendiente"
                    });
                }
            }

            cargandoJornadas = true;

            try
            {
                var numeros = partidos
                    .Select(p => p.Jornada)
                    .Distinct()
                    .OrderBy(n => n)
                    .ToList();

                selectorJornada.DataSource = null;
                selectorJornada.DataSource = numeros;

                if (numeros.Contains(jornadaAnterior))
                {
                    selectorJornada.SelectedItem =
                        jornadaAnterior;
                }
            }
            finally
            {
                cargandoJornadas = false;
            }

            MostrarPartidosJornada();
        }

        // MOSTRAR PARTIDOS POR JORNADA

        private void MostrarPartidosJornada()
        {
            if (seccionActual != "Calendario" &&
                seccionActual != "Resultados")
            {
                return;
            }

            if (selectorJornada.SelectedItem is not int numero)
            {
                tabla.DataSource = null;
                estado.Text = "No hay jornadas disponibles.";
                return;
            }

            tabla.DataSource = partidos
                .Where(p => p.Jornada == numero)
                .OrderBy(p => p.PartidoID)
                .ToList();

            estado.Text =
                $"Jornada {numero}: {tabla.Rows.Count} partidos.";
        }

        // CARGAR CLASIFICACIÓN

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

        // REGISTRAR RESULTADOS

        private async Task RegistrarResultado()
        {
            if (tabla.SelectedRows.Count == 0 ||
                tabla.SelectedRows[0].DataBoundItem
                    is not PartidoFila partido)
            {
                MessageBox.Show(
                    "Selecciona primero un partido de la tabla.",
                    "Seleccionar partido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            bool actualizar = false;

            operacionEnCurso = true;
            barraAcciones.Enabled = false;
            selectorLiga.Enabled = false;
            tabla.Enabled = false;

            try
            {
                var datos =
                    await api.ObtenerAsync<JsonElement>(
                        $"api/Resultados/{partido.PartidoID}");

                if (datos.ValueKind != JsonValueKind.Object)
                {
                    MessageBox.Show("No se encontró el partido.");
                    return;
                }

                int version = Entero(datos, "version");

                if (version < 1)
                {
                    MessageBox.Show(
                        "No se pudo obtener la versión del partido.");

                    return;
                }

                using Form ventana = new()
                {
                    Text = "Registrar resultado",
                    ClientSize = new Size(440, 300),
                    StartPosition =
                        FormStartPosition.CenterParent,
                    FormBorderStyle =
                        FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    BackColor = fondo
                };

                ventana.Controls.Add(new Label
                {
                    Text =
                        $"{partido.Local} VS {partido.Visitante}",
                    Location = new Point(20, 20),
                    Size = new Size(400, 55),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font(
                        "Segoe UI", 13, FontStyle.Bold)
                });

                ventana.Controls.Add(new Label
                {
                    Text = "Goles local",
                    Location = new Point(65, 95),
                    AutoSize = true
                });

                ventana.Controls.Add(new Label
                {
                    Text = "Goles visitante",
                    Location = new Point(245, 95),
                    AutoSize = true
                });

                NumericUpDown golesLocal = new()
                {
                    Location = new Point(65, 125),
                    Width = 100,
                    Minimum = 0,
                    Maximum = 99,
                    Value = Math.Clamp(
                        Entero(datos, "golesLocal"), 0, 99)
                };

                ventana.Controls.Add(golesLocal);

                NumericUpDown golesVisitante = new()
                {
                    Location = new Point(245, 125),
                    Width = 100,
                    Minimum = 0,
                    Maximum = 99,
                    Value = Math.Clamp(
                        Entero(datos, "golesVisitante"), 0, 99)
                };

                ventana.Controls.Add(golesVisitante);

                Button confirmar =
                    CrearBoton("Guardar resultado", 170);

                confirmar.Location = new Point(135, 205);
                confirmar.DialogResult = DialogResult.OK;

                ventana.Controls.Add(confirmar);
                ventana.AcceptButton = confirmar;

                if (ventana.ShowDialog(this) != DialogResult.OK)
                    return;

                var cuerpo = new
                {
                    golesLocal = (int)golesLocal.Value,
                    golesVisitante = (int)golesVisitante.Value,
                    version
                };

                using var respuesta =
                    await api.ModificarAsync(
                        $"api/Resultados/{partido.PartidoID}",
                        cuerpo);

                if (respuesta.IsSuccessStatusCode)
                {
                    actualizar = true;

                    MessageBox.Show(
                        "Resultado guardado correctamente.",
                        "FootballLeague",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else if ((int)respuesta.StatusCode == 409)
                {
                    actualizar = true;

                    MessageBox.Show(
                        "El partido ha sido modificado " +
                        "por otro usuario.\n" +
                        "Se actualizarán los datos.",
                        "Conflicto de concurrencia",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                else
                {
                    string detalle =
                        await respuesta.Content.ReadAsStringAsync();

                    MessageBox.Show(
                        "No se pudo guardar el resultado.\n" +
                        $"Código: {(int)respuesta.StatusCode}\n\n" +
                        detalle,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error al registrar resultado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                operacionEnCurso = false;
                barraAcciones.Enabled = true;
                selectorLiga.Enabled = true;
                tabla.Enabled = true;
            }

            if (actualizar)
                await MostrarSeccion("Resultados");
        }

        // MÉTODOS AUXILIARES

        private static string NombreEquipo(
            JsonElement partido, string propiedad)
        {
            if (!partido.TryGetProperty(
                propiedad, out var valor))
            {
                return "Equipo desconocido";
            }

            if (valor.ValueKind == JsonValueKind.String)
            {
                return valor.GetString()
                    ?? "Equipo desconocido";
            }

            if (valor.ValueKind == JsonValueKind.Object &&
                valor.TryGetProperty("nombre", out var nombre))
            {
                return nombre.GetString()
                    ?? "Equipo desconocido";
            }

            return "Equipo desconocido";
        }

        private static string Texto(
            JsonElement objeto, string propiedad)
        {
            if (objeto.ValueKind == JsonValueKind.Object &&
                objeto.TryGetProperty(propiedad, out var valor) &&
                valor.ValueKind != JsonValueKind.Null &&
                valor.ValueKind != JsonValueKind.Undefined)
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
                Texto(objeto, propiedad), out int numero)
                ? numero
                : 0;
        }

        private static bool Booleano(
            JsonElement objeto, string propiedad)
        {
            return bool.TryParse(
                Texto(objeto, propiedad), out bool valor)
                && valor;
        }
    }
}