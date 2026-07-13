# Fase de Análisis - Sistema POS para Mozos (SDD)

## 1. Visión General del Producto
Plataforma web POS (Point of Sale) orientada a digitalizar la toma de comandas en restaurantes de la región de Ayacucho, eliminando el uso de libretas de papel, previniendo errores matemáticos y agilizando la comunicación con cocina.

## 2. Actores del Sistema
* **Mozo:** Usuario principal. Utiliza dispositivos móviles (tablets) para registrar pedidos en las mesas.
* **Cajero / Administrador:** Encargado de cobrar, liberar mesas y gestionar el catálogo del menú.
* **Base de Datos (Supabase):** Servicio externo PostgreSQL que almacena la persistencia de forma transaccional.

## 3. Requerimientos Funcionales (RF)
* **RF-01 Autenticación:** Acceso mediante PIN numérico de 4 dígitos.
* **RF-02 Gestión de Salón:** Visualización de mesas con estados dinámicos (Libre, Ocupada, Cuenta).
* **RF-03 Toma de Comandas:** Selección de platos desde un catálogo digital categorizado.
* **RF-04 Totalización Automática:** Cálculo estricto y automático del subtotal y total de la orden.
* **RF-05 Procesamiento de Pagos:** Liquidación de órdenes y liberación automática de la mesa.

## 4. Requerimientos No Funcionales (RNF)
* **RNF-01 Rendimiento:** Respuestas menores a 1 segundo en la UI.
* **RNF-02 Resiliencia:** Tolerancia a micro-cortes de red mediante `EnableRetryOnFailure`.
* **RNF-03 Usabilidad:** Diseño *Mobile-First* con *Dark Theme* de alto contraste.
* **RNF-04 Calidad:** Cobertura de pruebas unitarias/integración > 70% usando xUnit.