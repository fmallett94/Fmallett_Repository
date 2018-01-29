using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using System.Reflection;

namespace SQL_Northwnd_Suppliers
{
    // Program to run CRUD operations on NORTHWND Database, Suppliers table 
    // Sean Gentry, Francesca Mallett, and Cody Walton
    // January 26, 2018

    class Program
    {
        static void Main(string[] args)
        {
            //Instantiate menu option variable and create array of valid menu options
            string menuOption;
            string[] validMenuOptions = new string[] { "1", "2", "3", "4", "5", "6" };

            //do while to return to main menu after each operation, until quit is selected
            do
            {
                //menu information to pass into input check function
                string menuDisplay = "Please select an option below." +
                    "\n" +
                    "\n 1) View all suppliers" +
                    "\n 2) View a supplier by ID" +
                    "\n 3) Add a supplier" +
                    "\n 4) Update existing supplier info" +
                    "\n 5) Delete a supplier" +
                    "\n 6) Quit";

                Console.WriteLine();

                //Get valid menu option selection from user and clear menu display
                menuOption = GetValidInput(menuDisplay, validMenuOptions);
                Console.Clear();

                try
                {
                    //Switch to choose operation by menu selection
                    switch (menuOption)
                    {
                        case "1": //View all
                            ViewAll();
                            Console.WriteLine("Press any key to continue.");
                            Console.ReadKey();
                            Console.Clear();
                            break;

                        case "2": //View by ID
                            //Get ID number from user
                            Console.WriteLine("Enter Supplier ID number to view");
                            string idInput = Console.ReadLine();
                            Console.Clear();

                            //Show user supplier information matching that ID
                            ViewByID(idInput);
                            Console.WriteLine("Press any key to continue.");
                            Console.ReadKey();
                            Console.Clear();
                            break;

                        case "3": //Create new
                            AddSupplier();
                            break;

                        case "4": //Update
                            ViewAll();
                            UpdateSupplier();
                            break;

                        case "5": //Delete
                            //Provides supplier list before method to delete supplier
                            ViewAll();
                            DeleteSupplier();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    //Store exception message and method where exception occurred
                    //and run method to log the exception
                    string exMessage = ex.Message;
                    MethodBase site = ex.TargetSite;
                    string methodName = site.ToString();
                    LogException(methodName, exMessage);
                }
                finally
                {
                    Console.Clear();
                }
            }
            while (menuOption != "6");
            //Back to main menu display to repeat until the quit option is selected
        }

        //Methods

        private static void LogException(string methodName, string exMessage)
        {
            //Getting required strings to write to log file
            string timeStamp = DateTime.Now.ToString();
            string logLevel = "Fatal";
            //Getting log path from config file
            string logPath = ConfigurationManager.AppSettings["LogPath"];

            //Streamwriter to append information to log
            StreamWriter writer = new StreamWriter(logPath, true);
            writer.WriteLine("{0}  {1}  {2}  {3}", timeStamp, logLevel, methodName, exMessage);

            writer.Close();
            writer.Dispose();

            Console.ReadKey();

            return;
        }

        private static string GetValidInput(string instructions, string[] validOptions)
        {
            //Initializing bool to check and string for input
            bool valid = false;
            string userInput = "";
            while (!valid)
            {
                //Provide menu and receive key input from user
                Console.WriteLine(instructions);
                userInput = Console.ReadKey().KeyChar.ToString();
                Console.WriteLine();
                //Check whether input matches an item in the valid options array
                valid = validOptions.Contains(userInput);

                if (!valid)
                {
                    //Invalid input message
                    Console.Clear();
                    Console.WriteLine("Invalid input!");
                }
            }
            //Escape while loop when a valid input is provided, and return that input
            return userInput;
        }

        private static void ViewAll()
        {   ///<summary>Method to view all suppliers and display in console</summary>
            //Method to view all suppliers
            string connectionString = ConfigurationManager.ConnectionStrings["dataSource"].ConnectionString;
            SqlConnection connectionToSql = null;
            SqlCommand storedProcedure = null;
            //New table to store query information, adapter to fill table
            DataTable suppliers = new DataTable();
            SqlDataAdapter adapter = null;

            try
            {   //Connection string to find database
                connectionToSql = new SqlConnection(connectionString);
                //Stored procedure to run
                storedProcedure = new SqlCommand("OBTAIN_SUPPLIERS", connectionToSql);
                storedProcedure.CommandType = System.Data.CommandType.StoredProcedure;
                //Open connection to SQL
                connectionToSql.Open();
                //Adapt query information into empty table
                adapter = new SqlDataAdapter(storedProcedure);
                adapter.Fill(suppliers);

                //for each loop to write each row's information to console
                foreach (DataRow row in suppliers.Rows)
                {
                    Console.WriteLine("{0}{1}: {2}", "ID", new string(' ', 11 - "ID".Length), row["SupplierID"].ToString().Trim());
                    Console.WriteLine("{0}{1}: {2}", "Company", new string(' ', 11 - "Company".Length), row["CompanyName"].ToString().Trim());
                    Console.WriteLine("{0}{1}: {2}", "Contact", new string(' ', 11 - "Contact".Length), row["ContactTitle"].ToString().Trim());
                    Console.WriteLine("{0}  {1}", new string(' ', 11), row["ContactName"].ToString().Trim());
                    Console.WriteLine("{0}{1}: {2}", "Country", new string(' ', 11 - "Country".Length), row["Country"].ToString().Trim());
                    Console.WriteLine("{0}{1}: {2}", "Phone", new string(' ', 11 - "Phone".Length), row["Phone"].ToString().Trim());
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                //If exception occurs, write message and throw to main to be logged
                Console.WriteLine(ex.Message);
                throw ex;
            }
            finally
            {
                if (connectionToSql != null)
                {
                    adapter.Dispose();
                    connectionToSql.Close();
                    connectionToSql.Dispose();
                }
            }
            return;
        }

        private static void ViewByID(string idInput)
        {   //Method to view a single supplier's information by ID

            string connectionString = ConfigurationManager.ConnectionStrings["dataSource"].ConnectionString;
            SqlConnection connectionToSql = null;
            SqlCommand storedProcedure = null;
            DataTable suppliers = new DataTable();
            SqlDataAdapter adapter = null;

            try
            {
                connectionToSql = new SqlConnection(connectionString);
                storedProcedure = new SqlCommand("OBTAIN_BY_ID", connectionToSql);

                storedProcedure.CommandType = System.Data.CommandType.StoredProcedure;
                storedProcedure.Parameters.AddWithValue("@SupplierID", idInput);

                connectionToSql.Open();
                adapter = new SqlDataAdapter(storedProcedure);
                adapter.Fill(suppliers);

                //Can display multiple rows (suppliers) if function is ever modified to take multiple IDs
                foreach (DataRow row in suppliers.Rows)
                {
                    Console.WriteLine("{0}{1}: {2}", "ID", new string(' ', 11 - "ID".Length), row["SupplierID"].ToString().Trim());
                    Console.WriteLine("{0}{1}: {2}", "Company", new string(' ', 11 - "Company".Length), row["CompanyName"].ToString().Trim());
                    Console.WriteLine("{0}{1}: {2}", "Contact", new string(' ', 11 - "Contact".Length), row["ContactTitle"].ToString().Trim());
                    Console.WriteLine("{0}  {1}", new string(' ', 11), row["ContactName"].ToString().Trim());
                    Console.WriteLine("{0}{1}: {2}", "Country", new string(' ', 11 - "Country".Length), row["Country"].ToString().Trim());
                    Console.WriteLine("{0}{1}: {2}", "Phone", new string(' ', 11 - "Phone".Length), row["Phone"].ToString().Trim());
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
            finally
            {
                if (connectionToSql != null)
                {
                    adapter.Dispose();
                    connectionToSql.Close();
                    connectionToSql.Dispose();
                }
            }
            return;
        }

        private static void AddSupplier()
        {   //Method to add new supplier

            string connectionString = ConfigurationManager.ConnectionStrings["dataSource"].ConnectionString;
            SqlConnection connectionToSql = null;
            SqlCommand storedProcedure = null;

            try
            {
                //Get new supplier's information from user
                Console.WriteLine("Please enter supplier information.");
                Console.WriteLine();
                Console.WriteLine("Company Name:");
                string companyName = Console.ReadLine();
                Console.WriteLine();

                Console.WriteLine("Title of Contact:");
                string contactTitle = Console.ReadLine();
                Console.WriteLine();

                Console.WriteLine("Name of Contact:");
                string contactName = Console.ReadLine();
                Console.WriteLine();

                Console.WriteLine("Country:");
                string country = Console.ReadLine();
                Console.WriteLine();

                Console.WriteLine("Phone number:");
                string phone = Console.ReadLine();
                Console.Clear();

                //Display new information to confirm
                Console.WriteLine("Confirm new supplier information");
                Console.WriteLine();
                Console.WriteLine("Company: {0}", companyName);
                Console.WriteLine("Contact position: {0}", contactTitle);
                Console.WriteLine("Contact name: {0}", contactName);
                Console.WriteLine("Country: {0}", country);
                Console.WriteLine("Phone: {0}", phone);
                Console.WriteLine();

                Console.WriteLine("Add this supplier information? Y/N");
                string confirmation = Console.ReadKey().KeyChar.ToString();
                Console.Clear();

                //Add information if user confirms, else cancel
                if (string.Equals(confirmation, "Y", StringComparison.CurrentCultureIgnoreCase))
                {
                    connectionToSql = new SqlConnection(connectionString);
                    storedProcedure = new SqlCommand("ADD_SUPPLIER", connectionToSql);

                    //Add required parameters for SQL procedure
                    storedProcedure.CommandType = System.Data.CommandType.StoredProcedure;
                    storedProcedure.Parameters.AddWithValue("@CompanyName", companyName);
                    storedProcedure.Parameters.AddWithValue("@ContactTitle", contactTitle);
                    storedProcedure.Parameters.AddWithValue("@ContactName", contactName);
                    storedProcedure.Parameters.AddWithValue("@Country", country);
                    storedProcedure.Parameters.AddWithValue("@Phone", phone);
                    //Open connection and execute create procedure
                    connectionToSql.Open();
                    storedProcedure.ExecuteNonQuery();
                    //Completion message
                    Console.Clear();
                    Console.WriteLine("Supplier added.");
                    Console.WriteLine();
                    Console.WriteLine("Press any key to continue.");
                }
                else
                {
                    //Cancellation message
                    Console.WriteLine("Operation aborted.");
                    Console.WriteLine();
                    Console.WriteLine("Press any key to continue.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
            finally
            {
                if (connectionToSql != null)
                {
                    connectionToSql.Close();
                    connectionToSql.Dispose();
                }
            }
            Console.ReadKey();
            return;
        }

        private static void UpdateSupplier()
        {   //Method to update all of a supplier's information
            string connectionString = ConfigurationManager.ConnectionStrings["dataSource"].ConnectionString;
            SqlConnection connectionToSql = null;
            SqlCommand storedProcedure = null;

            try
            {
                Console.WriteLine("Enter ID Number of supplier to update:");
                string idInput = Console.ReadLine();
                Console.Clear();
                //After ID selected, display supplier information while user provides new information
                ViewByID(idInput);

                Console.WriteLine("Please enter new information below.");
                Console.WriteLine();

                Console.WriteLine("Company Name:");
                string companyName = Console.ReadLine();
                Console.WriteLine();

                Console.WriteLine("Title of Contact:");
                string contactTitle = Console.ReadLine();
                Console.WriteLine();

                Console.WriteLine("Name of Contact:");
                string contactName = Console.ReadLine();
                Console.WriteLine();

                Console.WriteLine("Country:");
                string country = Console.ReadLine();
                Console.WriteLine();

                Console.WriteLine("Phone number:");
                string phone = Console.ReadLine();
                Console.Clear();

                Console.WriteLine("Confirm update information");
                Console.WriteLine();
                Console.WriteLine("Supplier ID: {0}", idInput);
                Console.WriteLine("Company: {0}", companyName);
                Console.WriteLine("Contact position: {0}", contactTitle);
                Console.WriteLine("Contact name: {0}", contactName);
                Console.WriteLine("Country: {0}", country);
                Console.WriteLine("Phone: {0}", phone);
                Console.WriteLine();
                Console.WriteLine("Update supplier information? Y/N");
                string confirmation = Console.ReadKey().KeyChar.ToString();
                Console.Clear();

                if (string.Equals(confirmation, "Y", StringComparison.CurrentCultureIgnoreCase))
                {
                    connectionToSql = new SqlConnection(connectionString);
                    storedProcedure = new SqlCommand("UPDATE_BY_ID", connectionToSql);

                    storedProcedure.CommandType = System.Data.CommandType.StoredProcedure;
                    storedProcedure.Parameters.AddWithValue("@SupplierID", idInput);
                    storedProcedure.Parameters.AddWithValue("@CompanyName", companyName);
                    storedProcedure.Parameters.AddWithValue("@ContactTitle", contactTitle);
                    storedProcedure.Parameters.AddWithValue("@ContactName", contactName);
                    storedProcedure.Parameters.AddWithValue("@Country", country);
                    storedProcedure.Parameters.AddWithValue("@Phone", phone);

                    connectionToSql.Open();
                    storedProcedure.ExecuteNonQuery();

                    Console.Clear();
                    Console.WriteLine("Supplier information updated.");
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("Operation aborted.");
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
            finally
            {
                if (connectionToSql != null)
                {
                    connectionToSql.Close();
                    connectionToSql.Dispose();
                }
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
            return;
        }

        private static void DeleteSupplier()
        {   //Method to permanently delete a supplier's information
            string connectionString = ConfigurationManager.ConnectionStrings["dataSource"].ConnectionString;
            SqlConnection connectionToSql = null;
            SqlCommand storedProcedure = null;

            try
            {
                Console.WriteLine("Enter Supplier ID number to delete that supplier's information.");
                string idInput = Console.ReadLine();
                Console.Clear();

                Console.WriteLine("Selected supplier information:");
                Console.WriteLine();
                //Show information while user chooses whether to confirm deletion
                ViewByID(idInput);

                Console.WriteLine();
                Console.WriteLine();
                //Warning message and confirmation
                Console.WriteLine("WARNING! You cannot undo this action.");
                Console.WriteLine();
                Console.WriteLine("Delete supplier {0} information from database? Y/N", idInput);
                string confirmation = Console.ReadKey().KeyChar.ToString();

                if (string.Equals(confirmation, "Y", StringComparison.CurrentCultureIgnoreCase))
                {   //Proceed with delete
                    connectionToSql = new SqlConnection(connectionString);
                    storedProcedure = new SqlCommand("DELETE_BY_ID", connectionToSql);

                    storedProcedure.CommandType = System.Data.CommandType.StoredProcedure;
                    storedProcedure.Parameters.AddWithValue("@SupplierID", idInput);

                    connectionToSql.Open();
                    storedProcedure.ExecuteNonQuery();

                    Console.Clear();
                    Console.WriteLine("Deletion successful.");
                    Console.WriteLine();
                }
                else
                {   //Cancel delete
                    Console.Clear();
                    Console.WriteLine("Operation aborted.");
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
            finally
            {
                if (connectionToSql != null)
                {
                    connectionToSql.Close();
                    connectionToSql.Dispose();
                }
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
            return;
        }
    }
}
