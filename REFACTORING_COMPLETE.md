# 📋 REFACTORIZACIÓN COMPLETA - Maple Fighters (Unity Client)

---

## 🎯 Objetivos de la Refactorización

1. Aplicar principios **SOLID**
2. Implementar **Clean Architecture**
3. Usar **Patrones de Diseño** apropiados
4. Implementar **Persistencia** desacoplada
5. **Evitar sobreingeniería** - mantener simplicidad

---

## 📁 ESTRUCTURA DE CARPETAS CREADAS

```
src/maple-fighters/Assets/Maple Fighters/Scripts/
├── Core/                                    # 🆕 NUEVA - Capa de infraestructura core
│   ├── Domain/                              # 🆕 Capa de dominio (interfaces)
│   │   └── Interfaces/                      # 🆕 Contratos/abstracciones
│   │       ├── ISaveService.cs              # Persistencia
│   │       ├── IApiProvider.cs              # Factory de APIs
│   │       ├── INetworkConfiguration.cs     # Configuración de red
│   │       ├── IInputService.cs             # Abstracción de input
│   │       ├── IAuthenticationValidator.cs  # Validación de autenticación
│   │       ├── IEntityRepository.cs         # Repository de entidades
│   │       ├── IEntityFactory.cs            # Factory de entidades
│   │       ├── IGameEntity.cs               # Entidad base del juego
│   │       ├── IPlayerCredentials.cs        # 🆕 Datos de credenciales de jugador
│   │       ├── IPlayerRepository.cs         # 🆕 Repository de jugadores
│   │       ├── ICredentialValidator.cs      # 🆕 Validación de credenciales
│   │       ├── ILoginAttemptTracker.cs      # 🆕 Tracking de intentos de login
│   │       └── IAdminService.cs             # 🆕 Servicios de administración
│   │
│   └── Infrastructure/                      # 🆕 Implementaciones concretas
│       ├── ServiceLocator.cs                # Localizador de servicios
│       ├── ServiceLocatorInitializer.cs     # Inicializador (MonoBehaviour)
│       ├── Persistence/                     # 🆕 Servicios de persistencia
│       │   ├── JsonSaveService.cs           # Implementación JSON
│       │   └── PlayerPrefsSaveService.cs    # Implementación PlayerPrefs
│       ├── Configuration/                   # 🆕 Adaptadores de configuración
│       │   └── NetworkConfigurationAdapter.cs
│       ├── Services/                        # 🆕 Servicios de aplicación
│       │   ├── ApiProviderService.cs        # Implementación IApiProvider
│       │   ├── UnityInputService.cs         # Implementación IInputService
│       │   ├── CredentialValidator.cs       # 🆕 Validación de credenciales
│       │   ├── LoginAttemptTracker.cs       # 🆕 Tracking de intentos
│       │   └── AdminService.cs              # 🆕 Servicio de administración
│       ├── Repositories/                    # 🆕 Repositorios
│       │   ├── EntityRepository.cs          # Implementación IEntityRepository
│       │   └── PlayerRepository.cs          # 🆕 Implementación IPlayerRepository
│       └── Factories/                       # 🆕 Fábricas
│           └── EntityFactory.cs             # Implementación IEntityFactory
│
├── Services/
│   └── PlayerLoginApi/                      # 🆕 NUEVA - API de login de jugadores
│       ├── IPlayerLoginApi.cs               # Interface de la API
│       ├── DummyPlayerLoginApi.cs           # Implementación local/desarrollo
│       ├── PlayerLoginSettings.cs           # Configuración del sistema
│       └── Data/
│           └── PlayerCredentialsData.cs     # Estructura de datos
│
├── Gameplay/
│   └── Player/
│       └── Components/                      # 🆕 Componentes extraídos
│           ├── GroundDetector.cs            # Detección de suelo
│           └── PlayerEffects.cs             # Efectos visuales
│
└── UI/
    ├── Authenticator/
    │   ├── View/                            # 🆕 Interfaces de vista
    │   │   └── ILoginView.cs
    │   └── Presenter/                       # 🆕 Presentadores MVP
    │       ├── LoginPresenter.cs
    │       └── RegistrationPresenter.cs
    │
    └── PlayerLogin/                         # 🆕 NUEVA - Sistema de login por nombre
        ├── Controller/
        │   ├── PlayerLoginController.cs     # Controlador principal
        │   └── PlayerLoginIntegration.cs    # Integración con sistema existente
        ├── View/
        │   ├── IPlayerLoginView.cs          # Interface de la vista
        │   └── PlayerLoginState.cs          # Estados del login
        ├── Presenter/
        │   └── PlayerLoginPresenter.cs      # Lógica de presentación (MVP)
        ├── Window/
        │   └── PlayerLoginWindow.cs         # Ventana UI
        └── Admin/
            └── AdminPanel.cs                # Panel de administración
```

