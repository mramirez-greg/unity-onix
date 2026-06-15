using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Herramientas para (1) arreglar la configuración de la escena, (2) limpiar el fondo de
/// Onix.jpeg y (3) generar los sprites y animaciones del perro automáticamente.
/// Ejecuta los menús en orden desde la barra superior de Unity: Onix → 1, 2, 3.
/// </summary>
public static class OnixTools
{
    const string OutDir = "Assets/sprites/Onix";
    const string PngPath = "Assets/sprites/Onix/onix_sheet.png";
    const string PrefabPath = "Assets/Prefab/character_berie.prefab";
    const string AnimDir = "Assets/Animations/Player";

    // Estados esperados, en el orden vertical (de arriba hacia abajo) del spritesheet.
    static readonly string[] RowStates = { "idle", "walk", "run", "jump", "fall" };

    // Alto deseado del perro en unidades de mundo (ajustable si queda muy grande/pequeño).
    // El cerdito original medía ~0.34; 0.4 lo deja un poco más grande pero proporcionado.
    const float TargetHeightUnits = 0.4f;

    // ------------------------------------------------------------------ 1. CONFIG

    [MenuItem("Onix/1 - Arreglar configuracion del juego")]
    static void FixSetup()
    {
        try
        {
            var player = Object.FindFirstObjectByType<Player>();
            if (player == null)
            {
                EditorUtility.DisplayDialog("Onix", "No encontré el objeto con el script Player en la escena abierta. Abre SampleScene primero.", "OK");
                return;
            }
            var playerGO = player.gameObject;

            // --- Cámara que sigue al jugador ---
            var cam = Camera.main;
            if (cam == null) cam = Object.FindFirstObjectByType<Camera>();
            if (cam != null)
            {
                var follow = cam.GetComponent<CameraFollow>();
                if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();
                follow.target = playerGO.transform;
                EditorUtility.SetDirty(follow);
            }

            // --- AudioSource en el jugador ---
            var audio = playerGO.GetComponent<AudioSource>();
            if (audio == null) audio = playerGO.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            player.audioSource = audio;

            // --- Clips de sonido (si existen en el proyecto) ---
            if (player.coinClip == null) player.coinClip = FindAudioClip("coin", "moneda");
            if (player.barrelClip == null) player.barrelClip = FindAudioClip("barrel", "barril", "explos");

            // --- UI del contador de monedas ---
            if (player.textCoins == null)
            {
                var tmp = CreateCoinUI();
                if (tmp != null) player.textCoins = tmp;
            }

            EditorUtility.SetDirty(player);
            EditorSceneManager.MarkSceneDirty(playerGO.scene);
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Onix] Configuración arreglada: cámara que sigue, AudioSource y referencias asignadas.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Onix] Error en paso 1: " + e);
        }
    }

    static TMP_Text CreateCoinUI()
    {
        var existing = GameObject.Find("CoinText");
        if (existing != null)
        {
            var t = existing.GetComponent<TMP_Text>();
            if (t != null) return t;
        }

        var canvasGO = GameObject.Find("CoinCanvas");
        if (canvasGO == null)
        {
            canvasGO = new GameObject("CoinCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        }

        var textGO = new GameObject("CoinText", typeof(TextMeshProUGUI));
        textGO.transform.SetParent(canvasGO.transform, false);
        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = "0";
        tmp.fontSize = 36;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        var rt = tmp.rectTransform;
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(20, -20);
        rt.sizeDelta = new Vector2(200, 60);
        return tmp;
    }

    static AudioClip FindAudioClip(params string[] keywords)
    {
        foreach (var guid in AssetDatabase.FindAssets("t:AudioClip"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var name = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            if (keywords.Any(k => name.Contains(k.ToLowerInvariant())))
                return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
        return null;
    }

    // ----------------------------------------------------- 4. OBJETOS DE PRUEBA

    [MenuItem("Onix/4 - Colocar monedas y obstaculos de prueba")]
    static void PlaceTestObjects()
    {
        try
        {
            var player = Object.FindFirstObjectByType<Player>();
            if (player == null)
            {
                EditorUtility.DisplayDialog("Onix", "Abre SampleScene (con el jugador) primero.", "OK");
                return;
            }
            Vector3 p = player.transform.position;

            // Buscar la altura del piso con un raycast hacia abajo (capa Floor = 3); si no, usar al jugador.
            float groundY = p.y;
            int floorMask = 1 << LayerMask.NameToLayer("Floor");
            var hit = Physics2D.Raycast(new Vector2(p.x, p.y + 3f), Vector2.down, 10f, floorMask);
            if (hit.collider != null) groundY = hit.point.y;

            var parent = GameObject.Find("TestObjects") ?? new GameObject("TestObjects");

            Place("Assets/Prefab/Coin.prefab", new Vector3(p.x + 0.8f, groundY + 0.35f, 0f), parent.transform);
            Place("Assets/Prefab/Coin.prefab", new Vector3(p.x + 1.2f, groundY + 0.35f, 0f), parent.transform);
            Place("Assets/Prefab/Coin.prefab", new Vector3(p.x + 1.6f, groundY + 0.35f, 0f), parent.transform);
            Place("Assets/Prefab/object_barrel_light_0.prefab", new Vector3(p.x + 2.3f, groundY + 0.12f, 0f), parent.transform);
            Place("Assets/Prefab/trap_spike.prefab", new Vector3(p.x + 3.1f, groundY + 0.05f, 0f), parent.transform);

            EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"[Onix] Coloqué 3 monedas, 1 barril y 1 pincho cerca del jugador (pisoY={groundY:0.00}).");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Onix] Error en paso 4: " + e);
        }
    }

    static void Place(string prefabPath, Vector3 pos, Transform parent)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) { Debug.LogWarning($"[Onix] No encontré {prefabPath}."); return; }
        var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        inst.transform.position = pos;
        if (parent != null) inst.transform.SetParent(parent, true);
    }

    // ------------------------------------------------------------ 2. QUITAR FONDO

    [MenuItem("Onix/2 - Convertir Onix.jpeg a PNG transparente")]
    static void ConvertJpeg()
    {
        try
        {
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            var src = Path.Combine(projectRoot, "Onix.jpeg");
            if (!File.Exists(src))
            {
                EditorUtility.DisplayDialog("Onix", "No encontré Onix.jpeg en la raíz del proyecto.", "OK");
                return;
            }

            // OJO: LoadImage con un JPEG (sin alfa) convierte la textura a RGB24 y descarta el
            // canal alfa. Por eso leemos los píxeles aquí y luego los escribimos en una textura
            // NUEVA RGBA32, para que la transparencia sí se conserve al guardar el PNG.
            var src0 = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            src0.LoadImage(File.ReadAllBytes(src));
            int w = src0.width, h = src0.height;
            var px = src0.GetPixels32();

            // Flood-fill desde los bordes: vuelve transparente todo el fondo (cuadriculado claro)
            // conectado al borde. El blanco interior del perro queda intacto porque está rodeado
            // por su contorno oscuro y no se alcanza desde el borde.
            var isBg = new bool[w * h];
            var stack = new Stack<int>();

            void Seed(int x, int y)
            {
                int i = y * w + x;
                if (!isBg[i] && IsBackground(px[i])) { isBg[i] = true; stack.Push(i); }
            }
            for (int x = 0; x < w; x++) { Seed(x, 0); Seed(x, h - 1); }
            for (int y = 0; y < h; y++) { Seed(0, y); Seed(w - 1, y); }

            while (stack.Count > 0)
            {
                int i = stack.Pop();
                int x = i % w, y = i / w;
                if (x > 0) Seed(x - 1, y);
                if (x < w - 1) Seed(x + 1, y);
                if (y > 0) Seed(x, y - 1);
                if (y < h - 1) Seed(x, y + 1);
            }

            int removed = 0;
            for (int i = 0; i < px.Length; i++)
            {
                if (isBg[i]) { px[i].a = 0; removed++; }
            }

            // Textura nueva RGBA32 para conservar la transparencia en el PNG.
            var outTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            outTex.SetPixels32(px);
            outTex.Apply();

            Directory.CreateDirectory(Path.Combine(projectRoot, OutDir.Replace('/', Path.DirectorySeparatorChar)));
            File.WriteAllBytes(Path.Combine(projectRoot, PngPath.Replace('/', Path.DirectorySeparatorChar)), outTex.EncodeToPNG());
            AssetDatabase.Refresh();
            Debug.Log($"[Onix] PNG transparente creado en {PngPath} ({removed} píxeles de fondo eliminados de {w}x{h}).");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Onix] Error en paso 2: " + e);
        }
    }

    static bool IsBackground(Color32 c)
    {
        int mx = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
        int mn = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
        // Claro y poco saturado = cuadriculado de fondo (blanco o gris claro).
        return mx >= 170 && (mx - mn) <= 30;
    }

    // ------------------------------------------------------- 3. SPRITES Y ANIMACIONES

    [MenuItem("Onix/3 - Generar sprites y animaciones del perro")]
    static void GenerateDog()
    {
        try
        {
            if (!File.Exists(Path.Combine(Directory.GetParent(Application.dataPath).FullName, PngPath.Replace('/', Path.DirectorySeparatorChar))))
            {
                EditorUtility.DisplayDialog("Onix", "Primero ejecuta 'Onix → 2' para crear el PNG.", "OK");
                return;
            }

            // Importador como sprite múltiple y legible para poder analizar los píxeles.
            var importer = (TextureImporter)AssetImporter.GetAtPath(PngPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.isReadable = true;
            importer.SaveAndReimport();

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(PngPath);
            int w = tex.width, h = tex.height;
            var px = tex.GetPixels32();

            // Detectar frames como componentes conexos (8-vecinos) de píxeles opacos.
            var (labels, allBoxes) = FindComponents(px, w, h);
            if (allBoxes.Count == 0)
            {
                EditorUtility.DisplayDialog("Onix", "No detecté frames opacos. ¿Quedó el PNG totalmente transparente?", "OK");
                return;
            }

            // Quedarnos solo con los componentes grandes (los perros); descartar el texto de las
            // etiquetas (IDLE / WALK RIGHT / ...) y cualquier mancha pequeña.
            float maxArea = allBoxes.Max(b => b.Area);
            var boxes = allBoxes.Where(b => b.Area >= maxArea * 0.12f && b.W >= 16 && b.H >= 16).ToList();
            if (boxes.Count == 0)
            {
                EditorUtility.DisplayDialog("Onix", "Tras filtrar no quedaron frames válidos. Revisa el PNG.", "OK");
                return;
            }

            // Borrar de la imagen todo lo que NO pertenezca a un frame válido (así el texto de las
            // etiquetas no aparece dentro del rectángulo de ningún perro) y reimportar el PNG limpio.
            var keep = new HashSet<int>(boxes.Select(b => b.Label));
            var clean = new Color32[px.Length];
            for (int i = 0; i < px.Length; i++)
                clean[i] = (labels[i] >= 0 && keep.Contains(labels[i])) ? px[i] : new Color32(0, 0, 0, 0);

            var cleanTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            cleanTex.SetPixels32(clean);
            cleanTex.Apply();
            File.WriteAllBytes(
                Path.Combine(Directory.GetParent(Application.dataPath).FullName, PngPath.Replace('/', Path.DirectorySeparatorChar)),
                cleanTex.EncodeToPNG());
            AssetDatabase.ImportAsset(PngPath, ImportAssetOptions.ForceUpdate);
            importer = (TextureImporter)AssetImporter.GetAtPath(PngPath);

            // Agrupar en filas (por cercanía vertical) y ordenar.
            var rows = GroupRows(boxes);
            if (rows.Count == 0)
            {
                EditorUtility.DisplayDialog("Onix", "No pude agrupar los frames en filas.", "OK");
                return;
            }

            // Construir los rects de sprite con pivote inferior-centro.
            var metas = new List<SpriteMetaData>();
            var perState = new Dictionary<string, List<string>>();
            float tallestIdle = 0f;
            for (int r = 0; r < rows.Count; r++)
            {
                string state = r < RowStates.Length ? RowStates[r] : "extra" + r;
                perState[state] = new List<string>();
                var frames = rows[r].OrderBy(b => b.MinX).ToList();
                for (int f = 0; f < frames.Count; f++)
                {
                    var b = frames[f];
                    string name = $"onix_{state}_{f + 1}";
                    metas.Add(new SpriteMetaData
                    {
                        name = name,
                        rect = new Rect(b.MinX, b.MinY, b.W, b.H),
                        alignment = (int)SpriteAlignment.BottomCenter,
                        pivot = new Vector2(0.5f, 0f),
                        border = Vector4.zero,
                    });
                    perState[state].Add(name);
                    if (state == "idle") tallestIdle = Mathf.Max(tallestIdle, b.H);
                }
            }

            // Pixels-per-unit para que el idle mida ~TargetHeightUnits de alto.
            if (tallestIdle < 1f) tallestIdle = rows[0].Max(b => b.H);
            float ppu = Mathf.Round(tallestIdle / TargetHeightUnits);
            ppu = Mathf.Clamp(ppu, 16f, 512f);

            importer.spritePixelsPerUnit = ppu;
#pragma warning disable CS0618
            importer.spritesheet = metas.ToArray();
#pragma warning restore CS0618
            importer.SaveAndReimport();

            // Cargar los sub-sprites recortados.
            var sprites = AssetDatabase.LoadAllAssetsAtPath(PngPath).OfType<Sprite>()
                .ToDictionary(s => s.name, s => s);

            Sprite[] FramesFor(string state)
            {
                if (!perState.TryGetValue(state, out var names) || names.Count == 0) return null;
                return names.Where(sprites.ContainsKey).Select(n => sprites[n]).ToArray();
            }

            var idle = FramesFor("idle");
            var run = FramesFor("run") ?? FramesFor("walk");
            var jump = FramesFor("jump");
            var fall = FramesFor("fall");

            // Reescribir los clips existentes (el Player.controller ya los referencia).
            WriteClip($"{AnimDir}/Idle.anim", idle, 0.12f, true);
            WriteClip($"{AnimDir}/Run.anim", run, 0.08f, true);
            WriteClip($"{AnimDir}/Jump.anim", jump, 0.12f, false);
            WriteClip($"{AnimDir}/Fall.anim", fall, 0.12f, false);

            // Poner el primer frame idle en el prefab y ajustar el collider al tamaño del perro.
            if (idle != null && idle.Length > 0)
                ApplyToPrefab(idle[0], ppu);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            int total = metas.Count;
            Debug.Log($"[Onix] Listo: {rows.Count} filas, {total} sprites. PPU={ppu}. " +
                      $"Frames -> idle:{Count(idle)} run:{Count(run)} jump:{Count(jump)} fall:{Count(fall)}.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Onix] Error en paso 3: " + e);
        }
    }

    static int Count(Sprite[] s) => s == null ? 0 : s.Length;

    // ----------------------------------------------------------------- utilidades

    class Box
    {
        public int Label;
        public int MinX, MinY, MaxX, MaxY;
        public int W => MaxX - MinX + 1;
        public int H => MaxY - MinY + 1;
        public float Area => W * H;
        public float CenterY => (MinY + MaxY) * 0.5f;
    }

    // Etiqueta cada componente conexo. Devuelve el mapa de etiquetas (por píxel; -1 = vacío)
    // y la lista de cajas. La etiqueta permite luego borrar todo lo que no sea un frame válido.
    static (int[] labels, List<Box> boxes) FindComponents(Color32[] px, int w, int h)
    {
        var labels = new int[w * h];
        for (int i = 0; i < labels.Length; i++) labels[i] = -1;
        var boxes = new List<Box>();
        var stack = new Stack<int>();

        bool Solid(int i) => px[i].a > 16;

        int cur = 0;
        for (int start = 0; start < px.Length; start++)
        {
            if (labels[start] != -1 || !Solid(start)) continue;
            var box = new Box { Label = cur, MinX = int.MaxValue, MinY = int.MaxValue, MaxX = int.MinValue, MaxY = int.MinValue };
            stack.Push(start);
            labels[start] = cur;

            while (stack.Count > 0)
            {
                int i = stack.Pop();
                int x = i % w, y = i / w;
                if (x < box.MinX) box.MinX = x;
                if (x > box.MaxX) box.MaxX = x;
                if (y < box.MinY) box.MinY = y;
                if (y > box.MaxY) box.MaxY = y;

                for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx, ny = y + dy;
                    if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                    int ni = ny * w + nx;
                    if (labels[ni] == -1 && Solid(ni)) { labels[ni] = cur; stack.Push(ni); }
                }
            }
            boxes.Add(box);
            cur++;
        }
        return (labels, boxes);
    }

    static List<List<Box>> GroupRows(List<Box> boxes)
    {
        // Ordenar por Y de arriba (mayor Y en coords de textura) hacia abajo.
        var ordered = boxes.OrderByDescending(b => b.CenterY).ToList();
        float avgH = boxes.Average(b => (float)b.H);
        var rows = new List<List<Box>>();
        foreach (var b in ordered)
        {
            var row = rows.LastOrDefault();
            if (row == null || Mathf.Abs(row[0].CenterY - b.CenterY) > avgH * 0.6f)
            {
                row = new List<Box>();
                rows.Add(row);
            }
            row.Add(b);
        }
        return rows;
    }

    static void WriteClip(string clipPath, Sprite[] frames, float frameDur, bool loop)
    {
        if (frames == null || frames.Length == 0)
        {
            Debug.LogWarning($"[Onix] Sin frames para {Path.GetFileName(clipPath)}, se deja sin cambios.");
            return;
        }
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null)
        {
            Debug.LogWarning($"[Onix] No existe el clip {clipPath}.");
            return;
        }

        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        var keys = new ObjectReferenceKeyframe[frames.Length];
        for (int i = 0; i < frames.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i * frameDur, value = frames[i] };

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
    }

    static void ApplyToPrefab(Sprite idleSprite, float ppu)
    {
        var root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            var sr = root.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.sprite = idleSprite;

            // Ajustar el collider al tamaño del perro (pivote en los pies => offset = mitad del alto).
            var col = root.GetComponent<CapsuleCollider2D>();
            if (col != null)
            {
                float hUnits = idleSprite.rect.height / ppu;
                float wUnits = idleSprite.rect.width / ppu;
                col.size = new Vector2(wUnits * 0.7f, hUnits * 0.95f);
                col.offset = new Vector2(0f, hUnits * 0.5f);
            }
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
