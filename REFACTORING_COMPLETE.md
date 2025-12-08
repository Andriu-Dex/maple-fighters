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
│       │   ├── JsonSaveService.cs           # Implementación JSON (archivos) / PlayerPrefs (WebGL)
│       │   └── PlayerPrefsSaveService.cs    # Implementación PlayerPrefs
│       ├── Configuration/                   # 🆕 Adaptadores de configuración
│       │   └── NetworkConfigurationAdapter.cs
│       ├── Services/                        # 🆕 Servicios de aplicación
│       │   ├── ApiProviderService.cs        # Implementación IApiProvider (siempre usa DummyCharacterProviderApi)
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
    ├── GameMenu/                            # 🆕 NUEVA - Menú del juego
    │   └── GameMenuPanel.cs                 # Panel de menú con logout (ESC)
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
            └── AdminPanel.cs                # Panel de administración (F12)
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
| `JsonSaveService.cs` | Guarda datos en JSON. En WebGL usa PlayerPrefs (IndexedDB), en otras plataformas usa archivos |
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
| `IAdminService.cs` | Contrato para operaciones administrativas (IsAdmin, IsAdminByEmail, Unblock) | ISP |

#### Capa de Infraestructura (Implementaciones)

| Archivo | Propósito | Patrón |
|---------|-----------|--------|
| `PlayerRepository.cs` | Almacena jugadores usando `ISaveService` | Repository |
| `CredentialValidator.cs` | Valida nombre y contraseña | Strategy |
| `LoginAttemptTracker.cs` | Gestiona intentos fallidos y bloqueos | - |
| `AdminService.cs` | Operaciones de administrador (detección por nombre y email) | Facade |

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
| `AdminPanel.cs` | Panel OnGUI para administrador (Singleton, DontDestroyOnLoad) | Singleton |

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
- Solo el usuario administrador puede desbloquear
- Presionar **F12** abre el panel de administración

#### Usuario Administrador
- Detección por nombre: "Admin" (configurable en `PlayerLoginSettings`)
- Detección por email: `admin@gmail.com`, `admin@test.com`, `admin@admin.com`
- Puede ver lista de jugadores bloqueados
- Puede desbloquear jugadores individualmente o todos
- Panel persiste entre escenas usando `DontDestroyOnLoad`

#### Menú del Juego (GameMenuPanel)
- Presionar **ESC** durante el juego abre el menú
- Opciones disponibles:
  - **Continuar**: Cierra el menú y vuelve al juego
  - **Cerrar Sesión**: Limpia la sesión y vuelve a la pantalla de login
- Panel creado automáticamente por `GameSystemsCreator`
- UI escalable según resolución de pantalla

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

## 🆕 FASE 7: Sistema de Login V2 - Vinculación UserId con Cuenta

### 7.1 Problema Identificado

El sistema original tenía un problema crítico: **el `userId` no estaba vinculado a la cuenta del usuario**.

```
┌─────────────────────────────────────────────────────────────────┐
│                     FLUJO ANTERIOR (INCORRECTO)                  │
└─────────────────────────────────────────────────────────────────┘

1. UserMetadata genera userId = "ABC123" al arrancar (aleatorio)
2. Usuario 1 hace login con email "user1@test.com"
3. Crea personaje → Se guarda con userId="ABC123"
4. Usuario 2 hace login con "user2@test.com"
5. CharacterProviderApi busca personajes con userId="ABC123"
6. ¡Usuario 2 VE el personaje de Usuario 1! ← ERROR

RESULTADO: Todos los usuarios en la misma máquina compartían personajes
```

### 7.2 Solución Implementada

```
┌─────────────────────────────────────────────────────────────────┐
│                     FLUJO CORREGIDO                              │
└─────────────────────────────────────────────────────────────────┘

1. Usuario hace login con email "user1@test.com"
2. Sistema obtiene playerData.Id (GUID único de la cuenta)
3. UserMetadata.SetUserIdFromCredentials(playerData.Id)
4. CharacterProviderApi.GetCharacters(userId) filtra por ese ID
5. Solo se muestran personajes de ESE usuario específico
6. Flujo inteligente: sin personaje → crear, con personaje → jugar
```

