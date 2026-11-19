namespace ProgramacionXEfsi_Puente.Models
{
    public class Publicacion
    {
        public int Id { get; set; }
        public int IdOrganizacion { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string TipoPropuesta { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public string Necesidades { get; set; } = string.Empty;
        public string CosasQueNecesitan { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime? FechaLimite { get; set; }
        public string NombreOrganizacion { get; set; } = string.Empty;
        public string TelefonoOrganizacion { get; set; } = string.Empty;
        public string DireccionOrganizacion { get; set; } = string.Empty;
        public string BarrioOrganizacion { get; set; } = string.Empty;
        public List<NecesidadPublicacion> NecesidadesDetalle { get; set; } = new();
        public int PorcentajeGeneral
        {
            get
            {
                if (NecesidadesDetalle.Count == 0) return 0;
                var solicitado = NecesidadesDetalle.Sum(x => x.CantidadSolicitada);
                var recibido = NecesidadesDetalle.Sum(x => x.CantidadRecibida);
                if (solicitado <= 0) return 0;
                return Math.Min(100, (int)Math.Round((decimal)recibido * 100 / solicitado));
            }
        }
    }

    public class NecesidadPublicacion
    {
        public int IdNecesidad { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public int CantidadSolicitada { get; set; }
        public int CantidadRecibida { get; set; }
        public int Porcentaje => CantidadSolicitada <= 0 ? 0 : Math.Min(100, (int)Math.Round((decimal)CantidadRecibida * 100 / CantidadSolicitada));
        public int Restante => Math.Max(0, CantidadSolicitada - CantidadRecibida);
    }

    public class ResultadoDonacionPublicacion
    {
        public int Resultado { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public int Porcentaje { get; set; }
    }
}
