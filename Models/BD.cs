using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using Dapper;

namespace ProgramacionXEfsi_Puente.Models
{
    public static class BD
    {
        private static readonly string connectionString = @"Server=(local);Database=Puente Solidario;Trusted_Connection=True;TrustServerCertificate=True;";

        private static SqlConnection CrearConexion()
        {
            return new SqlConnection(connectionString);
        }

        private static Organizacion MapearOrganizacion(dynamic row)
        {
            return new Organizacion
            {
                Id = row.Id,
                Nombre = row.Nombre ?? string.Empty,
                Direccion = $"{row.UbicacionCalle ?? string.Empty} {row.UbicacionNumero ?? string.Empty}".Trim(),
                Barrio = row.UbicacionLocalidad ?? string.Empty,
                Telefono = row.Telefono?.ToString() ?? string.Empty,
                Descripcion = row.RedSocial ?? "La organización aún no agregó una descripción.",
                Necesidades = row.Necesidades ?? "No especificado",
                EsAprobado = row.Valida == true || row.Valida == 1,
                Email = row.Mail ?? string.Empty,
                Clave = row.Contraseña ?? string.Empty
            };
        }

        private static Usuario MapearUsuario(dynamic row)
        {
            var datos = row as IDictionary<string, object>;

            object? Obtener(string nombre, object? valorPredeterminado = null)
            {
                if (datos != null && datos.TryGetValue(nombre, out var valor))
                {
                    return valor;
                }

                try
                {
                    return ((IDictionary<string, object>)row)[nombre];
                }
                catch
                {
                    return valorPredeterminado;
                }
            }

            return new Usuario
            {
                Id = Convert.ToInt32(Obtener("Id", 0)),
                Nombre = Obtener("Nombre", string.Empty)?.ToString() ?? string.Empty,
                Apellido = Obtener("Apellido", string.Empty)?.ToString() ?? string.Empty,
                NombreUsuario = Obtener("NombreUsuario", string.Empty)?.ToString() ?? string.Empty,
                Email = Obtener("Mail", Obtener("Email", string.Empty))?.ToString() ?? string.Empty,
                Telefono = Obtener("Telefono", string.Empty)?.ToString() ?? string.Empty,
                RedSocial = Obtener("RedSocial", string.Empty)?.ToString() ?? string.Empty,
                FotoPerfil = Obtener("FotoPerfil", string.Empty)?.ToString() ?? string.Empty,
                Clave = Obtener("Contraseña", Obtener("Clave", string.Empty))?.ToString() ?? string.Empty,
                DonacionRealizada = Obtener("DonacionRealizada", 0) is int donacion
                    ? donacion
                    : Convert.ToInt32(Obtener("DonacionRealizada", 0)),
                PersonasBeneficiadas = Obtener("PersonasBeneficiadas", "0")?.ToString() ?? "0",
                Rol = Obtener("Rol", "Usuario")?.ToString() ?? "Usuario"
            };
        }

        public static List<Organizacion> ObtenerOrganizaciones()
        {
            using var connection = CrearConexion();

            var rows = connection.Query<dynamic>(
                "ListarOrganizaciones",
                commandType: CommandType.StoredProcedure).ToList();

            return rows.Select(MapearOrganizacion).ToList();
        }

        public static Usuario? ObtenerUsuarioPorId(int id)
        {
            using var connection = CrearConexion();

            try
            {
                var row = connection.QueryFirstOrDefault<dynamic>(
                    "ObtenerUsuario",
                    new { Id = id },
                    commandType: CommandType.StoredProcedure);

                return row == null ? null : MapearUsuario(row);
            }
            catch (SqlException ex) when (ex.Number == 2812 || ex.Number == 208)
            {
                return new Usuario
                {
                    Id = id,
                    Nombre = "Usuario",
                    Apellido = string.Empty,
                    NombreUsuario = "Usuario",
                    Email = string.Empty,
                    Rol = "Usuario"
                };
            }
        }

        public static Organizacion? ObtenerOrganizacionPorId(int id)
        {
            using var connection = CrearConexion();

            var row = connection.QueryFirstOrDefault<dynamic>(
                "ObtenerOrganizacion",
                new { Id = id },
                commandType: CommandType.StoredProcedure);

            if (row == null)
            {
                return null;
            }

            var necesidades = connection.QueryFirstOrDefault<string>(
                "ObtenerNecesidadesPorOrganizacion",
                new { IdOrganizacion = id },
                commandType: CommandType.StoredProcedure);

            var organizacion = MapearOrganizacion(row);
            organizacion.Necesidades = string.IsNullOrWhiteSpace(necesidades) ? "No especificado" : necesidades;
            return organizacion;
        }