### 7.3 Archivos Modificados - Fase 7

#### `UserMetadata.cs`
**Cambios:**
- Agregado campo `private string loggedInUserId`
- Nuevo método `SetUserIdFromCredentials(string credentialsId)` - establece el userId desde la cuenta
- Nuevo método `ClearSession()` - limpia todos los datos de sesión al cerrar
- Modificado `GetUserId()` - prioriza el userId de la cuenta logueada

```csharp
// NUEVO: Campo para almacenar userId de la cuenta logueada
private string loggedInUserId;

// NUEVO: Establecer userId desde credenciales de login
public void SetUserIdFromCredentials(string credentialsId)
{
    loggedInUserId = credentialsId;
    UserData = new UserData { id = credentialsId };
    Debug.Log($"[UserMetadata] UserId establecido desde login: {credentialsId}");
}

// NUEVO: Limpiar sesión completa
public void ClearSession()
{
    loggedInUserId = null;
    UserData = default;
    CharacterData = default;
    Debug.Log("[UserMetadata] Sesión limpiada");
}
```

#### `PlayerLoginIntegration.cs`
**Cambios:**
- Modificado `OnLoginSuccessful()` - ahora establece userId y llama a flujo inteligente
- Modificado `Logout()` - ahora llama a `ClearSession()` para limpiar datos
- Integración con `CharacterViewController.ShowCharacterSelectionSmart()`

```csharp
private void OnLoginSuccessful(string email, string playerName, IPlayerCredentials playerData)
{
    // 1. Establecer el userId correcto ANTES de mostrar personajes
    var userMetadata = FindObjectOfType<UserMetadata>();
    if (userMetadata != null && playerData != null)
    {
        userMetadata.SetUserIdFromCredentials(playerData.Id);
        Debug.Log($"[PlayerLoginIntegration] UserMetadata configurado con userId: {playerData.Id}");
    }
    
    // 2. Mostrar selección de personajes con flujo inteligente
    if (characterViewController != null)
    {
        characterViewController.ShowCharacterSelectionSmart(); // ← NUEVO
    }
}
```

#### `DummyCharacterProviderApi.cs`
**Cambios:**
- Modificado `GetCharacters(string userid)` - ahora filtra personajes usando LINQ
- Solo devuelve personajes donde `character.userid == userid`

```csharp
public void GetCharacters(string userid)
{
    LoadCharactersFromStorage();
    
    // FILTRAR: Solo personajes de este usuario específico
    var userCharacters = characterItems
        .Where(c => c.userid == userid)
        .ToArray();
    
    Debug.Log($"[DummyCharacterProviderApi] GetCharacters para userId={userid}: {userCharacters.Length} personajes encontrados");
    
    var filteredCollection = new CharacterDataCollection(userCharacters);
    var filteredJson = filteredCollection.ToString();
    
    GetCharactersCallback?.Invoke((long)StatusCodes.Ok, filteredJson);
}
```

#### `CharacterViewController.cs`
**Cambios principales:**

1. **Nuevo método `ShowCharacterSelectionSmart()`** - Inicia flujo inteligente post-login
2. **Nuevo método `HandleSmartFlow()`** - Decide si crear personaje o ir al juego
3. **Nuevo método `HasAnyCharacter()`** - Verifica si hay personajes válidos (ID > 0, nombre no vacío)
4. **Nuevo método `GoToGameWithFirstCharacter()`** - Auto-selecciona primer personaje y va al juego
5. **Nuevo método `GoToCreateCharacter()`** - Va directo a pantalla de creación
6. **Modificado `Start()`** - Detecta si hay sistema de login presente
7. **Modificado `LoadCharacters()`** - Limpia `characterViewCollection = null` antes de cargar

