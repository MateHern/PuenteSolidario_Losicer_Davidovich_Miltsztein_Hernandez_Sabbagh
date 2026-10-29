using System.Data.SqlClient;
using Dapper;

namespace ProgramacionXEfsi_Puente.Models
{
    public static class BD
    {
        // La DB creada en BD.sql se llama "Puente Solidario" (con espacio)
        // y no es LocalDB. Si tu instancia es SQL Express, cambia "(local)" por ".\SQLEXPRESS".
        private static readonly string connectionString = @"Server=(local);Database=Puente Solidario;Trusted_Connection=True;TrustServerCertificate=True;";

        public static List<Organizacion> ObtenerOrganizaciones()
        {
            using var connection = new SqlConnection(connectionString);

            return connection.Query<Organizacion>(@"
                SELECT
                    o.Id,
                    o.Nombre,
                    CONCAT(o.UbicacionCalle, ' ', o.UbicacionNumero) AS Direccion,
                    o.UbicacionLocalidad AS Barrio,
                    CAST(o.Telefono AS varchar(50)) AS Telefono
                FROM Organizaciones o
                WHERE o.EsAprobado = 1
                ORDER BY o.Nombre").ToList();
        }

        public static Usuario? ObtenerUsuarioPorId(int id)
        {
            using var connection = new SqlConnection(connectionString);

            var row = connection.QueryFirstOrDefault<dynamic>(
                "SELECT TOP 1 Id, Nombre, Apellido, Mail AS Email, Contraseña AS Clave FROM Usuarios WHERE Id = @Id",
                new { Id = id });

            if (row == null)
            {
                return null;
            }

            return new Usuario
            {
                Id = (int)row.Id,
                Nombre = row.Nombre ?? string.Empty,
                Apellido = row.Apellido ?? string.Empty,
                Email = row.Email ?? string.Empty,
                Clave = row.Clave ?? string.Empty
            };
        }

        public static Organizacion? ObtenerOrganizacionPorId(int id)
        {
            using var connection = new SqlConnection(connectionString);

            var row = connection.QueryFirstOrDefault<dynamic>(
                "SELECT TOP 1 Id, Nombre, Mail, Contraseña, Telefono, UbicacionCalle, UbicacionNumero, UbicacionLocalidad FROM Organizaciones WHERE Id = @Id",
                new { Id = id });

            if (row == null)
            {
                return null;
            }

            return new Organizacion
            {
                Id = (int)row.Id,
                Nombre = row.Nombre ?? string.Empty,
                Direccion = $"{row.UbicacionCalle ?? string.Empty} {row.UbicacionNumero ?? string.Empty}".Trim(),
                Barrio = row.UbicacionLocalidad ?? string.Empty,
                Telefono = row.Telefono?.ToString() ?? string.Empty
            };
        }

        public static Usuario? ValidarLoginUsuario(string email, string clave)
        {
            using var connection = new SqlConnection(connectionString);

            var row = connection.QueryFirstOrDefault<dynamic>(
                "SELECT TOP 1 Id, Nombre, Apellido, Mail AS Email, Contraseña AS Clave FROM Usuarios WHERE Mail = @email AND Contraseña = @clave",
                new { email, clave });

            if (row == null)
            {
                return null;
            }

            return new Usuario
            {
                Id = (int)row.Id,
                Nombre = row.Nombre ?? string.Empty,
                Apellido = row.Apellido ?? string.Empty,
                Email = row.Email ?? string.Empty,
                Clave = row.Clave ?? string.Empty
            };
        }

        public static Organizacion? ValidarLoginOrganizacion(string email, string clave)
        {
            using var connection = new SqlConnection(connectionString);

            var row = connection.QueryFirstOrDefault<dynamic>(
                "SELECT TOP 1 Id, Nombre, Mail, Contraseña, Telefono, UbicacionCalle, UbicacionNumero, UbicacionLocalidad FROM Organizaciones WHERE Mail = @email AND Contraseña = @clave",
                new { email, clave });

            if (row == null)
            {
                return null;
            }

            return new Organizacion
            {
                Id = (int)row.Id,
                Nombre = row.Nombre ?? string.Empty,
                Direccion = $"{row.UbicacionCalle ?? string.Empty} {row.UbicacionNumero ?? string.Empty}".Trim(),
                Barrio = row.UbicacionLocalidad ?? string.Empty,
                Telefono = row.Telefono?.ToString() ?? string.Empty
            };
        }

        public static bool ExisteUsuarioPorEmail(string email)
        {
            using var connection = new SqlConnection(connectionString);
            var count = connection.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM Usuarios WHERE Mail = @email",
                new { email });
            return count > 0;
        }

