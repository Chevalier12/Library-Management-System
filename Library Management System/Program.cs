using System;
using System.Data;
using System.Security.AccessControl;

namespace Library_Management_System
{
    internal class Program
    {
        public static string ConnectionString = "Server=localhost;User ID = root; Database=librarydatabase; Allow User Variables = true";
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Librarian, what would you like to do today?");
            Console.WriteLine();
            Console.WriteLine("1. Add a new book to the library.");
            Console.WriteLine("2. Add a new rental.");
            Console.WriteLine("3. Add a new return.");
            Console.WriteLine("4. Show all books.");
            Console.WriteLine("5. Show all rentals.");

            while (true)
            {
                var Input = Console.ReadLine();

                switch (Input)
                {
                    case "1":
                        case1();
                        break;
                    case "2":
                        case2();
                        break;
                    case "3":
                        case3();
                        break;
                    case "4":
                        case4();
                        break;
                    case "5":
                        case5();
                        break;
                }
            }
        }

        public static void case1()
        {
            //Create new book with user input
            var newBook = new Books();
            Console.WriteLine("Enter the title of the book:");
            newBook.BookName = Console.ReadLine();
            Console.WriteLine("Enter the ISBN of the book:");
            newBook.ISBN = Console.ReadLine();
            Console.WriteLine("Enter the renting price of the book:");
            newBook.RentPrice = Double.Parse(Console.ReadLine());
            Console.WriteLine("Enter the quantity of the books:");
            newBook.Quantity = int.Parse(Console.ReadLine());


            //Read books table
            var SQLTable = SQLMethods.SQLTable_Load("books", ConnectionString);
            //Find duplicate, if duplicate is found then increase quantity of books by user input
            bool DuplicateFound = false;
            foreach (DataRow item in SQLTable.Rows)
            {
                var BookConc = newBook.BookName + newBook.ISBN + newBook.RentPrice + newBook.Quantity;
                var itemConc = item[1].ToString() + item[2].ToString() + item[3].ToString() + item[4].ToString();

                if (BookConc == itemConc)
                {
                    DuplicateFound = true;
                    Console.WriteLine("Found this book in the database, insert quantity to add:");
                    var addQuantity = Console.ReadLine();
                    item[4] = int.Parse(item[4].ToString()) + int.Parse(addQuantity);
                    break;
                }

            }

            //Read current database and insert the new book if there are no duplicates, otherwise just increase quantity
            if (!DuplicateFound)
            {
                DataRow newDataRow = SQLTable.NewRow();
                newDataRow["Name"] = newBook.BookName;
                newDataRow["ISBN"] = newBook.ISBN;
                newDataRow["Rent Price"] = newBook.RentPrice;
                newDataRow["Quantity"] = newBook.Quantity;
                SQLTable.Rows.Add(newDataRow);
            }


            SQLMethods.SQLTable_Update(SQLTable, "books", ConnectionString);
        }

        public static void case2()
        {
            var newClient = new Client();
            Console.WriteLine("Enter the name of the client:");
            newClient.ClientName = Console.ReadLine();

            var SQLTableBooks = SQLMethods.SQLTable_Load("books", ConnectionString);
            Console.WriteLine("Select the book you would like to rent.");
            Console.WriteLine();

            //Find book to rent
            var index = 0;
            var error = false;
            foreach (DataRow item in SQLTableBooks.Rows)
            {
                if (item[4].ToString() != "0")
                {error = false;}
                else error = true;

                Console.WriteLine(index + ". Name: " + item[1].ToString() + " ISBN: " + item[2].ToString() +
                                  " Rent Price: " + item[3].ToString() + " Quantity: " + item[4].ToString());
                index++;
            }

            Console.WriteLine();
            var BookIndex = int.Parse(Console.ReadLine());
            newClient.BookToRent = SQLTableBooks.Rows[BookIndex][1].ToString();

            if((int)SQLTableBooks.Rows[BookIndex][4] > 0)
            SQLTableBooks.Rows[BookIndex][4] = int.Parse(SQLTableBooks.Rows[BookIndex][4].ToString()) - 1;

            SQLMethods.SQLTable_Update(SQLTableBooks, "books",ConnectionString);
            Console.WriteLine("Selected " + '"' + newClient.BookToRent + '"' + ".");
            newClient.RentDate = DateTime.Now;
            newClient.PenaltiesAccumulated = 0;

            if (!error)
            {
                var SQLTableClients = SQLMethods.SQLTable_Load("clients", ConnectionString);
                DataRow newDataRow = SQLTableClients.NewRow();
                newDataRow["Client Name"] = newClient.ClientName;
                newDataRow["Book"] = newClient.BookToRent;
                newDataRow["ISBN"] = SQLTableBooks.Rows[BookIndex][2];
                newDataRow["Rent Price"] = SQLTableBooks.Rows[BookIndex][3];
                newDataRow["Rent Date"] = newClient.RentDate;
                newDataRow["Penalties Accumulated"] = newClient.PenaltiesAccumulated;
                SQLTableClients.Rows.Add(newDataRow);
                SQLMethods.SQLTable_Update(SQLTableClients, "clients", ConnectionString);
                Console.WriteLine("");
                Console.WriteLine("Succesfully rented the book: " + newClient.BookToRent + " to " + newClient.ClientName);
            }
            else
            {
                Console.WriteLine("We are sorry, we do not have the book in stock.");
            }



        }