```csharp
/// <summary>
/// Inicia el flujo inteligente de selección de personajes post-login.
/// Determina automáticamente si el usuario debe crear un personaje o ir al juego.
/// </summary>
public void ShowCharacterSelectionSmart()
{
    useSmartFlow = true;
    CreateAndShowCharacterView();
}

/// <summary>
/// Maneja el flujo inteligente después de recibir los personajes.
/// </summary>
private void HandleSmartFlow()
{
    bool hasCharacter = HasAnyCharacter();
    Debug.Log($"[CharacterViewController] HandleSmartFlow - hasCharacter={hasCharacter}");
    
    if (hasCharacter)
    {
        Debug.Log("[CharacterViewController] Flujo inteligente: Usuario tiene personaje, yendo al juego...");
        GoToGameWithFirstCharacter();
    }
    else
    {
        Debug.Log("[CharacterViewController] Flujo inteligente: Usuario nuevo, yendo a crear personaje...");
        GoToCreateCharacter();
    }
}

/// <summary>
/// Verifica si hay algún personaje VÁLIDO en la colección.
/// Un personaje válido debe tener ID > 0 y nombre no vacío.
/// </summary>
private bool HasAnyCharacter()
{
    if (characterViewCollection == null) return false;
    
    foreach (var character in characterViewCollection.Value.GetAll())
    {
        // Verificar que sea un personaje VÁLIDO (no solo que exista el objeto)
        if (character != null && 
            character.Id > 0 && 
            !string.IsNullOrEmpty(character.CharacterName))
        {
            return true;
        }
    }
    return false;
}
```

### 7.4 Corrección de Errores de Escena

Durante las pruebas se identificaron varios errores al cambiar de escena:

#### Problema 1: "Some objects were not cleaned up when closing the scene"
**Causa:** Singletons estáticos mantenían referencias después de destruirse.
**Solución:** Limpiar `instance = null` en `OnDestroy()` de todos los singletons.

#### Problema 2: "Connection to the game server has been lost"
**Causa:** `DummyGameApi.OnDestroy()` disparaba `Disconnected` con código "Normal", mostrando mensaje innecesario.
**Solución:** `GameServerDisconnectionHandler` ahora ignora desconexión "Normal".

#### Problema 3: MissingReferenceException en EntityIdentifier
**Causa:** `EntityRepository` mantenía referencias a GameObjects destruidos.
**Solución:** `EntityContainer.OnDisable()` ahora llama a `entityRepository.Clear()`.

### 7.5 Archivos Modificados para Corrección de Errores

#### `GameServerDisconnectionHandler.cs`
```csharp
private void OnDisconnected(WebSocketCloseCode code)
{
    Debug.Log($"Game server disconnection reason: {code}");

    // NUEVO: Ignorar desconexión normal (esperada al cambiar de escena)
    if (code == WebSocketCloseCode.Normal)
    {
        return;
    }
    
    // Solo mostrar mensaje para desconexiones inesperadas
    NoticeUtils.ShowNotice(NoticeMessages.GameServer.ConnectionClosed, OnClicked);
}
```

#### `UICreator.cs`
```csharp
// NUEVO: Limpiar singleton al destruir
private void OnDestroy()
{
    if (instance == this)
    {
        instance = null;
    }
    uiCanvas = null;
}
```

#### `NoticeController.cs`
```csharp
private void OnDestroy()
{
    UnsubscribeFromNoticeWindow();
    
    // NUEVO: Destruir la ventana para evitar objetos huérfanos
    if (noticeView != null)
    {
        var viewGameObject = (noticeView as MonoBehaviour)?.gameObject;
        if (viewGameObject != null)
        {
            Destroy(viewGameObject);
        }
        noticeView = null;
    }
}
```

#### `EntityContainer.cs`
```csharp
private void OnDisable()
{
    gameApi?.SceneEntered?.RemoveListener(OnSceneEntered);
    gameApi?.GameObjectsAdded?.RemoveListener(OnGameObjectsAdded);
    gameApi?.GameObjectsRemoved?.RemoveListener(OnGameObjectsRemoved);
    
    // NUEVO: Limpiar repositorio al cambiar de escena
    entityRepository?.Clear();
    localCollection?.Clear();
    localEntity = null;
    
    // NUEVO: Limpiar singleton
    if (instance == this)
    {
        instance = null;
    }
}
```

#### `DummyGameApi.cs`
```csharp
private void OnDestroy()
{
    // NUEVO: Limpiar singleton
    if (instance == this)
    {
        instance = null;
    }
    
    ApiProvider.RemoveGameApiProvider();
    Disconnected?.Invoke(WebSocketCloseCode.Normal);
}
```

