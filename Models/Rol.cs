using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLosAndes.Models
{
    [Table("Roles")]
    public class Rol
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string? Nombre { get; set; }

        public virtual ICollection<Usuario>? Usuarios { get; set; }
    }
}