---

## 🔷 PRINCIPIOS SOLID APLICADOS

### 1. **S - Single Responsibility Principle (SRP)**

| Clase Original | Problema | Solución | Nuevas Clases |
|----------------|----------|----------|---------------|
| `PlayerController` | Manejaba input, física, efectos, estado, detección de suelo | Extraer responsabilidades a componentes separados | `GroundDetector`, `PlayerEffects`, `IInputService` |
| `AuthenticatorController` | Mezclaba validación, UI, navegación, persistencia | Separar en capas MVP | `LoginPresenter`, `RegistrationPresenter`, `IAuthenticationValidator` |
| `EntityContainer` | Creaba, almacenaba y destruía entidades | Separar creación y almacenamiento | `IEntityRepository`, `IEntityFactory` |
| `PlayerLoginController` | 🆕 Solo coordina flujo de login | Separar validación, tracking, UI | `ICredentialValidator`, `ILoginAttemptTracker`, `PlayerLoginPresenter` |

**Ejemplo concreto - PlayerController:**
```csharp
// ANTES: PlayerController hacía todo
private void CheckGround() { /* lógica de detección */ }
private void CreateDustEffect() { /* lógica de efectos */ }
private void HandleInput() { /* lógica de input */ }

// DESPUÉS: Responsabilidades separadas
private GroundDetector groundDetector;      // Solo detecta suelo
private PlayerEffects playerEffects;         // Solo maneja efectos
private IInputService inputService;          // Solo lee input
```

---

### 2. **O - Open/Closed Principle (OCP)**

| Implementación | Descripción |
|----------------|-------------|
| `ISaveService` | Abierto para extensión (nuevas implementaciones), cerrado para modificación |
| `IInputService` | Permite agregar `MobileInputService`, `AIInputService` sin modificar código existente |
| `IEntityFactory` | Nuevos tipos de entidades sin cambiar el factory existente |

**Ejemplo - Persistencia extensible:**
```csharp
// Podemos agregar nuevas implementaciones sin modificar código existente
public interface ISaveService { /* ... */ }

public class JsonSaveService : ISaveService { /* ... */ }      // Implementación 1
public class PlayerPrefsSaveService : ISaveService { /* ... */ } // Implementación 2
// public class CloudSaveService : ISaveService { /* ... */ }   // Futura implementación
```

---

### 3. **L - Liskov Substitution Principle (LSP)**

| Interface | Implementaciones Intercambiables |
|-----------|----------------------------------|
| `ISaveService` | `JsonSaveService` ↔ `PlayerPrefsSaveService` |
| `IInputService` | `UnityInputService` (intercambiable con futuras) |
| `IGameEntity` | `IEntity` hereda y es compatible |

**Ejemplo:**
```csharp
// Cualquier implementación de ISaveService funciona igual
ISaveService saveService = new JsonSaveService();
// O
ISaveService saveService = new PlayerPrefsSaveService();

// El código cliente no sabe ni le importa cuál es
saveService.SetString("key", "value");
saveService.Save();
```

---

### 4. **I - Interface Segregation Principle (ISP)**

| Interface | Responsabilidad Específica |
|-----------|---------------------------|
| `ISaveService` | Solo operaciones de persistencia |
| `IInputService` | Solo lectura de input |
| `IEntityRepository` | Solo almacenamiento de entidades |
| `IEntityFactory` | Solo creación/destrucción de entidades |
| `IAuthenticationValidator` | Solo validación de datos |
| `ILoginView` | Solo contrato de vista de login |

