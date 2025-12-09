
---

### Service Locator — detalle completo

- Definición técnica
    - Un registro global que mapea tipos/interfaces a instancias o factories. Soporta `Register<T>(T service)` y `TryGet<T>(out T service)`.

- Propósito explícito en este proyecto
    - Evitar introducir un contenedor DI completo y permitir una inicialización central desde un MonoBehaviour (`ServiceLocatorInitializer`) sin tener que modificar decenas de MonoBehaviours para pasar dependencias por constructor.

- Archivo(s)
    - `src/maple-fighters/Assets/Maple Fighters/Scripts/Core/Infrastructure/ServiceLocator.cs`
    - `src/maple-fighters/Assets/Maple Fighters/Scripts/Core/Infrastructure/ServiceLocatorInitializer.cs`

- Uso práctico (callsites)
    - UI/Presenters: obtienen `IAuthenticationValidator`, `ISaveService`.
    - Gameplay: `EntityContainer` obtiene `IEntityRepository`/`IEntityFactory`.

- Snippet (representativo)
    ```csharp
    // ServiceLocatorInitializer.Awake()
    ServiceLocator.Register<ISaveService>(new JsonSaveService());
    ServiceLocator.Register<IEntityRepository>(new EntityRepository());

    // Consumer
    if (ServiceLocator.TryGet(out IPlayerRepository playerRepo)) {
            var p = playerRepo.FindByName(name);
    }
    ```

- Implicaciones para testing
    - Tests unitarios pueden sustituir servicios añadiendo `ServiceLocator.Register<ISaveService>(mockSave)` en el setup del test. Esto permite tests puros de `LoginPresenter` o handlers sin modificar el wiring.

- Trade-offs y mitigaciones
    - Trade-off: ocultamiento de dependencias. Mitigación: documentar en `ServiceLocatorInitializer` qué servicios se registran (se añadió en la documentación) y preferir pasar explícitamente dependencias en clases nuevas.

---

### Repository — detalle completo

- Definición técnica
    - Abstracción (interfaz) que ofrece operaciones CRUD y queries sobre colecciones de entidades del dominio.

- Propósito en el proyecto
    - Evitar acoplar la lógica de aplicación a Unity `GameObject`s y permitir implementaciones en memoria o persistentes.

- Archivo(s)
    - `src/maple-fighters/Assets/Maple Fighters/Scripts/Core/Domain/Interfaces/IEntityRepository.cs`
    - `src/maple-fighters/Assets/Maple Fighters/Scripts/Core/Infrastructure/Repositories/EntityRepository.cs`
    - `src/maple-fighters/Assets/Maple Fighters/Scripts/Core/Infrastructure/Repositories/PlayerRepository.cs`

- API pública (contrato)
    - `IGameEntity GetLocalEntity();`
    - `bool TryGetEntity(int id, out IGameEntity entity);`
    - `void AddEntity(int id, IGameEntity entity);`
    - `bool RemoveEntity(int id);`
    - `void Clear();`

- Ejemplo de interacción
    - Al entrar en escena, `EntityContainer` registra entidades en `IEntityRepository`. Al destruir la escena, `EntityContainer.OnDisable()` llama `entityRepository.Clear()`.

- Testing
    - Reemplazar `IEntityRepository` por una implementación en memoria (o NSubstitute) para tests unitarios de managers y handlers. Los tests pueden verificar que `AddEntity` fue llamado o que `TryGetEntity` devuelve la entidad esperada.

---

### Factory — detalle completo

- Definición técnica
    - Encapsula la creación de objetos complejos (prefabs, inicialización de componentes). En Unity, centraliza `Resources.Load` y `Instantiate`.

- Propósito en el proyecto
    - Aislar la lógica de instanciación (nombres de recursos, path, post-configuración) y permitir sustituir la fábrica en tests o para variaciones de contenido.

- Archivo(s)
    - `src/maple-fighters/Assets/Maple Fighters/Scripts/Core/Domain/Interfaces/IEntityFactory.cs`
    - `src/maple-fighters/Assets/Maple Fighters/Scripts/Core/Infrastructure/Factories/EntityFactory.cs`

- Ejemplo real (representativo)
    ```csharp
    public IGameEntity CreateEntity(string name, Vector2 position) {
            var path = string.Format(Paths.Resources.Game.Entities, name);
            var prefab = Resources.Load<GameObject>(path);
            var go = Object.Instantiate(prefab, position, Quaternion.identity);
            var entity = go.GetComponent<Entity>();
            entity.Initialize(...);
            return entity;
    }
    ```

- Consideraciones
    - Para tests puros evitar `Instantiate`: mockear `IEntityFactory` o implementar una `TestEntityFactory` que devuelva objetos no-MonoBehaviour.

---

### Strategy — detalle completo

- Definición técnica
    - Permite seleccionar entre varias implementaciones de una misma familia de algoritmos en tiempo de ejecución.