#### `DummyCharacterProviderApi.cs`
```csharp
private void OnDestroy()
{
    // NUEVO: Limpiar singleton
    if (instance == this)
    {
        instance = null;
    }
    
    ApiProvider.RemoveCharacterProviderApi();
}
```

#### `DummyGameProviderApi.cs`
```csharp
private void OnDestroy()
{
    // NUEVO: Limpiar singleton
    if (instance == this)
    {
        instance = null;
    }
    
    ApiProvider.RemoveGameProviderApi();
}
```

#### `DummyChatApi.cs`
```csharp
private void OnDestroy()
{
    // NUEVO: Limpiar singleton
    if (instance == this)
    {
        instance = null;
    }
    
    ApiProvider.RemoveChatApiProvider();
}
```

### 7.6 Correcciones de NullReferenceException

#### `LoadingText.cs`
```csharp
private void OnDestroy()
{
    // NUEVO: Verificar null antes de desuscribir
    if (uiFadeAnimation != null)
    {
        uiFadeAnimation.FadeInCompleted -= OnFadeInCompleted;
    }
}
```

#### `ScreenFadeImage.cs`
```csharp
private void OnDestroy()
{
    // NUEVO: Verificar null antes de desuscribir
    if (uiFadeAnimation != null)
    {
        uiFadeAnimation.FadeOutCompleted -= OnFadeOutCompleted;
    }
}
```

#### `NoticeWindow.cs`
```csharp
private void OnDestroy()
{
    // NUEVO: Verificar null antes de desuscribir
    if (uiFadeAnimation != null)
    {
        uiFadeAnimation.FadeOutCompleted -= OnFadeOutCompleted;
    }
}
```

#### `ChatController.cs` (TextMesh Pro)
```csharp
private void Start()
{
    // NUEVO: Verificar null antes de suscribir
    if (chatInputField != null)
    {
        chatInputField.onEndEdit.AddListener(OnChatInputFieldEndEdit);
    }
    
    if (chatScrollbar != null)
    {
        chatScrollbar.onValueChanged.AddListener(OnChatScrollbarValueChanged);
    }
}

private void OnDestroy()
{
    // NUEVO: Verificar null antes de desuscribir
    if (chatInputField != null)
    {
        chatInputField.onEndEdit.RemoveListener(OnChatInputFieldEndEdit);
    }
    
    if (chatScrollbar != null)
    {
        chatScrollbar.onValueChanged.RemoveListener(OnChatScrollbarValueChanged);
    }
}
```

### 7.7 Flujo Final del Sistema Login V2

