using System;

namespace ProgramacionXEfsi_Puente.Models
{
    public class Colaboracion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int NecesidadId { get; set; }
        public DateTime Fecha { get; set; }
        public string Comentario { get; set; } = string.Empty;
    }
}
