using LineaClient.Services;
using LineaClient.ViewModels;

namespace LineaClient.Views
{
    public partial class ContenedoresPage : ContentPage
    {
        private readonly ContenedoresViewModel _vm;
        private IDispatcherTimer? _relojTimer;

        public ContenedoresPage()
        {
            InitializeComponent();

            // TODO: Cuando se integre DI, recibir IContenedorService por constructor
            var service = new ContenedorServiceSimulado();
            _vm = new ContenedoresViewModel(service);
            BindingContext = _vm;

            _vm.SesionCerrada += OnSesionCerrada;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await _vm.CargarContenedoresAsync();

            // Timer para actualizar la hora cada 30 segundos
            _relojTimer = Dispatcher.CreateTimer();
            _relojTimer.Interval = TimeSpan.FromSeconds(30);
            _relojTimer.Tick += (_, _) => _vm.RefrescarReloj();
            _relojTimer.Start();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _relojTimer?.Stop();
        }

        private async void OnSesionCerrada()
        {
            _relojTimer?.Stop();
            // Volver al login reemplazando la pila de navegacion
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