- Aplicaciones concretas
    - `ISaveService` → `JsonSaveService` (archivos) o `PlayerPrefsSaveService` (WebGL fallback).
    - `IInputService` → `UnityInputService` (entrada real) o potencial `AIInputService` para bots.

- Archivo(s)
    - `src/maple-fighters/Assets/Maple Fighters/Scripts/Core/Domain/Interfaces/ISaveService.cs`
    - `src/maple-fighters/Assets/Maple Fighters/Scripts/Core/Infrastructure/Persistence/JsonSaveService.cs`
    - `src/maple-fighters/Assets/Maple Fighters/Scripts/Core/Infrastructure/Persistence/PlayerPrefsSaveService.cs`

- Cómo se selecciona
    - En `ServiceLocatorInitializer`, se detecta la plataforma (`#if UNITY_WEBGL`) y se registra la estrategia adecuada. En configuración local se puede forzar una estrategia concreta en `ServiceLocatorInitializer`.

- Implicaciones de datos
    - Garantizar compatibilidad de formato JSON entre estrategias (si se cambia schema puede requerirse migración de datos en `JsonSaveService`).

---

### Adapter — detalle completo

- Definición técnica
    - Permite que clases con interfaces incompatibles colaboren mediante una clase adaptadora.

- Uso en repo
    - `NetworkConfigurationAdapter` — permite que un `ScriptableObject` (config asset del Editor) cumpla con `INetworkConfiguration` sin exponer `ScriptableObject` en capas superiores.
    - `PlayerLoginIntegration` — actúa como adaptador entre callbacks de la API antigua y el nuevo flujo que usa `IPlayerCredentials`.

- Archivos
    - `src/maple-fighters/Assets/Maple Fighters/Scripts/Core/Infrastructure/Configuration/NetworkConfigurationAdapter.cs`

- Beneficios
    - Mantiene separado el código runtime del código de Editor/Assets y facilita testing puesto que `INetworkConfiguration` puede ser mocked.

---

### MVP — detalle completo

- Definición técnica
    - Separación de responsabilidades en vista (View), presentación (Presenter) y modelo/servicios.

- Por qué en Unity
    - Las MonoBehaviours están fuertemente ligados al ciclo de vida de Unity y no son fáciles de testear en unit tests. El presenter es un POCO que contiene la lógica de UI y puede ser probado fuera del Editor.

- Archivos concretos
    - `src/maple-fighters/Assets/Maple Fighters/Scripts/UI/Authenticator/ILoginView.cs`
    - `src/maple-fighters/Assets/Maple Fighters/Scripts/UI/Authenticator/LoginPresenter.cs`
    - `src/maple-fighters/Assets/Maple Fighters/Scripts/UI/Authenticator/LoginWindow.cs`

- Ejemplo de prueba recomendada
    - Crear un `LoginPresenterTests` en EditMode que use un `ILoginView` mock (NSubstitute in backend or a simple test double in Unity) y un mock de `IAuthenticationValidator`, verificar que `ShowError` o `NavigateToCharacterSelection` son llamados.

---

### State — detalle completo

- Definición técnica
    - Encapsula comportamiento dependiente de estado en clases separadas que implementan una interfaz común.

- Implementación
    - `IPlayerStateBehaviour` y varias implementaciones (Idle, Running, Jumping) mantenidas por `PlayerController`.

- Beneficio
    - Testing por estado: cada `IPlayerStateBehaviour` puede ser probado por separado si su lógica está desacoplada de MonoBehaviour.

---

### Singleton + Observer — detalle completo

- Definición técnica
    - Singletons proporcionan acceso global; Observers (eventos) notifican cambios.

- Correcciones durante refactor
    - Se detectaron memory leaks y `MissingReferenceException` al cambiar de escena; se añadió limpieza en `OnDestroy()` y `OnDisable()` para remover listeners y poner `instance = null` en singletons.

- Archivos
    - `src/maple-fighters/Assets/Maple Fighters/Scripts/Networking/DummyGameApi.cs`
    - `src/maple-fighters/Assets/Maple Fighters/Scripts/Core/Infrastructure/ApiProvider.cs`

---

### Facade — detalle completo

- Definición técnica
    - Simplifica acceso a un subsistema complejo exponiendo una interfaz más simple.

- Aplicación
    - `AdminService` agrupa llamadas a repositorios y `ISaveService` para operaciones administrativas (listar/desbloquear usuarios) y se registra en `ServiceLocator`.

---

### Template Method y Null Object — detalle rápido

En el refactor detectamos que varios controladores repetían la misma secuencia de pasos al resolver una acción (cargar datos, validar estado, aplicar efectos y actualizar la vista) pero con pequeñas variaciones. Para mantener el flujo de alto nivel consistente y, a la vez, permitir personalizar los pasos concretos, se consolidó la lógica en un Template Method: un método orquestador define el algoritmo y delega en métodos auxiliares sobrescribibles los puntos de variación. Así reducimos duplicidad, clarificamos el orden de las operaciones y evitamos desalineaciones entre controladores.