        public static void case3()
        {

            var SQLTableClients = SQLMethods.SQLTable_Load("clients", ConnectionString);
            var SQLTableBooks = SQLMethods.SQLTable_Load("books", ConnectionString);
            Console.WriteLine("Select the book you would like to return.");
            Console.WriteLine();

            //Find book to return for specific clients
            var index = 0;
            foreach (DataRow item in SQLTableClients.Rows)
            {
                Console.WriteLine(index + ". Name: " + item[1].ToString() + " Book: " + item[2].ToString() + " ISBN: " + item[3].ToString() + " Rent Price: " + item[4].ToString());
                index++;
            }

            Console.WriteLine();
            var ClientIndex = int.Parse(Console.ReadLine());
            var ClientBookConc = SQLTableClients.Rows[ClientIndex][2].ToString() +
                                 SQLTableClients.Rows[ClientIndex][3].ToString() +
                                 SQLTableClients.Rows[ClientIndex][4].ToString();

            foreach (DataRow item in SQLTableBooks.Rows)
            {
                var BookConc = item[1].ToString() + item[2].ToString() + item[3].ToString();
                if (ClientBookConc == BookConc)
                {
                    item[4] = (int)item[4] + 1;
                    SQLMethods.SQLTable_Update(SQLTableBooks, "books", ConnectionString);
                    break;
                }

            }

            DateTime DateToReturn = (DateTime)SQLTableClients.Rows[ClientIndex][5];
            var CalculatePenalties = (int)(DateTime.Now - DateToReturn).TotalDays * 0.01 * (int)SQLTableClients.Rows[ClientIndex][4];

            if (CalculatePenalties <= 0)
                Console.WriteLine("No penalties to pay.");
            else
            {
                Console.WriteLine("Due to penalties, you have to pay the balance: " + CalculatePenalties);
            }

            Console.WriteLine("Succesfully returned the book.");
            SQLTableClients.Rows[ClientIndex].Delete();
            SQLMethods.SQLTable_Update(SQLTableClients, "clients", ConnectionString);
        }

        public static void case4()
        {
            Console.WriteLine("");
            var SQLTableBooks = SQLMethods.SQLTable_Load("books", ConnectionString);
            var index = 0;
            foreach (DataRow item in SQLTableBooks.Rows)
            {
                Console.WriteLine(index + ". Name: " + item[1] + " ISBN: " + item[2] + " Rent Price: " + item[3] + " Quantity: " + item[4]);
                index++;
            }
        }

        public static void case5()
        {
            Console.WriteLine("");
            var SQLTableClients = SQLMethods.SQLTable_Load("books", ConnectionString);
            var index = 0;
            foreach (DataRow item in SQLTableClients.Rows)
            {
                Console.WriteLine(index + ". Name: " + item[1] + " Book: " + item[2] + " ISBN: " + item[3] + " Rent Price: " + item[4]);
                index++;
            }
        }
    }
}
