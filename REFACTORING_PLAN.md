# 📋 PLAN DE REFACTORIZACIÓN - Maple Fighters (Unity Client)

## 📊 Análisis del Estado Actual

### ✅ Aspectos Positivos del Código Actual
El proyecto ya tiene algunas buenas prácticas implementadas:

1. **Patrón State** - Ya implementado en `PlayerController` con `IPlayerStateBehaviour`
2. **Interfaces definidas** - `IGameApi`, `IEntity`, `IView`, `IPlayerStateBehaviour`
3. **Separación parcial de APIs** - Servicios separados por dominio (AuthenticatorApi, GameApi, ChatApi)
4. **ScriptableObjects** - Uso de configuraciones como `NetworkConfiguration`, `PlayerConfiguration`
5. **Provider Pattern** - `ApiProvider` para obtener implementaciones de APIs

### ⚠️ Code Smells y Problemas Identificados

| Problema | Ubicación | Severidad |
|----------|-----------|-----------|
| `FindObjectOfType` excesivo | `EntityContainer`, `AuthenticatorController`, múltiples clases | 🔴 Alta |
| Singleton con estado estático | `ApiProvider`, `UICreator`, `EntityContainer` | 🟡 Media |
| PlayerPrefs para persistencia | `AuthenticatorController.SaveLoginEmail()` | 🟡 Media |
| Acoplamiento UI-Lógica | `AuthenticatorController` (mezcla validación, UI, navegación) | 🔴 Alta |
| Clases con múltiples responsabilidades | `PlayerController` (input, física, efectos, estado) | 🟡 Media |
| Dependencias directas a implementaciones concretas | Múltiples clases | 🟡 Media |
| Falta de capa de dominio clara | General | 🟡 Media |

---

## 🎯 Arquitectura Objetivo

```
Assets/Maple Fighters/Scripts/
├── Domain/                          # Capa de dominio (entidades, interfaces, reglas)
│   ├── Entities/
│   ├── Interfaces/
│   └── ValueObjects/
├── Application/                     # Casos de uso y servicios de aplicación
│   ├── Services/
│   └── UseCases/
├── Infrastructure/                  # Implementaciones concretas
│   ├── Persistence/
│   ├── Network/
│   └── DependencyInjection/
└── Presentation/                    # UI y MonoBehaviours de Unity
    ├── UI/
    ├── Gameplay/
    └── Controllers/
```

---

## 📅 FASES DE REFACTORIZACIÓN

### 🔵 FASE 1: Fundamentos y Persistencia (Bajo Riesgo)
**Duración estimada:** 1-2 días  
**Objetivo:** Crear la base sin romper funcionalidad existente

#### 1.1 Crear estructura de carpetas base
```
Scripts/
├── Core/                    # Nueva carpeta
│   ├── Domain/
│   │   └── Interfaces/
│   ├── Application/
│   │   └── Services/
│   └── Infrastructure/
│       └── Persistence/
```

#### 1.2 Implementar Sistema de Persistencia
**Reemplazar PlayerPrefs por un servicio de persistencia**

**Archivos a crear:**
- `Core/Domain/Interfaces/ISaveService.cs`
- `Core/Infrastructure/Persistence/JsonSaveService.cs`
- `Core/Infrastructure/Persistence/PlayerPrefsSaveService.cs` (fallback)

**Cambios necesarios:**
- `AuthenticatorController.cs` - Reemplazar `PlayerPrefs.SetString("email", ...)` por `ISaveService`

#### 1.3 Crear Service Locator Simple
**Para evitar FindObjectOfType sin usar DI complejo**

**Archivos a crear:**
- `Core/Infrastructure/ServiceLocator.cs`

**Patrones aplicados:** 
- Service Locator (simple, sin sobreingeniería)
- Strategy Pattern (para persistencia)

---

### 🟢 FASE 2: Desacoplar APIs y Providers (Riesgo Medio-Bajo)
**Duración estimada:** 2-3 días  
**Objetivo:** Mejorar el ApiProvider usando Factory Pattern correctamente

#### 2.1 Refactorizar ApiProvider
**Convertir de clase estática a servicio inyectable**

