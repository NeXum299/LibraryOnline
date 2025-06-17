namespace AspAppLibrary.DTO
{
    public class BookDTO
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Author { get; set; }
        public string YearPublished { get; set; }
        public string Genre { get; set; }
        public bool IsAvailable { get; set; }
    }
}
