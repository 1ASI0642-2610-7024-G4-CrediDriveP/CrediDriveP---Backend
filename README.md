# CrediDriveP — Backend API

Sistema de gestión de créditos vehiculares para Perú, desarrollado con **ASP.NET Core 8**, **Entity Framework Core** y **MySQL**.

---

## Stack Tecnológico

| Tecnología | Versión | Uso |
|---|---|---|
| ASP.NET Core | 8.0 | Framework Web API |
| Entity Framework Core | 8.0 | ORM |
| Pomelo.EFCore.MySql | 8.0 | Conector MySQL |
| Swashbuckle (Swagger) | 6.5 | Documentación API |
| BCrypt.Net-Next | 4.0.3 | Hash de contraseñas |
| JWT Bearer | 8.0 | Autenticación |
| MySQL | 8.x | Base de datos |

---

## Requisitos Previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [MySQL 8.x](https://dev.mysql.com/downloads/)
- [Rider](https://www.jetbrains.com/rider/) o Visual Studio 2022+
- Git

---

## Clonar el Repositorio

```bash
git clone https://github.com/1ASI0642-2610-7024-G4-CrediDriveP/CrediDriveP---Backend.git
cd CrediDriveP---Backend
```

---

## Configuración

### 1. Crear base de datos en MySQL

```sql
CREATE DATABASE credidrivep CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

### 2. Configurar credenciales

Copia el archivo de ejemplo y edítalo con tus datos:

```bash
cp appsettings.Example.json appsettings.json
```

Edita `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=credidrivep;User=root;Password=TU_PASSWORD;"
  },
  "Jwt": {
    "Key": "ClaveSecretaSuperLargaParaJWT_CrediDriveP_2024!",
    "Issuer": "CrediDriveP",
    "Audience": "CrediDrivePUsers",
    "ExpiresInHours": 8
  }
}
```

> ⚠️ El archivo `appsettings.json` está en `.gitignore` — nunca se sube al repositorio.

### 3. Instalar dependencias

```bash
dotnet restore
```

### 4. Ejecutar migraciones

```bash
dotnet ef database update
```

Esto crea todas las tablas y el **usuario Admin inicial**:

| Campo | Valor |
|---|---|
| Email | admin@credidrivep.com |
| Password | Admin123! |
| Rol | ADMIN |

> ⚠️ Cambia la contraseña del admin después del primer login.

### 5. Ejecutar el proyecto

```bash
dotnet run
```

Swagger disponible en: `https://localhost:{puerto}`

---

## Autenticación

La API usa **JWT Bearer Token**.

1. Llama a `POST /api/auth/login` con email y password
2. Copia el `token` de la respuesta
3. En Swagger: click en **Authorize** → ingresa `Bearer {token}`
4. Todos los endpoints protegidos ya funcionan

---

## Endpoints

### 🔐 Auth — `/api/auth`

| Método | Ruta | Rol | Descripción |
|---|---|---|---|
| POST | `/api/auth/login` | Público | Iniciar sesión → retorna JWT |
| GET | `/api/auth/me` | Todos | Ver perfil del usuario autenticado |
| POST | `/api/auth/officers` | ADMIN | Crear nuevo Officer |
| GET | `/api/auth/officers` | ADMIN | Listar todos los Officers |
| PUT | `/api/auth/officers/{id}` | ADMIN | Editar datos de un Officer |
| PATCH | `/api/auth/officers/{id}/toggle` | ADMIN | Activar o desactivar Officer |
| PUT | `/api/auth/officers/{id}/reset-password` | ADMIN | Resetear contraseña de Officer |

**Login — Request:**
```json
{
  "email": "admin@credidrivep.com",
  "password": "Admin123!"
}
```

**Crear Officer — Request:**
```json
{
  "name": "María García",
  "email": "maria@credidrivep.com",
  "password": "Officer123!"
}
```

---