```
┌─────────────────────────────────────────────────────────────────────┐
│                     FLUJO LOGIN V2 COMPLETO                          │
└─────────────────────────────────────────────────────────────────────┘

INICIO
   │
   ▼
┌─────────────────┐
│  Pantalla Login │
│ "Ingresa email" │
└─────────────────┘
   │
   ▼
┌─────────────────┐     ┌──────────────────┐
│  CheckEmail()   │────►│ ¿Email existe?   │
└─────────────────┘     └──────────────────┘
                               │
              ┌────────────────┴────────────────┐
              │ NO                              │ SÍ
              ▼                                 ▼
     ┌─────────────────┐               ┌─────────────────┐
     │"Crear contraseña"│               │"Ingresa password"│
     └─────────────────┘               └─────────────────┘
              │                                 │
              ▼                                 ▼
     ┌─────────────────┐               ┌─────────────────┐
     │RegisterWithEmail│               │  LoginByEmail   │
     │ (crea cuenta    │               │ (valida pass)   │
     │  con Id único)  │               └─────────────────┘
     └─────────────────┘                       │
              │                                 │
              └────────────┬────────────────────┘
                           │
                           ▼
              ┌─────────────────────────┐
              │   LOGIN EXITOSO         │
              │ playerData.Id = "xyz"   │
              └─────────────────────────┘
                           │
                           ▼
              ┌─────────────────────────┐
              │ UserMetadata.           │
              │ SetUserIdFromCredentials│
              │ (playerData.Id)         │
              └─────────────────────────┘
                           │
                           ▼
              ┌─────────────────────────┐
              │ CharacterViewController.│
              │ ShowCharacterSelection  │
              │ Smart()                 │
              └─────────────────────────┘
                           │
                           ▼
              ┌─────────────────────────┐
              │ GetCharacters(userId)   │
              │ FILTRADO por userId     │
              └─────────────────────────┘
                           │
              ┌────────────┴────────────┐
              │ 0 personajes           │ 1+ personajes
              ▼                        ▼
     ┌─────────────────┐      ┌─────────────────┐
     │ GoToCreate      │      │ GoToGameWith    │
     │ Character()     │      │ FirstCharacter()│
     └─────────────────┘      └─────────────────┘
              │                        │
              ▼                        ▼
     ┌─────────────────┐      ┌─────────────────┐
     │ Selección clase │      │ Auto-selecciona │
     │ Knight/Archer/  │      │ primer personaje│
     │ Wizard          │      │ válido          │
     └─────────────────┘      └─────────────────┘
              │                        │
              ▼                        │
     ┌─────────────────┐               │
     │ Ingresar nombre │               │
     │ del personaje   │               │
     └─────────────────┘               │
              │                        │
              ▼                        │
     ┌─────────────────┐               │
     │ CreateCharacter │               │
     │ (con userId)    │               │
     └─────────────────┘               │
              │                        │
              └────────────┬───────────┘
                           │
                           ▼
              ┌─────────────────────────┐
              │       LOBBY/JUEGO       │
              └─────────────────────────┘
```

### 7.8 Resumen de Cambios - Fase 7

| Archivo | Tipo de Cambio | Descripción |
|---------|----------------|-------------|
| `UserMetadata.cs` | Modificado | Vinculación userId-cuenta, ClearSession() |
| `PlayerLoginIntegration.cs` | Modificado | Flujo inteligente post-login |
| `DummyCharacterProviderApi.cs` | Modificado | Filtrado de personajes por userId |
| `CharacterViewController.cs` | Modificado | Smart flow, detección login, validación personajes |
| `GameServerDisconnectionHandler.cs` | Modificado | Ignorar desconexión normal |
| `UICreator.cs` | Modificado | Limpieza singleton |
| `NoticeController.cs` | Modificado | Destruir ventana en OnDestroy |
| `EntityContainer.cs` | Modificado | Limpiar repositorio y singleton |
| `DummyGameApi.cs` | Modificado | Limpieza singleton |
| `DummyCharacterProviderApi.cs` | Modificado | Limpieza singleton |
| `DummyGameProviderApi.cs` | Modificado | Limpieza singleton |
| `DummyChatApi.cs` | Modificado | Limpieza singleton |
| `LoadingText.cs` | Modificado | Null check en OnDestroy |
| `ScreenFadeImage.cs` | Modificado | Null check en OnDestroy |
| `NoticeWindow.cs` | Modificado | Null check en OnDestroy |
| `ChatController.cs` | Modificado | Null checks en Start/OnDestroy |

### 7.9 Principios SOLID Aplicados en Fase 7

| Principio | Aplicación |
|-----------|------------|
| **SRP** | `HasAnyCharacter()` solo verifica existencia, `HandleSmartFlow()` solo decide flujo |
| **OCP** | Sistema extensible para nuevos flujos post-login sin modificar código existente |
| **LSP** | `IUserSession` permite sustituir `UserMetadata` por otra implementación |
| **ISP** | Interfaces específicas: `IUserSession` solo para sesión, `ICharacterProviderApi` solo para personajes |
| **DIP** | `CharacterViewController` depende de abstracciones (`IUserSession`), no de `UserMetadata` directo |

### 7.10 Patrones de Diseño Aplicados en Fase 7

| Patrón | Uso |
|--------|-----|
| **Strategy** | Flujo inteligente decide estrategia (crear vs jugar) según estado |
| **Singleton** | Limpieza correcta de singletons al cambiar escena |
| **Observer** | Eventos de login notifican a `CharacterViewController` |
| **Null Object** | Verificaciones de null antes de operaciones |
| **Template Method** | `HandleSmartFlow()` define algoritmo, métodos auxiliares implementan pasos |

