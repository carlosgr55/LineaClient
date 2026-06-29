namespace LineaClient.Models
{
    // Estado posible de un contenedor en la linea
    public enum EstadoContenedor
    {
        Disponible,     // Vacio en la linea, listo para llenarse
        Lleno,          // Marcado lleno por la linea, solicitud de reemplazo enviada
        EsperandoVacio  // Solicitud confirmada por el almacen, esperando entrega del vacio
    }

    // Estado de una solicitud de contenedor
    public enum EstadoSolicitud
    {
        Pendiente,   // Enviada, sin atender
        Entregado,   // El almacen entrego el contenedor vacio a la linea
        Cancelada,   // La linea cancelo la solicitud (ej. error al marcar lleno)
        Rechazado
    }

    public class Contenedor
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;         // Ej: "Caja 48L", "Tote 600x400"
        public string ImagenNombre { get; set; } = string.Empty; // Nombre del recurso de imagen en Resources/Images
        public EstadoContenedor Estado { get; set; }
        public int LineaId { get; set; }
    }

    public class SolicitudContenedor
    {
        public int Id { get; set; }
        public int ContenedorId { get; set; }
        public int LineaId { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public EstadoSolicitud Estado { get; set; }
    }

    // Datos del usuario activo; se asigna en LoginViewModel tras autenticar
    public class SesionUsuario
    {
        public static SesionUsuario? Actual { get; set; }

        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public int LineaId { get; set; }
        public string NombreLinea { get; set; } = string.Empty;
    }
}