Al mismo tiempo, Unity puede destruir o recrear objetos de escena fuera de nuestro control; referencias a `GameObject`, `Transform` o servicios pueden quedar nulas entre frames. Para evitar excepciones intermitentes y capturar mejor la intención de "no hay nada que hacer", se introdujeron guards/Null-checks siguiendo el espíritu de Null Object: preferimos un comportamiento seguro y explícito ante referencias faltantes (o instancias nulas) en lugar de confiar en que siempre existirán. Esto endurece el flujo frente a cambios de escena y facilita pruebas.

- `CharacterViewController.HandleSmartFlow()` implementa un template method donde el esqueleto del algoritmo está en un método y pasos concretos son métodos auxiliares.
- Se añadió uso extendido de guards/Null-checks (Null Object pattern concept) para robustez frente a cambios de escena.

---

## Impacto en CI y ejecución automatizada

- Backend
    - Ejecutar `dotnet test` en `src/game-service` prueba la capa de aplicación y dominio. Recomiendo añadir `--collect:"XPlat Code Coverage"` y publicar el resultado en la pipeline.

- Unity
    - Para tests automáticos en CI: ejecutar Unity en batch mode con `-runTests` y filtrar por EditMode/PlayMode. Recomendación: usar `game-ci` o `Unity Builder` actions en GitHub Actions.

---

Si desea, continúo con la acción propuesta (mover la lógica pura a `Core/Domain/Logic` y actualizar `asmdef`), o genero la lista completa de tests con rutas y descripciones. ¿Cuál prefiere que haga ahora? 


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
 
---

## **Cómo funciona el proyecto (visión general, carpetas y archivos clave)**

Esta sección describe de forma práctica qué hace cada carpeta y los archivos más relevantes del repositorio, cómo encajan entre sí y cómo interactuar con ellos (ejecución / pruebas). Está pensada como guía rápida para un mantenedor que llega al proyecto.

- **Raíz del repositorio**: contiene documentación, configuraciones Docker / Kubernetes y el README.
    - `README.md`: instrucciones generales del proyecto, cómo levantar servicios y referencias básicas.
    - `REFACTORING_COMPLETE.md`: este documento (estado y detalle de la refactorización).
    - `docker-compose.yml`, `docker-compose.prod.yml`: orquestación local / producción (servicios auxiliares si se usaran).
    - `kustomize/`: manifiestos k8s usados para despliegue (opcional para despliegues en clúster).

- **`src/game-service/` (Backend - Game Service)**:
    - `GameService.sln`: solución .NET que agrupa los proyectos del servidor.
    - `Game.Application/`: lógica de aplicación (casos de uso, handlers, DTOs). Aquí viven los servicios que procesan mensajes del juego.
    - `Game.Domain/`: modelos de dominio y contratos relevantes para negocio (entidades, ValueObjects, interfaces del dominio).
    - `Game.Infrastructure/`: adaptadores (repositorios, persistencia, mapeos) para el backend.
    - `Game.Server/`: capa de entrada (Sockets / HTTP / handlers) que recibe eventos externos y delega a `Game.Application`.
    - `Game.UnitTests/`: pruebas unitarias xUnit + NSubstitute + Shouldly. Ejecutar con `dotnet test`.
    - Qué hace: procesa la lógica del servidor (mecánicas del juego que no dependen de Unity) y expone endpoints / handlers que el cliente puede usar o que se usan en simulaciones.

- **`src/maple-fighters/` (Cliente Unity)**:
    - `maple-fighters.sln`, `Assembly-CSharp.csproj`, `Assembly-CSharp-Editor.csproj`: proyectos del cliente Unity (generados por Unity).
    - `Assets/Maple Fighters/Scripts/`: el código fuente principal del cliente separado por capas (Core, UI, Gameplay, Services y demás). Puntos clave:
        - `Core/Domain/Interfaces/`: interfaces/contratos (por ejemplo `ISaveService`, `IEntityRepository`, `IInputService`) — la capa que define el contrato estable.
        - `Core/Infrastructure/`: implementaciones concretas (por ejemplo `JsonSaveService`, `ServiceLocator`, `EntityFactory`).
        - `UI/`: vistas y presentadores (MVP) — `ILoginView`, `LoginPresenter`, `PlayerLoginWindow`.
        - `Gameplay/`: componentes de juego (`PlayerController`, `GroundDetector`, `PlayerEffects`, comportamientos de mobs).
        - `Services/`: adaptadores/`Dummy*` APIs usados en desarrollo (`DummyCharacterProviderApi`, `DummyGameApi`).
    - `Assets/Tests/EditMode/` y `Assets/Tests/PlayMode/`: pruebas Unity (EditMode = lógica pura; PlayMode = tests que usan frames / coroutines).
    - Qué hace: ejecuta la parte cliente del juego (render, input, UI, flujo de login y selección de personajes). Se conecta a los `Dummy*` providers para persistencia local en desarrollo.

