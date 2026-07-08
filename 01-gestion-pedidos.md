# Spec: Gestión de Pedidos de Mozos en Mesa

## 1. Contexto
El mozo necesita registrar los productos solicitados en una mesa para enviarlos a la base de datos (Supabase).

## 3. Criterios de Aceptación (GIVEN-WHEN-THEN)

**Escenario 1: Cálculo del total (INV-01)**
- GIVEN un pedido con 2 productos: uno de precio 10 y cantidad 2, y otro de precio 15 y cantidad 1.
- WHEN se invoca el método `CalcularTotal()`
- THEN el total retornado debe ser 35.