using LineaClient.Models;

namespace LineaClient.Services
{
    // Interfaz que el repositorio real de SQL Server debera implementar
    public interface IContenedorService
    {
        // Retorna los contenedores asociados a la linea del usuario
        Task<List<Contenedor>> ObtenerContenedoresPorLineaAsync(int lineaId);

        // Marca el contenedor como lleno y registra la solicitud de uno nuevo
        Task<bool> MarcarLlenoYSolicitarAsync(int contenedorId, int lineaId);

        // Cancela la solicitud pendiente (ej. el contenedor se marco lleno por error)
        // Solo valido mientras la solicitud no haya sido entregada por el almacen
        Task<bool> CancelarSolicitudAsync(int contenedorId, int lineaId);

        // Consulta si la solicitud pendiente ya fue marcada como entregada
        Task<bool> VerificarEntregaAsync(int lineaId, int contenedorId);
    }

    // Implementacion temporal con datos simulados
    // TODO: Reemplazar por ContenedorRepository usando Microsoft.Data.SqlClient
    // Patron sugerido:
    //   - Inyectar IContenedorService via MauiProgram.cs con builder.Services.AddSingleton
    //   - El repositorio recibe la cadena de conexion desde IConfiguration o AppSettings
    //   - Usar stored procedures parametrizados para todas las operaciones
    public class ContenedorServiceSimulado : IContenedorService
    {
        // Datos de prueba: contenedores genericos en distintos estados,
        // para poder visualizar el mockup completo de la pantalla
        private readonly List<Contenedor> _contenedores =
        [
            new() { Id = 1, LineaId = 1, Tipo = "Contenedor tipo A", ImagenNombre = "contenedor_a.png", Estado = EstadoContenedor.Disponible },
            new() { Id = 2, LineaId = 1, Tipo = "Contenedor tipo B", ImagenNombre = "contenedor_b.png", Estado = EstadoContenedor.Lleno },
            new() { Id = 3, LineaId = 1, Tipo = "Contenedor tipo C", ImagenNombre = "contenedor_c.png", Estado = EstadoContenedor.EsperandoVacio },
            new() { Id = 4, LineaId = 2, Tipo = "Contenedor tipo A", ImagenNombre = "contenedor_a.png", Estado = EstadoContenedor.Disponible },
        ];

        private readonly List<SolicitudContenedor> _solicitudes =
        [
            new() { Id = 1, ContenedorId = 3, LineaId = 1, FechaSolicitud = DateTime.Now, Estado = EstadoSolicitud.Pendiente }
        ];

        public Task<List<Contenedor>> ObtenerContenedoresPorLineaAsync(int lineaId)
        {
            var resultado = _contenedores.Where(c => c.LineaId == lineaId).ToList();
            return Task.FromResult(resultado);
        }

        public Task<bool> MarcarLlenoYSolicitarAsync(int contenedorId, int lineaId)
        {
            var contenedor = _contenedores.FirstOrDefault(c => c.Id == contenedorId);
            if (contenedor is null) return Task.FromResult(false);

            contenedor.Estado = EstadoContenedor.Lleno;

            _solicitudes.Add(new SolicitudContenedor
            {
                Id = _solicitudes.Count + 1,
                ContenedorId = contenedorId,
                LineaId = lineaId,
                FechaSolicitud = DateTime.Now,
                Estado = EstadoSolicitud.Pendiente
            });

            return Task.FromResult(true);
        }

        public Task<bool> CancelarSolicitudAsync(int contenedorId, int lineaId)
        {
            var contenedor = _contenedores.FirstOrDefault(c => c.Id == contenedorId);
            if (contenedor is null) return Task.FromResult(false);

            var solicitud = _solicitudes
                .Where(s => s.LineaId == lineaId && s.ContenedorId == contenedorId)
                .OrderByDescending(s => s.FechaSolicitud)
                .FirstOrDefault();

            // Solo se puede cancelar si la solicitud sigue pendiente
            // (si ya fue entregada, ya no hay nada que cancelar)
            if (solicitud is null || solicitud.Estado != EstadoSolicitud.Pendiente)
                return Task.FromResult(false);

            solicitud.Estado = EstadoSolicitud.Cancelada;
            contenedor.Estado = EstadoContenedor.Disponible;

            return Task.FromResult(true);
        }

        public async Task<bool> VerificarEntregaAsync(int lineaId, int contenedorId)
        {
            // TODO: Consultar SQL Server
            // SELECT TOP 1 Estado FROM SolicitudesContenedor
            // WHERE LineaId = @lineaId AND ContenedorId = @contenedorId
            // AND Estado = 'Entregado'
            // ORDER BY FechaSolicitud DESC

            await Task.Delay(600); // Simula latencia

            var solicitud = _solicitudes
                .Where(s => s.LineaId == lineaId && s.ContenedorId == contenedorId)
                .OrderByDescending(s => s.FechaSolicitud)
                .FirstOrDefault();

            if (solicitud is null || solicitud.Estado != EstadoSolicitud.Pendiente)
                return false;

            var contenedor = _contenedores.First(c => c.Id == contenedorId);

            // Mientras se espera, el contenedor queda marcado como esperando vacio
            if (contenedor.Estado == EstadoContenedor.Lleno)
                contenedor.Estado = EstadoContenedor.EsperandoVacio;

            // Simular que despues de 10 segundos el almacen "entrega" el contenedor
            if ((DateTime.Now - solicitud.FechaSolicitud).TotalSeconds >= 10)
            {
                solicitud.Estado = EstadoSolicitud.Entregado;
                contenedor.Estado = EstadoContenedor.Disponible;
                return true;
            }

            return false;
        }
    }
}
