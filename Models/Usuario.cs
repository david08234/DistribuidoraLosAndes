using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraLosAndes.Models
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [Column("NombreCompleto")] // <- Conecta con tu SQL
        [Display(Name = "Nombre")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "El correo es requerido")]
        [Column("Correo")] // <- Conecta con tu SQL
        [EmailAddress(ErrorMessage = "Email no válido")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        // Ojo: Quitamos Edad, Ciudad y FechaRegistro porque no existen en tu tabla de SQL

        [Required]
        public int RolId { get; set; }

        [ForeignKey("RolId")]
        public virtual Rol? Rol { get; set; }
    }
}