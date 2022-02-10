namespace DummyBot
{
    class Program
    {
        static void Main(string[] args) =>
            new Bot().MainAsync().GetAwaiter().GetResult();
    }
}