- **`lib/interest-management/` (librería independiente)**:
    - Código reutilizable para manejo de áreas de interés y detección de proximidad (clases como `InterestArea`, `MatrixRegion`, `NearbySceneObjectsCollection`).
    - Uso: utilizable por el juego para optimizar qué entidades deben procesarse en función de la proximidad.

- **`src/frontend/`**:
    - Proyecto web (React / SPA) usado para el frontend de la parte web si existe. Contiene `Dockerfile`, `nginx.conf` y `package.json`.
    - Qué hace: sirve recursos estáticos, UI web o admin panels si se integran.

- **`release/` y `docs/`**:
    - `release/kubernetes-manifests.yaml`: manifiestos para releases.
    - `docs/`: imágenes y documentación complementaria.

### Archivos y componentes críticos (ruta → qué hace / por qué es importante)

- `src/maple-fighters/Assets/Maple Fighters/Scripts/Core/Infrastructure/ServiceLocator.cs`:
    - Qué hace: registro y resolución global de servicios. Permite inyectar dependencias sin introducir un contenedor DI pesado.
    - Por qué es crítico: muchos módulos obtienen servicios desde aquí; cambiarlo afecta al wiring de la app.

- `src/maple-fighters/Assets/Maple Fighters/Scripts/Core/Infrastructure/ServiceLocatorInitializer.cs`:
    - Qué hace: MonoBehaviour que configura e instancia implementaciones concretas en `Awake()` (ej. `ISaveService`, `IEntityRepository`).
    - Nota operativa: editar este archivo cambia la configuración por defecto usada en tiempo de ejecución.

- `src/maple-fighters/Assets/Maple Fighters/Scripts/Core/Infrastructure/Persistence/JsonSaveService.cs`:
    - Qué hace: persistencia multi-plataforma (archivos en standalone, `PlayerPrefs` en WebGL).
    - Impacto: guarda y restaura el progreso del jugador, personajes y configuraciones. Un bug aquí puede corromper los saves.

- `src/maple-fighters/Assets/Maple Fighters/Scripts/Core/Infrastructure/Persistence/PlayerPrefsSaveService.cs`:
    - Qué hace: fallback de persistencia para plataformas que no soportan archivos (WebGL).

- `src/maple-fighters/Assets/Maple Fighters/Scripts/Core/Infrastructure/Repositories/EntityRepository.cs`:
    - Qué hace: almacena referencias a entidades del juego y gestiona su ciclo de vida lógico (Add/Remove/Clear).
    - Importancia: evita referencias a `GameObject` destruidos y facilita la sincronización con el servidor.

- `src/maple-fighters/Assets/Maple Fighters/Scripts/Core/Infrastructure/Factories/EntityFactory.cs`:
    - Qué hace: encapsula `Resources.Load` + `Instantiate` y cualquier configuración post-inicialización.
    - Recomendación: mockear en tests unitarios para evitar instanciación de `GameObject`.

- `src/maple-fighters/Assets/Maple Fighters/Scripts/Gameplay/Player/PlayerController.cs`:
    - Qué hace: orchestrador principal del jugador (lee input, gestiona estados y coordina componentes). Tras refactor, delega a `IInputService`, `GroundDetector` y `PlayerEffects`.
    - Nota: mantener este archivo estable; las reglas de juego suelen residir aquí (transiciones de estado, físicas básicas).

- `src/maple-fighters/Assets/Maple Fighters/Scripts/Gameplay/Player/Components/GroundDetector.cs`:
    - Qué hace: detecta si el jugador está en suelo y expone eventos o métodos para el `PlayerController`.

- `src/maple-fighters/Assets/Maple Fighters/Scripts/UI/Authenticator/LoginPresenter.cs` y `ILoginView.cs`:
    - Qué hacen: separan la lógica de la vista (validaciones, navegación) de la UI concreta (LoginWindow). Muy útiles para pruebas unitarias sin Unity Editor.

- `src/maple-fighters/Assets/Maple Fighters/Scripts/Services/DummyCharacterProviderApi.cs`:
    - Qué hace: proveedor local para crear/guardar/leer personajes; persistencia usa `ISaveService`.
    - Importancia: en desarrollo y pruebas sirve como fuente única de verdad para personajes; su comportamiento afecta la experiencia del jugador (creación / listado de personajes).

- `src/maple-fighters/Assets/Maple Fighters/Scripts/Character/CharacterViewController.cs`:
    - Qué hace: controla la selección y creación de personajes. Contiene el `HandleSmartFlow()` que decide si crear o entrar al juego tras login.

- `src/maple-fighters/Assets/Maple Fighters/Scripts/User/UserMetadata.cs`:
    - Qué hace: contiene la información de sesión (userId, personaje seleccionado) y se integra con `ISaveService`.
    - Importancia: vínculo entre cuenta y personajes; corrección de `userId` fue crítica para evitar compartir personajes entre cuentas.

