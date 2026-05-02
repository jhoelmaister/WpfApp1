using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;

namespace WpfApp1
{
    public class Articulo
    {
        public double Id { get; set; }
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public string Modelo { get; set; }
        public string Estado { get; set; }
        public string Estadof { get; set; }
        public double Categoria { get; set; }
        public double Familia { get; set; }
        public double Industria { get; set; }
        public DateTime? Emision { get; set; }
        public DateTime? Edicion { get; set; }
    }

    public partial class MainWindow : Window
    {
        private SqlConnection? conexionGlobal;
        private ObservableCollection<Articulo> _vista = new ObservableCollection<Articulo>();
        private double _editandoId = -1;

        public MainWindow()
        {
            InitializeComponent();
            DgArticulos.ItemsSource = _vista;
            CargarArticulos();
            this.Closed += MainWindow_Closed;
        }

        // ── CARGAR DESDE SQL SERVER ───────────────────────────
        private void CargarArticulos()
        {
            try
            {
                if (conexionGlobal == null || conexionGlobal.State == ConnectionState.Closed)
                {
                    conexionGlobal = ConexionDB.AbrirConexion();
                }

                var busqueda = TxtBuscar?.Text?.ToLower() ?? "";

                string sql = @"
                    SELECT id, codigo, descripcion, modelo,
                           estado, estadof, categoria, familia, industria,
                           emision, edicion
                    FROM   dbo.articulos
                    WHERE  LOWER(ISNULL(descripcion,'')) LIKE @b
                        OR LOWER(ISNULL(codigo,''))      LIKE @b
                        OR LOWER(ISNULL(modelo,''))      LIKE @b
                    ORDER  BY descripcion";

                using var cmd = new SqlCommand(sql, conexionGlobal);
                cmd.Parameters.AddWithValue("@b", $"%{busqueda}%");

                _vista.Clear();
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    _vista.Add(new Articulo
                    {
                        Id = r.IsDBNull(0) ? 0 : r.GetDouble(0),
                        Codigo = r.IsDBNull(1) ? "" : r.GetString(1),
                        Descripcion = r.IsDBNull(2) ? "" : r.GetString(2),
                        Modelo = r.IsDBNull(3) ? "" : r.GetString(3),
                        Estado = r.IsDBNull(4) ? "" : r.GetString(4),
                        Estadof = r.IsDBNull(5) ? "" : r.GetString(5),
                        Categoria = r.IsDBNull(6) ? 0 : r.GetDouble(6),
                        Familia = r.IsDBNull(7) ? 0 : r.GetDouble(7),
                        Industria = r.IsDBNull(8) ? 0 : r.GetDouble(8),
                        Emision = r.IsDBNull(9) ? null : r.GetDateTime(9),
                        Edicion = r.IsDBNull(10) ? null : r.GetDateTime(10),
                    });
                }

                TxtTotal.Text = $"Total: {_vista.Count} artículos";
                TxtPromedio.Text = "";
                TxtValorTotal.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con SQL Server:\n{ex.Message}",
                    "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── AGREGAR ───────────────────────────────────────────
        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            string descripcion = TxtNombre.Text.Trim();
            string codigo = TxtCategoria.Text.Trim();
            string modelo = TxtPrecio.Text.Trim();

            if (string.IsNullOrEmpty(descripcion))
            {
                MessageBox.Show("Ingresa la descripción.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtNombre.Focus(); return;
            }

            try
            {
                if (conexionGlobal == null || conexionGlobal.State == ConnectionState.Closed)
                {
                    conexionGlobal = ConexionDB.AbrirConexion();
                }

                // Calcular nuevo ID
                double nuevoId;
                using (var cmdMax = new SqlCommand("SELECT ISNULL(MAX(id), 0) + 1 FROM dbo.articulos", conexionGlobal))
                    nuevoId = Convert.ToDouble(cmdMax.ExecuteScalar());

                using var cmd = new SqlCommand(@"
                    INSERT INTO dbo.articulos (id, descripcion, codigo, modelo, estado, emision)
                    VALUES (@id, @desc, @cod, @mod, 'A', GETDATE())", conexionGlobal);
                cmd.Parameters.AddWithValue("@id", nuevoId);
                cmd.Parameters.AddWithValue("@desc", descripcion);
                cmd.Parameters.AddWithValue("@cod", codigo);
                cmd.Parameters.AddWithValue("@mod", modelo);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            LimpiarFormulario();
            CargarArticulos();
            MessageBox.Show("Artículo agregado correctamente.", "Éxito",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ── EDITAR (cargar en formulario) ─────────────────────
        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            var id = Convert.ToDouble(((Button)sender).Tag);
            var art = _vista.FirstOrDefault(a => a.Id == id);
            if (art == null) return;

            _editandoId = id;
            TxtNombre.Text = art.Descripcion;
            TxtCategoria.Text = art.Codigo;
            TxtPrecio.Text = art.Modelo;
            BtnGuardar.IsEnabled = true;
            TxtNombre.Focus();
        }

        // ── GUARDAR CAMBIOS ───────────────────────────────────
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (_editandoId < 0) return;

            string descripcion = TxtNombre.Text.Trim();
            string codigo = TxtCategoria.Text.Trim();
            string modelo = TxtPrecio.Text.Trim();

            if (string.IsNullOrEmpty(descripcion))
            {
                MessageBox.Show("Ingresa la descripción.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtNombre.Focus(); 
                return;
            }

            try
            {
                if(conexionGlobal == null || conexionGlobal.State == ConnectionState.Closed)
                {
                    conexionGlobal = ConexionDB.AbrirConexion();
                }
                using var cmd = new SqlCommand(@"
                    UPDATE dbo.articulos
                    SET descripcion = @desc,
                        codigo      = @cod,
                        modelo      = @mod,
                        edicion     = GETDATE()
                    WHERE id = @id", conexionGlobal);
                cmd.Parameters.AddWithValue("@desc", descripcion);
                cmd.Parameters.AddWithValue("@cod", codigo);
                cmd.Parameters.AddWithValue("@mod", modelo);
                cmd.Parameters.AddWithValue("@id", _editandoId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            LimpiarFormulario();
            CargarArticulos();
            MessageBox.Show("Artículo actualizado correctamente.", "Éxito",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ── BORRAR ────────────────────────────────────────────
        private void BtnBorrar_Click(object sender, RoutedEventArgs e)
        {
            var id = Convert.ToDouble(((Button)sender).Tag);
            var art = _vista.FirstOrDefault(a => a.Id == id);
            if (art == null) return;

            var res = MessageBox.Show($"¿Eliminar \"{art.Descripcion}\"?", "Confirmar",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res != MessageBoxResult.Yes) return;

            try
            {
                if(conexionGlobal == null || conexionGlobal.State == ConnectionState.Closed)
                {
                    conexionGlobal = ConexionDB.AbrirConexion();
                }
                using var cmd = new SqlCommand("DELETE FROM dbo.articulos WHERE id = @id", conexionGlobal);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_editandoId == id) LimpiarFormulario();
            CargarArticulos();
        }

        // ── BUSCAR ────────────────────────────────────────────
        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e) => CargarArticulos();
        private void BtnLimpiar_Click(object sender, RoutedEventArgs e) => TxtBuscar.Text = "";
        private void DgArticulos_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        // ── HELPERS ───────────────────────────────────────────
        private bool ValidarCampos(out string nombre, out string categoria, out decimal precio)
        {
            nombre = TxtNombre.Text.Trim();
            categoria = TxtCategoria.Text.Trim();
            precio = 0;
            if (string.IsNullOrEmpty(nombre))
            {
                MessageBox.Show("Ingresa la descripción.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtNombre.Focus(); return false;
            }
            return true;
        }

        private void LimpiarFormulario()
        {
            TxtNombre.Text = "";
            TxtCategoria.Text = "";
            TxtPrecio.Text = "";
            BtnGuardar.IsEnabled = false;
            _editandoId = -1;
        }
        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            ConexionDB.CerrarConexion(conexionGlobal);
        }
    }
}