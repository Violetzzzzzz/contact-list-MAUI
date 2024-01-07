namespace MAUIAPP1;

public partial class RegisteredPage : ContentPage
{
	public RegisteredPage()
	{
        InitializeComponent();
        BackToHomepage();
        
    }

    private async void BackToHomepage()
    {
        await Task.Delay(3000);
        await Navigation.PushAsync(new MainPage());
    }
}
