using System.ComponentModel.DataAnnotations.Schema;

namespace MTFClientServerCommon.DbEntities
{
    public abstract class DbEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
    }
}