**Ejemplo - Interfaces segregadas:**
```csharp
// NO tenemos una interfaz "IEverything" gigante
// Cada interfaz tiene un propósito específico

public interface IEntityRepository
{
    IGameEntity GetLocalEntity();
    bool TryGetEntity(int id, out IGameEntity entity);
    void AddEntity(int id, IGameEntity entity);
    bool RemoveEntity(int id);
    // Solo métodos de almacenamiento
}

public interface IEntityFactory
{
    IGameEntity CreateEntity(string name, Vector2 position);
    void DestroyEntity(IGameEntity entity);
    // Solo métodos de creación/destrucción
}
```

---

### 5. **D - Dependency Inversion Principle (DIP)**

| Capa Alta | Depende de | No de |
|-----------|------------|-------|
| `AuthenticatorController` | `IAuthenticationValidator` | `AuthenticationValidator` (concreto) |
| `EntityContainer` | `IEntityRepository`, `IEntityFactory` | Implementaciones concretas |
| `PlayerController` | `IInputService` | `UnityInputService` (concreto) |
| `ApiProvider` | `IApiProvider` | `ApiProviderService` (concreto) |

**Ejemplo:**
```csharp
// ANTES: Dependencia directa a implementación concreta
private AuthenticationValidator validator = new AuthenticationValidator();

// DESPUÉS: Dependencia a abstracción
private IAuthenticationValidator validator;

private void Awake()
{
    // Obtener del ServiceLocator (inversión de control)
    ServiceLocator.TryGet(out validator);
}
```

---

## 🎨 PATRONES DE DISEÑO IMPLEMENTADOS

### 1. **Service Locator Pattern**

**Ubicación:** `Core/Infrastructure/ServiceLocator.cs`

**Propósito:** Proveer un punto central para obtener servicios sin acoplar clases a implementaciones concretas. Alternativa ligera a Dependency Injection containers como Zenject.

```csharp
public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> services = new();
    
    public static void Register<T>(T service) where T : class
    public static T Get<T>() where T : class
    public static bool TryGet<T>(out T service) where T : class
}
```

**Uso:**
```csharp
// Registro (en ServiceLocatorInitializer)
ServiceLocator.Register<ISaveService>(new JsonSaveService());

// Obtención (en cualquier clase)
var saveService = ServiceLocator.Get<ISaveService>();
```

**¿Por qué no Zenject?** El proyecto es pequeño; ServiceLocator es suficiente y evita sobreingeniería.

---

### 2. **Strategy Pattern**

**Ubicaciones:**
- `ISaveService` → `JsonSaveService`, `PlayerPrefsSaveService`
- `IInputService` → `UnityInputService`

**Propósito:** Permitir intercambiar algoritmos/comportamientos en tiempo de ejecución.

```csharp
// La estrategia de persistencia se puede cambiar
public enum PersistenceType { Json, PlayerPrefs }

private void RegisterSaveService()
{
    ISaveService saveService = persistenceType switch
    {
        PersistenceType.Json => new JsonSaveService(),
        PersistenceType.PlayerPrefs => new PlayerPrefsSaveService(),
        _ => new JsonSaveService()
    };
    ServiceLocator.Register<ISaveService>(saveService);
}
```

---

### 3. **Factory Pattern**

**Ubicaciones:**
- `IApiProvider` / `ApiProviderService` - Factory para APIs
- `IEntityFactory` / `EntityFactory` - Factory para entidades del juego

**Propósito:** Encapsular la lógica de creación de objetos complejos.

```csharp
public interface IEntityFactory
{
    IGameEntity CreateEntity(string name, Vector2 position);
    void DestroyEntity(IGameEntity entity);
}

public class EntityFactory : IEntityFactory
{
    public IGameEntity CreateEntity(string name, Vector2 position)
    {
        var path = string.Format(Paths.Resources.Game.Entities, name);
        var prefab = Resources.Load(path);
        var gameObject = Object.Instantiate(prefab, position, Quaternion.identity);
        // ... configuración
        return entity;
    }
}
```

---

### 4. **Repository Pattern**

**Ubicación:** `IEntityRepository` / `EntityRepository`

**Propósito:** Abstraer el acceso a colecciones de entidades, permitiendo cambiar la implementación sin afectar al código cliente.

