using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System
{
    [Alias("ImportLogs")]
    public class ImportLog
    {
        [AutoIncrement]
        public int Id { get; set; }

        public byte ImporterId { get; set; }

        public short TypeId { get; set; }

        public string SourceId { get; set; }

        public int TargetId { get; set; }

        public DateTime LastActivity { get; set; }

        public string Hash { get; set; }
    }
}