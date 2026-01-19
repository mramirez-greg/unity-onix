# Instrucciones para Cambiar el Personaje del Cerdito por el Perro

## 🎨 Prompt para Gemini Nano Banana

### ⚠️ IMPORTANTE: Estados del Personaje a Generar

Antes de generar los sprites, debes tener en cuenta que necesitarás crear animaciones para **todos estos estados del personaje**:

1. **Idle** (Quieto/Reposo) - El perro está parado sin moverse
2. **Mover a la izquierda** - El perro camina lentamente hacia la izquierda
3. **Mover a la derecha** - El perro camina lentamente hacia la derecha
4. **Correr a la izquierda** - El perro corre rápidamente hacia la izquierda
5. **Correr a la derecha** - El perro corre rápidamente hacia la derecha
6. **Saltar** - El perro está en el aire, subiendo durante el salto
7. **Caer** - El perro está cayendo después del salto

**Nota importante sobre direcciones**: 
- En Unity, puedes usar el mismo sprite volteado horizontalmente para las direcciones izquierda y derecha usando `transform.localScale` (el código del Player.cs ya hace esto automáticamente).
- **Opción 1 (Más simple)**: Genera solo los sprites mirando hacia la derecha. Unity los volteará automáticamente cuando el personaje mire a la izquierda.
- **Opción 2 (Más realista)**: Genera sprites específicos para izquierda y derecha si quieres que se vea más detallado (por ejemplo, la lengua podría estar en diferentes posiciones según la dirección).

---

### Prompt Principal

Usa este prompt base para generar la imagen del perro (Boston Terrier):

```
Crea un sprite de personaje 2D estilo pixel art para videojuego de plataformas, de un Boston Terrier de perfil mirando hacia la derecha. El perro debe tener:
- Color café (marrón) con manchas blancas características de la raza
- Lengua fuera y visible, colgando hacia un lado
- Expresión feliz y juguetona
- Estilo pixel art consistente con sprites de videojuego 2D retro
- Tamaño similar a un personaje de 29x34 píxeles
- Vista lateral (perfil) para que pueda mirar izquierda y derecha
- Colores vibrantes pero no saturados
- Fondo transparente
- Diseño simple y limpio, fácil de animar
- Postura de pie, listo para correr o saltar
```

---

### Prompts Específicos por Estado

**1. Para animación de Idle (quieto/reposo):**
```
Sprite pixel art de Boston Terrier de perfil mirando a la derecha, color café con manchas blancas, lengua fuera, en posición de reposo completamente quieto, patas en el suelo, estilo videojuego 2D retro, fondo transparente, 29x34 píxeles aproximadamente. Necesito 4-5 frames para crear una animación sutil de respiración o parpadeo.
```

**2. Para animación de Mover/Caminar a la derecha:**
```
Sprite pixel art de Boston Terrier caminando de perfil mirando a la derecha, color café con manchas blancas, lengua fuera, patas en movimiento de caminata lenta, estilo videojuego 2D retro, fondo transparente. Necesito 4-6 frames mostrando el ciclo completo de caminata.
```

**3. Para animación de Mover/Caminar a la izquierda:**
```
Sprite pixel art de Boston Terrier caminando de perfil mirando a la izquierda, color café con manchas blancas, lengua fuera, patas en movimiento de caminata lenta, estilo videojuego 2D retro, fondo transparente. Necesito 4-6 frames mostrando el ciclo completo de caminata.
```

**4. Para animación de Correr a la derecha:**
```
Sprite pixel art de Boston Terrier corriendo rápidamente de perfil mirando a la derecha, color café con manchas blancas, lengua fuera y moviéndose, patas en movimiento rápido, cuerpo inclinado hacia adelante, estilo videojuego 2D retro, fondo transparente. Necesito 4-6 frames mostrando el ciclo completo de carrera.
```

**5. Para animación de Correr a la izquierda:**
```
Sprite pixel art de Boston Terrier corriendo rápidamente de perfil mirando a la izquierda, color café con manchas blancas, lengua fuera y moviéndose, patas en movimiento rápido, cuerpo inclinado hacia adelante, estilo videojuego 2D retro, fondo transparente. Necesito 4-6 frames mostrando el ciclo completo de carrera.
```

**6. Para animación de Saltar:**
```
Sprite pixel art de Boston Terrier saltando de perfil mirando a la derecha, color café con manchas blancas, lengua fuera, patas extendidas hacia abajo, cuerpo en posición de salto ascendente, estilo videojuego 2D retro, fondo transparente. Necesito 2-3 frames mostrando el inicio del salto.
```

**7. Para animación de Caer:**
```
Sprite pixel art de Boston Terrier cayendo de perfil mirando a la derecha, color café con manchas blancas, lengua fuera, patas extendidas hacia abajo, cuerpo en posición de caída, estilo videojuego 2D retro, fondo transparente. Necesito 1-2 frames mostrando la caída.
```

