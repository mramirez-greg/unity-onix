# Prompts de arte — "Onix: El Rescate de la Familia"

Estos prompts generan sprites **pixel-art** coherentes con Onix (mismo estilo, misma
escala aproximada). Genera cada imagen con **fondo liso de un solo color** (blanco o
magenta puro `#FF00FF`) o transparente: el pipeline de importación
(`Onix/Familia → Importar sprites de familia y villano`) quita el fondo por flood-fill
desde los bordes y recorta los frames por componentes conexos, igual que con Onix.

## Estilo base (incluir en TODOS los prompts)

pixel art sprite, 2D side-scroller platformer character, clean outline, flat cel
shading, limited palette, facing right, full body, centered, **plain solid white
background**, no shadow on the ground, consistent with a cute Boston Terrier dog hero
named Onix, same art style and proportions.

Consejos:
- Pide **un solo personaje por imagen** (o una fila de frames de idle, bien separados).
- Para animación de idle: "a horizontal row of 2–3 idle animation frames, evenly spaced,
  same character, same size".
- Mantén **alturas parecidas** entre familiares para que el `PixelsPerUnit` calculado sea
  consistente.

---

## Grego (hijo) — Nivel 1 (Playa)
Gorra gris, camiseta de "Cartoon Network", juguetón y veloz, perrito joven.

- **Idle:** `... a young playful Boston Terrier puppy wearing a grey cap and a cartoon
  t-shirt, happy energetic pose, idle stance, plain solid white background.`
- **Rescatado/feliz:** `... same puppy jumping with joy, arms/paws up, big smile,
  plain solid white background.`

## Lilo (mamá) — Nivel 2 (Selva de palmeras)
Lentes de sol, blusa rosa, shorts floreados, collar. Astuta y cariñosa.

- **Idle:** `... a female Boston Terrier wearing sunglasses, a pink blouse, floral shorts
  and a necklace, gentle confident pose, idle stance, plain solid white background.`
- **Rescatado/feliz:** `... same character waving happily, relieved smile, plain solid
  white background.`

## Mao (papá) — Nivel 3 (Muelle / Tormenta)
Gorra azul, lentes negros, camiseta vinotinto. Protector.

- **Idle:** `... a strong male Boston Terrier wearing a blue cap, black sunglasses and a
  dark red (maroon) t-shirt, protective confident stance, idle pose, plain solid white
  background.`
- **Rescatado/feliz:** `... same character with a proud relieved smile, paw raised,
  plain solid white background.`

## Kiwi (villana) — gata mandona
Antagonista cómica y caricaturesca: una gata mandona, gesto autoritario y altivo.

- **Idle:** `... a cartoonish bossy female cat villain named Kiwi, hands on hips, bossy
  haughty expression, arched eyebrow, small crown or fancy collar, mischievous grin,
  idle pose, plain solid white background, same pixel art style.`

## Hueso (coleccionable, reskin de la moneda)
- `... a single cartoon dog bone collectible item, pixel art, shiny, plain solid white
  background, centered, small icon size, same style as the game's coins.`

---

## Flujo de importación
1. Genera los PNG/JPEG y colócalos en `Assets/sprites/Familia/` (un archivo por
   personaje, o una hoja con su fila de frames).
2. Menú **`Onix/Familia → Importar sprites de familia y villano`**: limpia el fondo,
   recorta y los importa como Sprite (estático o 2–3 frames de idle).
3. Asigna cada sprite al `SpriteRenderer` del `FamilyMember` correspondiente y, para el
   hueso, haz reskin del `Coin.prefab`.