        public static bool ExisteOrganizacionPorEmail(string email)
        {
            using var connection = new SqlConnection(connectionString);
            var count = connection.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM Organizaciones WHERE Mail = @email",
                new { email });
            return count > 0;
        }

        public static void RegistrarUsuario(string nombre, string apellido, string email, string password)
        {
            using var connection = new SqlConnection(connectionString);

            connection.Execute(@"
                INSERT INTO Usuarios (
                    NombreUsuario,
                    Nombre,
                    Apellido,
                    FechaNacimiento,
                    Dni,
                    Mail,
                    Contraseña,
                    UbicacionCodigoPostal,
                    UbicacionNumero,
                    UbicacionLocalidad,
                    UbicacionCalle,
                    IdProvincia)
                VALUES (
                    @NombreUsuario,
                    @Nombre,
                    @Apellido,
                    @FechaNacimiento,
                    @Dni,
                    @Mail,
                    @Contraseña,
                    @UbicacionCodigoPostal,
                    @UbicacionNumero,
                    @UbicacionLocalidad,
                    @UbicacionCalle,
                    @IdProvincia)",
                new
                {
                    NombreUsuario = email.Split('@')[0],
                    Nombre = nombre,
                    Apellido = apellido,
                    FechaNacimiento = DateTime.Today.AddYears(-18),
                    Dni = 0,
                    Mail = email,
                    Contraseña = password,
                    UbicacionCodigoPostal = 0,
                    UbicacionNumero = 0,
                    UbicacionLocalidad = "Sin especificar",
                    UbicacionCalle = "Sin especificar",
                    IdProvincia = 1
                });
        }

        public static void RegistrarOrganizacion(string nombre, string email, string password, string telefono, string descripcion)
        {
            using var connection = new SqlConnection(connectionString);

            connection.Execute(@"
                INSERT INTO Organizaciones (
                    Nombre,
                    Valida,
                    Telefono,
                    RedSocial,
                    Mail,
                    Contraseña,
                    UbicacionCodigoPostal,
                    UbicacionNumero,
                    UbicacionLocalidad,
                    UbicacionCalle,
                    IdProvincia,
                    EsAprobado)
                VALUES (
                    @Nombre,
                    @Valida,
                    @Telefono,
                    @RedSocial,
                    @Mail,
                    @Contraseña,
                    @UbicacionCodigoPostal,
                    @UbicacionNumero,
                    @UbicacionLocalidad,
                    @UbicacionCalle,
                    @IdProvincia,
                    @EsAprobado)",
                new
                {
                    Nombre = nombre,
                    Valida = true,
                    Telefono = int.TryParse(telefono, out var tel) ? tel : 0,
                    RedSocial = descripcion,
                    Mail = email,
                    Contraseña = password,
                    UbicacionCodigoPostal = 0,
                    UbicacionNumero = 0,
                    UbicacionLocalidad = "Sin especificar",
                    UbicacionCalle = "Sin especificar",
                    IdProvincia = 1,
                    EsAprobado = false
                });
        }

        public static int RegistrarPublicacion(int idOrganizacion, string titulo, string tipoPropuesta, string descripcion, string ubicacion, string necesidades)
        {
            using var connection = new SqlConnection(connectionString);

            return connection.ExecuteScalar<int>(@"
                INSERT INTO Publicaciones (
                    IdOrganizacion,
                    Titulo,
                    TipoPropuesta,
                    Descripcion,
                    Ubicacion,
                    Necesidades,
                    FechaAlta)
                VALUES (
                    @IdOrganizacion,
                    @Titulo,
                    @TipoPropuesta,
                    @Descripcion,
                    @Ubicacion,
                    @Necesidades,
                    @FechaAlta);
                SELECT CAST(SCOPE_IDENTITY() AS int);",
                new
                {
                    IdOrganizacion = idOrganizacion,
                    Titulo = titulo,
                    TipoPropuesta = tipoPropuesta,
                    Descripcion = descripcion,
                    Ubicacion = ubicacion,
                    Necesidades = necesidades,
                    FechaAlta = DateTime.Now
                });
        }

