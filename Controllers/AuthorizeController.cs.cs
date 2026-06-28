using Microsoft.AspNetCore.Authorization;

namespace RestauranteCbba.Controllers
{
    [Authorize]
    public class AuthorizeController
    {
        // Esta clase base sirve para que todos los controladores que hereden de ella
        // requieran autenticación automáticamente.
    }
}