- `src/game-service/Game.UnitTests/`:
    - Qué contiene: pruebas unitarias del backend. Estructura por features / handlers.
    - Cómo ejecutar: desde la raíz del repo:
        ```pwsh
        dotnet restore src/game-service/GameService.sln
        dotnet test src/game-service/Game.UnitTests/Game.UnitTests.csproj
        ```

### Cómo realizar cambios seguros y pruebas locales

- Para cambiar persistencia:
    1. Modifique `JsonSaveService.cs` y `PlayerPrefsSaveService.cs`.
    2. Ejecute pruebas locales (Unity Editor + backend tests) y valide save/restore.

- Para cambiar la API dummy (persistencia de personajes):
    1. Modifique `DummyCharacterProviderApi.cs` (filtros y almacenamiento).
    2. Abra Unity → arranque el flujo de login → cree/registre personajes para verificar que se guardan y cargan.

- Para añadir tests Unity EditMode correctamente:
 1. Mover la lógica pura a `src/maple-fighters/Assets/Maple Fighters/Scripts/Core/Domain/Logic`.
 2. Crear un asmdef para `Core.Domain.Logic` y referenciarlo desde `Assets/Tests/EditMode/*.asmdef`.
 3. Escribir pruebas NUnit `[Test]` en `Assets/Tests/EditMode/` apuntando a las clases del `Core.Domain.Logic`.

---

Si quiere, puedo:
- (A) Mover ahora las clases de lógica pura de `Assets/Tests/EditMode/GameLogic.cs` a `Core/Domain/Logic` y actualizar `asmdef` (recomendado), o
- (B) Generar un listado exhaustivo de archivos críticos con una breve descripción en CSV/MD para referencia rápida.

Indique qué prefiere y lo hago a continuación.


## **Unity Tests**

- **Resumen**: Se implementaron pruebas unitarias para el cliente Unity en **EditMode** y **PlayMode** usando el Unity Test Runner (NUnit). Las pruebas EditMode son pruebas puras de C# (sin dependencias de Unity runtime) y las PlayMode usan `UnityTest` para pruebas que requieren el ciclo de frames.
- **Herramientas usadas**: **Unity Test Runner** (NUnit) para el cliente Unity. Para el backend se usó **xUnit**, **NSubstitute** y **Shouldly** (ver sección de Game Service).
- **Archivos añadidos (Unity)**:
    - `src/maple-fighters/Assets/Tests/EditMode/EditModeTests.asmdef` - Assembly definition para pruebas EditMode.
    - `src/maple-fighters/Assets/Tests/EditMode/GameLogic.cs` - Clases de lógica pura extraídas/duplicadas para permitir tests sin asmdefs complejos.
    - `src/maple-fighters/Assets/Tests/EditMode/DamageCalculatorTests.cs`
    - `src/maple-fighters/Assets/Tests/EditMode/HealthManagerTests.cs`
    - `src/maple-fighters/Assets/Tests/EditMode/MovementValidatorTests.cs`
    - `src/maple-fighters/Assets/Tests/EditMode/ScoreCalculatorTests.cs`
    - `src/maple-fighters/Assets/Tests/PlayMode/PlayModeTests.asmdef` - Assembly definition para pruebas PlayMode (referencia a Unity Test Runner añadida).
    - `src/maple-fighters/Assets/Tests/PlayMode/GameObjectTests.cs` - Ejemplo de pruebas PlayMode que usan `UnityTest`.

- **Cómo ejecutar (cliente Unity)**:
    - Abrir el proyecto en Unity Editor.
    - Window → General → Test Runner.
    - Ejecutar las pruebas EditMode o PlayMode desde el Test Runner.
    - Nota: las pruebas EditMode son rápidas y no requieren Play Mode; las PlayMode ejecutan coroutines y pueden tardar más.

- **Resultados y observaciones**:
    - Las pruebas EditMode añadidas aparecen y pasan correctamente en el Unity Test Runner (confirmado en el Editor).
    - Se actualizó el `asmdef` de PlayMode para referenciar el runner de tests de Unity y así poder usar `UnityTest` sin errores de compilación.
    - Mantener la lógica de juego separada de `MonoBehaviour` facilita la testabilidad (p. ej. `DamageCalculator`, `HealthManager`, `MovementValidator`).

- **Notas adicionales**:
    - Se duplicaron/extraeron clases de lógica pura a `Assets/Tests/EditMode/GameLogic.cs` únicamente para permitir pruebas sin necesidad de asmdefs y evitar acoplamiento con código existente; plan a largo plazo: refactorizar esas clases al espacio `Core/Domain/Logic` y referenciarlas desde el juego y los tests.
    - Para ejecutar los tests del backend (Game Service): `dotnet test` en `src/game-service/` (requiere .NET SDK 6.0 o superior; los proyectos se actualizaron de `net5.0` a `net6.0`).