        public static List<Publicacion> ObtenerPublicacionesPorOrganizacion(int idOrganizacion)
        {
            using var connection = new SqlConnection(connectionString);

            return connection.Query<Publicacion>(@"
                SELECT Id, IdOrganizacion, Titulo, TipoPropuesta, Descripcion, Ubicacion, Necesidades, FechaAlta
                FROM Publicaciones
                WHERE IdOrganizacion = @IdOrganizacion
                ORDER BY FechaAlta DESC",
                new { IdOrganizacion = idOrganizacion }).ToList();
        }

        public static Organizacion? ObtenerOrganizacionPorEmail(string email)
        {
            using var connection = new SqlConnection(connectionString);

            var row = connection.QueryFirstOrDefault<dynamic>(
                "SELECT TOP 1 Id, Nombre, Mail, Contraseña, Telefono, UbicacionCalle, UbicacionNumero, UbicacionLocalidad FROM Organizaciones WHERE Mail = @Email",
                new { Email = email });

            if (row == null)
            {
                return null;
            }

            return new Organizacion
            {
                Id = (int)row.Id,
                Nombre = row.Nombre ?? string.Empty,
                Direccion = $"{row.UbicacionCalle ?? string.Empty} {row.UbicacionNumero ?? string.Empty}".Trim(),
                Barrio = row.UbicacionLocalidad ?? string.Empty,
                Telefono = row.Telefono?.ToString() ?? string.Empty
            };
        }

        public static List<Organizacion> ObtenerOngsPendientes()
        {
            using var connection = new SqlConnection(connectionString);

            return connection.Query<Organizacion>(@"
                SELECT Id, Nombre, Mail AS Email, Contraseña AS Clave, Telefono,
                       UbicacionCalle AS Direccion, UbicacionLocalidad AS Barrio, EsAprobado
                FROM Organizaciones
                WHERE EsAprobado = 0
                ORDER BY Nombre").ToList();
        }

        public static void AprobarOrganizacion(int id)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Execute(
                "UPDATE Organizaciones SET EsAprobado = 1 WHERE Id = @Id",
                new { Id = id });
        }

        public static void RegistrarAdmin(string nombre, string apellido, string email, string password)
        {
            using var connection = new SqlConnection(connectionString);

            connection.Execute(@"
                INSERT INTO Usuarios (
                    NombreUsuario,
                    Nombre,
                    Apellido,
                    FechaNacimiento,
                    Dni,
                    Mail,
                    Contraseña,
                    UbicacionCodigoPostal,
                    UbicacionNumero,
                    UbicacionLocalidad,
                    UbicacionCalle,
                    IdProvincia,
                    Rol,
                    EsAprobado)
                VALUES (
                    @NombreUsuario,
                    @Nombre,
                    @Apellido,
                    @FechaNacimiento,
                    @Dni,
                    @Mail,
                    @Contraseña,
                    @UbicacionCodigoPostal,
                    @UbicacionNumero,
                    @UbicacionLocalidad,
                    @UbicacionCalle,
                    @IdProvincia,
                    @Rol,
                    @EsAprobado)",
                new
                {
                    NombreUsuario = email.Split('@')[0],
                    Nombre = nombre,
                    Apellido = apellido,
                    FechaNacimiento = DateTime.Today.AddYears(-25),
                    Dni = 0,
                    Mail = email,
                    Contraseña = password,
                    UbicacionCodigoPostal = 0,
                    UbicacionNumero = 0,
                    UbicacionLocalidad = "Sistema",
                    UbicacionCalle = "Interna",
                    IdProvincia = 1,
                    Rol = "Admin",
                    EsAprobado = true
                });
        }
    }
}
