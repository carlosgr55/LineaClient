using LineaClient.ViewModels;

namespace LineaClient.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();

            if (BindingContext is LoginViewModel vm)
                vm.LoginExitoso += OnLoginExitoso;
        }

        private async void OnLoginExitoso()
        {
            // Navegar a la pantalla principal reemplazando la pila
            await Shell.Current.GoToAsync("//ContenedoresPage");
        }
    }
}