**Cambios:**
- Crear `IApiFactory` interface
- Implementar `ApiFactory` que registre en ServiceLocator
- Eliminar campos estáticos de `ApiProvider`

#### 2.2 Crear interfaces faltantes
- `INetworkConfiguration` - Para abstraer ScriptableObject
- `IUserSession` - Para manejar estado de usuario logueado

**Patrones aplicados:**
- Factory Pattern
- Dependency Inversion Principle

---

### 🟡 FASE 3: Refactorizar Player Controller (Riesgo Medio)
**Duración estimada:** 2-3 días  
**Objetivo:** Aplicar SRP al PlayerController

#### 3.1 Separar responsabilidades del PlayerController

**Extraer a clases separadas:**
| Responsabilidad Actual | Nueva Clase | Patrón |
|------------------------|-------------|--------|
| Manejo de estados | `PlayerStateMachine` | State Pattern (ya existe) |
| Input del jugador | `IPlayerInput` / `PlayerKeyboardInput` | Strategy |
| Efectos visuales | `PlayerEffectsController` | - |
| Detección de suelo | `GroundDetector` | - |
| Movimiento físico | `PlayerMovement` | - |

#### 3.2 Crear InputService
**Para abstraer el input de Unity**

```csharp
public interface IInputService
{
    float GetAxis(string axisName);
    bool GetKeyDown(KeyCode key);
}
```

**Patrones aplicados:**
- Single Responsibility Principle
- Strategy Pattern (para input)
- Composition over Inheritance

---

### 🟠 FASE 4: Refactorizar UI y Controllers (Riesgo Medio-Alto)
**Duración estimada:** 3-4 días  
**Objetivo:** Separar lógica de presentación usando MVP/MVC

#### 4.1 Implementar patrón MVP para Authenticator

**Estructura:**
```
UI/Authenticator/
├── Model/
│   └── AuthenticationModel.cs       # Datos y estado
├── View/
│   ├── ILoginView.cs               # Ya existe
│   └── LoginWindow.cs              # Ya existe
├── Presenter/
│   └── LoginPresenter.cs           # Nueva: lógica de UI
└── Controller/
    └── AuthenticatorController.cs  # Simplificado
```

#### 4.2 Extraer validación a servicio
- `IAuthenticationValidator` interface
- `AuthenticationValidator` ya existe, solo crear interface

#### 4.3 Refactorizar UICreator
- Eliminar Singleton estático
- Usar Factory Pattern registrado en ServiceLocator

**Patrones aplicados:**
- MVP (Model-View-Presenter)
- Factory Pattern
- Observer Pattern (ya existe con eventos)

---

### 🔴 FASE 5: Entidades y Contenedores (Riesgo Alto)
**Duración estimada:** 2-3 días  
**Objetivo:** Mejorar EntityContainer sin romper networking

#### 5.1 Refactorizar EntityContainer
**Eliminar Singleton y FindObjectOfType**

**Cambios:**
- Crear `IEntityRepository` interface
- Implementar `EntityRepository`
- Registrar en ServiceLocator
- Actualizar referencias en `CharacterCreator`, etc.

#### 5.2 Crear Entity Factory
```csharp
public interface IEntityFactory
{
    IEntity CreateLocalPlayer(int id, Vector2 position);
    IEntity CreateRemotePlayer(int id, string name, Vector2 position);
}
```

**Patrones aplicados:**
- Repository Pattern
- Factory Pattern
- Dependency Inversion

---

### 🟣 FASE 6: Pruebas y Documentación (Opcional pero Recomendado)
**Duración estimada:** 2-3 días

#### 6.1 Agregar pruebas unitarias
- Tests para `AuthenticationValidator`
- Tests para `JsonSaveService`
- Tests para `PlayerStateMachine`

#### 6.2 Documentación
- Diagrama de arquitectura actualizado
- README con instrucciones de uso de servicios

---

## ⚠️ CONSIDERACIONES IMPORTANTES