---

## 📝 Instrucciones para Cambiar los Sprites en Unity

### Paso 1: Generar las Imágenes del Perro

1. Usa los prompts específicos de Gemini Nano Banana para generar las imágenes del perro
2. **DEBES generar sprites para TODOS estos estados del personaje:**
   - **Idle** (quieto/reposo): 4-5 frames - El perro parado sin moverse
   - **Mover a la izquierda** (caminar): 4-6 frames - El perro caminando lentamente hacia la izquierda
   - **Mover a la derecha** (caminar): 4-6 frames - El perro caminando lentamente hacia la derecha
   - **Correr a la izquierda**: 4-6 frames - El perro corriendo rápidamente hacia la izquierda
   - **Correr a la derecha**: 4-6 frames - El perro corriendo rápidamente hacia la derecha
   - **Saltar**: 2-3 frames - El perro en el aire, subiendo durante el salto
   - **Caer**: 1-2 frames - El perro cayendo después del salto

3. **Organización recomendada**: Guarda todas las imágenes generadas organizadas por estado en subcarpetas:
   - `Assets/sprites/Perro/Idle/` - Frames de idle
   - `Assets/sprites/Perro/Mover_Izquierda/` - Frames de caminar izquierda
   - `Assets/sprites/Perro/Mover_Derecha/` - Frames de caminar derecha
   - `Assets/sprites/Perro/Correr_Izquierda/` - Frames de correr izquierda
   - `Assets/sprites/Perro/Correr_Derecha/` - Frames de correr derecha
   - `Assets/sprites/Perro/Saltar/` - Frames de salto
   - `Assets/sprites/Perro/Caer/` - Frames de caída

   **Alternativa más simple**: Si prefieres, puedes guardar todo en `Assets/sprites/Perro/` pero nombra los archivos claramente (ej: `perro_idle_01.png`, `perro_correr_derecha_01.png`, etc.)

### Paso 2: Importar las Imágenes a Unity

1. Abre Unity y navega a la carpeta donde guardaste las imágenes del perro
2. Si no existe, crea una nueva carpeta: `Assets/sprites/Perro/`
3. Arrastra las imágenes del perro a esta carpeta en Unity
4. Unity importará automáticamente las imágenes

### Paso 3: Configurar los Sprites Importados

Para cada imagen importada:

1. Selecciona la imagen en Unity
2. En el Inspector, cambia el **Texture Type** a `Sprite (2D and UI)`
3. Si tienes múltiples frames en una sola imagen:
   - Cambia **Sprite Mode** a `Multiple`
   - Haz clic en **Sprite Editor**
   - En el Sprite Editor, haz clic en **Slice** → **Grid By Cell Count** o **Automatic**
   - Ajusta el número de columnas y filas según tus frames
   - Haz clic en **Apply**
4. Si cada frame es una imagen separada:
   - Mantén **Sprite Mode** en `Single`
   - Ajusta el **Pixels Per Unit** (normalmente 100 es un buen valor)

### Paso 4: Crear las Animaciones del Perro

1. Ve a la carpeta `Assets/Animations/Player/`
2. **Crea una animación para CADA estado del personaje:**
   - `Perro_Idle.anim` - Animación de reposo
   - `Perro_Mover_Izquierda.anim` - Animación de caminar izquierda
   - `Perro_Mover_Derecha.anim` - Animación de caminar derecha
   - `Perro_Correr_Izquierda.anim` - Animación de correr izquierda
   - `Perro_Correr_Derecha.anim` - Animación de correr derecha
   - `Perro_Saltar.anim` - Animación de salto
   - `Perro_Caer.anim` - Animación de caída

3. Para cada animación:

   **Opción A: Crear animación desde sprites individuales**
   - Selecciona todos los sprites de una animación (por ejemplo, todos los frames de "Idle")
   - Arrástralos a la escena o al Hierarchy
   - Unity te preguntará dónde guardar la animación
   - Guárdala en `Assets/Animations/Player/` con el nombre apropiado (ej: `Perro_Idle.anim`)
   - Elimina el GameObject que Unity creó automáticamente (solo necesitábamos la animación)

   **Opción B: Crear animación manualmente**
   - Crea un nuevo Animation Clip: Click derecho en `Assets/Animations/Player/` → Create → Animation
   - Nómbralo (ej: `Perro_Idle.anim`)
   - Abre la ventana Animation (Window → Animation → Animation)
   - Selecciona el personaje en la escena
   - En la ventana Animation, haz clic en el botón de grabación (círculo rojo)
   - Arrastra los sprites al timeline en el orden correcto
   - Ajusta el tiempo entre frames (normalmente 0.1 segundos)
   - Detén la grabación

### Paso 5: Actualizar el Animator Controller

