
using FootballLeague.Cliente.Services;
using System.Drawing;

namespace FootballLeague.Cliente.Forms
{
    public class CrearEquipoForm : Form
    {
        private readonly ApiService api = new ApiService();

        private TextBox txtNombre = null!;
        private TextBox txtCiudad = null!;
        private TextBox txtEstadio = null!;

        private Button btnGuardar = null!;

        public CrearEquipoForm()
        {
            CrearInterfaz();
        }

        private void CrearInterfaz()
        {
            Text = "FootballLeague - Crear equipo";

            Size = new Size(500, 470);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;

            MaximizeBox = false;
            MinimizeBox = false;

            BackColor = Color.FromArgb(245, 247, 251);

            Label titulo = new Label
            {
                Text = "Nuevo equipo",
                Location = new Point(30, 25),
                Size = new Size(400, 45),
                Font = new Font(
                    "Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 35, 67)
            };

            Controls.Add(titulo);

            Controls.Add(CrearEtiqueta(
                "Nombre del equipo", 90));

            txtNombre = CrearCaja(120);
            Controls.Add(txtNombre);

            Controls.Add(CrearEtiqueta(
                "Ciudad", 175));

            txtCiudad = CrearCaja(205);
            Controls.Add(txtCiudad);

            Controls.Add(CrearEtiqueta(
                "Estadio", 260));

            txtEstadio = CrearCaja(290);
            Controls.Add(txtEstadio);

            btnGuardar = new Button
            {
                Text = "Guardar equipo",
                Location = new Point(150, 365),
                Size = new Size(180, 42),
                BackColor = Color.FromArgb(33, 65, 112),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            btnGuardar.FlatAppearance.BorderSize = 0;

            btnGuardar.Click += async (_, _) =>
                await GuardarEquipo();

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

        private async Task GuardarEquipo()
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
                var equipo = new
                {
                    nombre,
                    ciudad,
                    estadio
                };

                using var respuesta =
                    await api.CrearAsync("api/Equipos", equipo);

                if (respuesta.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "Equipo creado correctamente.",
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
                        $"No se pudo crear el equipo.\n" +
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
                    "Error de conexión:\n" + ex.Message,
                    "FootballLeague",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnGuardar.Enabled = true;
            }
        }
    }
}
