
using FootballLeague.Cliente.Services;
using System.Drawing;
using System.Text.Json;

namespace FootballLeague.Cliente.Forms
{
    public class EditarEquipoForm : Form
    {
        private readonly ApiService api = new ApiService();
        private readonly int equipoId;

        private TextBox txtNombre = null!;
        private TextBox txtCiudad = null!;
        private TextBox txtEstadio = null!;
        private Button btnGuardar = null!;

        public EditarEquipoForm(int id)
        {
            equipoId = id;
            CrearInterfaz();

            Shown += async (_, _) => await CargarEquipo();
        }

        private void CrearInterfaz()
        {
            Text = "FootballLeague - Editar equipo";
            Size = new Size(500, 470);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(245, 247, 251);

            Controls.Add(new Label
            {
                Text = "Editar equipo",
                Location = new Point(30, 25),
                Size = new Size(400, 45),
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 35, 67)
            });

            Controls.Add(CrearEtiqueta("Nombre del equipo", 90));
            txtNombre = CrearCaja(120);
            Controls.Add(txtNombre);

            Controls.Add(CrearEtiqueta("Ciudad", 175));
            txtCiudad = CrearCaja(205);
            Controls.Add(txtCiudad);

            Controls.Add(CrearEtiqueta("Estadio", 260));
            txtEstadio = CrearCaja(290);
            Controls.Add(txtEstadio);

            btnGuardar = new Button
            {
                Text = "Guardar cambios",
                Location = new Point(150, 365),
                Size = new Size(180, 42),
                BackColor = Color.FromArgb(33, 65, 112),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };

            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += async (_, _) => await GuardarCambios();

            Controls.Add(btnGuardar);
            AcceptButton = btnGuardar;
        }

        private Label CrearEtiqueta(string texto, int y)
        {
            return new Label
            {
                Text = texto,
                Location = new Point(40, y),
                Size = new Size(400, 25),
                Font = new Font("Segoe UI", 11)
            };
        }

        private TextBox CrearCaja(int y)
        {
            return new TextBox
            {
                Location = new Point(40, y),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 11)
            };
        }

        private async Task CargarEquipo()
        {
            try
            {
                var equipo = await api.ObtenerAsync<JsonElement>(
                    $"api/Equipos/{equipoId}");

                if (equipo.ValueKind != JsonValueKind.Object)
                {
                    MessageBox.Show("No se encontró el equipo.");
                    Close();
                    return;
                }

                txtNombre.Text = LeerTexto(equipo, "nombre");
                txtCiudad.Text = LeerTexto(equipo, "ciudad");
                txtEstadio.Text = LeerTexto(equipo, "estadio");

                btnGuardar.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudieron cargar los datos:\n" + ex.Message,
                    "FootballLeague",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                Close();
            }
        }

        private async Task GuardarCambios()
        {
            string nombre = txtNombre.Text.Trim();
            string ciudad = txtCiudad.Text.Trim();
            string estadio = txtEstadio.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(ciudad) ||
                string.IsNullOrWhiteSpace(estadio))
            {
                MessageBox.Show(
                    "Debes completar todos los campos.",
                    "Datos incompletos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            btnGuardar.Enabled = false;

            try
            {
                var datos = new
                {
                    nombre,
                    ciudad,
                    estadio
                };

                using var respuesta = await api.ModificarAsync(
                    $"api/Equipos/{equipoId}", datos);

                if (respuesta.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "Equipo actualizado correctamente.",
                        "FootballLeague",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    string detalle =
                        await respuesta.Content.ReadAsStringAsync();

                    MessageBox.Show(
                        $"No se pudo actualizar el equipo.\n" +
                        $"Código: {(int)respuesta.StatusCode}\n\n" +
                        detalle,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al guardar los cambios:\n" + ex.Message,
                    "FootballLeague",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnGuardar.Enabled = true;
            }
        }

        private static string LeerTexto(
            JsonElement objeto, string propiedad)
        {
            if (objeto.TryGetProperty(propiedad, out var valor) &&
                valor.ValueKind == JsonValueKind.String)
            {
                return valor.GetString() ?? "";
            }

            return "";
        }
    }
}
