using System.Threading;

namespace library;

public class Customer
{
    private Book? _currentBook;
    private readonly int _id;
    public Thread Thread;

    public Customer(int customerId)
    {
        _currentBook = null;
        _id = customerId++;
        Thread = new Thread(DoWork);
    }

    public Book? GetCurrentBook()
    {
        return _currentBook;
    }

    public void DoWork()
    {
        // the customer can pick a book when one is ready at the pickup counter and will return it at the dropoff counter
        Program.counterSemaphore.WaitOne(); //wait unitl a book is available
        Program.counterMutex.WaitOne(); //lock counter, a shared resource

        _currentBook = Program.counter.First();
        Program.counter.RemoveFirst();
        
        Program.counterMutex.ReleaseMutex(); //unlock counter

        Console.WriteLine($"Customer {_id} is about to read the book {_currentBook.BookId}");
        Thread.Sleep(new Random().Next(100, 500)); //reading the book...
        Console.WriteLine($"Customer {_id} is dropping off the book {_currentBook.BookId}");

        Program.dropoffMutex.WaitOne(); //lock dropoff, a shared resource
        Program.dropoff.AddFirst(_currentBook);
        Program.dropoffSemaphore.Release(); //signal a book has been dropped off
        Program.dropoffMutex.ReleaseMutex(); //unlock dropoff

        Console.WriteLine($"Customer {_id} is leaving the library");
    }
}