        public static Usuario? ValidarLoginUsuario(string email, string clave)
        {
            using var connection = CrearConexion();

            var row = connection.QueryFirstOrDefault<dynamic>(
                "IniciarSesion",
                new { Mail = email, Contraseña = clave },
                commandType: CommandType.StoredProcedure);

            return row == null ? null : MapearUsuario(row);
        }

        public static Organizacion? ValidarLoginOrganizacion(string email, string clave)
        {
            using var connection = CrearConexion();

            var row = connection.QueryFirstOrDefault<dynamic>(
                "ObtenerOrganizacionPorEmail",
                new { Email = email },
                commandType: CommandType.StoredProcedure);

            if (row == null)
            {
                return null;
            }

            if (!string.Equals((row.Contraseña ?? string.Empty), clave, StringComparison.Ordinal))
            {
                return null;
            }

            var necesidades = connection.QueryFirstOrDefault<string>(
                "ObtenerNecesidadesPorOrganizacion",
                new { IdOrganizacion = (int)row.Id },
                commandType: CommandType.StoredProcedure);

            var organizacion = MapearOrganizacion(row);
            organizacion.Necesidades = string.IsNullOrWhiteSpace(necesidades) ? "No especificado" : necesidades;
            return organizacion;
        }

        public static bool ExisteUsuarioPorEmail(string email)
        {
            using var connection = CrearConexion();

            var count = connection.ExecuteScalar<int>(
                "ExisteUsuarioPorEmail",
                new { Email = email },
                commandType: CommandType.StoredProcedure);

            return count > 0;
        }

        public static bool ExisteOrganizacionPorEmail(string email)
        {
            using var connection = CrearConexion();

            var count = connection.ExecuteScalar<int>(
                "ExisteOrganizacionPorEmail",
                new { Email = email },
                commandType: CommandType.StoredProcedure);

            return count > 0;
        }

        public static void RegistrarUsuario(string nombre, string apellido, string email, string password, string telefono, string redSocial)
        {
            using var connection = CrearConexion();

            connection.Execute(
                "RegistrarUsuario",
                new
                {
                    NombreUsuario = email.Split('@')[0],
                    Nombre = nombre,
                    Apellido = apellido,
                    FechaNacimiento = DateTime.Today.AddYears(-18),
                    Dni = 0,
                    Mail = email,
                    Contraseña = password,
                    Telefono = telefono ?? string.Empty,
                    RedSocial = redSocial ?? string.Empty,
                    UbicacionCodigoPostal = 0,
                    UbicacionNumero = 0,
                    UbicacionLocalidad = "Sin especificar",
                    UbicacionCalle = "Sin especificar",
                    IdProvincia = 1
                },
                commandType: CommandType.StoredProcedure);
        }

