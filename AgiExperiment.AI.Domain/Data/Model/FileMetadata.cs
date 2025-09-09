namespace AgiExperiment.AI.Domain.Data.Model
{
    public class FileMetadata
    {
        public string? DocumentId { get; set; }
        public string? Name { get; set; } = default!;
        public long Size { get; set; }
        public DateTimeOffset LastModified { get; set; }
    }
}