---

## **Commit y Cambios Relacionados**

- **Cambios principales realizados en esta tarea**:
    - `TESTING_PLAN.md` añadido en la raíz del repositorio con el plan de pruebas.
    - Backend: nuevos tests xUnit en `src/game-service/Game.UnitTests/` y actualización de `Game.Application.csproj` / `Game.UnitTests.csproj` a `net6.0`.
    - Unity: tests EditMode y PlayMode añadidos bajo `src/maple-fighters/Assets/Tests/` y asmdefs correspondientes.
    - Documentación: sección `Unity Tests` añadida a `REFACTORING_COMPLETE.md`.

- **Instrucciones de commit**: se ha incluido este cambio en el commit que sigue (mensaje: `chore(tests): add Unity tests documentation and test artifacts; update csproj to net6.0; add TESTING_PLAN.md`).

---

Si desea, puedo crear una sección separada con la lista completa de tests (nombres de tests y rutas exactas) o mover las clases de lógica pura a `Core/Domain/Logic` para evitar duplicación —¿quiere que haga eso ahora?

## **Análisis Detallado de la Refactorización**

Este apartado explica en profundidad qué se hizo durante la refactorización, por qué se tomaron las decisiones arquitectónicas y exactamente en qué archivos y capas se aplicaron los cambios. Está pensado como guía técnica para desarrolladores que deban mantener, revisar o ampliar el trabajo.

**Resumen (rápido):**
- **Objetivo principal:** separar responsabilidades, facilitar testeo y permitir evolución sin romper código existente.
- **Estrategia:** aplicar Clean Architecture + principios SOLID y patrones (Service Locator, Repository, Factory, Adapter, Strategy, MVP, etc.) manteniendo compatibilidad mediante fallbacks.

### **1) Capas del sistema y responsabilidades**

- **Core / Domain (`src/maple-fighters/Assets/Maple Fighters/Scripts/Core/Domain`)**: contiene las abstracciones (interfaces) y modelos de dominio. Propósito: definir contratos estables que no dependen de Unity ni de infraestructuras concretas.
    - Ejemplos de archivos y responsabilidades:
        - `IEntityRepository.cs`: contrato para almacenamiento de entidades en memoria o persistencia.
        - `IPlayerRepository.cs`: contrato para almacenar/consultar jugadores.
        - `IPlayerCredentials.cs`, `IGameEntity.cs`: DTOs / contratos de negocio.
    - Por qué: reducir acoplamiento y permitir mocks en tests unitarios.

- **Core / Infrastructure (`.../Core/Infrastructure`)**: implementaciones concretas que adaptan las abstracciones a Unity/entorno.
    - Archivos clave:
        - `ServiceLocator.cs` / `ServiceLocatorInitializer.cs`: registro y resolución de servicios en tiempo de ejecución.
        - `JsonSaveService.cs`, `PlayerPrefsSaveService.cs`: estrategia de persistencia (plataforma-aware).
        - `NetworkConfigurationAdapter.cs`: adapta ScriptableObjects a `INetworkConfiguration`.
    - Por qué: aislar dependencias de plataforma y centralizar adaptaciones.

- **Services (`src/maple-fighters/Assets/Maple Fighters/Scripts/Services`)**: capas que implementan adaptadores y providers (APIs de desarrollo local, dummy APIs).
    - `ApiProviderService.cs`: fábrica de APIs usada por la aplicación (siempre devuelve `DummyCharacterProviderApi` localmente para persistencia consistente en desarrollo).
    - Por qué: permitir cambiar proveedor remoto sin tocar lógica de negocio.

- **Gameplay (`/.../Gameplay`)**: componentes y Behaviour específicos del juego (ej. `PlayerController`, `GroundDetector`, `PlayerEffects`). Mantienen la interacción con Unity (MonoBehaviours) y delegan lógica a clases testables.

- **UI (`/.../UI`)**: pantallas y la implementación del patrón MVP para vistas como login.
    - `LoginPresenter.cs` (presenter) y `ILoginView.cs` (contrato de vista)
    - Por qué: separar lógica de UI de la vista para permitir testing unitario y reutilización.

- **Tests (Unity y Backend)**
    - Unity EditMode tests: `src/maple-fighters/Assets/Tests/EditMode/*` (pruebas rápidas, lógica pura)
    - Unity PlayMode tests: `src/maple-fighters/Assets/Tests/PlayMode/*` (pruebas con `UnityTest` y ciclo de frames)
    - Backend tests (Game Service): `src/game-service/Game.UnitTests/*` (xUnit + NSubstitute + Shouldly)

### **2) Patrones aplicados, dónde y por qué (detalle técnico)**

