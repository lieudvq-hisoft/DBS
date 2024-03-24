using System;
namespace Data.Models
{
    public class FileEModel
    {
        public byte[] Content { get; set; }
        //public string ContentType { get; set; }
        public string Extension { get; set; }
    }

    public class FileModel
    {
        public Guid Id { get; set; }
        public string Path { get; set; }
    }
}

