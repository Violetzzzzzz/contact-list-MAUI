namespace MAUIAPP1;

using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

public partial class RegisterPage : ContentPage
{
    string connectionString;
    byte[] key;

    public RegisterPage(string connectionString, byte[] key)
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
                usernameTip.Text = "Username cannot be empty. A valid username use only letters, numbers, or symbols (@, ., -, _), and have a length between 6 and 8 characters.";
                usernameTip.TextColor = Color.FromRgb(255, 0, 0);
            }
            else
            {
                string username = entry.Text;
                string usernameRule = @"^[a-zA-Z0-9@._-]{6,8}$"; 
                Regex usernameRegex = new Regex(usernameRule);

                if (!usernameRegex.IsMatch(username))
                {
                    usernameTip.Text = "Invalid username. A valid username use only letters, numbers, or symbols (@, ., -, _), and have a length between 6 and 8 characters.";
                    usernameTip.TextColor = Color.FromRgb(255, 0, 0);
                }
                else if (!UsernameAvaliable(username))
                {
                    usernameTip.Text = "This username is already in use. A valid username use only letters, numbers, or symbols (@, ., -, _), and have a length between 6 and 8 characters.";
                    usernameTip.TextColor = Color.FromRgb(255, 0, 0);
                }
                else
                {
                    usernameTip.Text = "Username is valid.";
                    usernameTip.TextColor = Color.FromRgb(0, 255, 0);
                }
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
                passwordTip.Text = "Password cannot be empty. A valid password must include at least one digit, one uppercase letter, one lowercase letter, one special character from (@, ., -, _), and have a length between 8 and 10 characters.";
                passwordTip.TextColor = Color.FromRgb(255, 0, 0);
            }
            else
            {
                string password = entry.Text;
                string passwordRule = @"^(?=.*\d)(?=.*[A-Z])(?=.*[a-z])(?=.*[@._-]).{8,10}$";
                Regex passwordRegex = new Regex(passwordRule);

                if (!passwordRegex.IsMatch(password))
                {
                    passwordTip.Text = "Invalid password. A valid password must include at least one digit, one uppercase letter, one lowercase letter, one special character from (@, ., -, _), and have a length between 8 and 10 characters.";
                    passwordTip.TextColor = Color.FromRgb(255, 0, 0);
                }
                else
                {
                    passwordTip.Text = "Password is valid.";
                    passwordTip.TextColor = Color.FromRgb(0, 255, 0);
                }
            }

        }
    }

    private void CheckConfirmPasswordFormat(object sender, FocusEventArgs e)
    {

        Entry entry = sender as Entry;
        if (entry != null)
        {
            if (string.IsNullOrWhiteSpace(entry.Text))
            {
                confirmPasswordTip.Text = "Confirm password cannot be empty.";
                confirmPasswordTip.TextColor = Color.FromRgb(255, 0, 0);
            }
            else
            {
                string confirmPassword = entry.Text;
                if (!passwordTip.Text.Equals("Password is valid."))
                {
                    confirmPasswordTip.Text = "Please enter a valid password first.";
                    confirmPasswordTip.TextColor = Color.FromRgb(255, 0, 0);
                }
                else if (passwordEntry.Text.Equals(confirmPassword))
                {
                    confirmPasswordTip.Text = "Password has been comfirmed.";
                    confirmPasswordTip.TextColor = Color.FromRgb(0, 255, 0);
                }
                else
                {
                    confirmPasswordTip.Text = "Passwords do not match.";
                    confirmPasswordTip.TextColor = Color.FromRgb(255, 0, 0);
                }
            }

        }
    }

    private Boolean UsernameAvaliable(string username)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            using (MySqlCommand command = new MySqlCommand())
            {
                command.Connection = connection;
                command.CommandText = "SELECT * FROM `users` WHERE AES_DECRYPT(username, @Key) = @Username";
                command.Parameters.AddWithValue("@Key", key);
                command.Parameters.AddWithValue("@Username", username);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    return !reader.HasRows;
                }
            }
        }
    }


    private async void SubmitRegisterForm(object sender, EventArgs e)
    {
        string usernameValid = usernameTip.Text;
        string passwordValid = passwordTip.Text;
        string confirmValid = confirmPasswordTip.Text;
        if (usernameValid.Equals("Username is valid.") && passwordValid.Equals("Password is valid.") && confirmValid.Equals("Password has been comfirmed."))
        {
            string username = usernameEntry.Text;
            string password = passwordEntry.Text;
            SaveNewAccount(username, password);

            submitTip.Text = "Registered successfully";
            submitTip.TextColor = Color.FromRgb(0, 255, 0);

            await Navigation.PushAsync(new RegisteredPage());

        }
        else
        {
            submitTip.Text = "Error. Please enter valid username and password.";
            submitTip.TextColor = Color.FromRgb(255, 0, 0);
        }
        
    }

    private void SaveNewAccount(string username, string password)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            using (MySqlCommand command = new MySqlCommand())
            {
                command.Connection = connection;
                command.CommandText = "INSERT INTO `users` (username, password) VALUES (AES_ENCRYPT(@Username, @Key), AES_ENCRYPT(@Password, @Key))";
                command.Parameters.AddWithValue("@Key", key);
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Password", hashedPassword(password));

                command.ExecuteNonQuery();
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