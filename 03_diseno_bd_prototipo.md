# Diseño de Base de Datos y UI/UX

## 1. Invariantes de Negocio (Reglas SDD)
Las siguientes reglas están protegidas a nivel de base de datos y controladores:
* **INV-01:** El PIN de acceso de los empleados debe ser exactamente de 4 dígitos.
* **INV-02:** Una mesa no puede abrir una nueva orden si no está en estado "Libre".
* **INV-03:** El precio del plato se congela al insertarse en el `DetalleOrden` para evitar fluctuaciones históricas.
* **INV-04:** El Total de la orden es el resultado inmutable de `SUM(Cantidad * Precio)`.
* **INV-05:** Una vez que la orden está "Pagada", no acepta modificaciones.

## 2. Modelo Entidad-Relación (Core)
* `Empleados` (1) ---> (N) `Ordenes`
* `Mesas` (1) ---> (N) `Ordenes`
* `Ordenes` (1) ---> (N) `DetallesOrden`
* `Platos` (1) ---> (N) `DetallesOrden`
* `Ordenes` (1) ---> (1) `TransaccionesPago`

## 3. Guía de Diseño UI/UX (Prototipo Mobile-First)
* **Patrón de Navegación:** Diseñado principalmente para uso en *Tablets* horizontales.
* **Tema Visual:** *Dark Theme* para reducir fatiga visual en entornos de restaurante con baja iluminación.
* **Componentes Clave:** * Modal interactivo para el PIN de acceso numérico (Touch-friendly).
  * *Grid* de colores para el salón: Verde (Mesa Libre), Rojo (Ocupada).
  * Tarjetas (Cards) amplias en el menú para evitar errores táctiles (*misclicks*).