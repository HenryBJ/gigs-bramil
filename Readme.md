# Lottery API

Este proyecto es una API para gestionar una aplicación de lotería. Implementa autenticación JWT y utiliza Swagger para la documentación y prueba de los endpoints.

## Características

- Autenticación mediante JWT.
- Documentación y pruebas de API con Swagger.
- Conexión a bases de datos mediante cadenas de conexión definidas en `appsettings.json`.

## Requisitos

- .NET 6.0 o superior
- SQL Server (o cualquier base de datos compatible con la cadena de conexión)

## Configuración

### Cadenas de conexión

En el archivo `appsettings.json`, las cadenas de conexión se definen en la sección `ConnectionStrings`. La configuración por defecto incluye una cadena de conexión llamada `DefaultConnection`.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Jwt": {
    "Key": "my_secret_here",
    "Issuer": "Lottery"
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;"
  }
}

### Login

El endpoint "api/Lottery/CheckLogin", permite la autentificacion, devolviendo el accessToken de JWT