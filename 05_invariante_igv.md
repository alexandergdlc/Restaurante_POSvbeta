# INV-08: Desglose de Impuestos (IGV) Inmutable

## 1. Contexto del Negocio
En el régimen tributario peruano, los precios de los platos en la carta ya incluyen el Impuesto General a las Ventas (IGV del 18%). Para fines de transparencia y futura facturación electrónica con SUNAT, el sistema debe desglosar el Subtotal y el IGV a partir del Total cerrado de la orden. 

Este cálculo es de misión crítica; un error de redondeo o alteración en la vista (frontend) generaría inconsistencias contables. Por lo tanto, se define como una Invariante protegida en la Capa de Dominio.

## 2. Contrato SDD (Spec-kit)

* **GIVEN (Dado que):** La Orden actual se encuentra totalizada con un `Total` mayor a 0 (el cual ya incluye el 18% de impuesto).
* **WHEN (Cuando):** El sistema o el mozo solicita el resumen de la cuenta o comprobante de pago.
* **THEN (Entonces):** La entidad `Orden` debe exponer de forma inmutable y de solo lectura el `Subtotal` (calculado como Total / 1.18) y el `MontoIGV` (calculado como Total - Subtotal), redondeados a dos decimales.

PROMPT

Actúa como un Ingeniero de Software Senior experto en .NET 10 y C#.
En mi proyecto Restaurante POS, estamos aplicando estrictamente la metodología SDD (Specification Driven Development) y Arquitectura de 3 Capas.

Acabo de documentar una nueva invariante (INV-08) para Desglosar matemáticamente el Subtotal, el IGV (18%) y el Total en la orden. >
Mi contrato GIVEN-WHEN-THEN exige que a partir de un Total ya calculado, la entidad exponga inmutablemente el Subtotal (Total / 1.18) y el MontoIGV (Total - Subtotal).

Genera el código en este orden estricto para que yo lo integre:

Prueba Unitaria (xUnit): Escribe el test (Arrange, Act, Assert) instanciando una Orden con un Total de 118.00. Verifica que el Subtotal retorne 100.00 y el MontoIGV retorne 18.00.

Capa de Dominio (Orden.cs): Genera las propiedades calculadas de solo lectura usando Math.Round(..., 2) para la entidad.

Capa de Presentación (Vista Razor): Muestra el bloque HTML (con Bootstrap) enseñando cómo invocar @Model.Subtotal, @Model.MontoIGV y @Model.Total en el resumen del carrito.