using System;
using System.Collections.Generic;
using System.Text;
namespace Aurea.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime Created { get; set; }
        public long FileSize { get; set; }
        public List<Collaborator> Collaborators { get; set; }
    }
}