using System.Collections.ObjectModel;
using Microsoft.Maui.ApplicationModel.Communication;
using MySql.Data.MySqlClient;

namespace MAUIAPP1;

public partial class UserPage : ContentPage
{
	User user;
    ViewModel viewModel;
    string connectionString;
    byte[] key;
    Contact selectedContact;

    public UserPage(User user, string connectionString, byte[] key)
	{
		this.user = user;
        this.connectionString = connectionString;
        this.key = key;
        InitializeComponent();
		greetingTitle.Text = "Hello, " + user.GetName();
        viewModel = new ViewModel(user.GetId(), connectionString, key);
        BindingContext = viewModel;
        int contactsNumb = viewModel.Contacts.Count();
        contactCounts.Text = contactsNumb + " Contacts";

    }

    private void NewContactClicked(object sender, EventArgs e)
    {
        viewModel.Contacts.Clear();
        viewModel.LoadData();
        searchEntry.Text = "";
        searchTip.Text = "          ";
        userAddContactTitle.Text = "Add New Contact";
        searchEntry.Text = "";
        contactNameEntry.Text = "";
        numberEntry.Text = "";
        emailEntry.Text = "";
        userHomeView.IsVisible = false;
        userAddContactView.IsVisible = true;
        UpdateBtn.IsVisible = false;
        DeleteBtn.IsVisible = false;
        SaveBtn.IsVisible = true;
        BackBtn.IsVisible = true;
    }

    private async void LogOutClicked(object sender, EventArgs e)
    {
        user = null;
        await Navigation.PushAsync(new MainPage());
    }

    private void SearcchContactClicked(object sender, EventArgs e)
    {
        string keyWord = searchEntry.Text;
        if (string.IsNullOrWhiteSpace(keyWord))
        {
            viewModel.Contacts.Clear();
            viewModel.LoadData();
            int contactsNumb = viewModel.Contacts.Count();
            contactCounts.Text = contactsNumb + " Contacts";
            searchTip.Text = "          ";
        }
        else if (ContactExist(keyWord))
        {
            int contactsNumb = viewModel.Contacts.Count();
            contactCounts.Text = contactsNumb + " Contacts";
            searchTip.Text = "          ";
        }
        else
        {
            searchTip.Text = "Can't find contact: " + keyWord;
            searchTip.TextColor = Color.FromRgb(255, 0, 0);
        }
    }

    private void ClearSearchClicked(object sender, EventArgs e)
    {
        viewModel.Contacts.Clear();
        viewModel.LoadData();
        int contactsNumb = viewModel.Contacts.Count();
        contactCounts.Text = contactsNumb + " Contacts";
        searchTip.Text = "          ";
        searchEntry.Text = "";
    }