- **Service Locator**
    - Dónde: `Core/Infrastructure/ServiceLocator.cs`, `ServiceLocatorInitializer.cs`.
    - Qué hace: registro global / simple contenedor de servicios para resolver implementaciones.
    - Por qué se eligió: proyecto pequeño, queríamos una solución ligera y no introducir una dependencia externa (Zenject). Permite desacoplamiento y facilita la inyección manual en puntos de entrada. Además se mantiene fallback para compatibilidad (si no hay servicio, se usan colecciones locales).
    - Consideraciones: no es un contenedor DI completo (no composición profunda ni scope). Recomendación futura: migrar a un DI container (Zenject o Microsoft.Extensions.DependencyInjection) si la complejidad crece.

- **Repository Pattern**
    - Dónde: `IEntityRepository.cs`, `EntityRepository.cs`, `PlayerRepository.cs`.
    - Qué hace: abstrae colección/almacenamiento de entidades para que la lógica de juego no dependa de Unity `GameObject` directamente.
    - Beneficio: permite tests unitarios de lógica de negocio usando implementaciones en memoria o mocks.

- **Factory Pattern**
    - Dónde: `IEntityFactory.cs`, `EntityFactory.cs`, `ApiProvider.ProvidePlayerLoginApi()`.
    - Qué hace: centraliza la creación de objetos complejos (entidades, APIs). Oculta detalles de `Resources.Load` o `Instantiate`.
    - Ejemplo corto:
        ```csharp
        public IGameEntity CreateEntity(string name, Vector2 position) {
                var prefab = Resources.Load<GameObject>(path);
                var go = Object.Instantiate(prefab, position, Quaternion.identity);
                return go.GetComponent<Entity>();
        }
        ```

- **Strategy Pattern**
    - Dónde: `ISaveService` + `JsonSaveService` / `PlayerPrefsSaveService`; `ICredentialValidator` y sus implementaciones.
    - Qué hace: permite intercambiar comportamientos en tiempo de ejecución (persistencia en archivo vs PlayerPrefs, validadores de credenciales).

- **Adapter Pattern**
    - Dónde: `NetworkConfigurationAdapter.cs` y `PlayerLoginIntegration` (al adaptar la API existente al nuevo flujo).
    - Qué hace: adapta ScriptableObjects y APIs antiguas a las nuevas interfaces.

- **MVP (Model-View-Presenter)**
    - Dónde: `UI/Authenticator/ILoginView.cs`, `LoginPresenter.cs`, `LoginWindow.cs`.
    - Qué hace: separa la vista (Unity UI/MonoBehaviour) de la lógica de presentación, facilitando testeo del presenter sin UI.
    - Ejemplo: `LoginPresenter` recibe una instancia de `ILoginView` (mockeable) y llama a `ILoginView.ShowError()` sin depender de Unity UI.

- **State Pattern**
    - Dónde: `PlayerController` mantiene comportamientos de estado (`IPlayerStateBehaviour`).
    - Qué hace: encapsula estados de jugador y facilita transiciones claras.

- **Singleton + Observer (eventos)**
    - Dónde: `DummyGameApi` y algunos `ApiProvider` implementan singletons; eventos C# (o UnityEvents) se usan para notificaciones de escena/servidor.
    - Qué hace: simplifica acceso global y notificación de cambios. Se agregó limpieza en `OnDestroy()` para evitar leaks y excepciones.

- **Facade**
    - Dónde: `AdminService` simplifica operaciones administrativas complejas.

### **3) Cambios concretos y sus efectos (archivo → impacto)**

- `ServiceLocator.cs` / `ServiceLocatorInitializer.cs`
    - Impacto: centraliza la configuración en runtime; facilita swapping de implementaciones en `Awake` (ej. `ServiceLocator.Register<ISaveService>(new JsonSaveService())`).
    - Testabilidad: los tests pueden registrar mocks en el `ServiceLocator` antes de ejecutar la lógica.

- `JsonSaveService.cs` / `PlayerPrefsSaveService.cs`
    - Impacto: persistencia adaptable por plataforma; `JsonSaveService` detecta WebGL y usa `PlayerPrefs` cuando corresponda.
    - Por qué: WebGL no tiene filesystem tradicional → IndexedDB via PlayerPrefs.

- `EntityRepository.cs`, `EntityFactory.cs`
    - Impacto: el `EntityContainer` dejó de crear/gestionar entidades directamente; ahora delega a `IEntityRepository`/`IEntityFactory`. Reduce uso de `FindObjectOfType`.

- `PlayerController.cs` y nuevos componentes (`GroundDetector`, `PlayerEffects`) 
    - Impacto: separación de responsabilidades (SRP), facilita pruebas de física y lógica por separado.

- `PlayerLogin*` (LoginPresenter, PlayerLoginIntegration, PlayerRepository, CredentialValidator, LoginAttemptTracker)
    - Impacto: nuevo flujo de login con bloqueo por intentos y administración. Presenter y servicios testables independientemente de la UI.

### **4) Tests: estructura, herramientas y decisiones**

