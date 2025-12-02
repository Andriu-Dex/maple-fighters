Aquí tienes **todos los requisitos del proyecto**, organizados de forma clara y basados exactamente en el documento que entregaste.
(Referencia: )

---

# ✅ **REQUISITOS DEL PROYECTO FINAL – Patrones de Software**

## **1. Tema General del Proyecto**

Desarrollar **un juego desde cero** o **refactorizar totalmente un juego existente** en Unity o cualquier otro framework C#, **aplicando Clean Architecture, SOLID y Patrones de Diseño**.
()

---

# 🧩 **2. Objetivos que debes cumplir**

1. Aplicar correctamente los **principios SOLID**.
2. Implementar **Clean Architecture** dentro del proyecto Unity/C#.
3. Utilizar **múltiples patrones de diseño** (Factory, Strategy, Observer, Singleton, State, Command…).
4. Asegurar buenas prácticas de diseño y código limpio.
5. Aplicar **Inversión de Dependencia (IoC)** y DI (Zenject opcional).
6. Fomentar un código modular, escalable y reutilizable.
   ()

---

# 📝 **3. Actividades que deben desarrollarse**

### **3.1 Análisis del juego base**

* Revisar código original.
* Identificar malas prácticas:

  * Acoplamiento fuerte
  * Código duplicado
  * Violaciones SRP
  * Clases gigantes
  * Problemas de arquitectura
* Documentar todos los *code smells* encontrados.
  ()

---

### **3.2 Propuesta de nueva arquitectura**

* Diseñar una **Clean Architecture** (Hexagonal u Onion).
* Crear **diagrama de capas**.
* Planificar la migración por módulos (ej. Player → Enemy → UI → Spawner).
  ()

---

### **3.3 Refactorización por etapas**

* Aplicar patrones de diseño donde corresponda.
* Separar lógica de presentación.
* Introducir interfaces y servicios.
* Reemplazar Find / GetComponent mal usados.
  ()

---

### **3.4 Mejoras opcionales**

* Inyección de dependencias con Zenject.
* Object Pooling.
* Pruebas unitarias con NUnit.
* Documentación del *antes vs después*.
  ()

---

# 🧱 **4. Arquitectura esperada**

Se espera una estructura similar a esta:

```
/Game
├── Domain/
├── Application/
├── Infrastructure/
└── Presentation/
```

Donde cada capa debe tener **responsabilidad única y cero acoplamiento**.
()

---

# 🎮 **5. Persistencia de datos**

Se debe reemplazar PlayerPrefs por:

* JsonSaveService
* SQLite
* ScriptableObjects
* O cualquier servicio que implemente ISaveService
  ()

---

# 📦 **6. Entregables Obligatorios**

## **6.1 Proyecto refactorizado completo**

Debe incluir:

* Estructura en capas (Clean Architecture).
* Persistencia de datos.
* Clases desacopladas y comentadas.
* Proyecto funcional.
  ()

---