        public static int RegistrarOrganizacion(string nombre, string email, string password, string telefono, string descripcion, string ubicacionCalle, string ubicacionNumero, string ubicacionLocalidad)
        {
            using var connection = CrearConexion();

            var calle = string.IsNullOrWhiteSpace(ubicacionCalle) ? "Sin especificar" : ubicacionCalle.Trim();
            var numero = string.IsNullOrWhiteSpace(ubicacionNumero) ? "0" : ubicacionNumero.Trim();
            var localidad = string.IsNullOrWhiteSpace(ubicacionLocalidad) ? "Sin especificar" : ubicacionLocalidad.Trim();
            var telefonoNumerico = int.TryParse(telefono, out var tel) ? tel : 0;

            var parametros = new DynamicParameters();
            parametros.Add("@Nombre", nombre);
            parametros.Add("@Telefono", telefonoNumerico);
            parametros.Add("@RedSocial", descripcion);
            parametros.Add("@Mail", email);
            parametros.Add("@Contraseña", password);
            parametros.Add("@UbicacionCodigoPostal", 0);
            parametros.Add("@UbicacionNumero", int.TryParse(numero, out var num) ? num : 0);
            parametros.Add("@UbicacionLocalidad", localidad);
            parametros.Add("@UbicacionCalle", calle);
            parametros.Add("@IdProvincia", 1);
            parametros.Add("@IdOrganizacion", dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute(
                "RegistrarOrganizacion",
                parametros,
                commandType: CommandType.StoredProcedure);

            return parametros.Get<int>("@IdOrganizacion");
        }

        public static void RegistrarNecesidadesOrganizacion(int idOrganizacion, IEnumerable<string>? necesidades)
        {
            if (idOrganizacion <= 0)
            {
                return;
            }

            var categorias = (necesidades ?? Enumerable.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var categoria in categorias)
            {
                using var connection = CrearConexion();

                var necesidadId = connection.QuerySingleOrDefault<int?>(
                    "ObtenerNecesidadPorCategoria",
                    new { Categoria = categoria },
                    commandType: CommandType.StoredProcedure);

                if (!necesidadId.HasValue)
                {
                    necesidadId = connection.QuerySingleOrDefault<int?>(
                        "RegistrarNecesidad",
                        new { Categoria = categoria },
                        commandType: CommandType.StoredProcedure);
                }

                if (!necesidadId.HasValue)
                {
                    continue;
                }

                var existe = connection.ExecuteScalar<int>(
                    "ExisteRelacionNecesidadOrganizacion",
                    new { IdOrganizacion = idOrganizacion, IdNecesidad = necesidadId.Value },
                    commandType: CommandType.StoredProcedure);

                if (existe == 0)
                {
                    connection.Execute(
                        "RegistrarNecesidadOrganizacion",
                        new
                        {
                            Categoria = categoria,
                            IdOrganizacion = idOrganizacion,
                            IdNecesidad = necesidadId.Value
                        },
                        commandType: CommandType.StoredProcedure);
                }
            }
        }

        public static int RegistrarDonacion(int usuarioId, int idOrganizacion, string tipoAporte, string cantidad, string mensaje)
        {
            var tipo = string.IsNullOrWhiteSpace(tipoAporte) ? "Donación" : tipoAporte.Trim();
            var cantidadTexto = string.IsNullOrWhiteSpace(cantidad) ? "1" : cantidad.Trim();
            var mensajeTexto = mensaje ?? string.Empty;

            using var connection = CrearConexion();

            var necesidadId = connection.QuerySingleOrDefault<int?>(
                "ObtenerNecesidadPorCategoria",
                new { Categoria = tipo },
                commandType: CommandType.StoredProcedure);

            if (!necesidadId.HasValue)
            {
                necesidadId = connection.QuerySingleOrDefault<int?>(
                    "RegistrarNecesidad",
                    new { Categoria = tipo },
                    commandType: CommandType.StoredProcedure);
            }

            if (!necesidadId.HasValue)
            {
                throw new InvalidOperationException("No se pudo registrar la necesidad asociada a la donación.");
            }

            var idDonacion = connection.ExecuteScalar<int>(
                "RegistrarDonacionNecesidad",
                new
                {
                    Categoria = tipo,
                    IdOrganizacion = idOrganizacion,
                    IdNecesidad = necesidadId.Value
                },
                commandType: CommandType.StoredProcedure);

            connection.Execute(
                "ActualizarDonacionUsuario",
                new { IdUsuario = usuarioId },
                commandType: CommandType.StoredProcedure);

            connection.Execute(
                "RegistrarUsuarioDonacion",
                new
                {
                    Fecha = DateTime.Now,
                    IdUsuario = usuarioId,
                    IdDonacionesNecesidades = idDonacion
                },
                commandType: CommandType.StoredProcedure);

            return idDonacion;
        }

        private static bool TieneColumnaFechaLimitePublicaciones()
        {
            return true;
        }

        private static bool TieneColumnaImagenUrlPublicaciones()
        {
            return true;
        }

        public static int RegistrarPublicacion(int idOrganizacion, string titulo, string tipoPropuesta, string descripcion, string necesidades, string cosasQueNecesitan, DateTime? fechaLimite, string? imagenUrl)
        {
            var organizacion = ObtenerOrganizacionPorId(idOrganizacion);
            if (organizacion == null)
            {
                return 0;
            }

            var ubicacion = string.Join(", ", new[] { organizacion.Direccion, organizacion.Barrio }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase));

            try
            {
                using var connection = CrearConexion();
                return connection.ExecuteScalar<int>(
                    "RegistrarPublicacionConImagen",
                    new
                    {
                        IdOrganizacion = idOrganizacion,
                        Titulo = titulo,
                        TipoPropuesta = tipoPropuesta,
                        Descripcion = descripcion,
                        Ubicacion = ubicacion,
                        Necesidades = necesidades,
                        CosasQueNecesitan = cosasQueNecesitan,
                        FechaAlta = DateTime.Now,
                        FechaLimite = fechaLimite,
                        ImagenUrl = imagenUrl ?? string.Empty
                    },
                    commandType: CommandType.StoredProcedure);
            }
            catch
            {
                try
                {
                    using var connection = CrearConexion();
                    return connection.ExecuteScalar<int>(
                        "RegistrarPublicacion",
                        new
                        {
                            IdOrganizacion = idOrganizacion,
                            Titulo = titulo,
                            TipoPropuesta = tipoPropuesta,
                            Descripcion = descripcion,
                            Ubicacion = ubicacion,
                            Necesidades = necesidades,
                            CosasQueNecesitan = cosasQueNecesitan,
                            FechaAlta = DateTime.Now,
                            FechaLimite = fechaLimite
                        },
                        commandType: CommandType.StoredProcedure);
                }
                catch
                {
                    using var connection = CrearConexion();
                    return connection.ExecuteScalar<int>(
                        "RegistrarPublicacionLegacy",
                        new
                        {
                            IdOrganizacion = idOrganizacion,
                            Titulo = titulo,
                            TipoPropuesta = tipoPropuesta,
                            Descripcion = descripcion,
                            Ubicacion = ubicacion,
                            Necesidades = necesidades,
                            CosasQueNecesitan = cosasQueNecesitan,
                            FechaAlta = DateTime.Now
                        },
                        commandType: CommandType.StoredProcedure);
                }
            }
        }