```csharp
public interface IEntityRepository
{
    IGameEntity GetLocalEntity();
    bool TryGetEntity(int id, out IGameEntity entity);
    void AddEntity(int id, IGameEntity entity);
    bool RemoveEntity(int id);
    int Count { get; }
    void Clear();
}
```

**Beneficios:**
- Centraliza la gestión de entidades
- Elimina la necesidad de `FindObjectOfType<EntityContainer>()`
- Facilita testing con mocks

---

### 5. **Adapter Pattern**

**Ubicación:** `NetworkConfigurationAdapter`

**Propósito:** Adaptar la interfaz de `NetworkConfiguration` (ScriptableObject) a `INetworkConfiguration`.

```csharp
public class NetworkConfigurationAdapter : INetworkConfiguration
{
    private readonly NetworkConfiguration config;

    public NetworkConfigurationAdapter()
    {
        config = Resources.Load<NetworkConfiguration>("NetworkConfiguration");
    }

    public string GetProtocol() => config?.Protocol ?? "ws";
    public string GetHost() => config?.Host ?? "localhost";
    public bool IsProduction() => config?.IsProduction ?? false;
}
```

---

### 6. **MVP Pattern (Model-View-Presenter)**

**Ubicación:** `UI/Authenticator/`

**Propósito:** Separar la lógica de presentación de la vista en UI.

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│      View       │◄────│    Presenter    │────►│      Model      │
│  (LoginWindow)  │     │ (LoginPresenter)│     │ (ISaveService)  │
│                 │     │                 │     │                 │
│ - Muestra UI    │     │ - Valida datos  │     │ - Guarda email  │
│ - Captura input │     │ - Coordina      │     │                 │
│ - Eventos       │     │ - Lógica UI     │     │                 │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

**Componentes:**
- `ILoginView` - Contrato de la vista
- `LoginWindow : UIElement, ILoginView` - Implementación de la vista
- `LoginPresenter` - Lógica de presentación
- `IAuthenticationValidator` - Validación (parte del modelo)

---

### 7. **State Pattern (Ya existente)**

**Ubicación:** `Gameplay/Player/Controller/PlayerController.cs`

**Nota:** Este patrón ya estaba implementado en el proyecto original con `IPlayerStateBehaviour`. No se modificó, solo se respetó.

---

## 📦 DETALLE DE CADA FASE

### FASE 1: Fundamentos y Persistencia

**Commit:** `859c40b71`

**Archivos creados:**
| Archivo | Propósito |
|---------|-----------|
| `ISaveService.cs` | Interface para abstracción de persistencia |
| `JsonSaveService.cs` | Guarda datos en JSON en `Application.persistentDataPath` |
| `PlayerPrefsSaveService.cs` | Usa PlayerPrefs de Unity (fallback) |
| `ServiceLocator.cs` | Registro central de servicios |
| `ServiceLocatorInitializer.cs` | MonoBehaviour que inicializa servicios |

**Archivos modificados:**
| Archivo | Cambio |
|---------|--------|
| `AuthenticatorController.cs` | Usa `ISaveService` en lugar de `PlayerPrefs` directo |

**Principios aplicados:** SRP, DIP, OCP
**Patrones aplicados:** Service Locator, Strategy

---

### FASE 2: Desacoplar APIs y Providers

**Commit:** `b51b31e7e`

**Archivos creados:**
| Archivo | Propósito |
|---------|-----------|
| `IApiProvider.cs` | Interface para factory de APIs |
| `INetworkConfiguration.cs` | Interface para configuración de red |
| `NetworkConfigurationAdapter.cs` | Adapta ScriptableObject a interface |
| `ApiProviderService.cs` | Implementación de IApiProvider |

**Archivos modificados:**
| Archivo | Cambio |
|---------|--------|
| `ApiProvider.cs` | Delega a ServiceLocator si está disponible |

**Principios aplicados:** DIP, OCP, ISP
**Patrones aplicados:** Factory, Adapter

---

### FASE 3: Refactorizar PlayerController

**Commit:** `222ee698c`

**Archivos creados:**
| Archivo | Propósito |
|---------|-----------|
| `IInputService.cs` | Interface para abstracción de input |
| `UnityInputService.cs` | Implementación usando Unity Input |
| `GroundDetector.cs` | Componente para detectar suelo |
| `PlayerEffects.cs` | Componente para efectos visuales |

