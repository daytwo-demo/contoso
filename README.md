# 🚀 DayTwo + IBM: .NET Contoso Inventory Demo (S2I)

> Demo comercial para equipos de preventa — ASP.NET Core 8 Minimal API desplegada en Red Hat OpenShift sin Dockerfile.

---

## Qué es S2I en OpenShift

**Source-to-Image (S2I)** es la magia que hace que OpenShift convierta tu código fuente directamente en un contenedor listo para producción, **sin que tú escribas ni un solo `FROM` en un Dockerfile**.

Así funciona por debajo, en lenguaje simple:

```
Tu repositorio GitHub
        │
        ▼
OpenShift detecta el archivo .csproj → "esto es .NET"
        │
        ▼
Descarga la imagen builder oficial de Red Hat (dotnet-8)
        │
        ▼
Corre `dotnet restore` y `dotnet publish` DENTRO de un contenedor temporal
        │
        ▼
Copia solo el output compilado a una imagen runtime mínima
        │
        ▼
Despliega el Pod con tu app corriendo en el puerto 8080
```

**Por qué importa:** El equipo de seguridad y operaciones no necesita confiar en que el desarrollador escribió un Dockerfile seguro. OpenShift usa imágenes base certificadas y parcheadas por Red Hat. Menos superficie de ataque, menos trabajo de auditoría, cumplimiento automático.

---

## Cómo hacer la Demo en 4 clics

### Prerequisito
Tener acceso a un clúster OpenShift (Developer Sandbox, ROSA, ARO, o On-Premise). El usuario debe estar en la **vista Developer** (toggle arriba a la izquierda).

### Pasos

**1. Vista Developer**
- En la consola web de OpenShift, asegúrate de estar en la perspectiva **"Developer"** (no "Administrator").
- Navega al proyecto/namespace donde quieres hacer la demo.

**2. `+Add` → `Import from Git`**
- Haz clic en el botón **"+Add"** en el menú lateral izquierdo.
- Selecciona la opción **"Import from Git"**.

**3. Pegar la URL del repositorio**
- En el campo **"Git Repo URL"**, pega la URL de este repositorio.
- OpenShift detectará automáticamente el archivo `.csproj` y seleccionará el builder **`dotnet`** de Red Hat.
- Verifica que aparezca **"Builder Image: .NET"** con el ícono de .NET.

**4. Marcar opciones y crear**
- Baja hasta la sección **"Advanced options"**.
- Asegúrate de que **"Create a route"** esté marcado (esto expone la app públicamente con HTTPS automático).
- Haz clic en **"Create"**.

OpenShift iniciará el build S2I. En ~2 minutos tendrás la app corriendo con una URL HTTPS pública.

---

## El Efecto Wow

Este es el momento clave de la demo. Así lo ejecutas:

### Setup
1. Abre la URL pública de la app en el navegador. Observa el **banner superior** que muestra el nombre del Pod actual (ej. `contoso-inventory-7d9f8c-xkp2b`).
2. En la tabla, agrega un equipo nuevo: **"Surface Pro 9"**, estado **"Asignado"**.

### El momento wow
3. Ve a la consola de OpenShift → vista **Topology** → haz clic en el círculo de tu deployment.
4. En el panel lateral, busca el campo **"Replicas"** y súbelo de **1 a 3** (o usa el comando: `oc scale deployment/contoso-inventory --replicas=3`).
5. Espera ~10 segundos a que los 3 Pods estén en estado `Running` (círculos azules).
6. Vuelve al navegador y **recarga la página varias veces** (F5 o Ctrl+R).

### Qué verá el cliente
El banner superior cambiará de nombre entre los 3 Pods en cada recarga:
```
Pod: contoso-inventory-7d9f8c-xkp2b  →  recarga  →
Pod: contoso-inventory-7d9f8c-mn4rt  →  recarga  →
Pod: contoso-inventory-7d9f8c-zw8lq
```

Esto demuestra **3 cosas en 30 segundos:**
- **Alta disponibilidad:** la app sigue corriendo aunque un Pod muera.
- **Balanceo de carga nativo:** OpenShift distribuye el tráfico sin configuración extra.
- **Escalabilidad instantánea:** de 1 a N réplicas con un slider, sin downtime.

> **Tip de presentación:** Pregunta al cliente "¿cuánto tiempo les tomaría hacer esto mismo en su infraestructura actual?" Luego di: *"Con OpenShift, fueron 10 segundos."*

---

## Reglas de Engagement

Cuándo llamar a **DayTwo** para profundizar en la conversación técnica y comercial:

| Escenario | Señal de compra | Mensaje DayTwo |
|-----------|----------------|----------------|
| **On-Premise** | "No podemos poner nada en la nube pública" | OpenShift On-Premise (OCP) sobre VMware/Bare Metal. Misma experiencia, dentro de su datacenter. |
| **Multi-nube** | "Tenemos AWS, Azure y un datacenter propio" | OpenShift corre igual en los tres. Un solo plano de control, múltiples nubes. |
| **Migración de Monolitos .NET Framework** | "Tenemos apps en .NET Framework 4.x que no podemos reescribir" | Windows Nodes en OpenShift + contenedores Windows. Lift-and-shift sin reescritura. |
| **Cumplimiento y Seguridad** | "El equipo de seguridad bloquea todo lo que no esté certificado" | Imágenes base RHEL certificadas, FIPS 140-2, políticas de red con NetworkPolicy, y SCC (Security Context Constraints). |
| **Equipos de Desarrollo Lentos** | "El ciclo dev-to-prod nos toma semanas" | OpenShift Pipelines (Tekton) + GitOps (ArgoCD) incluidos. El S2I que acabas de ver es el primer paso. |
| **Costos de Licenciamiento** | "Pagamos muchísimo en licencias de middleware" | JBoss EAP, ActiveMQ, y otras licencias de middleware vienen incluidas con la suscripción OpenShift. |
| **Kubernetes "difícil"** | "Probamos Kubernetes puro y fue muy complejo" | OpenShift es Kubernetes + la experiencia del operador. La consola, el S2I, y el operador de día 2 ya vienen resueltos. |

---

## Arquitectura del proyecto

```
contoso-demo/
├── ContosoInventory.csproj   ← EN LA RAÍZ (requerido por S2I)
├── Program.cs                ← Minimal API: todos los endpoints
├── wwwroot/
│   └── index.html            ← SPA: Tailwind CSS + Fetch API
└── README.md
```

## Endpoints de la API

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/info` | Nombre del Pod actual (`MachineName`) |
| `GET` | `/api/devices` | Lista todos los equipos |
| `POST` | `/api/devices` | Agrega un equipo `{"name": "...", "estado": "..."}` |
| `DELETE` | `/api/devices/{id}` | Elimina un equipo por ID |

## Notas técnicas

- Puerto configurado en `8080` directamente en `Program.cs` (`builder.WebHost.UseUrls("http://*:8080")`).
- Datos en memoria (`ConcurrentDictionary`). Sin base de datos. Al reiniciar el Pod, los datos se resetean — úsalo para mostrar la naturaleza stateless de los microservicios.
- El auto-refresh del banner de Pod corre cada 5 segundos en el frontend para maximizar el efecto de balanceo de carga durante la demo.