---

## ✅ ESTADO FINAL DE LA REFACTORIZACIÓN

### Fases Completadas

- [x] **Fase 1:** Fundamentos y Persistencia (`ISaveService`, `ServiceLocator`)
- [x] **Fase 2:** Desacoplar APIs y Providers (`IApiProvider`, `INetworkConfiguration`)
- [x] **Fase 3:** Refactorizar PlayerController (`IInputService`, componentes)
- [x] **Fase 4:** UI con Patrón MVP (`ILoginView`, presenters)
- [x] **Fase 5:** EntityContainer con Repository Pattern (`IEntityRepository`, `IEntityFactory`)
- [x] **Fase 6:** Sistema de Login por Nombre (`IPlayerRepository`, `ICredentialValidator`)
- [x] **Fase 7:** Login V2 - Vinculación UserId con Cuenta (flujo inteligente)
- [x] **Fase 8:** Persistencia WebGL y Character API unificada

### Flujo de Usuario Final

1. **Usuario nuevo:**
   - Email → Crear contraseña → Crear personaje → Juego ✅

2. **Usuario existente sin personaje:**
   - Email → Contraseña → Crear personaje → Juego ✅

3. **Usuario existente con personaje:**
   - Email → Contraseña → Juego (directo) ✅

4. **Cambio de usuario:**
   - Cada usuario solo ve sus propios personajes ✅

5. **Persistencia de progreso:**
   - Nivel y experiencia se guardan automáticamente ✅
   - Funciona en WebGL (navegador) y standalone ✅

---

## 🆕 FASE 8: Persistencia WebGL y Character API Unificada

### 8.1 Objetivo

Implementar persistencia de datos del personaje (nivel, experiencia) que funcione tanto en builds standalone como en WebGL (navegador).

### 8.2 Arquitectura de Persistencia Multi-Plataforma

```
┌─────────────────────────────────────────────────────────────────┐
│                     JsonSaveService                              │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────┐          ┌──────────────────┐             │
│  │    STANDALONE    │          │      WEBGL       │             │
│  │   (Windows/Mac)  │          │    (Browser)     │             │
│  ├──────────────────┤          ├──────────────────┤             │
│  │ File.WriteAllText│          │ PlayerPrefs      │             │
│  │ File.ReadAllText │          │ (IndexedDB)      │             │
│  │                  │          │                  │             │
│  │ persistentDataPath          │ Almacenamiento   │             │
│  │ /game_save_data  │          │ del navegador    │             │
│  └──────────────────┘          └──────────────────┘             │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 8.3 Implementación de JsonSaveService

```csharp
public class JsonSaveService : ISaveService
{
    private const string SaveFileName = "game_save_data.json";
    private const string PlayerPrefsKey = "JsonSaveService_Data";
    
    private Dictionary<string, object> data;
    private readonly string savePath;
    private bool isDirty;
    private readonly bool usePlayerPrefs;

    public JsonSaveService()
    {
        // En WebGL no se puede usar el sistema de archivos
#if UNITY_WEBGL && !UNITY_EDITOR
        usePlayerPrefs = true;
        savePath = string.Empty;
        Debug.Log("[JsonSaveService] WebGL detectado - usando PlayerPrefs (IndexedDB)");
#else
        usePlayerPrefs = false;
        savePath = Path.Combine(Application.persistentDataPath, SaveFileName);
        Debug.Log($"[JsonSaveService] Usando archivos - path: {savePath}");
#endif
        data = new Dictionary<string, object>();
        LoadFromFile();
    }

    public void Save()
    {
        if (!isDirty) return;

        var json = SerializeDictionary(data);
        
        if (usePlayerPrefs)
        {
            // WebGL: usa PlayerPrefs (se mapea a IndexedDB del navegador)
            PlayerPrefs.SetString(PlayerPrefsKey, json);
            PlayerPrefs.Save();
        }
        else
        {
            // Otras plataformas: usa archivo
            File.WriteAllText(savePath, json);
        }
        
        isDirty = false;
    }

