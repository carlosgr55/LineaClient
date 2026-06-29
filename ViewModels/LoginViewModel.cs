using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using LineaClient.Models;

namespace LineaClient.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
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
                // TODO: Reemplazar con repositorio real
                // var repo = new UsuarioRepository(connectionString);
                // var resultado = await repo.AutenticarAsync(Usuario, Contrasena);
                // Consulta sugerida:
                //   SELECT u.Id, u.NombreUsuario, u.Rol, l.Id AS LineaId, l.Nombre AS NombreLinea
                //   FROM Usuarios u
                //   INNER JOIN Lineas l ON l.Id = u.LineaId
                //   WHERE u.NombreUsuario = @usuario
                //   AND u.Contrasena = HASHBYTES('SHA2_256', @contrasena)

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

                // Guardar sesion global
                SesionUsuario.Actual = resultado;

                LoginExitoso?.Invoke();
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al conectar: {ex.Message}";
            }
            finally
            {
                Cargando = false;
            }
        }

        // Usuarios de prueba:
        //   linea1 / 1234  -> rol Linea, LineaId 1 "Linea 1"
        //   linea2 / 1234  -> rol Linea, LineaId 2 "Linea 2"
        //   admin  / 1234  -> rol Admin (debe rechazarse)
        private static async Task<SesionUsuario?> SimularAutenticacionAsync(string usuario, string contrasena)
        {
            await Task.Delay(800);

            if (contrasena != "1234") return null;

            return usuario.ToLower() switch
            {
                "linea1" => new SesionUsuario { UsuarioId = 1, NombreUsuario = "linea1", Rol = "Linea", LineaId = 1, NombreLinea = "Linea 1" },
                "linea2" => new SesionUsuario { UsuarioId = 2, NombreUsuario = "linea2", Rol = "Linea", LineaId = 2, NombreLinea = "Linea 2" },
                "admin"  => new SesionUsuario { UsuarioId = 3, NombreUsuario = "admin",  Rol = "Admin", LineaId = 0, NombreLinea = string.Empty },
                _        => null
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
