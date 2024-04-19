using ApiRest.DTO;
using ApiRest.Modelo;
using ApiRest.Repositorio;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ApiRest.Controllers
{
    [Route("productos")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class ProductosController : ControllerBase
    {
        private readonly IProductosEnMemoria repositorio;
        public ProductosController(IProductosEnMemoria r)
        {
            this.repositorio = r;
        }

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<ProductoDTO>> DameProductos(int pag,int reg)
        {
            var listaProductos = (await repositorio.DameProductosAsincrono(pag, reg)).Select(p=>p.convertirDTO());
            return listaProductos;
        }

        [HttpGet("{codProducto}")]
        [Authorize]
        public async Task<ActionResult<ProductoDTO>> DameProducto(string codProducto)
        {
            var producto = (await repositorio.DameProductoAsincrono(codProducto)).convertirDTO();

            if (producto is null)
            {
                return NotFound();
            }

            return producto;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ProductoDTO>> CrearProducto(ProductoDTO p)
        {
            Producto producto = null;

                producto = new Producto
                {
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    SKU = p.SKU,
                    FechaAlta = DateTime.Now,

                };

                await repositorio.CrearProductoAsincrono(producto);
            


            return producto.convertirDTO();
        }

        [HttpPut]
        [Authorize]
        public async Task<ActionResult<ProductoDTO>> ModificarProducto(String codProducto, ProductoActualizarDTO p)
        {
            Producto existeProducto = await repositorio.DameProductoAsincrono(codProducto);
            if (existeProducto is null)
            {
                return NotFound();
            }

            existeProducto.Nombre = p.Nombre;
            existeProducto.Descripcion = p.Descripcion;
            existeProducto.Precio = p.Precio;
 
            await repositorio.ModificarProductoAsincrono(existeProducto);

            return existeProducto.convertirDTO();
        }

        [HttpDelete]
        [Authorize]
        public async Task<ActionResult> BorrarProducto(String codProducto)
        {
            Producto existeProducto = await repositorio.DameProductoAsincrono(codProducto);
            if (existeProducto is null)
            {
                return NotFound();
            }

            await repositorio.BorrarProductoAsincrono(codProducto);

            return NoContent();
        }


        [HttpPost("GuardarImagen")]
        public async Task<string> GuardarImagen([FromForm] SubirImagenAPI fichero)
        {
            var ruta = String.Empty;
            if (fichero.Archivo.Length > 0)
            {
                var nombreArchivo = Guid.NewGuid().ToString() + ".jpg";
                ruta = $"Imagenes/{nombreArchivo}";
                using (var stream = new FileStream(ruta, FileMode.Create))
                {
                    await fichero.Archivo.CopyToAsync(stream);
                }
            }
            return ruta;

        }



    }
}