    private void LoadFromFile()
    {
        string json = string.Empty;
        
        if (usePlayerPrefs)
        {
            if (PlayerPrefs.HasKey(PlayerPrefsKey))
            {
                json = PlayerPrefs.GetString(PlayerPrefsKey);
            }
        }
        else
        {
            if (File.Exists(savePath))
            {
                json = File.ReadAllText(savePath);
            }
        }
        
        data = !string.IsNullOrEmpty(json) 
            ? DeserializeDictionary(json) 
            : new Dictionary<string, object>();
    }
}
```

### 8.4 API de Personajes Unificada

`ApiProviderService` siempre usa `DummyCharacterProviderApi` para garantizar persistencia local consistente:

```csharp
public class ApiProviderService : IApiProvider
{
    private ICharacterProviderApi characterProviderApi;

    public ICharacterProviderApi GetCharacterProviderApi()
    {
        if (characterProviderApi == null)
        {
            characterProviderApi = CreateCharacterProviderApi();
        }
        return characterProviderApi;
    }

    private ICharacterProviderApi CreateCharacterProviderApi()
    {
        // SIEMPRE usar DummyCharacterProviderApi para persistencia local
        return DummyCharacterProviderApi.GetInstance();
    }
}
```

### 8.5 Flujo de Persistencia de Nivel

```
┌─────────────────────────────────────────────────────────────────┐
│                  FLUJO DE GUARDADO DE NIVEL                      │
└─────────────────────────────────────────────────────────────────┘

    Jugador mata enemigo
           │
           ▼
    ┌─────────────────┐
    │ MobBehaviour    │
    │ OnMobDied()     │
    └────────┬────────┘
             │
             ▼
    ┌─────────────────┐
    │ UserMetadata    │
    │ AddExperience   │
    │ Points(value)   │
    └────────┬────────┘
             │
             ▼
    ┌─────────────────┐
    │ VerifyCharacter │
    │ Level()         │
    │ (sube nivel?)   │
    └────────┬────────┘
             │
             ▼
    ┌─────────────────┐
    │ SaveCharacter   │
    │ Data()          │
    └────────┬────────┘
             │
             ▼
    ┌─────────────────────────┐
    │ DummyCharacterProvider  │
    │ Api.UpdateCharacter()   │
    │ (id, level, exp)        │
    └────────────┬────────────┘
                 │
                 ▼
    ┌─────────────────────────┐
    │ SaveCharacterCollection │
    │ ()                      │
    └────────────┬────────────┘
                 │
                 ▼
    ┌─────────────────────────┐
    │ ISaveService.SetString  │
    │ ("characters", json)    │
    └────────────┬────────────┘
                 │
                 ▼
    ┌─────────────────────────┐
    │ ISaveService.Save()     │
    │ (archivo o PlayerPrefs) │
    └─────────────────────────┘
```

### 8.6 Archivos Modificados

| Archivo | Cambio |
|---------|--------|
| `JsonSaveService.cs` | Detecta WebGL y usa PlayerPrefs (IndexedDB) en lugar de archivos |
| `ApiProviderService.cs` | Siempre retorna `DummyCharacterProviderApi` para persistencia local |
| `ApiProvider.cs` | Fallback a `DummyCharacterProviderApi` |
| `DummyCharacterProviderApi.cs` | Siempre actualiza diccionario desde datos del archivo/storage |

### 8.7 Principios Aplicados

| Principio | Aplicación |
|-----------|------------|
| **OCP** | `JsonSaveService` extensible para nuevas plataformas sin modificar código existente |
| **DIP** | Dependencia en `ISaveService`, no en implementación concreta |
| **SRP** | `JsonSaveService` solo maneja persistencia, detecta plataforma internamente |

### 8.8 Consideraciones WebGL

| Aspecto | Implementación |
|---------|----------------|
| Sistema de archivos | No disponible en WebGL - usa PlayerPrefs |
| PlayerPrefs en WebGL | Se mapea a IndexedDB del navegador |
| Persistencia | Datos persisten entre sesiones del navegador |
| Compilación condicional | `#if UNITY_WEBGL && !UNITY_EDITOR` |

---