**Archivos modificados:**
| Archivo | Cambio |
|---------|--------|
| `PlayerController.cs` | Usa componentes opcionales, IInputService |

**Principios aplicados:** SRP, DIP, OCP
**Patrones aplicados:** Strategy, Component (Unity)

**Nota importante:** Los componentes son **opcionales** para mantener compatibilidad hacia atrás.

---

### FASE 4: UI con Patrón MVP

**Commit:** `6573d1dcc`

**Archivos creados:**
| Archivo | Propósito |
|---------|-----------|
| `IAuthenticationValidator.cs` | Interface para validación |
| `ILoginView.cs` | Interface para vista de login |
| `LoginPresenter.cs` | Presenter para login (MVP) |
| `RegistrationPresenter.cs` | Presenter para registro (MVP) |

**Archivos modificados:**
| Archivo | Cambio |
|---------|--------|
| `AuthenticationValidator.cs` | Implementa `IAuthenticationValidator`, métodos consolidados |
| `AuthenticatorController.cs` | Usa validador del ServiceLocator, métodos simplificados |
| `LoginWindow.cs` | Implementa `ILoginView` correctamente |
| `ServiceLocatorInitializer.cs` | Registra `IAuthenticationValidator` |

**Principios aplicados:** SRP, DIP, ISP
**Patrones aplicados:** MVP, Strategy

---

### FASE 5: EntityContainer con Repository Pattern

**Commit:** `e2a4763ef`

**Archivos creados:**
| Archivo | Propósito |
|---------|-----------|
| `IEntityRepository.cs` | Interface para repositorio de entidades |
| `IGameEntity.cs` | Interface base para entidades (en Domain) |
| `IEntityFactory.cs` | Interface para factory de entidades |
| `EntityRepository.cs` | Implementación del repositorio |
| `EntityFactory.cs` | Implementación del factory |

**Archivos modificados:**
| Archivo | Cambio |
|---------|--------|
| `IEntity.cs` | Ahora hereda de `IGameEntity` |
| `EntityContainer.cs` | Usa `IEntityRepository` e `IEntityFactory` con fallback |
| `ServiceLocatorInitializer.cs` | Registra nuevos servicios |

**Principios aplicados:** SRP, DIP, ISP, OCP
**Patrones aplicados:** Repository, Factory

---

## 🔄 COMPATIBILIDAD HACIA ATRÁS

Todas las refactorizaciones mantienen **compatibilidad hacia atrás**:

```csharp
// Ejemplo en EntityContainer
private void Awake()
{
    ServiceLocator.TryGet(out entityRepository);
    ServiceLocator.TryGet(out entityFactory);
    
    // Fallback si no hay repositorio
    if (entityRepository == null)
    {
        localCollection = new Dictionary<int, IEntity>();
        Debug.Log("[EntityContainer] Using local collection (fallback mode)");
    }
}
```

**Beneficios:**
- El juego funciona aunque no esté configurado el ServiceLocator
- Migración gradual posible
- No rompe funcionalidad existente

---

## 📊 RESUMEN DE SERVICIOS REGISTRADOS

| Servicio | Interface | Implementación | Propósito |
|----------|-----------|----------------|-----------|
| Persistencia | `ISaveService` | `JsonSaveService` / `PlayerPrefsSaveService` | Guardar/cargar datos |
| Input | `IInputService` | `UnityInputService` | Leer input del jugador |
| Configuración | `INetworkConfiguration` | `NetworkConfigurationAdapter` | Configuración de red |
| APIs | `IApiProvider` | `ApiProviderService` | Factory de APIs |
| Validación Auth | `IAuthenticationValidator` | `AuthenticationValidator` | Validar datos de auth |
| Entidades | `IEntityRepository` | `EntityRepository` | Almacenar entidades |
| Factory | `IEntityFactory` | `EntityFactory` | Crear/destruir entidades |
| Sesión | `IUserSession` | `UserMetadata` | Datos del usuario/personaje |
| Jugadores | `IPlayerRepository` | `PlayerRepository` | 🆕 Almacenar jugadores |
| Credenciales | `ICredentialValidator` | `CredentialValidator` | 🆕 Validar login |
| Intentos | `ILoginAttemptTracker` | `LoginAttemptTracker` | 🆕 Tracking de intentos |
| Admin | `IAdminService` | `AdminService` | 🆕 Operaciones admin |

