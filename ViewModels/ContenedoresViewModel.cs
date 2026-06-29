using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using LineaClient.Models;
using LineaClient.Services;

namespace LineaClient.ViewModels
{
    public class ContenedoresViewModel : INotifyPropertyChanged
    {
        private readonly IContenedorService _service;
        private bool _cargando;
        private string _mensajeError = string.Empty;

        public ObservableCollection<ContenedorItemViewModel> Contenedores { get; } = [];

        // Datos de sesion expuestos a la vista
        public string NombreUsuario => SesionUsuario.Actual?.NombreUsuario ?? string.Empty;
        public string NombreLinea   => SesionUsuario.Actual?.NombreLinea   ?? string.Empty;
        public string FechaHora     => DateTime.Now.ToString("dd/MM/yyyy  HH:mm");

        public bool Cargando
        {
            get => _cargando;
            set { _cargando = value; OnPropertyChanged(); }
        }

        public string MensajeError
        {
            get => _mensajeError;
            set { _mensajeError = value; OnPropertyChanged(); OnPropertyChanged(nameof(TieneError)); }
        }

        public bool TieneError => !string.IsNullOrEmpty(MensajeError);

        public ICommand CargarCommand   { get; }
        public ICommand CerrarSesionCommand { get; }

        public event Action? SesionCerrada;

        public ContenedoresViewModel(IContenedorService service)
        {
            _service = service;
            CargarCommand       = new Command(async () => await CargarContenedoresAsync());
            CerrarSesionCommand = new Command(CerrarSesion);
        }

        public async Task CargarContenedoresAsync()
        {
            if (SesionUsuario.Actual is null) return;

            MensajeError = string.Empty;
            Cargando = true;

            try
            {
                // TODO: Reemplazar con llamada real al repositorio SQL Server
                // El lineaId viene de la sesion activa
                var lista = await _service.ObtenerContenedoresPorLineaAsync(SesionUsuario.Actual.LineaId);

                Contenedores.Clear();
                foreach (var c in lista)
                    Contenedores.Add(new ContenedorItemViewModel(c, _service));
            }
            catch (Exception ex)
            {
                // TODO: Loguear con ILogger en produccion
                MensajeError = $"Error al cargar contenedores: {ex.Message}";
            }
            finally
            {
                Cargando = false;
            }
        }

        // Actualiza la hora en pantalla (llamado por un timer desde la vista)
        public void RefrescarReloj() => OnPropertyChanged(nameof(FechaHora));

        private void CerrarSesion()
        {
            SesionUsuario.Actual = null;
            SesionCerrada?.Invoke();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
