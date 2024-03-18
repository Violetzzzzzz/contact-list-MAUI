using System.Security.Cryptography;

namespace MAUIAPP1;

public partial class MainPage : ContentPage
{
    string connectionString = "";
    byte[] key;

    public MainPage()
	{
		InitializeComponent();
        GetTheKey();
    }

    private async void OnGoToLoginPageClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage(connectionString, key));
    }

    private async void OnGoToRegisterPageClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage(connectionString, key));
    }

    private void GetTheKey()
    {
        string executablePath = AppDomain.CurrentDomain.BaseDirectory;
        string contentToRemove = "/bin/Debug/net7.0-maccatalyst/maccatalyst-x64/MAUIAPP1.app/Contents/MonoBundle";

        string keyFileName = "key.bin";
        string keyPath = Path.Combine(executablePath.Replace(contentToRemove, ""), keyFileName);

        if (File.Exists(keyPath))
        {
            Console.WriteLine("Key Exists");
            key = File.ReadAllBytes(keyPath);
        }
        else
        {
            Console.WriteLine("Key NOT Exists");
            byte[] randomBytes = new byte[16];
            RandomNumberGenerator.Fill(randomBytes);
            File.WriteAllBytes(keyPath, randomBytes);
            key = randomBytes;
        }
    }
}