### 👥 Clientes — `/api/clients`

| Método | Ruta | Rol | Descripción |
|---|---|---|---|
| GET | `/api/clients` | Todos | Listar clientes activos |
| GET | `/api/clients/{id}` | Todos | Detalle de cliente |
| POST | `/api/clients` | Todos | Registrar nuevo cliente |
| PUT | `/api/clients/{id}` | Todos | Editar cliente |
| DELETE | `/api/clients/{id}` | Todos | Desactivar cliente (soft delete) |

**Crear Cliente — Request:**
```json
{
  "dni": "12345678",
  "firstName": "Juan",
  "lastName": "Pérez",
  "birthDate": "1990-05-15",
  "phone": "987654321",
  "monthlyIncome": 5000.00,
  "creditScore": 750
}
```

---

### 🚗 Vehículos — `/api/vehicles`

| Método | Ruta | Rol | Descripción |
|---|---|---|---|
| GET | `/api/vehicles` | Todos | Listar catálogo (filtros: `?status=AVAILABLE&brand=Toyota`) |
| GET | `/api/vehicles/{id}` | Todos | Detalle de vehículo |
| POST | `/api/vehicles` | Todos | Registrar vehículo |
| PUT | `/api/vehicles/{id}` | Todos | Editar vehículo |
| DELETE | `/api/vehicles/{id}` | Todos | Desactivar vehículo |

**Filtros disponibles:**
- `?status=AVAILABLE` — solo disponibles
- `?status=SOLD` — solo vendidos
- `?brand=Toyota` — por marca

**Crear Vehículo — Request:**
```json
{
  "brand": "Toyota",
  "model": "Hilux",
  "year": 2024,
  "condition": "NEW",
  "price": 45000.00,
  "priceCurrency": "PEN",
  "vin": "1HGBH41JXMN109186",
  "imageUrl": "https://example.com/toyota-hilux.jpg",
  "stock": 12
}
```

**Estados de vehículo:** `AVAILABLE` / `SOLD` / `RESERVED`

**Condición:** `NEW` / `USED`

---

### 🧮 Simulaciones — `/api/simulations`

El módulo más importante. Calcula cronograma francés, VAN, TIR y TCEA automáticamente.

| Método | Ruta | Rol | Descripción |
|---|---|---|---|
| GET | `/api/simulations` | Todos | Historial (Officer ve solo las suyas) |
| GET | `/api/simulations/{id}` | Todos | Detalle completo con cronograma e indicadores |
| POST | `/api/simulations` | Todos | Crear y calcular simulación |
| DELETE | `/api/simulations/{id}` | Todos | Eliminar simulación guardada |
| POST | `/api/simulations/{id}/convert` | Todos | Convertir simulación a préstamo formal |

**Crear Simulación — Request:**
```json
{
  "clientId": 1,
  "vehicleId": 1,
  "name": "SIM-Toyota-Hilux-JuanPerez",
  "currency": "PEN",
  "downPayment": 9000.00,
  "rateType": "TEA",
  "interestRate": 0.12,
  "termMonths": 24,
  "graceType": "NONE",
  "graceMonths": 0,
  "paymentMethod": "FRENCH",
  "startDate": "2024-01-01",
  "cokAnnual": 0.10,
  "rateDesgravamen": 0.00035,
  "rateVehicular": 0.00025,
  "insuranceBaseDesgrv": "SALDO_INSOLUTO",
  "insuranceBaseVehic": "VALOR_VEHICULO",
  "commissionMonthly": 15.00
}
```

**Parámetros:**

| Campo | Valores posibles |
|---|---|
| `currency` | `PEN` / `USD` |
| `rateType` | `TEA` / `TNA` |
| `capitalization` | `DAILY` / `MONTHLY` / `QUARTERLY` / `SEMIANNUAL` / `ANNUAL` (solo si TNA) |
| `graceType` | `NONE` / `TOTAL` / `PARTIAL` |
| `paymentMethod` | `FRENCH` / `FRENCH_BALLOON` |

