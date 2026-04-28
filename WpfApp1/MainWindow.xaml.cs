using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public class Articulo
    {
        public int Id { get; set; }
        public int peped { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public decimal Precio { get; set; }
        public bool Activo { get; set; }

        public string PrecioFormateado => Precio.ToString("F2");
        public string Estado => Activo ? "Activo" : "Inactivo";
    }

    public partial class MainWindow : Window
    {
        private List<Articulo> _articulos = new List<Articulo>();
        private ObservableCollection<Articulo> _vista = new ObservableCollection<Articulo>();
        private int _nextId = 1;
        private int _editandoId = -1;

        public MainWindow()
        {
            InitializeComponent();
            DgArticulos.ItemsSource = _vista;
            // Datos de wqdqw
            // Datos de ejemplo
            AgregarEjemplo("Laptop HP 15\"", "Electrónica", 4500);
            AgregarEjemplo("Silla ergonómica", "Mobiliario", 850);
            AgregarEjemplo("Mouse inalámbrico", "Electrónica", 120);
        }

        private void AgregarEjemplo(string nombre, string categoria, decimal precio)
        {
            _articulos.Add(new Articulo
            {
                Id = _nextId++,
                Nombre = nombre,
                Categoria = categoria,
                Precio = precio,
                Activo = true
            });
            Actualizar();
        }

        // ── AGREGAR ──────────────────────────────────────────────
        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarCampos(out string nombre, out string cat, out decimal precio)) return;

            _articulos.Add(new Articulo
            {
                Id = _nextId++,
                Nombre = nombre,
                Categoria = cat,
                Precio = precio,
                Activo = true
            });

            LimpiarFormulario();
            Actualizar();
            MessageBox.Show("Artículo agregado correctamente.", "Éxito",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ── EDITAR (cargar en formulario) ─────────────────────────
        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            var id = (int)((Button)sender).Tag;
            var art = _articulos.FirstOrDefault(a => a.Id == id);
            if (art == null) return;

            _editandoId = id;
            TxtNombre.Text = art.Nombre;
            TxtCategoria.Text = art.Categoria;
            TxtPrecio.Text = art.Precio.ToString("F2");

            BtnGuardar.IsEnabled = true;
            TxtNombre.Focus();
        }

        // ── GUARDAR CAMBIOS ───────────────────────────────────────
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (_editandoId < 0) return;
            if (!ValidarCampos(out string nombre, out string cat, out decimal precio)) return;

            var art = _articulos.FirstOrDefault(a => a.Id == _editandoId);
            if (art == null) return;

            art.Nombre = nombre;
            art.Categoria = cat;
            art.Precio = precio;

            LimpiarFormulario();
            Actualizar();
            MessageBox.Show("Artículo actualizado correctamente.", "Éxito",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ── BORRAR ────────────────────────────────────────────────
        private void BtnBorrar_Click(object sender, RoutedEventArgs e)
        {
            var id = (int)((Button)sender).Tag;
            var art = _articulos.FirstOrDefault(a => a.Id == id);
            if (art == null) return;

            var res = MessageBox.Show($"¿Eliminar \"{art.Nombre}\"?", "Confirmar",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res != MessageBoxResult.Yes) return;

            _articulos.Remove(art);
            if (_editandoId == id) LimpiarFormulario();
            Actualizar();
        }

        // ── BUSCAR ────────────────────────────────────────────────
        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            Actualizar();
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            TxtBuscar.Text = "";
        }

        private void DgArticulos_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        // ── HELPERS ───────────────────────────────────────────────
        private bool ValidarCampos(out string nombre, out string categoria, out decimal precio)
        {
            nombre = TxtNombre.Text.Trim();
            categoria = TxtCategoria.Text.Trim();
            precio = 0;

            if (string.IsNullOrEmpty(nombre))
            {
                MessageBox.Show("Ingresa el nombre del artículo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtNombre.Focus(); return false;
            }
            if (string.IsNullOrEmpty(categoria))
            {
                MessageBox.Show("Ingresa la categoría.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtCategoria.Focus(); return false;
            }
            if (!decimal.TryParse(TxtPrecio.Text.Trim(), out precio) || precio < 0)
            {
                MessageBox.Show("Ingresa un precio válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtPrecio.Focus(); return false;
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

        private void Actualizar()
        {
            var q = TxtBuscar?.Text?.ToLower() ?? "";
            var filtrados = _articulos
                .Where(a => a.Nombre.ToLower().Contains(q) || a.Categoria.ToLower().Contains(q))
                .ToList();

            _vista.Clear();
            foreach (var a in filtrados) _vista.Add(a);

            // Estadísticas
            int total = _articulos.Count;
            decimal valorTotal = _articulos.Sum(a => a.Precio);
            decimal promedio = total > 0 ? valorTotal / total : 0;

            TxtTotal.Text = $"Total: {total} artículos";
            TxtPromedio.Text = $"Promedio: Bs. {promedio:F2}";
            TxtValorTotal.Text = $"Valor total: Bs. {valorTotal:F2}";
        }
    }
}