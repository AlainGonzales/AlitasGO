using System.Collections.Generic;
using AlitasGo.Domain.DTOs;
using AlitasGo.Domain.Interfaces;

namespace AlitasGo.Domain.Handlers
{
    public class ValidadorPedido : IValidadorPedido
    {
        private List<string> _errores = new();

        public bool Validar(CrearPedidoDto pedido)
        {
            _errores.Clear();

            if (pedido == null)
                _errores.Add("El pedido no puede ser nulo");

            if (pedido?.MesaId <= 0)
                _errores.Add("La mesa es requerida");

            if (string.IsNullOrWhiteSpace(pedido?.UsuarioId))
                _errores.Add("El usuario es requerido");

            return _errores.Count == 0;
        }

        public List<string> ObtenerErrores()
        {
            return _errores;
        }
    }
}