        public static List<Publicacion> ObtenerPublicacionesPorOrganizacion(int idOrganizacion)
        {
            try
            {
                using var connection = CrearConexion();
                var rows = connection.Query<dynamic>(
                    "ObtenerPublicacionesPorOrganizacion",
                    new { IdOrganizacion = idOrganizacion },
                    commandType: CommandType.StoredProcedure).ToList();

                return rows.Select(row =>
                {
                    var dict = (IDictionary<string, object>)row;
                    var fechaLimiteValue = dict.TryGetValue("FechaLimite", out var fechaLimiteObj) && fechaLimiteObj is DateTime fechaLimite ? fechaLimite : (DateTime?)null;
                    var imagenUrl = dict.TryGetValue("ImagenUrl", out var imagenObj) && imagenObj != null ? imagenObj.ToString() : null;
                    var cosasQueNecesitan = dict.TryGetValue("CosasQueNecesitan", out var cosasObj) && cosasObj != null ? cosasObj.ToString() : string.Empty;

                    return CompletarProgresoPublicacion(new Publicacion
                    {
                        Id = row.Id,
                        IdOrganizacion = row.IdOrganizacion,
                        Titulo = row.Titulo ?? string.Empty,
                        TipoPropuesta = row.TipoPropuesta ?? string.Empty,
                        Descripcion = row.Descripcion ?? string.Empty,
                        Ubicacion = row.Ubicacion ?? string.Empty,
                        Necesidades = row.Necesidades ?? string.Empty,
                        NombreOrganizacion = row.NombreOrganizacion ?? string.Empty,
                        CosasQueNecesitan = cosasQueNecesitan ?? string.Empty,
                        ImagenUrl = imagenUrl,
                        FechaAlta = row.FechaAlta is DateTime fecha ? fecha : DateTime.Now,
                        FechaLimite = fechaLimiteValue
                    });
                }).Select(CompletarProgresoPublicacion)
                  .Where(publicacion => !PublicacionEstaSaldada(publicacion))
                  .ToList();
            }
            catch
            {
                using var connection = CrearConexion();
                var rows = connection.Query<dynamic>(
                    "ObtenerPublicacionesPorOrganizacionLegacy",
                    new { IdOrganizacion = idOrganizacion },
                    commandType: CommandType.StoredProcedure).ToList();

                return rows.Select(row => CompletarProgresoPublicacion(new Publicacion
                {
                    Id = row.Id,
                    IdOrganizacion = row.IdOrganizacion,
                    Titulo = row.Titulo ?? string.Empty,
                    TipoPropuesta = row.TipoPropuesta ?? string.Empty,
                    Descripcion = row.Descripcion ?? string.Empty,
                    Ubicacion = row.Ubicacion ?? string.Empty,
                    Necesidades = row.Necesidades ?? string.Empty,
                    NombreOrganizacion = row.NombreOrganizacion ?? string.Empty,
                    FechaAlta = row.FechaAlta is DateTime fecha ? fecha : DateTime.Now,
                    FechaLimite = null
                })).Where(publicacion => !PublicacionEstaSaldada(publicacion)).ToList();
            }
        }