---

## 🆕 FASE 6: Mejoras Adicionales de Persistencia y Sesión

### Archivos creados:
| Archivo | Propósito |
|---------|-----------|
| `IUserSession.cs` | Interface para la sesión del usuario |

### Archivos refactorizados:

#### 1. `UserMetadata.cs`
- ✅ Implementa `IUserSession`
- ✅ Usa `ISaveService` para persistir el `userId`
- ✅ Se registra automáticamente en ServiceLocator
- ✅ Mantiene fallback a PlayerPrefs

#### 2. `DummyCharacterProviderApi.cs`
- ✅ Usa `ISaveService` para guardar/cargar personajes
- ✅ Usa `IUserSession` del ServiceLocator para obtener userId
- ✅ Mantiene fallback a PlayerPrefs y FindObjectOfType

#### 3. `PlayerDataController.cs`
- ✅ Usa `IUserSession` del ServiceLocator
- ✅ Elimina dependencia directa a `UserMetadata`
- ✅ Mantiene fallback a FindObjectOfType

### Principios aplicados:
- **DIP**: Depende de `IUserSession`, no de `UserMetadata`
- **OCP**: `IUserSession` permite nuevas implementaciones
- **ISP**: Interface específica para sesión de usuario

---

## 🆕 FASE 7: Sistema de Login por Nombre de Jugador

### Objetivo
Implementar un sistema de autenticación simplificado basado en nombre de jugador:
- Usuario ingresa **nombre de personaje**
- Si existe → pide **contraseña** → entra al juego
- Si no existe → crea nuevo jugador con contraseña por defecto
- Sistema de **bloqueo por intentos fallidos** (máx. 3)
- **Panel de administrador** para desbloquear usuarios

### Arquitectura Implementada

```
┌─────────────────────────────────────────────────────────────────┐
│                     CAPA DE PRESENTACIÓN (UI)                   │
├─────────────────────────────────────────────────────────────────┤
│  PlayerLoginWindow    │  AdminPanel     │  PlayerLoginController │
│  (Vista - UIElement)  │  (OnGUI Admin)  │  (Coordinador)         │
└──────────┬────────────┴────────┬────────┴──────────┬────────────┘
           │                     │                    │
           ▼                     ▼                    ▼
┌─────────────────────────────────────────────────────────────────┐
│                     CAPA DE APLICACIÓN                          │
├─────────────────────────────────────────────────────────────────┤
│  PlayerLoginPresenter  │  PlayerLoginIntegration                 │
│  (Lógica MVP)          │  (Integración con CharacterView)        │
└──────────┬─────────────┴────────────────────┬───────────────────┘
           │                                   │
           ▼                                   ▼
┌─────────────────────────────────────────────────────────────────┐
│                     CAPA DE SERVICIOS (API)                     │
├─────────────────────────────────────────────────────────────────┤
│  IPlayerLoginApi       │  DummyPlayerLoginApi                    │
│  (Interface)           │  (Implementación local)                 │
└──────────┬─────────────┴────────────────────┬───────────────────┘
           │                                   │
           ▼                                   ▼
┌─────────────────────────────────────────────────────────────────┐
│                     CAPA DE DOMINIO                             │
├─────────────────────────────────────────────────────────────────┤
│  IPlayerRepository     │  ICredentialValidator  │  IAdminService │
│  ILoginAttemptTracker  │  IPlayerCredentials    │                │
└──────────┬─────────────┴──────────┬────────────┴────────────────┘
           │                        │
           ▼                        ▼
┌─────────────────────────────────────────────────────────────────┐
│                     CAPA DE INFRAESTRUCTURA                     │
├─────────────────────────────────────────────────────────────────┤
│  PlayerRepository      │  CredentialValidator   │  AdminService  │
│  LoginAttemptTracker   │  (usa ISaveService)    │                │
└─────────────────────────────────────────────────────────────────┘
```

### Flujo de Estados del Login

