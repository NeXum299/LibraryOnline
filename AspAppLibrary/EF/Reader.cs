namespace AspAppLibrary.EF
{
    public class Reader
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime RegistrationDate { get; set; }
        public ICollection<BorrowRecord> BorrowRecords { get; set; }
    }
}