        public static List<Publicacion> ObtenerPublicacionesFiltradas(string? categoria, string? provincia, string? busqueda)
        {
            var categoriaFiltro = string.IsNullOrWhiteSpace(categoria) ? null : categoria.Trim();
            var provinciaFiltro = string.IsNullOrWhiteSpace(provincia) ? null : provincia.Trim();
            var busquedaFiltro = string.IsNullOrWhiteSpace(busqueda) ? null : busqueda.Trim();

            try
            {
                using var connection = CrearConexion();
                var rows = connection.Query<dynamic>(
                    "ObtenerPublicacionesFiltradas",
                    new
                    {
                        Categoria = categoriaFiltro,
                        Provincia = provinciaFiltro,
                        Busqueda = busquedaFiltro
                    },
                    commandType: CommandType.StoredProcedure).ToList();

                return rows.Select(row =>
                {
                    var dict = (IDictionary<string, object>)row;
                    var fechaLimiteValue = dict.TryGetValue("FechaLimite", out var fechaLimiteObj) && fechaLimiteObj is DateTime fechaLimite ? fechaLimite : (DateTime?)null;
                    var imagenUrl = dict.TryGetValue("ImagenUrl", out var imagenObj) && imagenObj != null ? imagenObj.ToString() : null;
                    var cosasQueNecesitan = dict.TryGetValue("CosasQueNecesitan", out var cosasObj) && cosasObj != null ? cosasObj.ToString() : string.Empty;

                    return new Publicacion
                    {
                        Id = row.Id,
                        IdOrganizacion = row.IdOrganizacion,
                        Titulo = row.Titulo ?? string.Empty,
                        TipoPropuesta = row.TipoPropuesta ?? string.Empty,
                        Descripcion = row.Descripcion ?? string.Empty,
                        Ubicacion = row.Ubicacion ?? string.Empty,
                        Necesidades = row.Necesidades ?? string.Empty,
                        NombreOrganizacion = row.NombreOrganizacion ?? string.Empty,
                        CosasQueNecesitan = cosasQueNecesitan ?? string.Empty,
                        ImagenUrl = imagenUrl,
                        FechaAlta = row.FechaAlta is DateTime fecha ? fecha : DateTime.Now,
                        FechaLimite = fechaLimiteValue
                    };
                }).ToList();
            }
            catch
            {
                using var connection = CrearConexion();
                var rows = connection.Query<dynamic>(
                    "ObtenerPublicacionesFiltradasLegacy",
                    new
                    {
                        Categoria = categoriaFiltro,
                        Provincia = provinciaFiltro,
                        Busqueda = busquedaFiltro
                    },
                    commandType: CommandType.StoredProcedure).ToList();

                return rows.Select(row => new Publicacion
                {
                    Id = row.Id,
                    IdOrganizacion = row.IdOrganizacion,
                    Titulo = row.Titulo ?? string.Empty,
                    TipoPropuesta = row.TipoPropuesta ?? string.Empty,
                    Descripcion = row.Descripcion ?? string.Empty,
                    Ubicacion = row.Ubicacion ?? string.Empty,
                    Necesidades = row.Necesidades ?? string.Empty,
                    NombreOrganizacion = row.NombreOrganizacion ?? string.Empty,
                    FechaAlta = row.FechaAlta is DateTime fecha ? fecha : DateTime.Now,
                    FechaLimite = null
                }).ToList();
            }
        }

        public static Organizacion? ObtenerOrganizacionPorEmail(string email)
        {
            using var connection = CrearConexion();

            var row = connection.QueryFirstOrDefault<dynamic>(
                "ObtenerOrganizacionPorEmail",
                new { Email = email },
                commandType: CommandType.StoredProcedure);

            return row == null ? null : MapearOrganizacion(row);
        }

