using System;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;

namespace MAUIAPP1
{
	public class ViewModel
	{
        string connectionString;
        byte[] key;
        int userID;

        public ObservableCollection<Contact> Contacts { get; set; }

        public ViewModel(int userID, string connectionString, byte[] key)
		{
            this.userID = userID;
            this.connectionString = connectionString;
            this.key = key;
            Contacts = new ObservableCollection<Contact>();
            LoadData();
        }

        public void LoadData()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "SELECT id, AES_DECRYPT(contact_name, @key) AS contact_name, AES_DECRYPT(contact_number, @key) AS contact_number, AES_DECRYPT(contact_email, @key) AS contact_email FROM contacts WHERE user_id = @userId";
                    command.Parameters.AddWithValue("@key", key);
                    command.Parameters.AddWithValue("@userId", userID);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                int id = reader.GetInt32("id");
                                string contactName = reader.GetString("contact_name");
                                string contactNumber = reader.GetString("contact_number");
                                string contactEmail = reader.GetString("contact_email");

                                Contact contact = new Contact
                                {
                                    _name = contactName,
                                    _email = contactEmail,
                                    _id = id,
                                    _phone = contactNumber
                                };

                                Contacts.Add(contact);
                            }
                        }

                    }
                }
            }

        }


    }
}