    private Boolean ContactExist(string keyWord)
    {
        viewModel.Contacts.Clear();
        viewModel.LoadData();

        Console.WriteLine("contactName: " + keyWord);

        ObservableCollection<Contact> ContactsFound = new ObservableCollection<Contact>();
        foreach (Contact c in viewModel.Contacts)
        {
            if (c._name.Contains(keyWord, StringComparison.OrdinalIgnoreCase))
            {
                ContactsFound.Add(c);
                Console.WriteLine("contactName: " + c._name);
            }
        }
        Console.WriteLine("contactName: " + ContactsFound.Count());

        viewModel.Contacts.Clear();
        foreach (Contact c in ContactsFound)
        {
            viewModel.Contacts.Add(c);
        }

        if (ContactsFound.Count() > 0)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    private void CheckContactNameFormat(object sender, FocusEventArgs e)
    {

        Entry entry = sender as Entry;
        if (entry != null)
        {
            if (string.IsNullOrWhiteSpace(entry.Text))
            {
                contactNameTip.Text = "Contact name cannot be empty.";
                contactNameTip.TextColor = Color.FromRgb(255, 0, 0);
            }
            else
            {
                contactNameTip.Text = "        ";
                contactNameTip.TextColor = Color.FromRgb(255, 0, 0);
            }

        }
    }

    private void SubmitNewContactForm(object sender, EventArgs e)
    {
        string name = contactNameEntry.Text;
        string phone = numberEntry.Text;
        string email = emailEntry.Text;

        if (string.IsNullOrWhiteSpace(name))
        {
            SaveTip.Text = "Contact name cannot be empty.";
            SaveTip.TextColor = Color.FromRgb(255, 0, 0);
        }
        else
        {
            SaveTip.Text = "        ";
            SaveTip.TextColor = Color.FromRgb(255, 0, 0);
            if (!string.IsNullOrWhiteSpace(phone))
            {
                phone = phone.Trim();
            }
            else
            {
                phone = "";
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                email = email.Trim();
            }
            else
            {
                email = "";
            }

            SaveNewContactToDatabase(name, phone, email);
            viewModel.Contacts.Clear();
            viewModel.LoadData();
            int contactsNumb = viewModel.Contacts.Count();
            contactCounts.Text = contactsNumb + " Contacts";
            searchTip.Text = "  ";
            searchEntry.Text = "";
            contactNameEntry.Text = "";
            numberEntry.Text = "";
            emailEntry.Text = "";
            userHomeView.IsVisible = true;
            userAddContactView.IsVisible = false;
        }

    }

    private void SaveNewContactToDatabase(string name, string phone, string email)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            using (MySqlCommand command = new MySqlCommand())
            {
                command.Connection = connection;
                command.CommandText = "INSERT INTO `contacts` (user_id, contact_name, contact_number, contact_email) " +
                              "VALUES (@userId, AES_ENCRYPT(@contactName, @key), AES_ENCRYPT(@contactNumber, @key), AES_ENCRYPT(@contactEmail, @key))";
                command.Parameters.AddWithValue("@userId", user.GetId());
                command.Parameters.AddWithValue("@contactName", name);
                command.Parameters.AddWithValue("@contactNumber", phone);
                command.Parameters.AddWithValue("@contactEmail", email);
                command.Parameters.AddWithValue("@key", key);

                command.ExecuteNonQuery();
            }
        }
    }

    private void OnListViewItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is Contact selectedContact)
        {
            this.selectedContact = selectedContact;
            viewModel.Contacts.Clear();
            viewModel.LoadData();
            searchEntry.Text = "";
            searchTip.Text = "          ";
            userAddContactTitle.Text = "Edit Contact";
            searchEntry.Text = "";
            contactNameEntry.Text = selectedContact._name;
            numberEntry.Text = selectedContact._phone;
            emailEntry.Text = selectedContact._email;
            userHomeView.IsVisible = false;
            userAddContactView.IsVisible = true;
            UpdateBtn.IsVisible = true;
            DeleteBtn.IsVisible = true;
            SaveBtn.IsVisible = false;
        }
        

        
    }

    private void UpdateContactForm(object sender, EventArgs e)
    {
        string name = contactNameEntry.Text;
        string phone = numberEntry.Text;
        string email = emailEntry.Text;

        if (string.IsNullOrWhiteSpace(name))
        {
            SaveTip.Text = "Contact name cannot be empty.";
            SaveTip.TextColor = Color.FromRgb(255, 0, 0);
        }
        else
        {
            SaveTip.Text = "        ";
            SaveTip.TextColor = Color.FromRgb(255, 0, 0);
            if (!string.IsNullOrWhiteSpace(phone))
            {
                phone = phone.Trim();
            }
            else
            {
                phone = "";
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                email = email.Trim();
            }
            else
            {
                email = "";
            }

            UpdateContactToDatabase(name, phone, email);
            viewModel.Contacts.Clear();
            viewModel.LoadData();
            int contactsNumb = viewModel.Contacts.Count();
            contactCounts.Text = contactsNumb + " Contacts";
            searchTip.Text = "  ";
            searchEntry.Text = "";
            contactNameEntry.Text = "";
            numberEntry.Text = "";
            emailEntry.Text = "";
            userHomeView.IsVisible = true;
            userAddContactView.IsVisible = false;
        }

    }

    private void UpdateContactToDatabase(string name, string phone, string email)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            using (MySqlCommand command = new MySqlCommand())
            {
                command.Connection = connection;
                command.CommandText = "UPDATE `contacts` SET " +
                      "contact_name = AES_ENCRYPT(@contactName, @key), " +
                      "contact_number = AES_ENCRYPT(@contactNumber, @key), " +
                      "contact_email = AES_ENCRYPT(@contactEmail, @key) " +
                      "WHERE id = @contactId";
                command.Parameters.AddWithValue("@contactId", selectedContact._id);
                command.Parameters.AddWithValue("@contactName", name);
                command.Parameters.AddWithValue("@contactNumber", phone);
                command.Parameters.AddWithValue("@contactEmail", email);
                command.Parameters.AddWithValue("@key", key);

                command.ExecuteNonQuery();
            }
        }
    }

    private void DeleteContact(object sender, EventArgs e)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            using (MySqlCommand command = new MySqlCommand())
            {
                command.Connection = connection;
                command.CommandText = "DELETE FROM `contacts` WHERE id = @contactId";
                command.Parameters.AddWithValue("@contactId", selectedContact._id);
               
                command.ExecuteNonQuery();
            }
        }

        viewModel.Contacts.Clear();
        viewModel.LoadData();
        int contactsNumb = viewModel.Contacts.Count();
        contactCounts.Text = contactsNumb + " Contacts";
        searchTip.Text = "  ";
        searchEntry.Text = "";
        contactNameEntry.Text = "";
        numberEntry.Text = "";
        emailEntry.Text = "";
        userHomeView.IsVisible = true;
        userAddContactView.IsVisible = false;
    }

    private void BackButtonClicked(object sender, EventArgs e)
    {
        viewModel.Contacts.Clear();
        viewModel.LoadData();
        searchEntry.Text = "";
        searchTip.Text = "          ";
        userAddContactTitle.Text = "";
        contactNameEntry.Text = "";
        numberEntry.Text = "";
        emailEntry.Text = "";
        userHomeView.IsVisible = true;
        userAddContactView.IsVisible = false;
    }
}