        public static List<Organizacion> ObtenerOngsPendientes()
        {
            using var connection = CrearConexion();

            var rows = connection.Query<dynamic>(
                "ObtenerOngsPendientes",
                commandType: CommandType.StoredProcedure).ToList();

            return rows.Select(row =>
            {
                var valida = row.Valida;
                var esAprobado = valida is bool valorBool
                    ? valorBool
                    : valida != null && Convert.ToInt32(valida) == 1;

                return new Organizacion
                {
                    Id = row.Id,
                    Nombre = row.Nombre ?? string.Empty,
                    Email = row.Mail ?? string.Empty,
                    Clave = row.Contraseña ?? string.Empty,
                    Direccion = $"{row.UbicacionCalle ?? string.Empty} {row.UbicacionNumero ?? string.Empty}".Trim(),
                    Barrio = row.UbicacionLocalidad ?? string.Empty,
                    Telefono = row.Telefono?.ToString() ?? string.Empty,
                    EsAprobado = esAprobado
                };
            }).ToList();
        }

        public static void AprobarOrganizacion(int id)
        {
            using var connection = CrearConexion();
            connection.Execute(
                "AprobarOrganizacion",
                new { Id = id },
                commandType: CommandType.StoredProcedure);
        }

        public static void RegistrarAdmin(string nombre, string apellido, string email, string password)
        {
            using var connection = CrearConexion();

            connection.Execute(
                "RegistrarUsuario",
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
                    Rol = "Admin"
                },
                commandType: CommandType.StoredProcedure);
        }

        public static List<string> ObtenerProvincias()
        {
            using var connection = CrearConexion();
            return connection.Query<string>(
                "ObtenerProvincias",
                commandType: CommandType.StoredProcedure).ToList();
        }

        public static List<Organizacion> ObtenerOrganizacionesFiltradas(string? categoria, string? provincia, string? busqueda)
        {
            var categoriaFiltro = string.IsNullOrWhiteSpace(categoria) ? null : categoria.Trim();
            var provinciaFiltro = string.IsNullOrWhiteSpace(provincia) ? null : provincia.Trim();
            var busquedaFiltro = string.IsNullOrWhiteSpace(busqueda) ? null : busqueda.Trim();

            using var connection = CrearConexion();

            var rows = connection.Query<dynamic>(
                "ObtenerOrganizacionesFiltradas",
                new
                {
                    Categoria = categoriaFiltro,
                    Provincia = provinciaFiltro,
                    Busqueda = busquedaFiltro
                },
                commandType: CommandType.StoredProcedure).ToList();

            return rows.Select(MapearOrganizacion).ToList();
        }

        public static List<string> ObtenerCategoriasNecesidad()
        {
            using var connection = CrearConexion();
            return connection.Query<string>(
                "ObtenerCategoriasNecesidad",
                commandType: CommandType.StoredProcedure).ToList();
        }

        public static Publicacion? ObtenerPublicacionPorId(int id)
        {
            using var connection = CrearConexion();

            var row = connection.QueryFirstOrDefault<dynamic>(
                "ObtenerPublicacionPorId",
                new { Id = id },
                commandType: CommandType.StoredProcedure);

            if (row == null)
            {
                return null;
            }

            var dict = (IDictionary<string, object>)row;
            var fechaLimiteValue = dict.TryGetValue("FechaLimite", out var fechaLimiteObj) && fechaLimiteObj is DateTime fechaLimite
                ? fechaLimite
                : (DateTime?)null;
            var imagenUrl = dict.TryGetValue("ImagenUrl", out var imagenObj) && imagenObj != null
                ? imagenObj.ToString()
                : null;
            var cosasQueNecesitan = dict.TryGetValue("CosasQueNecesitan", out var cosasObj) && cosasObj != null
                ? cosasObj.ToString()
                : string.Empty;

            var publicacion = new Publicacion
            {
                Id = row.Id,
                IdOrganizacion = row.IdOrganizacion,
                Titulo = row.Titulo ?? string.Empty,
                TipoPropuesta = row.TipoPropuesta ?? string.Empty,
                Descripcion = row.Descripcion ?? string.Empty,
                Ubicacion = row.Ubicacion ?? string.Empty,
                Necesidades = row.Necesidades ?? string.Empty,
                CosasQueNecesitan = cosasQueNecesitan ?? string.Empty,
                ImagenUrl = imagenUrl,
                FechaAlta = row.FechaAlta is DateTime fecha ? fecha : DateTime.Now,
                FechaLimite = fechaLimiteValue,
                NombreOrganizacion = row.NombreOrganizacion ?? string.Empty,
                TelefonoOrganizacion = row.TelefonoOrganizacion?.ToString() ?? string.Empty,
                DireccionOrganizacion = row.DireccionOrganizacion?.ToString() ?? string.Empty,
                BarrioOrganizacion = row.BarrioOrganizacion?.ToString() ?? string.Empty
            };

            try
            {
                publicacion.NecesidadesDetalle = ObtenerNecesidadesPublicacion(publicacion.Id);
            }
            catch
            {
                publicacion.NecesidadesDetalle = new List<NecesidadPublicacion>();
            }

            return publicacion;
        }

