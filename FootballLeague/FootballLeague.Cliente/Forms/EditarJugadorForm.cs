
using FootballLeague.Cliente.Services;
using System.Drawing;
using System.Text.Json;

namespace FootballLeague.Cliente.Forms
{
    public class EditarJugadorForm : Form
    {
        private readonly ApiService api = new ApiService();
        private readonly int jugadorId;

        private TextBox txtNombre = null!;
        private NumericUpDown numDorsal = null!;
        private ComboBox cmbPosicion = null!;
        private ComboBox cmbEquipo = null!;
        private Button btnGuardar = null!;
        private Button btnCancelar = null!;

        private class EquipoOpcion
        {
            public int Id { get; set; }
            public string Nombre { get; set; } = "";

            public override string ToString()
            {
                return Nombre;
            }
        }

        public EditarJugadorForm(int id)
        {
            jugadorId = id;

            CrearInterfaz();

            Shown += async (_, _) =>
                await CargarDatos();
        }

        private void CrearInterfaz()
        {
            Text = "FootballLeague - Editar jugador";

            ClientSize = new Size(500, 520);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;

            MaximizeBox = false;
            MinimizeBox = false;

            BackColor = Color.FromArgb(245, 247, 251);

            Controls.Add(new Label
            {
                Text = "Editar jugador",
                Location = new Point(30, 25),
                Size = new Size(440, 45),
                Font = new Font(
                    "Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 35, 67)
            });

            // NOMBRE

            Controls.Add(CrearEtiqueta(
                "Nombre del jugador", 90));

            txtNombre = new TextBox
            {
                Location = new Point(40, 120),
                Size = new Size(420, 30),
                Font = new Font("Segoe UI", 11)
            };

            Controls.Add(txtNombre);

            // DORSAL

            Controls.Add(CrearEtiqueta("Dorsal", 175));

            numDorsal = new NumericUpDown
            {
                Location = new Point(40, 205),
                Size = new Size(120, 30),
                Minimum = 1,
                Maximum = 99,
                Value = 1,
                Font = new Font("Segoe UI", 11)
            };

            Controls.Add(numDorsal);

            // POSICIÓN

            Controls.Add(CrearEtiqueta("Posición", 260));

            cmbPosicion = new ComboBox
            {
                Location = new Point(40, 290),
                Size = new Size(420, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11)
            };

            cmbPosicion.Items.AddRange(new object[]
            {
                "Portero",
                "Defensa",
                "Centrocampista",
                "Delantero"
            });

            Controls.Add(cmbPosicion);

            // EQUIPO

            Controls.Add(CrearEtiqueta("Equipo", 345));

            cmbEquipo = new ComboBox
            {
                Location = new Point(40, 375),
                Size = new Size(420, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11)
            };

            Controls.Add(cmbEquipo);

            // GUARDAR CAMBIOS

            btnGuardar = new Button
            {
                Text = "Guardar cambios",
                Location = new Point(65, 450),
                Size = new Size(180, 42),
                BackColor = Color.FromArgb(33, 65, 112),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };

            btnGuardar.FlatAppearance.BorderSize = 0;

            btnGuardar.Click += async (_, _) =>
                await GuardarCambios();

            Controls.Add(btnGuardar);

            // CANCELAR

            btnCancelar = new Button
            {
                Text = "Cancelar",
                Location = new Point(260, 450),
                Size = new Size(170, 42),
                BackColor = Color.FromArgb(220, 225, 232),
                ForeColor = Color.FromArgb(17, 35, 67),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };

            btnCancelar.FlatAppearance.BorderSize = 0;

            Controls.Add(btnCancelar);

            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;
        }

        private Label CrearEtiqueta(string texto, int y)
        {
            return new Label
            {
                Text = texto,
                Location = new Point(40, y),
                Size = new Size(420, 25),
                Font = new Font("Segoe UI", 11)
            };
        }

        private async Task CargarDatos()
        {
            try
            {
                // CARGAR TODOS LOS EQUIPOS

                var datosEquipos =
                    await api.ObtenerListaAsync<JsonElement>(
                        "api/Equipos");

                var equipos = datosEquipos.Select(x =>
                    new EquipoOpcion
                    {
                        Id = x.GetProperty("id").GetInt32(),

                        Nombre = x.GetProperty("nombre")
                            .GetString() ?? ""
                    })
                    .OrderBy(x => x.Nombre)
                    .ToList();

                if (IsDisposed)
                    return;

                cmbEquipo.DisplayMember = "Nombre";
                cmbEquipo.ValueMember = "Id";
                cmbEquipo.DataSource = equipos;

                // CARGAR EL JUGADOR SELECCIONADO

                var jugador =
                    await api.ObtenerAsync<JsonElement>(
                        $"api/Jugadores/{jugadorId}");

                if (IsDisposed)
                    return;

                if (jugador.ValueKind != JsonValueKind.Object)
                {
                    MessageBox.Show(
                        "No se encontró el jugador.");

                    Close();
                    return;
                }

                txtNombre.Text =
                    jugador.GetProperty("nombre")
                    .GetString() ?? "";

                numDorsal.Value =
                    jugador.GetProperty("dorsal")
                    .GetInt32();

                string posicion =
                    jugador.GetProperty("posicion")
                    .GetString() ?? "";

                if (!cmbPosicion.Items.Contains(posicion))
                {
                    cmbPosicion.Items.Add(posicion);
                }

                cmbPosicion.SelectedItem = posicion;

                int equipoId =
                    jugador.GetProperty("equipoId")
                    .GetInt32();

                cmbEquipo.SelectedValue = equipoId;

                btnGuardar.Enabled = equipos.Count > 0;
            }
            catch (Exception ex)
            {
                if (IsDisposed)
                    return;

                MessageBox.Show(
                    "No se pudieron cargar los datos:\n" +
                    ex.Message,
                    "FootballLeague",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                Close();
            }
        }

        private async Task GuardarCambios()
        {
            string nombre = txtNombre.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                MessageBox.Show(
                    "Introduce el nombre del jugador.",
                    "Datos incompletos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtNombre.Focus();
                return;
            }

            if (cmbPosicion.SelectedItem == null ||
                string.IsNullOrWhiteSpace(
                    cmbPosicion.SelectedItem.ToString()))
            {
                MessageBox.Show(
                    "Selecciona una posición.",
                    "Datos incompletos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (cmbEquipo.SelectedItem is not EquipoOpcion equipo)
            {
                MessageBox.Show(
                    "Selecciona un equipo.",
                    "Datos incompletos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            btnGuardar.Enabled = false;
            btnCancelar.Enabled = false;

            try
            {
                var datos = new
                {
                    nombre = nombre,
                    dorsal = (int)numDorsal.Value,
                    posicion = cmbPosicion.SelectedItem.ToString(),
                    equipoId = equipo.Id
                };

                using var respuesta =
                    await api.ModificarAsync(
                        $"api/Jugadores/{jugadorId}",
                        datos);

                if (IsDisposed)
                    return;

                if (respuesta.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "Jugador actualizado correctamente.",
                        "FootballLeague",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    DialogResult = DialogResult.OK;
                    Close();
                }
                else if ((int)respuesta.StatusCode == 409)
                {
                    string detalle =
                        await respuesta.Content.ReadAsStringAsync();

                    MessageBox.Show(
                        detalle,
                        "Conflicto al modificar jugador",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                else
                {
                    string detalle =
                        await respuesta.Content.ReadAsStringAsync();

                    MessageBox.Show(
                        $"No se pudo modificar el jugador.\n" +
                        $"Código: {(int)respuesta.StatusCode}\n\n" +
                        detalle,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                {
                    MessageBox.Show(
                        "Error al guardar los cambios:\n" +
                        ex.Message,
                        "FootballLeague",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            finally
            {
                if (!IsDisposed)
                {
                    btnGuardar.Enabled = true;
                    btnCancelar.Enabled = true;
                }
            }
        }
    }
}
