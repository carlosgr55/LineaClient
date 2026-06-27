using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace LineaClient.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        // --- Constante del rol permitido ---
        private const string RolRequerido = "Linea";

        private string _usuario = string.Empty;
        private string _contrasena = string.Empty;
        private string _mensajeError = string.Empty;
        private bool _cargando;

        public string Usuario
        {
            get => _usuario;
            set { _usuario = value; OnPropertyChanged(); }
        }

        public string Contrasena
        {
            get => _contrasena;
            set { _contrasena = value; OnPropertyChanged(); }
        }

        public string MensajeError
        {
            get => _mensajeError;
            set { _mensajeError = value; OnPropertyChanged(); OnPropertyChanged(nameof(TieneError)); }
        }

        public bool TieneError => !string.IsNullOrEmpty(MensajeError);

        public bool Cargando
        {
            get => _cargando;
            set { _cargando = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }

        // Evento para notificar login exitoso a la vista
        public event Action? LoginExitoso;

        public LoginViewModel()
        {
            LoginCommand = new Command(async () => await EjecutarLoginAsync());
        }

        private async Task EjecutarLoginAsync()
        {
            MensajeError = string.Empty;

            if (string.IsNullOrWhiteSpace(Usuario) || string.IsNullOrWhiteSpace(Contrasena))
            {
                MensajeError = "Ingresa usuario y contrasena.";
                return;
            }

            Cargando = true;

            try
            {
                // TODO: Reemplazar con llamada real al repositorio de SQL Server
                // Ejemplo de implementacion esperada:
                //
                //   var repo = new UsuarioRepository(connectionString);
                //   var resultado = await repo.AutenticarAsync(Usuario, Contrasena);
                //
                // El repositorio debe:
                //   - Conectarse con SqlConnection usando Microsoft.Data.SqlClient
                //   - Ejecutar un stored procedure o query parametrizado (nunca concatenar strings)
                //     Ejemplo: SELECT Rol FROM Usuarios WHERE Usuario = @usuario AND Contrasena = HASHBYTES('SHA2_256', @contrasena)
                //   - Devolver null si las credenciales no coinciden, o el Rol del usuario si son validas
                //   - Manejar SqlException internamente y lanzar una excepcion de dominio limpia

                var resultado = await SimularAutenticacionAsync(Usuario, Contrasena);

                if (resultado is null)
                {
                    MensajeError = "Usuario o contrasena incorrectos.";
                    return;
                }

                if (!resultado.Rol.Equals(RolRequerido, StringComparison.OrdinalIgnoreCase))
                {
                    MensajeError = "Acceso no permitido: tu rol no tiene acceso a esta aplicacion.";
                    return;
                }

                LoginExitoso?.Invoke();
            }
            catch (Exception ex)
            {
                // TODO: Loguear con ILogger en produccion
                MensajeError = $"Error al conectar: {ex.Message}";
            }
            finally
            {
                Cargando = false;
            }
        }

        // Simulacion temporal hasta conectar SQL Server
        // Usuario de prueba: "linea" / "1234" con rol "Linea"
        // Usuario de prueba: "admin" / "1234" con rol "Admin" (debe rechazarse)
        private static async Task<ResultadoLogin?> SimularAutenticacionAsync(string usuario, string contrasena)
        {
            await Task.Delay(800); // Simula latencia de red/BD

            var usuarios = new Dictionary<string, ResultadoLogin>(StringComparer.OrdinalIgnoreCase)
            {
                { "linea",  new ResultadoLogin("Linea",  "Linea") },
                { "admin",  new ResultadoLogin("Admin",  "Admin") },
            };

            if (usuarios.TryGetValue(usuario, out var resultado) && contrasena == "1234")
                return resultado;

            return null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    // DTO simple para el resultado de autenticacion
    public record ResultadoLogin(string NombreUsuario, string Rol);
}
