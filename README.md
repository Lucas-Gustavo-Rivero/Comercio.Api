# Comercio.Api

API REST para la gestión de un catálogo de productos, desarrollada en .NET 10 con Entity
Framework Core y SQL Server.

> **Estado del proyecto:** el módulo de Productos está completo y probado end-to-end
> (unitarios, integración con Docker, y contra la API completa). Es la primera pieza de
> un e-commerce más amplio, desarrollado de forma incremental.

## Características

- CRUD completo de productos, con paginación, búsqueda y filtros de precio.
- Validaciones de entrada con FluentValidation.
- **Idempotencia** en la creación: nombres duplicados se rechazan tanto a nivel de aplicación
  como con un índice único en base de datos, cubriendo también casos de concurrencia real.
- **Concurrencia optimista** en las actualizaciones, mediante un token `rowVersion` gestionado
  por SQL Server.
- Errores estandarizados en formato [RFC 7807 (ProblemDetails)](https://www.rfc-editor.org/rfc/rfc9457.html).
- Cobertura de tests en tres niveles: unitarios, integración con Testcontainers, y contra la
  API completa — incluyendo escenarios reales de idempotencia y concurrencia.

## Tecnologías

- .NET 10 / ASP.NET Core
- Entity Framework Core + SQL Server
- FluentValidation
- AutoMapper
- xUnit + Moq + Testcontainers

## Cómo correrlo localmente

### Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (o LocalDB, incluido con Visual Studio)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (solo necesario para
  correr los tests de integración)

### Pasos

1. Cloná el repositorio:
   ```bash
   git clone https://github.com/Lucas-Gustavo-Rivero/Comercio.Api.git
   cd Comercio.Api
   ```

2. Configurá la cadena de conexión en `appsettings.json` (dentro de `Comercio.Api/`):
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ComercioAppDB;trusted_Connection=True;MultipleActiveResultSets=true"
     }
   }
   ```

3. Aplicá las migraciones para crear la base de datos:
   ```bash
   dotnet ef database update --project Comercio.Api
   ```

4. Levantá el proyecto:
   ```bash
   dotnet run --project Comercio.Api
   ```

5. La API va a quedar disponible en la URL que indique la consola (por ejemplo
   `https://localhost:7069`). Podés explorar los endpoints desde Swagger/OpenAPI o desde la
   documentación pública (ver más abajo).

## Documentación de la API

La documentación completa de todos los endpoints, con ejemplos reales de request y response
para cada caso (éxito, validación, conflicto, no encontrado), está publicada en Postman:

**[Ver documentación completa](https://documenter.getpostman.com/view/53907460/2sBYAvuqMX)**

## Arquitectura

El proyecto sigue una arquitectura en capas clásica, con responsabilidades bien separadas:

```
Controller → Service → Repository → AppDbContext (EF Core)
```

- **Controllers**: reciben el request HTTP, delegan toda la lógica al service, y traducen el
  resultado a una respuesta HTTP (código de estado + `ProblemDetails` en caso de error).
  No contienen lógica de negocio.
- **Services**: contienen las reglas de negocio (idempotencia, mapeo de entidades a DTOs,
  manejo de conflictos de concurrencia). Nunca lanzan excepciones para flujo de control;
  devuelven un objeto `Result`/`Result<T>` que representa éxito o fallo de forma explícita.
- **Repositories**: encapsulan el acceso a datos vía EF Core. No conocen DTOs ni reglas de
  negocio — trabajan únicamente con entidades de dominio.
- **DTOs + AutoMapper**: separan la forma en que los datos se exponen hacia afuera (DTOs) de
  cómo se modelan internamente (entidades), con el mapeo centralizado en `AutoMapper`.
- **FluentValidation + ValidationFilter**: las validaciones de entrada viven en validadores
  dedicados por DTO, y se ejecutan automáticamente antes de cada action mediante un
  `IAsyncActionFilter` genérico, sin necesidad de código de validación repetido en cada
  controller.
- **Result → ProblemDetails**: un método de extensión (`ToErrorResponse`) centraliza la
  traducción entre el resultado interno de negocio (`Result`) y la respuesta HTTP estándar
  en formato RFC 7807, evitando que cada controller arme sus propios mensajes de error.

## Decisiones de diseño

- **Idempotencia sin token**: en vez de un `Idempotency-Key` por header, se optó por un
  índice único sobre `Nombre` a nivel de base de datos, combinado con un chequeo previo en
  el service para dar una respuesta rápida en el caso común. Esto cubre tanto el caso simple
  (usuario hace doble clic en "Crear") como la condición de carrera real entre requests
  concurrentes, que el chequeo previo por sí solo no puede garantizar.
- **Concurrencia optimista con RowVersion**: se eligió sobre locking pesimista para no
  bloquear lecturas mientras se procesa una edición, siguiendo el patrón estándar de EF Core
  con columnas `rowversion` autogeneradas por SQL Server.
- **Patrón Result en el service**: el service nunca lanza excepciones para flujo de control
  de negocio; devuelve un `Result`/`Result<T>` que el controller traduce a `ProblemDetails`,
  manteniendo la capa de negocio completamente desacoplada de conceptos HTTP.
- **Testing en tres niveles**: unitarios (rápidos, mockeados) para la lógica del service;
  integración con Testcontainers (SQL Server real en Docker) específicamente para validar
  restricciones que un mock no puede simular con fidelidad (índice único, concurrencia real,
  precisión decimal); y tests contra la API completa (`WebApplicationFactory`) para el
  pipeline HTTP end-to-end (filtros de validación, formato de errores).

## Testing

El proyecto cuenta con tres niveles de testing:

- **Unitarios** (`Comercio.Api.Tests`): cubren la lógica de negocio del service con mocks
  (Moq), sin dependencias externas.
- **Integración** (`Comercio.Api.IntegrationTest`): usan [Testcontainers](https://testcontainers.com/)
  para levantar SQL Server real en Docker, validando restricciones que no pueden simularse
  con mocks (índice único, concurrencia optimista, precisión decimal) y el pipeline HTTP
  completo vía `WebApplicationFactory`.

### Cómo correrlos

```bash
# Unitarios (rápidos, sin dependencias)
dotnet test Comercio.Api.Tests

# Integración (requiere Docker Desktop corriendo)
dotnet test Comercio.Api.IntegrationTest
```

## Roadmap

Este proyecto se desarrolla de forma incremental. El módulo de **Productos** está
completo (CRUD, paginación, validaciones, idempotencia, concurrencia, y testing en tres
niveles). Los próximos módulos planeados son:

- [ ] Carrito de compras
- [ ] Frontend en Angular consumiendo esta API
- [ ] Pipeline de CI (GitHub Actions) corriendo la batería de tests en cada push