        public static List<DonacionRecibida> ObtenerDonacionesRecibidasPorOrganizacion(int idOrganizacion)
        {
            using var connection = CrearConexion();

            var rows = connection.Query<dynamic>(
                "ObtenerDonacionesRecibidasPorOrganizacion",
                new { IdOrganizacion = idOrganizacion },
                commandType: CommandType.StoredProcedure).ToList();

            return rows.Select(row =>
            {
                var nombre = row.Nombre?.ToString() ?? string.Empty;
                var apellido = row.Apellido?.ToString() ?? string.Empty;
                var datos = (IDictionary<string, object>)row;

                string ObtenerDato(params string[] nombres)
                {
                    foreach (var nombreDato in nombres)
                    {
                        if (datos.TryGetValue(nombreDato, out var valor) && valor != null)
                        {
                            var texto = valor.ToString()?.Trim();
                            if (!string.IsNullOrWhiteSpace(texto))
                            {
                                return texto;
                            }
                        }
                    }

                    return string.Empty;
                }

                var usuarioId = ObtenerDato("UsuarioId", "IdUsuario");
                var idUsuario = int.TryParse(usuarioId, out var idParseado) ? idParseado : 0;
                var usuarioContacto = idUsuario > 0 ? ObtenerUsuarioPorId(idUsuario) : null;

                var email = ObtenerDato("Email", "Mail");
                var telefono = ObtenerDato("Telefono");
                var redSocial = ObtenerDato("RedSocial");

                if (usuarioContacto != null)
                {
                    if (string.IsNullOrWhiteSpace(email)) email = usuarioContacto.Email;
                    if (string.IsNullOrWhiteSpace(telefono)) telefono = usuarioContacto.Telefono;
                    if (string.IsNullOrWhiteSpace(redSocial)) redSocial = usuarioContacto.RedSocial;
                }

                return new DonacionRecibida
                {
                    Id = Convert.ToInt32(row.Id),
                    UsuarioId = idUsuario,
                    Fecha = row.Fecha is DateTime fecha ? fecha : Convert.ToDateTime(row.Fecha),
                    Categoria = row.Categoria?.ToString() ?? string.Empty,
                    NombreDonante = $"{nombre} {apellido}".Trim(),
                    NombreUsuario = row.NombreUsuario?.ToString() ?? string.Empty,
                    Email = email,
                    Telefono = telefono,
                    RedSocial = redSocial
                };
            }).ToList();
        }

        public static List<NecesidadPublicacion> ObtenerNecesidadesPublicacion(int idPublicacion)
        {
            using var connection = CrearConexion();
            var rows = connection.Query<dynamic>(
                "ObtenerNecesidadesPublicacion",
                new { IdPublicacion = idPublicacion },
                commandType: CommandType.StoredProcedure).ToList();

            return rows.Select(row => new NecesidadPublicacion
            {
                IdNecesidad = Convert.ToInt32(row.IdNecesidad),
                Categoria = row.Categoria?.ToString() ?? string.Empty,
                CantidadSolicitada = Convert.ToInt32(row.CantidadSolicitada),
                CantidadRecibida = Convert.ToInt32(row.CantidadRecibida)
            }).ToList();
        }

        public static ResultadoDonacionPublicacion RegistrarDonacionPublicacion(int usuarioId, int idPublicacion, int idNecesidad, int cantidad, string mensaje)
        {
            using var connection = CrearConexion();
            var resultado = connection.QueryFirstOrDefault<dynamic>(
                "RegistrarDonacionPublicacion",
                new
                {
                    IdUsuario = usuarioId,
                    IdPublicacion = idPublicacion,
                    IdNecesidad = idNecesidad,
                    Cantidad = cantidad,
                    Mensaje = mensaje ?? string.Empty
                },
                commandType: CommandType.StoredProcedure);

            if (resultado == null)
            {
                return new ResultadoDonacionPublicacion { Resultado = -99, Mensaje = "No se pudo registrar la donación." };
            }

            var dict = (IDictionary<string, object>)resultado;
            int LeerInt(params string[] nombres)
            {
                foreach (var nombre in nombres)
                {
                    if (dict.TryGetValue(nombre, out var valor) && valor != null)
                        return Convert.ToInt32(valor);
                }
                return 0;
            }

            return new ResultadoDonacionPublicacion
            {
                Resultado = LeerInt("Resultado", "Codigo"),
                Mensaje = dict.TryGetValue("Mensaje", out var mensajeResultado) ? mensajeResultado?.ToString() ?? string.Empty : string.Empty,
                Porcentaje = LeerInt("Porcentaje")
            };
        }