```
┌─────────────────┐
│   ENTER_NAME    │ ◄── Estado inicial
│ (Ingresa nombre)│
└────────┬────────┘
         │ Usuario confirma nombre
         ▼
┌─────────────────┐
│ CHECKING_NAME   │
│ (Verificando...)│
└────────┬────────┘
         │
    ┌────┴────┐
    │         │
    ▼         ▼
┌────────┐ ┌──────────────┐
│ EXISTS │ │  NOT_EXISTS  │
└───┬────┘ └──────┬───────┘
    │             │
    ▼             ▼
┌──────────────┐ ┌──────────────────┐
│ENTER_PASSWORD│ │  REGISTERING     │
│(Pide password)│ │ (Nuevo jugador)  │
└──────┬───────┘ └────────┬─────────┘
       │                  │
       ▼                  ▼
  ┌────┴────┐        ┌─────────┐
  │         │        │ SUCCESS │──► Crear personaje
  ▼         ▼        └─────────┘
┌────────┐ ┌────────┐
│SUCCESS │ │ FAILED │
└───┬────┘ └───┬────┘
    │          │
    ▼          ▼
┌────────┐ ┌─────────────┐
│ Entrar │ │ attempts++  │
│ juego  │ │ if >= 3:    │
└────────┘ │   BLOCKED   │
           └─────────────┘
```

### Archivos Creados

#### Capa de Dominio (Interfaces)

| Archivo | Propósito | Principios |
|---------|-----------|------------|
| `IPlayerCredentials.cs` | Define estructura de datos del jugador | ISP |
| `IPlayerRepository.cs` | Contrato para almacenamiento de jugadores | DIP, ISP |
| `ICredentialValidator.cs` | Contrato para validación de credenciales | SRP, DIP |
| `ILoginAttemptTracker.cs` | Contrato para tracking de intentos | SRP, ISP |
| `IAdminService.cs` | Contrato para operaciones administrativas | ISP |

#### Capa de Infraestructura (Implementaciones)

| Archivo | Propósito | Patrón |
|---------|-----------|--------|
| `PlayerRepository.cs` | Almacena jugadores usando `ISaveService` | Repository |
| `CredentialValidator.cs` | Valida nombre y contraseña | Strategy |
| `LoginAttemptTracker.cs` | Gestiona intentos fallidos y bloqueos | - |
| `AdminService.cs` | Operaciones de administrador | Facade |

#### Capa de Servicios (API)

| Archivo | Propósito | Patrón |
|---------|-----------|--------|
| `IPlayerLoginApi.cs` | Interface de la API de login | - |
| `DummyPlayerLoginApi.cs` | Implementación local para desarrollo | Strategy |
| `PlayerLoginSettings.cs` | Constantes de configuración | - |
| `PlayerCredentialsData.cs` | DTO serializable | DTO |

#### Capa de Presentación (UI)

| Archivo | Propósito | Patrón |
|---------|-----------|--------|
| `IPlayerLoginView.cs` | Interface de la vista | MVP |
| `PlayerLoginState.cs` | Enum de estados de UI | State |
| `PlayerLoginWindow.cs` | Implementación de la vista | MVP |
| `PlayerLoginPresenter.cs` | Lógica de presentación | MVP |
| `PlayerLoginController.cs` | Coordinador del flujo | Controller |
| `PlayerLoginIntegration.cs` | Integración con sistema existente | Adapter |
| `AdminPanel.cs` | Panel OnGUI para administrador | - |

### Archivos Modificados

| Archivo | Cambio |
|---------|--------|
| `ServiceLocatorInitializer.cs` | Registra `IPlayerRepository`, `ICredentialValidator`, `ILoginAttemptTracker`, `IAdminService` |
| `ApiProvider.cs` | Agrega método `ProvidePlayerLoginApi()` |
| `CharacterViewController.cs` | Agrega propiedad `showOnStart` y método `ShowCharacterSelection()` |
| `UserMetadata.cs` | Cambio de `UserData` a `UserData?` (nullable) |

### Principios SOLID Aplicados

