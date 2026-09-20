namespace Comercio.Api.Models
{
    public enum Roles
    {
        Usuario, Administrador
    }
    public class User
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public Roles Rol { get; set; } = Roles.Usuario;

        //Propiedad de navegacion

        public Carrito? Carrito { get; set; } 
    }
}