**Respuesta incluye:**
- Cronograma cuota por cuota (interés, amortización, seguros, comisiones, balloon)
- `VAN` — Valor Actual Neto desde perspectiva del deudor
- `TIR` mensual y anual
- `TCEA` — Tasa de Costo Efectivo Anual

---

### 💳 Préstamos — `/api/loans`

| Método | Ruta | Rol | Descripción |
|---|---|---|---|
| GET | `/api/loans` | Todos | Listar préstamos (Officer ve solo los suyos) |
| GET | `/api/loans/{id}` | Todos | Detalle con cronograma e indicadores |
| PUT | `/api/loans/{id}/status` | ADMIN / Todos | Cambiar estado del préstamo |

**Cambiar Estado — Request:**
```json
{
  "status": "APPROVED",
  "reason": "Cliente aprobado por historial crediticio."
}
```

**Flujo de estados:**

```
PENDING_APPROVAL → APPROVED (solo ADMIN)
PENDING_APPROVAL → REJECTED (solo ADMIN)
APPROVED         → ACTIVE   (solo ADMIN)
ACTIVE           → PAID
ACTIVE           → CANCELLED
PENDING_APPROVAL → CANCELLED
```

---

### 📊 Dashboard — `/api/dashboard`

| Método | Ruta | Rol | Descripción |
|---|---|---|---|
| GET | `/api/dashboard/summary` | Todos | Totales y actividad reciente |

**Respuesta:**
```json
{
  "totalClients": 124,
  "totalVehicles": 45,
  "totalSimulations": 312,
  "pendingLoans": 12,
  "approvedLoans": 45,
  "recentActivity": [
    {
      "type": "CLIENT",
      "description": "Nuevo cliente: Juan Pérez",
      "createdAt": "2024-01-15T10:30:00Z"
    }
  ]
}
```

---

### ⚙️ Seguros — `/api/insurances`

| Método | Ruta | Rol | Descripción |
|---|---|---|---|
| GET | `/api/insurances` | Todos | Listar seguros |
| GET | `/api/insurances/{id}` | Todos | Detalle de seguro |
| POST | `/api/insurances` | ADMIN | Crear seguro |
| PUT | `/api/insurances/{id}` | ADMIN | Editar seguro |
| PATCH | `/api/insurances/{id}/toggle` | ADMIN | Activar / desactivar |

**Crear Seguro — Request:**
```json
{
  "name": "Seguro Desgravamen BCP",
  "type": "DESGRAVAMEN",
  "rate": 0.00035,
  "base": "SALDO_INSOLUTO",
  "isMandatory": true
}
```

**Tipos:** `DESGRAVAMEN` / `VEHICULAR` / `OTHER`

**Base de cálculo:** `SALDO_INSOLUTO` / `VALOR_VEHICULO` / `MONTO_PRESTAMO`

---

### ⚙️ Comisiones — `/api/commissions`

| Método | Ruta | Rol | Descripción |
|---|---|---|---|
| GET | `/api/commissions` | Todos | Listar comisiones |
| GET | `/api/commissions/{id}` | Todos | Detalle de comisión |
| POST | `/api/commissions` | ADMIN | Crear comisión |
| PUT | `/api/commissions/{id}` | ADMIN | Editar comisión |
| PATCH | `/api/commissions/{id}/toggle` | ADMIN | Activar / desactivar |

**Crear Comisión — Request:**
```json
{
  "concept": "Comisión de gestión mensual",
  "amount": 15.00,
  "periodicity": "MONTHLY"
}
```

**Periodicidad:** `MONTHLY` / `ONE_TIME` / `ANNUAL`

---

### ⚙️ Planes de Crédito — `/api/loan-plans`

