using System;

namespace library
{
    internal class Program
    {
        public static int n_threads = 1000;
        public static int n_books = n_threads;
        public static readonly Clerk[] clerks = new Clerk[n_threads];
        public static readonly Customer[] customers = new Customer[n_threads];
        public static LinkedList<Book> counter = new();
        public static LinkedList<Book> dropoff = new();

        public static Mutex recordsMutex = new Mutex();
        public static Mutex counterMutex = new Mutex(); 
        public static Mutex dropoffMutex = new Mutex();

        public static Semaphore counterSemaphore = new(0, 1); 
        public static Semaphore dropoffSemaphore = new(0, 1); 
        public static Semaphore recordsSemaphore = new(1, 1); 

        static void Main(string[] args)
        {
            if (n_books > 0)
            {
                counterSemaphore = new Semaphore(0, n_books); //ensures customers wait until a book is available on the counter
                dropoffSemaphore = new Semaphore(0, n_books); //ensures clerks wait until a book is dropped off
                recordsSemaphore = new Semaphore(n_books, n_books); //semaphore for accessing inventory

                Console.WriteLine("Goodmorning, we are starting a new day at the library!");

                InitLibrary(); 
                InitCustomers();
                InitClerks();

                Clerk.initRecords(dropoff);
                dropoff.Clear();

                StartClerks();
                StartCustomers();

                //wait for all clerk threads to finish
                foreach (var clerk in clerks)
                {
                    clerk.Thread.Join();
                }

                //wait for all customer threads to finish
                foreach (var customer in customers)
                {
                    customer.Thread.Join();
                }

                //closing the library, the counters should be empty and the inventory full
                Console.WriteLine(new string('-', 56));

                Console.WriteLine("Book left in the library " + Clerk.checkBookInInventory());

                Console.WriteLine("Books left on the pickup counter and not processed: " + counter.Count);

                Console.WriteLine("Books left on the dropoff counter and not processed: " + dropoff.Count);
            }
            else
            {
                Console.WriteLine("Error: Someone tried to start a library without any books.");
            }
        }

        public static void InitLibrary()
        {
            for (int i = 0; i < n_books; i++)
            {
                Book book = new Book(i);
                dropoff.AddLast(book);     
            }
        }
        public static void InitCustomers()
        {
            for (int i = 0; i < n_threads; i++)
            {
                customers[i] = new Customer(i);
            }

        }
        public static void InitClerks()
        {
            for (int i = 0; i < n_threads; i++)
            {
                clerks[i] = new Clerk(i);
            }

        }
        public static void StartClerks()
        {
            foreach (var clerk in clerks)
            {
                clerk.Thread.Start();
            }
        }
        public static void StartCustomers()
        {
            foreach (var customer in customers)
            {
                customer.Thread.Start();
            }
        }
    }

    public class Book
    {
        public int BookId { get; set; }
        
        public Book(int bookId)
        {
            BookId = bookId;
        }
    }

    public class BookRecord
    {
        public Book Book { get; set; }
        public bool IsBorrowed { get; set; }

        public BookRecord(Book book, bool isBorrowed)
        {
            Book = book;
            IsBorrowed = isBorrowed;
        }
    }
}