1. Abre el Animator Controller del jugador: `Assets/Animations/Player/Player.controller`
2. En la ventana Animator, necesitarás configurar las transiciones entre todos los estados:
   - **Idle** → Se activa cuando el personaje está quieto
   - **Mover Izquierda/Derecha** → Se activa cuando el personaje camina lentamente
   - **Correr Izquierda/Derecha** → Se activa cuando el personaje corre rápidamente
   - **Saltar** → Se activa cuando el personaje está subiendo en el salto
   - **Caer** → Se activa cuando el personaje está cayendo

3. Para cada estado de animación:
   - Selecciona el estado en el Animator
   - En el Inspector, cambia el **Motion** para que apunte a la animación correspondiente del perro
   - Configura las transiciones basadas en los parámetros del Animator (Speed, VerticalVelocity, IsGrounded)

4. **Consejo**: Puedes usar el parámetro `Speed` del Animator para determinar si el personaje está quieto, caminando o corriendo. Si `Speed` es 0 → Idle, si es bajo → Mover, si es alto → Correr.

### Paso 6: Reemplazar el Sprite en el Prefab

**Método 1: Cambiar el Sprite directamente en el Prefab**

1. Abre el prefab del personaje: `Assets/Prefab/character_berie.prefab`
2. Selecciona el GameObject principal del personaje
3. En el Inspector, busca el componente **Sprite Renderer**
4. En el campo **Sprite**, arrastra el sprite del perro (el frame inicial, normalmente el de Idle)
5. Guarda el prefab (Ctrl+S o Cmd+S)

**Método 2: Cambiar el Sprite en la Escena**

1. Abre la escena: `Assets/Scenes/SampleScene.unity`
2. Selecciona el personaje en la Hierarchy
3. En el Inspector, busca el componente **Sprite Renderer**
4. Cambia el **Sprite** al sprite del perro
5. Si el personaje es un prefab, Unity te preguntará si quieres aplicar los cambios al prefab → Selecciona **Apply**

### Paso 7: Verificar que Todo Funcione

1. Presiona Play en Unity
2. Verifica que:
   - El perro aparece en lugar del cerdito
   - **Todas las animaciones funcionan correctamente:**
     - Idle cuando está quieto
     - Mover izquierda/derecha cuando camina lentamente
     - Correr izquierda/derecha cuando corre rápidamente
     - Saltar cuando salta
     - Caer cuando está cayendo
   - El perro se voltea correctamente al cambiar de dirección
   - El perro se mueve correctamente en todas las direcciones
   - El tamaño es apropiado (no muy grande ni muy pequeño)

### Paso 8: Ajustar el Tamaño si es Necesario

Si el perro es muy grande o muy pequeño:

1. Selecciona el personaje en la escena o en el prefab
2. En el Transform, ajusta la **Scale** (normalmente X: 1, Y: 1, Z: 1)
   - Si es muy grande, reduce a 0.8, 0.9, etc.
   - Si es muy pequeño, aumenta a 1.1, 1.2, etc.
3. También puedes ajustar el **Pixels Per Unit** de los sprites importados

---

## 🎯 Consejos Adicionales

### Para Mejorar las Animaciones:

1. **Consistencia de tamaño**: Asegúrate de que todos los sprites del perro tengan el mismo tamaño
2. **Punto de pivote**: Verifica que el punto de pivote (pivot point) esté en la parte inferior del sprite, donde toca el suelo
3. **Frames por segundo**: Las animaciones suelen funcionar bien con 10-12 FPS para pixel art

### Si las Animaciones No Funcionan:

1. Verifica que el Animator Controller tenga las animaciones correctas asignadas
2. Asegúrate de que los parámetros del Animator (Speed, VerticalVelocity, IsGrounded) estén configurados correctamente
3. Revisa que las transiciones entre estados de animación estén bien configuradas

### Para Agregar la Familia al Final:

1. Genera sprites de la familia usando prompts similares:
   - "Sprite pixel art de papá de perfil, estilo videojuego 2D retro"
   - "Sprite pixel art de mamá de perfil, estilo videojuego 2D retro"
   - "Sprite pixel art de niño/hijo de perfil, estilo videojuego 2D retro"

2. Importa los sprites siguiendo los mismos pasos
3. Crea GameObjects vacíos en la escena al final del nivel
4. Agrega un SpriteRenderer a cada uno y asigna el sprite correspondiente
5. Opcional: Crea un script que detecte cuando el perro llega al final y muestre un mensaje o activé la familia

---

## 📚 Recursos Útiles

- **Documentación de Unity sobre Sprites**: https://docs.unity3d.com/Manual/Sprites.html
- **Documentación sobre Animaciones**: https://docs.unity3d.com/Manual/AnimationSection.html
- **Sprite Editor de Unity**: Window → 2D → Sprite Editor

---

¡Buena suerte con el proyecto! 🐕🎮