- **Backend (Game Service)**
    - Frameworks: `xUnit` para pruebas, `NSubstitute` para mocks, `Shouldly` para aserciones legibles. `coverlet` para cobertura.
    - Ubicación: `src/game-service/Game.UnitTests/`.
    - Estructura adoptada: organizar tests por feature/handler (p. ej. `Handlers/AttackMobMessageHandlerTests.cs`).
    - Observaciones: proyectos actualizados a `net6.0` por compatibilidad con SDK disponible (antes `net5.0`).

- **Cliente Unity**
    - Framework: Unity Test Runner (NUnit-based). EditMode tests (NUnit [Test]) para lógica pura y PlayMode (UnityTest) para tests con frame/coroutines.
    - Ubicación: `src/maple-fighters/Assets/Tests/EditMode/*` y `.../PlayMode/*`.
    - Decisiones importantes:
        - Se creó `EditMode` asmdef para aislar pruebas que no necesitan Unity runtime.
        - Se actualizó `PlayMode` asmdef para referenciar el Test Runner y permitir uso de `UnityTest` sin errores de compilación.
        - Para evitar dependencias del asmdef del proyecto principal, se puso una copia ligera de las clases de lógica pura en `Assets/Tests/EditMode/GameLogic.cs`. Esto es una medida temporal —lo ideal es mover esas clases a `Core/Domain/Logic` y referenciarlas desde el código de juego y tests.

### **5) Cómo ejecutar y verificar (rápido, reproducible)**

- Backend (dotnet):
    - Requisitos: .NET SDK 6.0+ instalado.
    - Comandos:
        ```pwsh
        # desde la raíz del repo
        dotnet restore src/game-service/GameService.sln
        dotnet test src/game-service/Game.UnitTests/Game.UnitTests.csproj --logger "console;verbosity=detailed"
        ```
    - Resultado esperado: tests discover + run; la ejecución local mostró `Total: 67, Passed: 67` en mi entorno.

- Cliente Unity:
    - Abrir proyecto en Unity Editor compatible con la versión del proyecto.
    - Window → General → Test Runner → seleccionar EditMode o PlayMode → Run All.
    - Observación: EditMode tests son muy rápidos; PlayMode puede requerir esperar frames.

### **6) Decisiones técnicas y trade-offs**

- Migración a `net6.0` (Game Service)
    - Razón: entorno de desarrollo no tenía .NET 5 runtime; .NET 6 es LTS y ampliamente disponible.
    - Efecto: compilación y ejecución de tests en CI locales y desarrolladores más sencillo.

- Service Locator vs DI Container
    - Justificación: evitar añadir dependencia externa y mantener configuración simple. A corto plazo fue la opción pragmática. A medio-largo plazo, si el proyecto crece, recomiendo migrar a un container DI para mejores garantías de scope, ciclo de vida y test fixtures.

- Duplicación temporal de lógica en `Assets/Tests/EditMode/GameLogic.cs`
    - Por qué se hizo: evitar cambios intrusivos en el código principal para poder demostrar pruebas en Unity rápidamente.
    - Recomendación: refactorizar a `Core/Domain/Logic` y referenciar ese ensamblado desde los tests (remover duplicación).

### **7) Mapa de archivos clave y responsabilidades (rápido referenciamiento)**

- `src/maple-fighters/Assets/Maple Fighters/Scripts/Core/Domain/Interfaces/` : interfaces y contratos (persistencia, repositorios, servicios de login).
- `src/maple-fighters/Assets/Maple Fighters/Scripts/Core/Infrastructure/ServiceLocator.cs` : registro/recuperación de servicios.
- `src/maple-fighters/Assets/Maple Fighters/Scripts/Core/Infrastructure/Persistence/JsonSaveService.cs` : persistencia multi-plataforma.
- `src/maple-fighters/Assets/Maple Fighters/Scripts/UI/Authenticator/LoginPresenter.cs` : presenter de login (testable fuera de Unity UI).
- `src/maple-fighters/Assets/Maple Fighters/Scripts/Gameplay/Player/GroundDetector.cs` : detección de suelo, extraída de `PlayerController`.
- `src/game-service/Game.UnitTests/Handlers/` : pruebas por handler del servidor.
- `src/maple-fighters/Assets/Tests/EditMode/` : tests EditMode (lógica pura).

### **8) Recomendaciones y próximos pasos**

1. Mover clases de lógica pura de `Assets/Tests/EditMode/GameLogic.cs` a `Core/Domain/Logic` y referenciar esa carpeta desde el juego y los tests.
2. Considerar migración a un DI container (Zenject o Microsoft DI) si el número de servicios y life cycles crece.
3. Añadir integración de tests al CI (pipeline):
     - Backend: `dotnet test --collect:"XPlat Code Coverage"` y publicar cobertura.
     - Unity: usar Unity Test Runner en batch mode (Unity -runTests) o usar GitHub Actions con `game-ci` acciones para ejecutar PlayMode/EditMode.
4. Completar tests pendientes del plan (ProximityChecker, MobBehaviourManager, MobHealthController).

---