| Método | Ruta | Rol | Descripción |
|---|---|---|---|
| GET | `/api/loan-plans` | Todos | Listar planes con seguros y comisiones |
| GET | `/api/loan-plans/{id}` | Todos | Detalle de plan |
| POST | `/api/loan-plans` | ADMIN | Crear plan |
| PUT | `/api/loan-plans/{id}` | ADMIN | Editar plan |
| PATCH | `/api/loan-plans/{id}/toggle` | ADMIN | Activar / desactivar |

**Crear Plan — Request:**
```json
{
  "name": "Plan Estándar 24 meses PEN",
  "currency": "PEN",
  "rateType": "TEA",
  "interestRate": 0.12,
  "termMonths": 24,
  "graceType": "NONE",
  "graceMonths": 0,
  "paymentMethod": "FRENCH",
  "cokAnnual": 0.10,
  "insuranceIds": [1],
  "commissionIds": [1]
}
```

---

## Roles y Permisos

| Acción | ADMIN | OFFICER |
|---|---|---|
| Login | ✅ | ✅ |
| Gestionar Officers | ✅ | ❌ |
| Registrar clientes | ✅ | ✅ |
| Registrar vehículos | ✅ | ✅ |
| Crear simulaciones | ✅ | ✅ |
| Ver simulaciones propias | ✅ | ✅ |
| Ver todas las simulaciones | ✅ | ❌ |
| Convertir simulación a préstamo | ✅ | ✅ |
| Aprobar / Rechazar préstamos | ✅ | ❌ |
| Configurar seguros y comisiones | ✅ | ❌ |
| Ver dashboard | ✅ | ✅ |

---

## Motor Financiero

El sistema implementa el **método francés vencido ordinario** (meses de 30 días) según normativa peruana.

### Fórmulas implementadas

**Conversión TNA → TEA:**
```
TEA = (1 + TNA/m)^m - 1
```
donde `m` es la frecuencia de capitalización.

**Tasa Mensual Efectiva:**
```
TEM = (1 + TEA)^(1/12) - 1
```

**Cuota Francesa:**
```
C = PV × TEM / (1 - (1 + TEM)^-n)
```

**VAN (perspectiva deudor):**
```
VAN = PV - Σ [Cuota_t / (1 + COK_mensual)^t]
```

**TIR:** Calculada por Newton-Raphson sobre los flujos totales.

**TCEA:** Equivalente a la TIR anual del flujo total (incluye seguros y comisiones).

### Periodos de gracia
- **Total:** El interés se capitaliza al saldo, no se cobra
- **Parcial:** Solo se paga interés, no amortiza capital

---

## Estructura del Proyecto

```
CrediDriveP.API/
├── Controllers/         ← Endpoints HTTP
├── Data/                ← DbContext + Migraciones
├── DTOs/                ← Objetos de transferencia
│   ├── Auth/
│   ├── Client/
│   ├── Commission/
│   ├── Dashboard/
│   ├── Insurance/
│   ├── Loan/
│   ├── LoanPlan/
│   └── Simulation/
├── Helpers/             ← Motor financiero (VAN, TIR, TCEA)
├── Interfaces/          ← Contratos de servicios
├── Models/              ← Entidades EF Core
├── Services/            ← Lógica de negocio
└── Program.cs           ← Configuración y DI
```

---

## Variables de Entorno (Producción)

Para producción se recomienda usar variables de entorno en lugar de `appsettings.json`:

```bash
export ConnectionStrings__DefaultConnection="Server=...;Database=credidrivep;User=...;Password=...;"
export Jwt__Key="tu_clave_secreta_larga"
export Jwt__Issuer="CrediDriveP"
export Jwt__Audience="CrediDrivePUsers"
export Jwt__ExpiresInHours="8"
```

---

## Equipo

Proyecto desarrollado para el curso de Ingeniería Financiera — Grupo 4.

**Repositorio:** https://github.com/1ASI0642-2610-7024-G4-CrediDriveP/CrediDriveP---Backend