### Lo que NO se debe hacer (evitar sobreingeniería):
1. ❌ **NO usar Zenject/VContainer** - El proyecto es pequeño, ServiceLocator es suficiente
2. ❌ **NO crear demasiadas abstracciones** - Solo donde agregue valor real
3. ❌ **NO refactorizar el sistema de red** - Funciona y es crítico
4. ❌ **NO cambiar la estructura de mensajes** - Rompe compatibilidad con servidor
5. ❌ **NO crear capas excesivas** - Mantener simple

### Lo que SÍ se debe hacer:
1. ✅ **Hacer commits frecuentes** - Un commit por cada cambio significativo
2. ✅ **Probar después de cada fase** - Verificar que el juego funciona
3. ✅ **Mantener retrocompatibilidad** - Las APIs existentes deben seguir funcionando
4. ✅ **Documentar cambios** - Comentarios claros en código nuevo

### Riesgos por fase:

| Fase | Riesgo | Mitigación |
|------|--------|------------|
| 1 | 🟢 Bajo | Código nuevo, no modifica existente |
| 2 | 🟢 Bajo | Refactor gradual de providers |
| 3 | 🟡 Medio | Afecta gameplay, probar exhaustivamente |
| 4 | 🟡 Medio | Afecta UI, probar flujos de login |
| 5 | 🔴 Alto | Afecta entidades en red, probar multijugador |
| 6 | 🟢 Bajo | Solo agrega tests, no modifica código |

---

## 📝 CHECKLIST POR FASE

### Fase 1 - Fundamentos ✅ EN PROGRESO
- [x] Crear estructura de carpetas `Core/`
- [x] Implementar `ISaveService`
- [x] Implementar `JsonSaveService`
- [x] Implementar `PlayerPrefsSaveService` (fallback)
- [x] Implementar `ServiceLocator`
- [x] Implementar `ServiceLocatorInitializer`
- [x] Reemplazar PlayerPrefs en AuthenticatorController
- [ ] **PROBAR: Login/Logout funciona correctamente**

#### Archivos creados en Fase 1:
```
Scripts/Core/
├── Domain/
│   └── Interfaces/
│       └── ISaveService.cs
└── Infrastructure/
    ├── Persistence/
    │   ├── JsonSaveService.cs
    │   └── PlayerPrefsSaveService.cs
    ├── ServiceLocator.cs
    └── ServiceLocatorInitializer.cs
```

#### Instrucciones para probar:
1. Abrir Unity
2. En la escena inicial, crear un GameObject vacío llamado "Service Locator"
3. Agregar el componente `ServiceLocatorInitializer`
4. Ejecutar el juego y probar login
5. Verificar que el email se guarda correctamente

### Fase 2 - APIs
- [ ] Crear `IApiFactory` interface
- [ ] Refactorizar `ApiProvider`
- [ ] Registrar APIs en ServiceLocator
- [ ] **PROBAR: Conexión al servidor funciona**

### Fase 3 - Player
- [ ] Extraer `PlayerEffectsController`
- [ ] Extraer `GroundDetector`
- [ ] Crear `IInputService`
- [ ] Refactorizar `PlayerController`
- [ ] **PROBAR: Movimiento del jugador funciona**

### Fase 4 - UI
- [ ] Crear `LoginPresenter`
- [ ] Simplificar `AuthenticatorController`
- [ ] Crear `IAuthenticationValidator` interface
- [ ] Refactorizar `UICreator`
- [ ] **PROBAR: Flujo completo de autenticación**

### Fase 5 - Entidades
- [ ] Crear `IEntityRepository`
- [ ] Crear `IEntityFactory`
- [ ] Refactorizar `EntityContainer`
- [ ] Actualizar `CharacterCreator`
- [ ] **PROBAR: Spawn de jugadores local y remoto**

### Fase 6 - Tests
- [ ] Configurar NUnit en Unity
- [ ] Tests para servicios core
- [ ] Tests para validadores
- [ ] Documentación final

---

## 🚀 PRÓXIMOS PASOS

**Para comenzar con la Fase 1:**

1. Confirmar que la rama `develop` está lista
2. Crear la estructura de carpetas `Core/`
3. Implementar `ISaveService` y `JsonSaveService`
4. Implementar `ServiceLocator`
5. Probar que el juego sigue funcionando

