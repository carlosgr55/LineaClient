using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using LineaClient.Models;
using LineaClient.Services;

namespace LineaClient.ViewModels
{
    // ViewModel para un contenedor individual dentro de la lista
    public class ContenedorItemViewModel : INotifyPropertyChanged
    {
        private readonly IContenedorService _service;
        private EstadoContenedor _estado;
        private bool _verificando;
        private bool _cancelando;
        private string _mensajeEstado = string.Empty;

        public int Id { get; }
        public string Tipo { get; }
        public string ImagenNombre { get; }
        public int LineaId { get; }

        public EstadoContenedor Estado
        {
            get => _estado;
            set
            {
                _estado = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EtiquetaEstado));
                OnPropertyChanged(nameof(PuedeMarcarLleno));
                OnPropertyChanged(nameof(PuedeCancelar));
                OnPropertyChanged(nameof(SolicitudPendiente));
                OnPropertyChanged(nameof(PuedeVerificar));
                OnPropertyChanged(nameof(ColorEstado));
            }
        }

        public bool Verificando
        {
            get => _verificando;
            set { _verificando = value; OnPropertyChanged(); OnPropertyChanged(nameof(PuedeVerificar)); }
        }

        public bool Cancelando
        {
            get => _cancelando;
            set { _cancelando = value; OnPropertyChanged(); OnPropertyChanged(nameof(PuedeCancelar)); }
        }

        public string MensajeEstado
        {
            get => _mensajeEstado;
            set { _mensajeEstado = value; OnPropertyChanged(); OnPropertyChanged(nameof(TieneMensaje)); }
        }

        public bool TieneMensaje => !string.IsNullOrEmpty(MensajeEstado);

        // Propiedades derivadas del estado
        // El contenedor solo puede marcarse lleno cuando esta disponible (vacio) en la linea
        public bool PuedeMarcarLleno => Estado == EstadoContenedor.Disponible;

        // Hay una solicitud activa (lleno o esperando vacio): mientras tanto
        // queda bloqueado pedir un contenedor nuevo de este mismo tipo
        public bool SolicitudPendiente => Estado is EstadoContenedor.Lleno or EstadoContenedor.EsperandoVacio;

        // Se puede cancelar en cualquier momento mientras la solicitud este pendiente,
        // es decir hasta que el almacen efectivamente entregue el contenedor vacio
        public bool PuedeCancelar => SolicitudPendiente && !Cancelando;

        public bool PuedeVerificar => Estado == EstadoContenedor.EsperandoVacio && !Verificando;

        public string EtiquetaEstado => Estado switch
        {
            EstadoContenedor.Disponible     => "Disponible",
            EstadoContenedor.Lleno          => "Lleno - solicitud enviada",
            EstadoContenedor.EsperandoVacio => "Esperando contenedor vacio",
            _ => string.Empty
        };

        public Color ColorEstado => Estado switch
        {
            EstadoContenedor.Disponible     => Color.FromArgb("#4CAF50"),
            EstadoContenedor.Lleno          => Color.FromArgb("#FF9800"),
            EstadoContenedor.EsperandoVacio => Color.FromArgb("#9E9E9E"),
            _ => Color.FromArgb("#9E9E9E")
        };

        public ICommand MarcarLlenoCommand { get; }
        public ICommand CancelarSolicitudCommand { get; }
        public ICommand VerificarEntregaCommand { get; }

        public ContenedorItemViewModel(Contenedor contenedor, IContenedorService service)
        {
            _service = service;
            Id           = contenedor.Id;
            Tipo         = contenedor.Tipo;
            ImagenNombre = contenedor.ImagenNombre;
            LineaId      = contenedor.LineaId;
            _estado      = contenedor.Estado;

            MarcarLlenoCommand       = new Command(async () => await MarcarLlenoAsync(), () => PuedeMarcarLleno);
            CancelarSolicitudCommand = new Command(async () => await CancelarSolicitudAsync(), () => PuedeCancelar);
            VerificarEntregaCommand  = new Command(async () => await VerificarEntregaAsync(), () => PuedeVerificar);
        }

        private async Task MarcarLlenoAsync()
        {
            bool ok = await _service.MarcarLlenoYSolicitarAsync(Id, LineaId);
            if (ok)
            {
                Estado = EstadoContenedor.Lleno;
                MensajeEstado = "Marcado como lleno. Solicitud de reemplazo enviada al almacen.";
            }
            else
            {
                MensajeEstado = "No se pudo enviar la solicitud. Intenta de nuevo.";
            }
        }

        private async Task CancelarSolicitudAsync()
        {
            Cancelando = true;
            MensajeEstado = "Cancelando solicitud...";

            bool ok = await _service.CancelarSolicitudAsync(Id, LineaId);

            if (ok)
            {
                Estado = EstadoContenedor.Disponible;
                MensajeEstado = "Solicitud cancelada. El contenedor vuelve a estar disponible.";
            }
            else
            {
                MensajeEstado = "No se pudo cancelar: el almacen ya esta atendiendo la solicitud.";
            }

            Cancelando = false;
        }

        private async Task VerificarEntregaAsync()
        {
            Verificando = true;
            MensajeEstado = "Verificando con almacen...";

            bool entregado = await _service.VerificarEntregaAsync(LineaId, Id);

            if (entregado)
            {
                Estado = EstadoContenedor.Disponible;
                MensajeEstado = "Contenedor vacio entregado. Ya puedes llenarlo o solicitar de nuevo.";
            }
            else
            {
                Estado = EstadoContenedor.EsperandoVacio;
                MensajeEstado = "Aun sin entregar. Vuelve a verificar en unos minutos.";
            }

            Verificando = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
