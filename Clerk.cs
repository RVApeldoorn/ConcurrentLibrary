using System.Threading;

namespace library;

public class Clerk
{
    private static LinkedList<BookRecord> _records;
    private int _id;
    public Thread Thread;

    static Clerk()
    {
        _records = new LinkedList<BookRecord>();
    }

    public Clerk(int clerkId)
    {
        _id = clerkId;
        Thread = new Thread(DoWork);
        if (_records == null)
        {
            _records = new LinkedList<BookRecord>();
        }
    }

    public static void initRecords(LinkedList<Book> books)
    {
        if (_records == null)
        {
            _records = new LinkedList<BookRecord>();
        }
        foreach (Book book in books)
        {
            _records.AddFirst(new BookRecord(book, false));
        }
    }

    internal static int checkBookInInventory()
    // this method is called when the library is closing
    {
        int counter = 0;
        foreach (var record in _records)
        {
            if (record.IsBorrowed == false)
            {
                counter++;
            }
        }

        if (counter != _records.Count)
        {
            Console.WriteLine("Error: the number of books left in the library does not match the number of records." + counter + _records.Count);
        }
        return counter;
    }

    public void DoWork()
    {
        //the clerk will choose an available book from the inventory, bring it to the pickup counter and return it from the dropoff counter
        Console.WriteLine($"Clerk [{_id}] is going to check in the records for a book to put on the counter");

        Book? t_book = null;

        Program.recordsSemaphore.WaitOne(); //wait for a book to be available
        Program.recordsMutex.WaitOne(); //lock records, a shared resource

        foreach (var record in _records) //the clerk will search in the records for a book that is not yet borrowed
        {
            if (record.IsBorrowed == false)
            {
                t_book = record.Book;
                record.IsBorrowed = true;
                break;
            }
        }
        
        Program.recordsMutex.ReleaseMutex(); //unlock records

        Console.WriteLine($"Clerk [{_id}] putting book [{t_book.BookId}] on the counter");

        Program.counterMutex.WaitOne(); //lock counter, a shared resource
        Program.counter.AddFirst(t_book); //put book on the counter
        Program.counterSemaphore.Release(); //signal a book is available on the counter
        Program.counterMutex.ReleaseMutex(); //unlock counter
        
        Thread.Sleep(new Random().Next(100, 500)); //the clerk will take a nap for overworking

        Program.dropoffSemaphore.WaitOne(); //wait until a book is dropped off
        Program.dropoffMutex.WaitOne(); //lock the dropoff, a shared resource

        t_book = Program.dropoff.First();
        Program.dropoff.RemoveFirst();

        Program.dropoffMutex.ReleaseMutex(); //unlock dropoff

        Console.WriteLine($"Clerk [{_id}] is checking in the book [{t_book.BookId}] in the records");

        Program.recordsMutex.WaitOne(); //lock records, a shared resource
        foreach (BookRecord record in _records)
        {
            if (record.Book.BookId == t_book.BookId)
            {
                record.IsBorrowed = false;
                Program.recordsSemaphore.Release(); //signal al book is back available
                break;
            }
        }
        Program.recordsMutex.ReleaseMutex(); //unlock records
    }
}


