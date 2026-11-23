
using FootballLeague.Cliente.Services;
using FootballLeague.Cliente.Forms;
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
        private ComboBox selectorJornada = null!;
        private Label titulo = null!;
        private Label estado = null!;
        private Label etiquetaJornada = null!;
        private FlowLayoutPanel barraAcciones = null!;
        private Button botonCrearEquipo = null!;
        private Button botonEditarEquipo = null!;
        private Button botonResultado = null!;

        private readonly List<PartidoFila> partidos = new();

        private string seccionActual = "Clasificación";
        private bool iniciando = true;
        private bool cargandoJornadas = false;

        private class LigaOpcion
        {
            public int Id { get; set; }
            public string Nombre { get; set; } = "";

            public override string ToString()
                => $"{Nombre} (ID {Id})";
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

            Panel contenido = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = fondo
            };

            Controls.Add(contenido);

            Panel menu = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = azul
            };

            Controls.Add(menu);
            menu.BringToFront();

            menu.Controls.Add(new Label
            {
                Text = "FootballLeague",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(12, 25),
                Size = new Size(205, 55)
            });

            string[] secciones =
            {
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
                y += 57;
            }

            TableLayoutPanel distribucion = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
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
                new RowStyle(SizeType.Percent, 100));

            distribucion.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 35));

            contenido.Controls.Add(distribucion);

            FlowLayoutPanel barraSuperior = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                Padding = new Padding(0, 8, 0, 0)
            };

            selectorLiga = new ComboBox
            {
                Width = 330,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11),
                Margin = new Padding(0, 0, 15, 0)
            };

            selectorLiga.SelectedIndexChanged += async (_, _) =>
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

            titulo = new Label
            {
                Text = "Clasificación",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = azul,
                TextAlign = ContentAlignment.MiddleLeft
            };

            distribucion.Controls.Add(titulo, 0, 1);

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
                if (!cargandoJornadas)
                    MostrarPartidosJornada();
            };

            barraAcciones.Controls.Add(selectorJornada);

            botonResultado = CrearBoton(
                "Registrar resultado", 190);

            botonResultado.Margin = new Padding(20, 0, 0, 0);
            botonResultado.Click += async (_, _) =>
                await RegistrarResultado();

            barraAcciones.Controls.Add(botonResultado);

            botonCrearEquipo = CrearBoton("+ Crear equipo", 160);

            botonCrearEquipo.Click += async (_, _) =>
                await AbrirCrearEquipo();

            barraAcciones.Controls.Add(botonCrearEquipo);

            botonEditarEquipo = CrearBoton("Editar equipo", 160);
            botonEditarEquipo.Margin = new Padding(12, 0, 0, 0);

            botonEditarEquipo.Click += async (_, _) =>
                await AbrirEditarEquipo();

            barraAcciones.Controls.Add(botonEditarEquipo);

            distribucion.Controls.Add(barraAcciones, 0, 2);

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
                    DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ScrollBars = ScrollBars.Both,
                EnableHeadersVisualStyles = false
            };

            tabla.ColumnHeadersDefaultCellStyle.BackColor = azul;
            tabla.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            tabla.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 10, FontStyle.Bold);

            tabla.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(215, 229, 250);

            tabla.DefaultCellStyle.SelectionForeColor = Color.Black;

            tabla.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(241, 245, 250);

            tabla.DataBindingComplete += (_, _) =>
            {
                AjustarColumnas();
                tabla.ClearSelection();

                if (tabla.Rows.Count > 0)
                    tabla.FirstDisplayedScrollingRowIndex = 0;
            };

            distribucion.Controls.Add(tabla, 0, 3);

            estado = new Label
            {
                Text = "Preparando conexión...",
                Dock = DockStyle.Fill,
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft
            };

            distribucion.Controls.Add(estado, 0, 4);

            ActualizarBotones();
            ResumeLayout(true);
        }

        private Button CrearBoton(string texto, int ancho)
        {
            Button boton = new Button
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

        private void ActualizarBotones()
        {
            bool esCalendario = seccionActual == "Calendario";
            bool esResultados = seccionActual == "Resultados";
            bool esEquipos = seccionActual == "Equipos";

            barraAcciones.Visible =
                esCalendario || esResultados || esEquipos;

            etiquetaJornada.Visible = esCalendario || esResultados;
            selectorJornada.Visible = esCalendario || esResultados;

            botonResultado.Visible = esResultados;
            botonCrearEquipo.Visible = esEquipos;
            botonEditarEquipo.Visible = esEquipos;
        }

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
                tabla.Columns["PartidoID"]!.HeaderText = "ID Partido";
        }

        private async Task CargarSelectorLigas()
        {
            try
            {
                var datos =
                    await api.ObtenerListaAsync<JsonElement>("api/Ligas");

                var opciones = datos.Select(x => new LigaOpcion
                {
                    Id = Entero(x, "id"),
                    Nombre = Texto(x, "nombre")
                }).ToList();

                selectorLiga.DataSource = opciones;

                var principal = opciones.FirstOrDefault(x => x.Id == 2);

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
            return (selectorLiga.SelectedItem as LigaOpcion)?.Id ?? 2;
        }

        private async Task MostrarSeccion(string seccion)
        {
            seccionActual = seccion;
            titulo.Text = seccion;
            estado.Text = "Cargando datos...";

            ActualizarBotones();
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
                    case "Resultados":
                        await CargarCalendario(ligaId);
                        break;

                    case "Clasificación":
                        await CargarClasificacion(ligaId);
                        break;
                }

                estado.Text =
                    $"Datos cargados correctamente. " +
                    $"Registros visibles: {tabla.Rows.Count}";
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
        }

        private async Task AbrirCrearEquipo()
        {
            using CrearEquipoForm ventana = new CrearEquipoForm();

            if (ventana.ShowDialog(this) == DialogResult.OK)
                await MostrarSeccion("Equipos");
        }

        private async Task AbrirEditarEquipo()
        {
            if (seccionActual != "Equipos" ||
                tabla.CurrentRow == null ||
                !tabla.Columns.Contains("ID"))
            {
                MessageBox.Show(
                    "Selecciona primero un equipo de la tabla.",
                    "Editar equipo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            object? valor = tabla.CurrentRow.Cells["ID"].Value;

            if (valor == null ||
                !int.TryParse(valor.ToString(), out int equipoId))
            {
                MessageBox.Show(
                    "No se pudo identificar el equipo seleccionado.");
                return;
            }

            using EditarEquipoForm ventana =
                new EditarEquipoForm(equipoId);

            if (ventana.ShowDialog(this) == DialogResult.OK)
                await MostrarSeccion("Equipos");
        }

        private async Task CargarLigas()
        {
            var datos =
                await api.ObtenerListaAsync<JsonElement>("api/Ligas");

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
                await api.ObtenerListaAsync<JsonElement>("api/Equipos");

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
                await api.ObtenerListaAsync<JsonElement>("api/Jugadores");

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
                    continue;

                foreach (var partido in lista.EnumerateArray())
                {
                    bool finalizado = Booleano(partido, "finalizado");

                    partidos.Add(new PartidoFila
                    {
                        Jornada = numero,
                        PartidoID = Entero(partido, "id"),
                        Local = NombreEquipo(partido, "equipoLocal"),
                        Visitante = NombreEquipo(partido, "equipoVisitante"),
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

            var numeros = partidos
                .Select(p => p.Jornada)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            selectorJornada.DataSource = null;
            selectorJornada.DataSource = numeros;

            if (numeros.Contains(jornadaAnterior))
                selectorJornada.SelectedItem = jornadaAnterior;

            cargandoJornadas = false;
            MostrarPartidosJornada();
        }

        private void MostrarPartidosJornada()
        {
            if (selectorJornada.SelectedItem is not int numero)
            {
                tabla.DataSource = null;
                return;
            }

            tabla.DataSource = partidos
                .Where(p => p.Jornada == numero)
                .OrderBy(p => p.PartidoID)
                .ToList();

            estado.Text =
                $"Jornada {numero}: {tabla.Rows.Count} partidos.";
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

        private async Task RegistrarResultado()
        {
            if (tabla.CurrentRow?.DataBoundItem is not PartidoFila partido)
            {
                MessageBox.Show(
                    "Selecciona primero un partido de la tabla.",
                    "Seleccionar partido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            try
            {
                var datos = await api.ObtenerAsync<JsonElement>(
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

                using Form ventana = new Form
                {
                    Text = "Registrar resultado",
                    Size = new Size(440, 340),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    BackColor = fondo
                };

                ventana.Controls.Add(new Label
                {
                    Text = $"{partido.Local} VS {partido.Visitante}",
                    Location = new Point(20, 20),
                    Size = new Size(390, 55),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 13, FontStyle.Bold)
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

                NumericUpDown golesLocal = new NumericUpDown
                {
                    Location = new Point(65, 125),
                    Width = 100,
                    Minimum = 0,
                    Maximum = 99,
                    Value = Entero(datos, "golesLocal")
                };

                ventana.Controls.Add(golesLocal);

                NumericUpDown golesVisitante = new NumericUpDown
                {
                    Location = new Point(245, 125),
                    Width = 100,
                    Minimum = 0,
                    Maximum = 99,
                    Value = Entero(datos, "golesVisitante")
                };

                ventana.Controls.Add(golesVisitante);

                Button confirmar = CrearBoton("Guardar resultado", 170);
                confirmar.Location = new Point(125, 205);
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

                using var respuesta = await api.ModificarAsync(
                    $"api/Resultados/{partido.PartidoID}",
                    cuerpo);

                if (respuesta.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "Resultado guardado correctamente.",
                        "FootballLeague",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    await MostrarSeccion(seccionActual);
                }
                else if ((int)respuesta.StatusCode == 409)
                {
                    MessageBox.Show(
                        "El partido ha sido modificado por otro usuario.\n" +
                        "Actualiza los datos antes de intentarlo de nuevo.",
                        "Conflicto de concurrencia",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    await MostrarSeccion(seccionActual);
                }
                else
                {
                    string detalle =
                        await respuesta.Content.ReadAsStringAsync();

                    MessageBox.Show(
                        $"No se pudo guardar el resultado.\n" +
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
        }

        private static string NombreEquipo(
            JsonElement partido, string propiedad)
        {
            if (!partido.TryGetProperty(propiedad, out var valor))
                return "Equipo desconocido";

            if (valor.ValueKind == JsonValueKind.String)
                return valor.GetString() ?? "Equipo desconocido";

            if (valor.ValueKind == JsonValueKind.Object &&
                valor.TryGetProperty("nombre", out var nombre))
                return nombre.GetString() ?? "Equipo desconocido";

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
                ? numero : 0;
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