        public static void RegistrarNecesidadesPublicacion(int idPublicacion, IDictionary<string, int> cantidades)
        {
            if (idPublicacion <= 0 || cantidades == null || cantidades.Count == 0)
            {
                return;
            }

            var necesidadesJson = System.Text.Json.JsonSerializer.Serialize(
                cantidades
                    .Where(x => !string.IsNullOrWhiteSpace(x.Key) && x.Value > 0)
                    .Select(x => new { categoria = x.Key, cantidad = x.Value }));

            using var connection = CrearConexion();
            connection.Execute(
                "RegistrarNecesidadesPublicacion",
                new { IdPublicacion = idPublicacion, NecesidadesJson = necesidadesJson },
                commandType: CommandType.StoredProcedure);
        }

        private static void AsegurarComentariosLanding()
        {
        }

        public static List<ComentarioLanding> ObtenerComentariosLanding()
        {
            try
            {
                using var connection = CrearConexion();

                var rows = connection.Query<dynamic>(
                    "ObtenerComentariosLanding",
                    commandType: CommandType.StoredProcedure).ToList();

                return rows.Select(row => new ComentarioLanding
                {
                    Id = Convert.ToInt32(row.Id),
                    Nombre = row.Nombre?.ToString() ?? "Usuario",
                    Rol = row.Rol?.ToString() ?? "Usuario",
                    Texto = row.Texto?.ToString() ?? string.Empty,
                    FechaAlta = Convert.ToDateTime(row.FechaAlta)
                }).ToList();
            }
            catch (SqlException ex) when (
                ex.Number == 208
                || ex.Number == 2812
                || ex.Message.Contains("ComentariosLanding", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("ObtenerComentariosLanding", StringComparison.OrdinalIgnoreCase))
            {
                // La landing sigue funcionando aunque esta base no tenga incorporado el módulo de comentarios.
                return new List<ComentarioLanding>();
            }
        }

        public static void AgregarComentarioLanding(string nombre, string rol, string texto)
        {
            var nombreSeguro = nombre?.Trim();
            var textoSeguro = texto?.Trim();

            if (string.IsNullOrWhiteSpace(nombreSeguro)
                || string.IsNullOrWhiteSpace(textoSeguro)
                || textoSeguro.Length > 500)
            {
                throw new ArgumentException("El comentario no es válido.");
            }

            using var connection = CrearConexion();
            connection.Execute(
                "AgregarComentarioLanding",
                new { Nombre = nombreSeguro, Rol = rol, Texto = textoSeguro },
                commandType: CommandType.StoredProcedure);
        }

        public static bool ActualizarPerfilUsuario(int id, string nombreUsuario, string email, string telefono, string? nuevaClave, string fotoPerfil)
        {
            using var connection = CrearConexion();

            var resultado = connection.ExecuteScalar<int>(
                "ActualizarPerfilUsuario",
                new
                {
                    Id = id,
                    NombreUsuario = nombreUsuario,
                    Mail = email,
                    Telefono = telefono,
                    NuevaClave = nuevaClave,
                    FotoPerfil = fotoPerfil
                },
                commandType: CommandType.StoredProcedure);

            return resultado > 0;
        }

        private static Publicacion CompletarProgresoPublicacion(Publicacion publicacion)
        {
            try
            {
                publicacion.NecesidadesDetalle = ObtenerNecesidadesPublicacion(publicacion.Id);
            }
            catch
            {
                publicacion.NecesidadesDetalle = new List<NecesidadPublicacion>();
            }

            return publicacion;
        }

        private static bool PublicacionEstaSaldada(Publicacion publicacion)
        {
            if (publicacion.NecesidadesDetalle.Count == 0)
            {
                return false;
            }

            return publicacion.PorcentajeGeneral >= 100
                && publicacion.NecesidadesDetalle.All(necesidad => necesidad.Restante <= 0);
        }
    }
}
