using LineaClient.ViewModels;

namespace LineaClient.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();

            // Suscribir el evento de login exitoso para navegar al shell principal
            if (BindingContext is LoginViewModel vm)
                vm.LoginExitoso += OnLoginExitoso;
        }

        private async void OnLoginExitoso()
        {
            // Reemplazar el root de navegacion para que el usuario no pueda
            // volver atras al login con el boton fisico
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}
