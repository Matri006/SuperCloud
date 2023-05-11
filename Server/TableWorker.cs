using General;

using SQLite;

namespace Server
{
    [Table("Workers")]
    public sealed class TableWorker : Worker
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
    }
}