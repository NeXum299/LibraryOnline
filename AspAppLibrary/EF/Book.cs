namespace AspAppLibrary.EF
{
    public class Book
    {
        public Book()
        {
            BorrowRecords = new HashSet<BorrowRecord>();
        }

        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Author { get; set; }
        public string YearPublished { get; set; } = "Unknow";
        public string Genre { get; set; } = "Unknow";
        public bool IsAvailable { get; set; }
        public ICollection<BorrowRecord> BorrowRecords { get; set; }
    }
}
