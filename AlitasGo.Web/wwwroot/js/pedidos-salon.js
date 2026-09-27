let mesaActual = null;
let itemsComanda = [];
const modalProductoBs = new bootstrap.Modal(document.getElementById('modalProducto'));

// Actualizar precio unitario al cambiar producto en el modal
document.getElementById('selProducto').addEventListener('change', function () {
    const precio = this.options[this.selectedIndex].getAttribute('data-precio');
    document.getElementById('txtPrecioUnitario').value = `S/. ${parseFloat(precio).toFixed(2)}`;
});

function seleccionarMesa(id, numero, estado) {
    mesaActual = { id, numero, estado };
    document.getElementById('labelMesaSeleccionada').innerText = `Mesa: ${numero} (${estado})`;
    document.getElementById('btnAbrirModalItem').disabled = false;
    actualizarBotonesEnvio();
}

function abrirModalAgregar() {
    if (!mesaActual) {
        alert("Selecciona primero una mesa en el plano.");
        return;
    }
    document.getElementById('numCantidad').value = 1;
    document.getElementById('txtNotasCocina').value = '';
    modalProductoBs.show();
}

function agregarItemAComanda() {
    const sel = document.getElementById('selProducto');
    const productoId = parseInt(sel.value);
    const nombreProducto = sel.options[sel.selectedIndex].text.split(' - ')[0];
    const precioUnitario = parseFloat(sel.options[sel.selectedIndex].getAttribute('data-precio'));
    const salsa = document.getElementById('selSalsa').value;
    const cantidad = parseInt(document.getElementById('numCantidad').value);
    const notas = document.getElementById('txtNotasCocina').value.trim();

    if (cantidad <= 0) return;

    itemsComanda.push({
        productoId: productoId,
        nombre: nombreProducto,
        saborSalsa: salsa,
        cantidad: cantidad,
        precioUnitario: precioUnitario,
        importe: Math.round(precioUnitario * cantidad * 100) / 100,
        notasCocina: notas
    });

    renderizarComanda();
    modalProductoBs.hide();
}

function eliminarItem(index) {
    itemsComanda.splice(index, 1);
    renderizarComanda();
}

function renderizarComanda() {
    const tbody = document.getElementById('cuerpoTablaPedido');
    tbody.innerHTML = '';

    if (itemsComanda.length === 0) {
        tbody.innerHTML = `
            <tr id="filaVacia">
                <td colspan="5" class="text-center text-muted py-4">
                    Selecciona una mesa y agrega productos a la comanda.
                </td>
            </tr>`;
        recalcularTotales(0);
        actualizarBotonesEnvio();
        return;
    }

    let total = 0;
    itemsComanda.forEach((item, idx) => {
        total += item.importe;
        const fila = document.createElement('tr');
        fila.innerHTML = `
            <td class="fw-bold">${item.cantidad}</td>
            <td>
                <div>${item.nombre}</div>
                <small class="text-muted"><i class="bi bi-tag"></i> ${item.saborSalsa} ${item.notasCocina ? `(${item.notasCocina})` : ''}</small>
            </td>
            <td class="text-end">S/. ${item.precioUnitario.toFixed(2)}</td>
            <td class="text-end fw-bold">S/. ${item.importe.toFixed(2)}</td>
            <td class="text-end">
                <button class="btn btn-sm btn-outline-danger py-0" onclick="eliminarItem(${idx})">&times;</button>
            </td>
        `;
        tbody.appendChild(fila);
    });

    recalcularTotales(total);
    actualizarBotonesEnvio();
}

function recalcularTotales(total) {
    // Regla de negocio: desglose sobre el precio final de carta (18%)
    const subtotal = Math.round((total / 1.18) * 100) / 100;
    const igv = Math.round((total - subtotal) * 100) / 100;

    document.getElementById('txtSubTotal').innerText = `S/. ${subtotal.toFixed(2)}`;
    document.getElementById('txtIgv').innerText = `S/. ${igv.toFixed(2)}`;
    document.getElementById('txtTotal').innerText = `S/. ${total.toFixed(2)}`;
}

function limpiarComanda() {
    itemsComanda = [];
    renderizarComanda();
}

function actualizarBotonesEnvio() {
    const btn = document.getElementById('btnEnviarComanda');
    btn.disabled = !(mesaActual && itemsComanda.length > 0);
}

async function confirmarYEnviarPedido() {
    if (!mesaActual || itemsComanda.length === 0) return;

    const payload = {
        mesaId: mesaActual.id,
        usuarioId: "CajeroSalon",
        observaciones: `Pedido registrado desde salón para Mesa ${mesaActual.numero}`,
        detalles: itemsComanda.map(item => ({
            productoId: item.productoId,
            cantidad: item.cantidad,
            saborSalsa: item.saborSalsa,
            notasCocina: item.notasCocina
        }))
    };

    const btn = document.getElementById('btnEnviarComanda');
    btn.disabled = true;
    btn.innerHTML = `<span class="spinner-border spinner-border-sm me-2"></span>Registrando...`;

    try {
        const respuesta = await fetch('/Pedidos/Registrar', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        const data = await respuesta.json();

        if (respuesta.ok && data.exito) {
            alert(`Comanda enviada a cocina con éxito. Código: ${data.pedido.codigoPedido}`);
            limpiarComanda();
        } else {
            alert(`No se pudo registrar: ${data.mensaje || 'Error en stock o validación'}`);
        }
    } catch (err) {
        alert("Ocurrió un error al contactar al servidor: " + err.message);
    } finally {
        btn.disabled = false;
        btn.innerHTML = `<i class="bi bi-fire me-2"></i> Enviar a Cocina`;
        actualizarBotonesEnvio();
    }
}