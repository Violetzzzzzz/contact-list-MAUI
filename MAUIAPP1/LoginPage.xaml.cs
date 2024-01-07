using System.Security.Cryptography;
using System.Text;
using MySql.Data.MySqlClient;

namespace MAUIAPP1;

public partial class LoginPage : ContentPage
{
    string connectionString;
    byte[] key;
    User user;

    public LoginPage(string connectionString, byte[] key)
	{
		InitializeComponent();
        this.connectionString = connectionString;
        this.key = key;
    }

    private void CheckUsernameFormat(object sender, FocusEventArgs e)
    {

        Entry entry = sender as Entry;
        if (entry != null)
        {
            if (string.IsNullOrWhiteSpace(entry.Text))
            {
                usernameTip.Text = "Username cannot be empty.";
                usernameTip.TextColor = Color.FromRgb(255, 0, 0);
            }
            else
            {
                usernameTip.Text = "";
            }

        }
    }

    private void CheckPasswordFormat(object sender, FocusEventArgs e)
    {

        Entry entry = sender as Entry;
        if (entry != null)
        {
            if (string.IsNullOrWhiteSpace(entry.Text))
            {
                passwordTip.Text = "Password cannot be empty.";
                passwordTip.TextColor = Color.FromRgb(255, 0, 0);
            }
            else
            {
                passwordTip.Text = "";
            }

        }
    }

    private async void SubmitLoginForm(object sender, EventArgs e)
    {
        string username = usernameEntry.Text;
        string password = passwordEntry.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            loginTip.Text = "Username and Password can not be empty";
        }
        else if (AuthenticateLogin(username, password))
        {
            await Navigation.PushAsync(new UserPage(user, connectionString, key));
        }
    }

    private Boolean AuthenticateLogin(string username, string password)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            using (MySqlCommand command = new MySqlCommand())
            {
                command.Connection = connection;
                command.CommandText = "SELECT `id` FROM `users` WHERE AES_DECRYPT(username, @Key) = @Username AND AES_DECRYPT(password, @Key) = @Password";
                command.Parameters.AddWithValue("@Key", key);
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Password", hashedPassword(password));

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32("id");
                            user = new User(username, id);
                            return true;
                        }
                    }
                    return false;
                }
            }
        }
    }


    private string hashedPassword(string plainPassword)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(plainPassword));

            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < hashedBytes.Length; i++)
            {
                stringBuilder.Append(hashedBytes[i].ToString("x2"));
            }

            return stringBuilder.ToString();
        }
    }
}
