# Cambiar el personaje (cerdito → perrito Onix) y arreglar el juego

Este proyecto ya incluye un **script automático de Unity Editor** (`Assets/Editor/OnixTools.cs`)
que hace todo el trabajo. No necesitas generar nada con IA ni recortar sprites a mano: el
spritesheet del perro ya existe en `Onix.jpeg` (estados IDLE, WALK, RUN, JUMP, FALL).

## Qué se arregló en el código

- **La cámara no seguía al jugador** → el script asigna `CameraFollow` a la `Main Camera`.
- **Errores `NullReferenceException`** al recoger monedas/barriles → `Player.cs` ahora es
  null-safe, y el script crea/asigna `AudioSource` y un contador de monedas (TextMeshPro).

## Cómo usarlo (en Unity)

1. Abre el proyecto en Unity y abre la escena `Assets/Scenes/SampleScene.unity`.
2. En la barra de menús superior aparecerá un menú **`Onix`**. Ejecuta sus opciones **en orden**:

   - **`Onix → 1 - Arreglar configuracion del juego`**
     Asigna la cámara que sigue al jugador, añade el `AudioSource`, crea el contador de monedas
     y enlaza las referencias del script `Player`. Guarda la escena.

   - **`Onix → 2 - Convertir Onix.jpeg a PNG transparente`**
     Toma `Onix.jpeg` (que es un JPEG con fondo a cuadros) y genera
     `Assets/sprites/Onix/onix_sheet.png` con **fondo transparente**, eliminando el cuadriculado
     mediante un *flood-fill* desde los bordes (el blanco interior del perro se conserva).

   - **`Onix → 3 - Generar sprites y animaciones del perro`**
     Recorta el spritesheet automáticamente (detecta cada frame por componentes conexos, ignora
     el texto de las etiquetas), crea los sprites con pivote en los pies, **reescribe los clips
     `Idle`, `Run`, `Jump` y `Fall`** con los frames del perro y pone al perro en el prefab
     `character_berie`, ajustando su collider al nuevo tamaño.

   - **`Onix → 4 - Colocar monedas y obstaculos de prueba`** *(opcional)*
     Coloca 3 monedas, 1 barril y 1 pincho cerca del jugador (apoyados sobre el piso) para poder
     probar las mecánicas: el contador de monedas, el empujón del barril y el reinicio al tocar
     un pincho. Los agrupa en un objeto `TestObjects` que puedes borrar o mover cuando quieras.
     El prefab del barril ya quedó etiquetado como `Barrel`.

3. Pulsa **Play**. Deberías ver al perro moverse (A/D o flechas), saltar (Espacio) y que la
   cámara lo sigue. Si ejecutaste el paso 4, camina hacia la derecha para recoger monedas y
   chocar con el barril/pincho.

## Ajustes finos

- **Tamaño del perro**: si queda muy grande o pequeño, cambia `TargetHeightUnits` (arriba en
  `OnixTools.cs`, por defecto `0.6` unidades) y vuelve a ejecutar `Onix → 3`; o ajusta la
  `Scale` del prefab `character_berie`.
- **Velocidad de las animaciones**: cambia los segundos por frame en las llamadas `WriteClip(...)`
  dentro de `GenerateDog()` (idle 0.12, run 0.08, etc.).
- **Recorte del fondo**: si quedaran bordes sucios (por ser JPEG), baja la tolerancia en
  `IsBackground(...)` o usa un PNG con transparencia real como `Assets/sprites/Onix/onix_sheet.png`
  y ejecuta solo `Onix → 3`.

## Archivo de previsualización

En la raíz del proyecto se dejó `onix_preview.png` para que veas cómo queda el recorte del fondo
antes de abrir Unity. No forma parte del juego (está fuera de `Assets/`) y puedes borrarlo.

---

> El menú `Onix` solo aparece cuando los scripts compilan sin errores. Si no lo ves, abre la
> **Console** de Unity (Window → General → Console) y corrige cualquier error en rojo.