| Principio | Aplicación en Login System |
|-----------|---------------------------|
| **SRP** | `PlayerLoginController` solo coordina, `CredentialValidator` solo valida, `PlayerRepository` solo almacena |
| **OCP** | `IPlayerLoginApi` permite agregar `HttpPlayerLoginApi` sin modificar código existente |
| **LSP** | `DummyPlayerLoginApi` es intercambiable con futuras implementaciones |
| **ISP** | Interfaces pequeñas: `IPlayerRepository`, `ICredentialValidator`, `ILoginAttemptTracker` |
| **DIP** | Controllers dependen de interfaces (`IPlayerLoginApi`, `ICredentialValidator`), no de implementaciones |

### Patrones de Diseño Aplicados

| Patrón | Uso en Login System |
|--------|---------------------|
| **MVP** | `PlayerLoginWindow` (View) + `PlayerLoginPresenter` (Presenter) + Servicios (Model) |
| **Repository** | `IPlayerRepository` / `PlayerRepository` para gestión de jugadores |
| **Strategy** | `ICredentialValidator` permite diferentes estrategias de validación |
| **State** | `PlayerLoginState` enum para estados de la UI |
| **Factory** | `ApiProvider.ProvidePlayerLoginApi()` para crear instancias de API |
| **Adapter** | `PlayerLoginIntegration` adapta el login al sistema existente |
| **Facade** | `AdminService` simplifica operaciones administrativas |

### Configuración del Sistema

```csharp
public static class PlayerLoginSettings
{
    public const int MaxLoginAttempts = 3;        // Intentos antes de bloqueo
    public const string DefaultPassword = "1234"; // Contraseña por defecto
    public const string AdminPlayerName = "Admin"; // Usuario administrador
    public const int MinPlayerNameLength = 3;     // Longitud mínima nombre
    public const int MaxPlayerNameLength = 20;    // Longitud máxima nombre
}
```

### Servicios Registrados en ServiceLocator

| Servicio | Interface | Implementación |
|----------|-----------|----------------|
| Repositorio de Jugadores | `IPlayerRepository` | `PlayerRepository` |
| Validador de Credenciales | `ICredentialValidator` | `CredentialValidator` |
| Tracker de Intentos | `ILoginAttemptTracker` | `LoginAttemptTracker` |
| Servicio Admin | `IAdminService` | `AdminService` |

### Configuración en Unity

#### Prefab PlayerLoginWindow
El prefab reutiliza el diseño visual de `CharacterNameWindow`:
- Componente `PlayerLoginWindow` (Script)
- Componente `CanvasGroup` (alpha = 0 inicial)
- Componente `UIFadeAnimation` (para transiciones)
- Ubicación: `Resources/UI/PlayerLoginWindow.prefab`

#### GameObject "Player Login System"
```
Player Login System (GameObject)
├── PlayerLoginController (Component)
│   ├── Login Window: (auto-creado desde Resources)
│   └── Show On Start: ✓
│
└── PlayerLoginIntegration (Component)
    ├── Login Controller: → Player Login System
    ├── Character View Controller: → Character View Controller
    ├── Character Selection Scene: "CharacterSelection"
    ├── Use Scene Transition: ☐
    └── Admin Panel: → Admin Panel
```

#### GameObject "Character View Controller"
- **Show On Start**: ☐ (desactivado para esperar login)

### Uso del Sistema

#### Flujo Normal
1. Usuario inicia el juego → aparece ventana de login
2. Ingresa nombre → sistema verifica si existe
3. Si existe → pide contraseña → valida → entra a selección de personajes
4. Si no existe → registra con contraseña "1234" → crea personaje

#### Sistema de Bloqueo
- 3 intentos fallidos consecutivos = jugador bloqueado
- Solo el usuario "Admin" puede desbloquear
- Presionar **F12** abre el panel de administración

#### Usuario Administrador
- Nombre: "Admin" (configurable en `PlayerLoginSettings`)
- Puede ver lista de jugadores bloqueados
- Puede desbloquear jugadores individualmente o todos

---

## 📈 MÉTRICAS FINALES

| Métrica | Antes | Después |
|---------|-------|---------|
| Archivos en `Core/` | 0 | 21 |
| Interfaces creadas | ~5 | ~18 |
| Uso de `FindObjectOfType` | ~15 lugares | ~4 lugares (con fallback) |
| Uso directo de `PlayerPrefs` | ~5 lugares | 0 (todo usa ISaveService con fallback) |
| Patrones implementados | 1 (State) | 10+ |
| Principios SOLID aplicados | Parcial | Todos |

---
