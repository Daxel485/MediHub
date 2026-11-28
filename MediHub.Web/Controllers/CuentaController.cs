using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MediHub.Application.DTOs;
using MediHub.Application.Interfaces;
using MediHub.Infrastructure.Identity;
using MediHub.Web.Extensions;
using MediHub.Web.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace MediHub.Web.Controllers
{
    // Inicio de sesión, registro y datos de la cuenta. Todo lo demás del sitio exige sesión (filtro global).
    [Authorize]
    public class CuentaController : Controller
    {
        private readonly IUsuarioService _usuarios;
        private readonly IProveedorService _proveedores;

        public CuentaController(IUsuarioService usuarios, IProveedorService proveedores)
        {
            _usuarios = usuarios;
            _proveedores = proveedores;
        }

        private ApplicationSignInManager SignInManager
        {
            get { return HttpContext.GetOwinContext().Get<ApplicationSignInManager>(); }
        }

        private ApplicationUserManager UserManager
        {
            get { return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
        }

        private IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        // ---------- Login ----------

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel modelo, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid) return View(modelo);

            // Una cuenta desactivada (Usuario.Estado = false) no puede entrar aunque la contraseña sea correcta.
            var cuenta = await UserManager.FindByNameAsync(modelo.CorreoElectronico);
            if (cuenta != null
                && !await UserManager.IsLockedOutAsync(cuenta.Id)
                && await UserManager.CheckPasswordAsync(cuenta, modelo.Contrasena)
                && CuentaInactiva(cuenta))
            {
                ModelState.AddModelError("", "La cuenta está inactiva. Comuníquese con un administrador.");
                return View(modelo);
            }

            var resultado = await SignInManager.PasswordSignInAsync(
                modelo.CorreoElectronico, modelo.Contrasena, modelo.Recordarme, shouldLockout: true);

            switch (resultado)
            {
                case SignInStatus.Success:
                    return RedirigirALocal(returnUrl);
                case SignInStatus.LockedOut:
                    ModelState.AddModelError("", "La cuenta se bloqueó por varios intentos fallidos. Intente de nuevo en unos minutos.");
                    return View(modelo);
                default:
                    ModelState.AddModelError("", "Correo electrónico o contraseña incorrectos.");
                    return View(modelo);
            }
        }

        // ---------- Registro ----------

        [AllowAnonymous]
        public ActionResult Registro()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");

            return View(CompletarListas(new RegistroViewModel()));
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<ActionResult> Registro(RegistroViewModel modelo)
        {
            if (!ModelState.IsValid) return View(CompletarListas(modelo));

            var cuenta = new ApplicationUser
            {
                UserName = modelo.CorreoElectronico,
                Email = modelo.CorreoElectronico,
                NombreCompleto = string.Join(" ", modelo.Nombres.Trim(), modelo.PrimerApellido.Trim(), modelo.SegundoApellido.Trim())
            };

            var creada = await UserManager.CreateAsync(cuenta, modelo.Contrasena);
            if (!creada.Succeeded)
            {
                AgregarErrores(creada);
                return View(CompletarListas(modelo));
            }

            var conRol = await UserManager.AddToRoleAsync(cuenta.Id, modelo.Rol);
            if (!conRol.Succeeded)
            {
                await UserManager.DeleteAsync(cuenta);
                AgregarErrores(conRol);
                return View(CompletarListas(modelo));
            }

            // Los proveedores tienen además un registro de Usuario enlazado por IdNetUser.
            if (modelo.EsProveedor)
            {
                var dto = new UsuarioDto
                {
                    IdProveedor = modelo.IdProveedor.Value,
                    Nombres = modelo.Nombres.Trim(),
                    PrimerApellido = modelo.PrimerApellido.Trim(),
                    SegundoApellido = modelo.SegundoApellido.Trim(),
                    Identificacion = modelo.Identificacion.Trim(),
                    CorreoElectronico = modelo.CorreoElectronico.Trim()
                };

                var enlace = _usuarios.VincularCuenta(dto, Guid.Parse(cuenta.Id));
                if (!enlace.Exito)
                {
                    // Sin el enlace la cuenta quedaría sin proveedor: se deshace.
                    await UserManager.DeleteAsync(cuenta);
                    if (enlace.TieneErroresDeValidacion)
                        ModelState.AgregarErrores(enlace);
                    else
                        ModelState.AddModelError("", enlace.Mensaje);
                    return View(CompletarListas(modelo));
                }
            }

            await SignInManager.SignInAsync(cuenta, isPersistent: false, rememberBrowser: false);
            return RedirectToAction("Index", "Home");
        }

        // ---------- Sesión iniciada ----------

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CerrarSesion()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login");
        }

        public async Task<ActionResult> Perfil()
        {
            var cuenta = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (cuenta == null) return HttpNotFound();

            var roles = await UserManager.GetRolesAsync(cuenta.Id);
            var usuario = _usuarios.ObtenerPorIdNetUser(Guid.Parse(cuenta.Id));

            return View(new PerfilViewModel
            {
                NombreCompleto = cuenta.NombreCompleto,
                CorreoElectronico = cuenta.Email,
                Rol = string.Join(", ", roles),
                NombreProveedor = usuario == null ? null : usuario.NombreProveedor,
                Identificacion = usuario == null ? null : usuario.Identificacion
            });
        }

        // Se muestra cuando hay sesión pero el rol no alcanza (ver Startup.cs).
        [AllowAnonymous]
        public ActionResult AccesoDenegado()
        {
            Response.StatusCode = 403;
            Response.TrySkipIisCustomErrors = true;
            return View();
        }

        // ---------- Auxiliares ----------

        private bool CuentaInactiva(ApplicationUser cuenta)
        {
            var usuario = _usuarios.ObtenerPorIdNetUser(Guid.Parse(cuenta.Id));
            return usuario != null && !usuario.Estado;
        }

        private RegistroViewModel CompletarListas(RegistroViewModel modelo)
        {
            modelo.Proveedores = _proveedores.ObtenerTodos()
                .Where(p => p.Estado)
                .OrderBy(p => p.Nombre)
                .Select(p => new SelectListItem
                {
                    Value = p.IdProveedor.ToString(),
                    Text = p.Nombre,
                    Selected = p.IdProveedor == modelo.IdProveedor
                }).ToList();
            return modelo;
        }

        private void AgregarErrores(IdentityResult resultado)
        {
            foreach (var error in resultado.Errors)
                ModelState.AddModelError("", TraducirError(error));
        }

        // Identity devuelve los mensajes en inglés; se traducen los más comunes.
        private static string TraducirError(string error)
        {
            if (error.Contains("is already taken")) return "Ya existe una cuenta con ese correo electrónico.";
            if (error.Contains("Passwords must be at least")) return "La contraseña debe tener al menos 8 caracteres.";
            if (error.Contains("must have at least one digit")) return "La contraseña debe incluir al menos un número.";
            if (error.Contains("must have at least one lowercase")) return "La contraseña debe incluir al menos una minúscula.";
            if (error.Contains("must have at least one uppercase")) return "La contraseña debe incluir al menos una mayúscula.";
            return error;
        }

        private ActionResult RedirigirALocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }
    }
}
