using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
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
                WHERE o.Valida = 1
                ORDER BY o.Nombre").ToList();
        }

        public static Usuario? ObtenerUsuarioPorId(int id)
        {
            using var connection = new SqlConnection(connectionString);

            var row = connection.QueryFirstOrDefault<dynamic>(
                "SELECT TOP 1 Id, NombreUsuario, Nombre, Apellido, Mail AS Email, Contraseña AS Clave, DonacionRealizada, PersonasBeneficiadas, Rol FROM Usuarios WHERE Id = @Id",
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
                NombreUsuario = row.NombreUsuario ?? string.Empty,
                Email = row.Email ?? string.Empty,
                Clave = row.Clave ?? string.Empty,
                DonacionRealizada = row.DonacionRealizada is int donacion ? donacion : 0,
                PersonasBeneficiadas = row.PersonasBeneficiadas?.ToString() ?? "0",
                Rol = row.Rol ?? "Usuario"
            };
        }

        public static Organizacion? ObtenerOrganizacionPorId(int id)
        {
            using var connection = new SqlConnection(connectionString);

            var row = connection.QueryFirstOrDefault<dynamic>(
                "SELECT TOP 1 Id, Nombre, Mail, Contraseña, Telefono, UbicacionCalle, UbicacionNumero, UbicacionLocalidad, Valida FROM Organizaciones WHERE Id = @Id",
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
                Telefono = row.Telefono?.ToString() ?? string.Empty,
                EsAprobado = row.Valida == true || row.Valida == 1
            };
        }

        public static Usuario? ValidarLoginUsuario(string email, string clave)
        {
            using var connection = new SqlConnection(connectionString);

            var row = connection.QueryFirstOrDefault<dynamic>(
                "SELECT TOP 1 Id, NombreUsuario, Nombre, Apellido, Mail AS Email, Contraseña AS Clave, DonacionRealizada, PersonasBeneficiadas, Rol FROM Usuarios WHERE Mail = @email AND Contraseña = @clave",
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
                NombreUsuario = row.NombreUsuario ?? string.Empty,
                Email = row.Email ?? string.Empty,
                Clave = row.Clave ?? string.Empty,
                DonacionRealizada = row.DonacionRealizada is int donacion ? donacion : 0,
                PersonasBeneficiadas = row.PersonasBeneficiadas?.ToString() ?? "0",
                Rol = row.Rol ?? "Usuario"
            };
        }

        public static Organizacion? ValidarLoginOrganizacion(string email, string clave)
        {
            using var connection = new SqlConnection(connectionString);

            var row = connection.QueryFirstOrDefault<dynamic>(
                "SELECT TOP 1 Id, Nombre, Mail, Contraseña, Telefono, UbicacionCalle, UbicacionNumero, UbicacionLocalidad, Valida FROM Organizaciones WHERE Mail = @email AND Contraseña = @clave",
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
                Telefono = row.Telefono?.ToString() ?? string.Empty,
                EsAprobado = row.Valida == true || row.Valida == 1
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
                    IdProvincia,
                    Rol,
                    DonacionRealizada,
                    PersonasBeneficiadas)
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
                    @DonacionRealizada,
                    @PersonasBeneficiadas)",
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
                    IdProvincia = 1,
                    Rol = "Usuario",
                    DonacionRealizada = 0,
                    PersonasBeneficiadas = "0"
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
                    IdProvincia)
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
                    @IdProvincia)",
                new
                {
                    Nombre = nombre,
                    Valida = false,
                    Telefono = int.TryParse(telefono, out var tel) ? tel : 0,
                    RedSocial = descripcion,
                    Mail = email,
                    Contraseña = password,
                    UbicacionCodigoPostal = 0,
                    UbicacionNumero = 0,
                    UbicacionLocalidad = "Sin especificar",
                    UbicacionCalle = "Sin especificar",
                    IdProvincia = 1
                });
        }

        public static int RegistrarDonacion(int usuarioId, int idOrganizacion, string tipoAporte, string cantidad, string mensaje)
        {
            var tipo = string.IsNullOrWhiteSpace(tipoAporte) ? "Donación" : tipoAporte.Trim();
            var cantidadTexto = string.IsNullOrWhiteSpace(cantidad) ? "1" : cantidad.Trim();
            var mensajeTexto = mensaje ?? string.Empty;

            using var connection = new SqlConnection(connectionString);

            var necesidadId = connection.QuerySingleOrDefault<int?>(
                "SELECT TOP 1 Id FROM Necesidades WHERE Categoria = @Categoria",
                new { Categoria = tipo });

            if (!necesidadId.HasValue)
            {
                necesidadId = connection.ExecuteScalar<int>(@"
                    INSERT INTO Necesidades (Categoria)
                    VALUES (@Categoria);
                    SELECT CAST(SCOPE_IDENTITY() AS int);",
                    new { Categoria = tipo });
            }

            var fecha = DateTime.Now;
            var idDonacion = connection.ExecuteScalar<int>(@"
                INSERT INTO DonacionesNecesidades (
                    Categoria,
                    IdOrganizacion,
                    IdNecesidad)
                VALUES (
                    @Categoria,
                    @IdOrganizacion,
                    @IdNecesidad);
                SELECT CAST(SCOPE_IDENTITY() AS int);",
                new
                {
                    Categoria = tipo,
                    IdOrganizacion = idOrganizacion,
                    IdNecesidad = necesidadId.Value,
                });

            connection.Execute(@"
                UPDATE Usuarios
                SET DonacionRealizada = ISNULL(DonacionRealizada, 0) + 1
                WHERE Id = @IdUsuario",
                new { IdUsuario = usuarioId });

            connection.Execute(@"
                INSERT INTO UsuariosDonacionesNecesidades (
                    Fecha,
                    IdUsuario,
                    IdDonacionesNecesidades)
                VALUES (
                    @Fecha,
                    @IdUsuario,
                    @IdDonacionesNecesidades)",
                new
                {
                    Fecha = fecha,
                    IdUsuario = usuarioId,
                    IdDonacionesNecesidades = idDonacion
                });

            return idDonacion;
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
                       UbicacionCalle AS Direccion, UbicacionLocalidad AS Barrio, Valida AS EsAprobado
                FROM Organizaciones
                WHERE Valida = 0
                ORDER BY Nombre").ToList();
        }

        public static void AprobarOrganizacion(int id)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Execute(
                "UPDATE Organizaciones SET Valida = 1 WHERE Id = @Id",
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
                    DonacionRealizada,
                    PersonasBeneficiadas)
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
                    @DonacionRealizada,
                    @PersonasBeneficiadas)",
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
                    DonacionRealizada = 0,
                    PersonasBeneficiadas = "0"
                });
        }

        public static List<string> ObtenerProvincias()
        {
            using var connection = new SqlConnection(connectionString);

            return connection.Query<string>(@"
                SELECT DISTINCT p.Nombre
                FROM Provincias p
                WHERE p.Nombre IS NOT NULL AND LTRIM(RTRIM(p.Nombre)) <> ''
                ORDER BY p.Nombre").ToList();
        }

        public static List<Organizacion> ObtenerOrganizacionesFiltradas(string? categoria, string? provincia)
        {
            var categoriaFiltro = string.IsNullOrWhiteSpace(categoria) ? null : categoria.Trim();
            var provinciaFiltro = string.IsNullOrWhiteSpace(provincia) ? null : provincia.Trim();

            using var connection = new SqlConnection(connectionString);

            return connection.Query<Organizacion>(@"
                SELECT DISTINCT
                    o.Id,
                    o.Nombre,
                    CONCAT(o.UbicacionCalle, ' ', o.UbicacionNumero) AS Direccion,
                    o.UbicacionLocalidad AS Barrio,
                    CAST(o.Telefono AS varchar(50)) AS Telefono
                FROM Organizaciones o
                INNER JOIN Provincias p ON p.Id = o.IdProvincia
                LEFT JOIN DonacionesNecesidades dn ON dn.IdOrganizacion = o.Id
                LEFT JOIN Necesidades n ON n.Id = dn.IdNecesidad
                WHERE o.Valida = 1
                  AND (@Categoria IS NULL OR @Categoria = '' OR n.Categoria = @Categoria)
                  AND (@Provincia IS NULL OR @Provincia = '' OR p.Nombre = @Provincia)
                ORDER BY o.Nombre",
                new
                {
                    Categoria = categoriaFiltro,
                    Provincia = provinciaFiltro
                }).ToList();
        }

        public static List<string> ObtenerCategoriasNecesidad()
        {
            using var connection = new SqlConnection(connectionString);

            return connection.Query<string>(@"
                SELECT DISTINCT n.Categoria
                FROM Necesidades n
                WHERE n.Categoria IS NOT NULL AND LTRIM(RTRIM(n.Categoria)) <> ''
                ORDER BY n.Categoria").ToList();
        }
    }
}
