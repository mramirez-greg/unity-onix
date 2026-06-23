using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

/// <summary>
/// Andamiaje de escenas para "Onix: El Rescate de la Familia". Construye por código los
/// sistemas comunes (GameManager, HUD, panel de diálogo, LevelManager) para no tener que
/// cablear prefabs a mano, genera esqueletos de nivel y configura el Build Settings.
/// Menús bajo "Onix/Niveles".
/// </summary>
public static class OnixLevelTools
{
    const string PlayerPrefab = "Assets/Prefab/character_berie.prefab";
    // Controlador de Onix (sus clips Idle/Run/Jump/Fall apuntan al onix_sheet). El prefab trae
    // por defecto el controlador antiguo del cerdo, así que al instanciar lo reemplazamos.
    const string OnixController = "Assets/Animations/Player/Player.controller";
    const string ScenesDir = "Assets/Scenes";

    // Prefabs de contenido reutilizados. Coin.prefab cuenta como hueso (Player suma Coin/Bone).
    const string CoinPrefab = "Assets/Prefab/Coin.prefab";
    const string SpikePrefab = "Assets/Prefab/trap_spike.prefab";
    const string BarrelPrefab = "Assets/Prefab/object_barrel_light_0.prefab";

    // Tileset "forest" de la selva (ya importado, recortado como Multiple, PPU 100).
    const string SelvaRoot = "Assets/sprites/selva";
    const string ForestRoot = SelvaRoot + "/sprites selva/Forest tileset";
    const string GroundDir = ForestRoot + "/Tile-set/Ground";
    const string TrunksDir = ForestRoot + "/Tile-set/Trunks";
    const string SelvaBg = ForestRoot + "/Tile-set/Background/Background 01.png";
    const string SelvaTiles = SelvaRoot + "/Tiles";     // se crea
    const string SelvaPaletteDir = SelvaRoot + "/Palette"; // se crea
    const string SelvaPalettePath = SelvaPaletteDir + "/Selva Palette.prefab";

    // Sprites de familia/villano ya procesados por "Onix/Familia - Importar...".
    const string FamilyProcessedDir = "Assets/sprites/Familia/Processed";
    // Nombres de quienes pueden tener retrato (= nombre del archivo y del speaker en los diálogos).
    static readonly string[] PortraitSpeakers = { "Grego", "Lilo", "Mao", "Kiwi", "Emily" };

    // Orden de escenas en el Build Settings.
    static readonly string[] SceneOrder =
    {
        "MainMenu", "Level1_Playa", "Level2_Selva", "Level3_Muelle", "Victory"
    };

    // ============================================================ MENÚS PRINCIPALES

    [MenuItem("Onix/Niveles - Añadir sistemas a la escena actual")]
    static void AddSystemsToCurrentScene()
    {
        try
        {
            var player = Object.FindFirstObjectByType<Player>();
            if (player == null)
            {
                EditorUtility.DisplayDialog("Onix",
                    "No hay un Player en la escena. Abre tu nivel (p.ej. SampleScene) primero.", "OK");
                return;
            }

            EnsureGameManager();
            EnsureEventSystem();
            BuildHudCanvas();
            BuildDialogueCanvas();
            var lm = EnsureLevelManager(player);
            EnsureSpawnPoint(lm, player);

            EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Onix] Sistemas añadidos a la escena actual (GameManager, HUD, Diálogo, LevelManager, spawnPoint).");
            EditorUtility.DisplayDialog("Onix",
                "Sistemas añadidos. Falta colocar el FamilyMember al final del nivel " +
                "(menú 'Onix/Niveles - Colocar familiar al final').", "OK");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Onix] Error al añadir sistemas: " + e);
        }
    }

    [MenuItem("Onix/Niveles - Crear esqueleto de nivel nuevo")]
    static void CreateLevelSkeleton()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Cámara.
        var camGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(CameraFollow));
        camGO.tag = "MainCamera";
        var cam = camGO.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 3f;
        cam.backgroundColor = new Color(0.4f, 0.7f, 1f);
        cam.transform.position = new Vector3(0, 0, -10);

        EnsureGameManager();
        EnsureEventSystem();

        // Jugador desde el prefab.
        var playerGO = InstantiatePlayer(new Vector3(-4, 0, 0));
        var player = playerGO != null ? playerGO.GetComponentInChildren<Player>() : null;
        if (playerGO != null)
            camGO.GetComponent<CameraFollow>().target = playerGO.transform;

        // Piso básico para poder caminar mientras se pinta el tilemap real.
        CreateBasicFloor();

        BuildHudCanvas();
        BuildDialogueCanvas();
        var lm = EnsureLevelManager(player);
        EnsureSpawnPoint(lm, player);

        // Familiar placeholder al final.
        PlaceFamilyPlaceholder(new Vector3(6, 0.5f, 0));

        // Recorrido jugable: huesos a lo largo del camino + algunos obstáculos.
        PopulateLevel(new Vector3(-4, 0, 0), new Vector3(6, 0.5f, 0));

        Debug.Log("[Onix] Esqueleto de nivel creado. Guárdalo en Assets/Scenes con el nombre del nivel " +
                  "(Level2_Selva / Level3_Muelle) y pinta el tilemap.");
        EditorUtility.DisplayDialog("Onix",
            "Esqueleto listo. Guárdalo (Ctrl+S) en Assets/Scenes como 'Level2_Selva' o 'Level3_Muelle', " +
            "ajusta el LevelManager (zona, familiar, escena siguiente) y luego ejecuta " +
            "'Onix/Niveles - Configurar Build Settings'.", "OK");
    }

    [MenuItem("Onix/Niveles - Crear MainMenu y Victory")]
    static void CreateMenuScenes()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Directory.CreateDirectory(ScenesDir);

        CreateSimpleUIScene("MainMenu", isMenu: true);
        CreateSimpleUIScene("Victory", isMenu: false);

        Debug.Log("[Onix] Escenas MainMenu y Victory creadas en Assets/Scenes.");
        EditorUtility.DisplayDialog("Onix",
            "MainMenu y Victory creadas. Ejecuta 'Onix/Niveles - Configurar Build Settings' " +
            "para registrarlas en el orden correcto.", "OK");
    }

    [MenuItem("Onix/Niveles - Colocar familiar al final")]
    static void PlaceFamilyAtEnd()
    {
        var player = Object.FindFirstObjectByType<Player>();
        Vector3 pos = player != null ? player.transform.position + new Vector3(8, 0.5f, 0) : new Vector3(6, 0.5f, 0);
        var fm = PlaceFamilyPlaceholder(pos);
        Selection.activeGameObject = fm;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[Onix] FamilyMember colocado. Ajusta su memberName y rescueLines en el Inspector.");
    }

    [MenuItem("Onix/Niveles - Aplicar historia a este nivel")]
    static void ApplyStoryToLevel()
    {
        var lm = Object.FindFirstObjectByType<LevelManager>();
        if (lm == null)
        {
            EditorUtility.DisplayDialog("Onix",
                "No hay LevelManager en la escena. Usa 'Añadir sistemas' o 'Crear esqueleto' primero.", "OK");
            return;
        }

        int idx = lm.familyMemberIndex;
        if (idx < 0 || idx > 2)
        {
            EditorUtility.DisplayDialog("Onix",
                $"El LevelManager tiene Family Member Index = {idx}. Debe ser 0 (Grego), 1 (Lilo) o 2 (Mao).", "OK");
            return;
        }

        // Configurar el LevelManager (zona, intro, escena siguiente).
        lm.zoneName = StoryZone[idx];
        lm.introLines = StoryIntro[idx];
        lm.nextSceneName = StoryNextScene[idx];
        // Kiwi es quien desata el conflicto en cada intro: su retrato aparece al empezar el nivel.
        lm.introSpeaker = "Kiwi";

        // Configurar el FamilyMember (nombre, rescate y, en el Nivel 3, la confrontación con Kiwi).
        var fm = Object.FindFirstObjectByType<FamilyMember>();
        if (fm != null)
        {
            fm.memberIndex = idx;
            fm.memberName = StoryName[idx];
            fm.rescueLines = StoryRescue[idx];
            fm.postRescueSpeaker = (idx == 2) ? "Kiwi" : "";
            fm.postRescueLines = (idx == 2) ? StoryConfrontation : new string[0];
            fm.epilogueSpeaker = (idx == 2) ? "Emily" : "";
            fm.epilogueLines = (idx == 2) ? StoryEpilogue : new string[0];
            EditorUtility.SetDirty(fm);
        }

        EditorUtility.SetDirty(lm);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[Onix] Historia aplicada al nivel de {StoryName[idx]} (zona '{StoryZone[idx]}').");
        EditorUtility.DisplayDialog("Onix",
            $"Historia aplicada: {StoryName[idx]} — {StoryZone[idx]}." +
            (fm == null ? "\n\n(No había FamilyMember; coloca uno con 'Colocar familiar al final'.)" : ""), "OK");
    }

    [MenuItem("Onix/Niveles - Reparar personaje (usar Onix)")]
    static void FixPlayerToOnix()
    {
        var player = Object.FindFirstObjectByType<Player>();
        if (player == null)
        {
            EditorUtility.DisplayDialog("Onix", "No hay Player en la escena. Abre el nivel a reparar.", "OK");
            return;
        }

        ApplyOnixController(player.gameObject);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Onix] Animator del jugador cambiado al controlador de Onix. Dale Play para verlo como perro.");
        EditorUtility.DisplayDialog("Onix", "Listo: el personaje usará a Onix. Dale Play para confirmarlo.", "OK");
    }

    [MenuItem("Onix/Niveles - Poblar nivel con huesos y obstaculos")]
    static void PopulateCurrentLevel()
    {
        var player = Object.FindFirstObjectByType<Player>();
        var family = Object.FindFirstObjectByType<FamilyMember>();
        if (player == null)
        {
            EditorUtility.DisplayDialog("Onix", "No hay Player en la escena. Abre un nivel primero.", "OK");
            return;
        }

        Vector3 pPos = player.transform.position;
        Vector3 fPos = family != null ? family.transform.position : pPos + new Vector3(10, 0, 0);
        int n = PopulateLevel(pPos, fPos);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[Onix] Nivel poblado con {n} huesos y algunos obstáculos entre el jugador y el familiar.");
    }

    [MenuItem("Onix/Niveles - Configurar Build Settings")]
    static void ConfigureBuildSettings()
    {
        var list = new List<EditorBuildSettingsScene>();
        var found = new List<string>();
        foreach (var name in SceneOrder)
        {
            string path = $"{ScenesDir}/{name}.unity";
            if (File.Exists(path))
            {
                list.Add(new EditorBuildSettingsScene(path, true));
                found.Add(name);
            }
        }

        if (list.Count == 0)
        {
            EditorUtility.DisplayDialog("Onix",
                "No encontré ninguna escena en Assets/Scenes con los nombres esperados " +
                "(MainMenu, Level1_Playa, Level2_Selva, Level3_Muelle, Victory). " +
                "Renombra/crea las escenas y reintenta.", "OK");
            return;
        }

        EditorBuildSettings.scenes = list.ToArray();
        Debug.Log("[Onix] Build Settings configurado en orden: " + string.Join(", ", found));
        EditorUtility.DisplayDialog("Onix", "Build Settings actualizado:\n" + string.Join("\n", found), "OK");
    }

    [MenuItem("Onix/Niveles - Anadir retratos (Kiwi, Emily, familia) al dialogo")]
    static void AddPortraitsToDialogue()
    {
        var dp = Object.FindFirstObjectByType<DialoguePanel>();
        if (dp == null)
        {
            EditorUtility.DisplayDialog("Onix",
                "No hay DialoguePanel en la escena. Usa 'Añadir sistemas' primero (crea el panel de diálogo).", "OK");
            return;
        }

        EnsureDialoguePortrait(dp);
        dp.portraits = BuildPortraitTable(out var missing);
        EditorUtility.SetDirty(dp);

        // Para que Kiwi aparezca ya en la intro (escena inicial).
        var lm = Object.FindFirstObjectByType<LevelManager>();
        if (lm != null && string.IsNullOrEmpty(lm.introSpeaker))
        {
            lm.introSpeaker = "Kiwi";
            EditorUtility.SetDirty(lm);
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        int loaded = dp.portraits.Length;
        string msg = $"Retratos cableados: {loaded} ({string.Join(", ", dp.portraits.Select(p => p.speaker))}).";
        if (missing.Count > 0)
            msg += $"\n\nFaltan sprites en {FamilyProcessedDir} para: {string.Join(", ", missing)}.\n" +
                   "Genera/importa esos PNG y vuelve a ejecutar este menú.";
        Debug.Log("[Onix] " + msg);
        EditorUtility.DisplayDialog("Onix", msg, "OK");
    }

    /// <summary>Construye la tabla speaker→sprite leyendo los PNG procesados de la familia/villano.</summary>
    static DialoguePanel.CharacterPortrait[] BuildPortraitTable(out List<string> missing)
    {
        missing = new List<string>();
        var list = new List<DialoguePanel.CharacterPortrait>();
        foreach (var name in PortraitSpeakers)
        {
            var sprite = LoadProcessedSprite(name);
            if (sprite != null)
                list.Add(new DialoguePanel.CharacterPortrait { speaker = name, sprite = sprite });
            else
                missing.Add(name);
        }
        return list.ToArray();
    }

    static Sprite LoadProcessedSprite(string name)
    {
        string path = $"{FamilyProcessedDir}/{name}.png";
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s != null) return s;
        // Si se importó como Multiple, el sprite principal puede ser null: toma el primer sub-sprite.
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
    }

    /// <summary>
    /// Garantiza una Image "Portrait" a la izquierda del panel de diálogo y la cablea en
    /// dp.portraitImage; desplaza el nombre y el cuerpo del texto para dejarle hueco.
    /// </summary>
    static void EnsureDialoguePortrait(DialoguePanel dp)
    {
        if (dp.panelRoot == null) return;
        var panel = dp.panelRoot.transform;

        var existing = panel.Find("Portrait");
        GameObject go = existing != null ? existing.gameObject : new GameObject("Portrait", typeof(Image));
        if (existing == null) go.transform.SetParent(panel, false);

        var img = go.GetComponent<Image>();
        img.preserveAspect = true;
        img.raycastTarget = false;
        var rt = img.rectTransform;
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = new Vector2(16f, 0f);
        rt.sizeDelta = new Vector2(220f, 220f);
        go.SetActive(false); // oculto hasta que hable alguien con retrato

        dp.portraitImage = img;

        // Dejar hueco al retrato: empujar el texto a la derecha (valor absoluto => idempotente).
        SetLeftInset(panel, "DialogueText", 252f);
        SetLeftInset(panel, "Speaker", 252f);
    }

    static void SetLeftInset(Transform panel, string childName, float left)
    {
        var t = panel.Find(childName);
        if (t == null) return;
        var rt = t.GetComponent<RectTransform>();
        if (rt == null) return;
        // Elemento estirado horizontalmente => mover offsetMin; si no, mover anchoredPosition.
        if (Mathf.Abs(rt.anchorMin.x - rt.anchorMax.x) > 0.001f)
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);
        else
            rt.anchoredPosition = new Vector2(left, rt.anchoredPosition.y);
        EditorUtility.SetDirty(rt);
    }

    // ============================================================ CONSTRUCTORES

    static void EnsureGameManager()
    {
        if (Object.FindFirstObjectByType<GameManager>() != null) return;
        var go = new GameObject("GameManager");
        go.AddComponent<GameManager>();
    }

    static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null) return;
        new GameObject("EventSystem",
            typeof(UnityEngine.EventSystems.EventSystem),
            typeof(UnityEngine.EventSystems.StandaloneInputModule));
    }

    static GameObject InstantiatePlayer(Vector3 pos)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefab);
        if (prefab == null)
        {
            Debug.LogWarning($"[Onix] No encontré {PlayerPrefab}; creo un placeholder de jugador.");
            var ph = new GameObject("Player", typeof(Rigidbody2D), typeof(Player));
            ph.transform.position = pos;
            return ph;
        }
        var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        inst.transform.position = pos;
        ApplyOnixController(inst);
        return inst;
    }

    /// <summary>
    /// Asigna el controlador de Onix al Animator del jugador (como override de prefab en la
    /// escena), igual que ya hace SampleScene. Sin esto, el prefab usa el controlador del
    /// cerdo por defecto y el personaje se anima como cerdo al darle Play.
    /// </summary>
    static void ApplyOnixController(GameObject playerInstance)
    {
        var animator = playerInstance.GetComponentInChildren<Animator>();
        if (animator == null) return;

        var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(OnixController);
        if (ctrl == null)
        {
            Debug.LogWarning($"[Onix] No encontré {OnixController}; el jugador podría animarse como cerdo.");
            return;
        }
        animator.runtimeAnimatorController = ctrl;
    }

    static void CreateBasicFloor()
    {
        if (GameObject.Find("Floor_Basic") != null) return;
        var floor = new GameObject("Floor_Basic", typeof(BoxCollider2D), typeof(SpriteRenderer));
        int floorLayer = LayerMask.NameToLayer("Floor");
        if (floorLayer >= 0) floor.layer = floorLayer;
        floor.transform.position = new Vector3(0, -1.5f, 0);

        var box = floor.GetComponent<BoxCollider2D>();
        box.size = new Vector2(40, 1);

        var sr = floor.GetComponent<SpriteRenderer>();
        sr.sprite = MakeSolidSprite();
        sr.color = new Color(0.85f, 0.75f, 0.5f);
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = new Vector2(40, 1);
        sr.sortingOrder = -5;
    }

    static GameObject PlaceFamilyPlaceholder(Vector3 pos)
    {
        var go = new GameObject("FamilyMember", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(FamilyMember));
        go.tag = "Family";
        go.transform.position = pos;

        var sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = MakeSolidSprite();
        sr.color = new Color(0.95f, 0.8f, 0.4f);
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = new Vector2(0.5f, 0.8f);
        sr.sortingOrder = 1;

        var col = go.GetComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 1.2f);

        var fm = go.GetComponent<FamilyMember>();
        fm.memberName = "Familiar";
        fm.rescueLines = new[] { "¡Onix! ¡Me encontraste!", "Vamos a buscar a los demás..." };
        return go;
    }

    /// <summary>
    /// Coloca huesos (Coin.prefab) a lo largo del camino entre el jugador y el familiar,
    /// más un par de pinchos y un barril como obstáculos. Devuelve cuántos huesos colocó.
    /// </summary>
    static int PopulateLevel(Vector3 playerPos, Vector3 familyPos)
    {
        // Altura del suelo: usa el techo del Floor_Basic si existe; si no, un poco bajo el jugador.
        float groundY = playerPos.y - 0.5f;
        var floor = GameObject.Find("Floor_Basic");
        if (floor != null)
        {
            var box = floor.GetComponent<BoxCollider2D>();
            if (box != null) groundY = box.bounds.max.y;
        }

        float minX = Mathf.Min(playerPos.x, familyPos.x) + 1.5f;
        float maxX = Mathf.Max(playerPos.x, familyPos.x) - 1.2f;
        if (maxX <= minX) maxX = minX + 4f;

        var parent = GameObject.Find("LevelContent") ?? new GameObject("LevelContent");

        int bones = 0;
        int step = 0;
        for (float x = minX; x <= maxX; x += 1.0f, step++)
        {
            // Hueso flotando sobre el suelo.
            if (Place(CoinPrefab, new Vector3(x, groundY + 0.4f, 0), parent.transform)) bones++;

            // Cada 4 pasos, un obstáculo en el suelo (alterna pincho/barril).
            if (step > 0 && step % 4 == 0)
            {
                string obstacle = (step % 8 == 0) ? BarrelPrefab : SpikePrefab;
                float yOff = (obstacle == BarrelPrefab) ? 0.12f : 0.05f;
                Place(obstacle, new Vector3(x + 0.5f, groundY + yOff, 0), parent.transform);
            }
        }
        return bones;
    }

    static bool Place(string prefabPath, Vector3 pos, Transform parent)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) { Debug.LogWarning($"[Onix] No encontré {prefabPath}."); return false; }
        var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        inst.transform.position = pos;
        if (parent != null) inst.transform.SetParent(parent, true);
        return true;
    }

    static LevelManager EnsureLevelManager(Player player)
    {
        var lm = Object.FindFirstObjectByType<LevelManager>();
        if (lm == null)
        {
            var go = new GameObject("LevelManager");
            lm = go.AddComponent<LevelManager>();
            lm.zoneName = "Playa";
            lm.familyMemberIndex = 0;
            lm.nextSceneName = "";
            lm.introLines = new[]
            {
                "Onix paseaba feliz con su familia por la costa...",
                "¡Kiwi, la gata mandona, desató una tormenta y los separó!",
                "Cruza la zona, junta huesos y rescata a tu familia."
            };
        }
        return lm;
    }

    static void EnsureSpawnPoint(LevelManager lm, Player player)
    {
        if (lm.spawnPoint != null) return;
        var sp = new GameObject("SpawnPoint").transform;
        sp.position = player != null ? player.transform.position : Vector3.zero;
        lm.spawnPoint = sp;
    }

    // ------------------------------------------------------------ HUD

    static void BuildHudCanvas()
    {
        if (Object.FindFirstObjectByType<HUD>() != null) return;

        var canvas = CreateCanvas("HUDCanvas", 0);
        var hud = canvas.AddComponent<HUD>();

        // Huesos (arriba-izquierda): icono textual + número.
        var bonesLabel = CreateText(canvas.transform, "BonesLabel", "Huesos:",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(20, -20), new Vector2(160, 50), TextAlignmentOptions.Left, 28, Color.white);

        var bonesNum = CreateText(canvas.transform, "BonesText", "0",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(170, -20), new Vector2(100, 50), TextAlignmentOptions.Left, 28, Color.white);
        hud.bonesText = bonesNum.GetComponent<TMP_Text>();

        // Vidas (arriba-derecha) como texto simple "Vidas x5".
        var livesLabel = CreateText(canvas.transform, "LivesLabel", "Vidas:",
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-150, -20), new Vector2(120, 50), TextAlignmentOptions.Right, 28, Color.white);

        var livesNum = CreateText(canvas.transform, "LivesText", "x5",
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-20, -20), new Vector2(80, 50), TextAlignmentOptions.Right, 28,
            new Color(1f, 0.4f, 0.4f));
        hud.livesText = livesNum.GetComponent<TMP_Text>();
    }

    // ------------------------------------------------------------ Diálogo

    static void BuildDialogueCanvas()
    {
        if (Object.FindFirstObjectByType<DialoguePanel>() != null) return;

        var canvas = CreateCanvas("DialogueCanvas", 10); // por encima del HUD
        var dp = canvas.AddComponent<DialoguePanel>();

        // Panel inferior.
        var panel = new GameObject("Panel", typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        var prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.1f, 0.05f);
        prt.anchorMax = new Vector2(0.9f, 0.32f);
        prt.offsetMin = Vector2.zero;
        prt.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.78f);

        var speaker = CreateText(panel.transform, "Speaker", "",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(24, -10), new Vector2(400, 44), TextAlignmentOptions.TopLeft, 26,
            new Color(1f, 0.85f, 0.4f));

        var body = CreateText(panel.transform, "DialogueText", "...",
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAlignmentOptions.Left, 26, Color.white);
        var brt = body.GetComponent<RectTransform>();
        brt.offsetMin = new Vector2(24, 20);
        brt.offsetMax = new Vector2(-24, -54);

        var hint = CreateText(panel.transform, "Hint", "▶ Espacio / Clic",
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0),
            new Vector2(-20, 12), new Vector2(220, 36), TextAlignmentOptions.Right, 20,
            new Color(0.8f, 0.8f, 0.8f));

        dp.panelRoot = panel;
        dp.dialogueText = body.GetComponent<TMP_Text>();
        dp.speakerText = speaker.GetComponent<TMP_Text>();

        // Retrato del personaje que habla (Kiwi, Emily, familia) cableado desde el inicio.
        EnsureDialoguePortrait(dp);
        dp.portraits = BuildPortraitTable(out _);

        panel.SetActive(false);
    }

    // ============================================================ ESCENAS UI SIMPLES

    static void CreateSimpleUIScene(string sceneName, bool isMenu)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camGO.tag = "MainCamera";
        var cam = camGO.GetComponent<Camera>();
        cam.orthographic = true;
        cam.backgroundColor = isMenu ? new Color(0.15f, 0.2f, 0.35f) : new Color(0.2f, 0.35f, 0.2f);
        camGO.transform.position = new Vector3(0, 0, -10);

        EnsureGameManager();
        EnsureEventSystem();

        var canvas = CreateCanvas(sceneName + "Canvas", 0);

        if (isMenu)
        {
            var menu = canvas.AddComponent<MainMenuUI>();
            menu.firstLevelScene = "Level1_Playa";

            CreateText(canvas.transform, "Title", "ONIX\nEl Rescate de la Familia",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -120), new Vector2(900, 160), TextAlignmentOptions.Center, 54, Color.white);

            var playBtn = CreateButton(canvas.transform, "PlayButton", "Jugar",
                new Vector2(0, -20), new Color(0.25f, 0.6f, 0.3f));
            AddOnClick(playBtn, menu, "PlayGame");

            var quitBtn = CreateButton(canvas.transform, "QuitButton", "Salir",
                new Vector2(0, -120), new Color(0.6f, 0.3f, 0.3f));
            AddOnClick(quitBtn, menu, "QuitGame");
        }
        else
        {
            var victory = canvas.AddComponent<VictoryUI>();
            victory.mainMenuScene = "MainMenu";

            CreateText(canvas.transform, "Title", "¡VICTORIA!",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -120), new Vector2(900, 120), TextAlignmentOptions.Center, 60,
                new Color(1f, 0.9f, 0.3f));

            var summary = CreateText(canvas.transform, "Summary", "¡Onix reunió a toda su familia!",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 40), new Vector2(900, 160), TextAlignmentOptions.Center, 32, Color.white);
            victory.summaryText = summary.GetComponent<TMP_Text>();

            var menuBtn = CreateButton(canvas.transform, "MenuButton", "Menú principal",
                new Vector2(0, -120), new Color(0.3f, 0.4f, 0.6f));
            AddOnClick(menuBtn, victory, "BackToMenu");
        }

        Directory.CreateDirectory(ScenesDir);
        EditorSceneManager.SaveScene(scene, $"{ScenesDir}/{sceneName}.unity");
    }

    // ============================================================ HELPERS UI

    static GameObject CreateCanvas(string name, int sortingOrder)
    {
        var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        return go;
    }

    static GameObject CreateText(Transform parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 size, TextAlignmentOptions align, float fontSize, Color color)
    {
        var go = new GameObject(name, typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = align;
        var rt = tmp.rectTransform;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        return go;
    }

    static GameObject CreateButton(Transform parent, string name, string label,
        Vector2 anchoredPos, Color color)
    {
        var go = new GameObject(name, typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = color;
        var rt = img.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(320, 80);

        var txt = CreateText(go.transform, "Text", label,
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, 32, Color.white);
        var trt = txt.GetComponent<RectTransform>();
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        return go;
    }

    // Conecta el OnClick del botón a un método público (sin argumentos) de un componente,
    // de forma persistente (queda guardado en la escena).
    static void AddOnClick(GameObject button, Object target, string methodName)
    {
        var btn = button.GetComponent<Button>();
        var action = (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(
            typeof(UnityEngine.Events.UnityAction), target, methodName);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, action);
    }

    // ============================================================ TEXTOS DE LA HISTORIA
    // Indexados por familiar: 0 = Grego (Playa), 1 = Lilo (Selva), 2 = Mao (Muelle + Kiwi).

    static readonly string[] StoryZone =
    {
        "Playa Soleada", "Selva de Palmeras", "Muelle en Tormenta"
    };

    static readonly string[] StoryName = { "Grego", "Lilo", "Mao" };

    static readonly string[] StoryNextScene = { "Level2_Selva", "Level3_Muelle", "" };

    static readonly string[][] StoryIntro =
    {
        new[]
        {
            "Onix paseaba feliz con su familia por la costa...",
            "De pronto, Kiwi, la gata mandona, conjuró una tormenta.",
            "¡El viento separó a toda la familia por la isla!",
            "Cruza la playa, junta huesos y encuentra a Grego, tu hermano."
        },
        new[]
        {
            "La tormenta arrastró a Lilo hasta la selva de palmeras.",
            "Kiwi dejó trampas escondidas en todo el camino.",
            "Esquiva los pinchos y reúne a Onix con mamá Lilo."
        },
        new[]
        {
            "En el muelle, entre rayos y lluvia, espera Kiwi.",
            "Kiwi: \"¡Jajaja! Ese perro nunca llegará a tiempo.\"",
            "Rescata a Mao y enfréntate a la gata mandona para reunir a la familia."
        }
    };

    static readonly string[][] StoryRescue =
    {
        new[]
        {
            "¡Hermanito Onix! ¡Sabía que vendrías por mí!",
            "Mamá y papá siguen perdidos por la tormenta...",
            "¡Vamos, te sigo!"
        },
        new[]
        {
            "¡Onix, mi amor! Estaba muy asustada.",
            "Esa gata mandona, Kiwi, se llevó a Mao hacia el muelle.",
            "¡Corre, hay que alcanzarlo!"
        },
        new[]
        {
            "¡Onix! Resististe todo... estoy orgulloso de ti.",
            "Pero ten cuidado... Kiwi sigue rondando."
        }
    };

    // Confrontación final tras rescatar a Mao (Nivel 3), antes del epílogo.
    static readonly string[] StoryConfrontation =
    {
        "¡Grrr! ¿Cómo cruzaste todas mis trampas, chucho?",
        "¡Esta familia no obedece nada! ¡Me largo! ¡Bah!"
    };

    // Epílogo: Emily, la mamá de Kiwi, la descubre y la castiga. Cierra antes de Victory.
    static readonly string[] StoryEpilogue =
    {
        "Justo entonces aparece Emily, la mamá de Kiwi.",
        "Emily: \"¡Kiwi! Lo vi TODO desde casa, jovencita.\"",
        "\"Mira que asustar a esta linda familia... ¡qué malcriada!\"",
        "\"Estás castigada: ¡a tu habitación ahora mismo!\"",
        "Kiwi se va cabizbaja a su cuarto. ¡Onix por fin reunió a su familia, felices para siempre!"
    };

    // ============================================================ SELVA (tileset forest)
    // Flujo híbrido para el Nivel 2: import nítido + tiles/paleta para el suelo, y helpers de
    // escena (Grid+Tilemap en capa Floor, fondo, y convertir Presets en plataformas sólidas).

    [MenuItem("Onix/Selva - 1) Preparar import del tileset")]
    static void PrepareForestImport()
    {
        if (!AssetDatabase.IsValidFolder(ForestRoot))
        {
            EditorUtility.DisplayDialog("Onix",
                $"No encontré el tileset en {ForestRoot}. Verifica que la carpeta exista.", "OK");
            return;
        }

        int n = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { ForestRoot }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!(AssetImporter.GetAtPath(path) is TextureImporter ti)) continue;
            // Pixel-art consistente. Respeta el slicing ya existente (no toca spriteImportMode).
            ti.textureType = TextureImporterType.Sprite;
            ti.spritePixelsPerUnit = 100;
            ti.filterMode = FilterMode.Point;
            ti.mipmapEnabled = false;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();
            n++;
        }

        Debug.Log($"[Onix] Import de selva preparado en {n} texturas (Point, sin compresión, PPU 100).");
        EditorUtility.DisplayDialog("Onix",
            $"Listo: {n} texturas del tileset forest quedaron nítidas (Point, sin compresión, PPU 100).\n\n" +
            "Siguiente: 'Onix/Selva - 2) Generar tiles y paleta'.", "OK");
    }

    [MenuItem("Onix/Selva - 2) Generar tiles y paleta")]
    static void GenerateForestTilesAndPalette()
    {
        if (!AssetDatabase.IsValidFolder(SelvaTiles))
            AssetDatabase.CreateFolder(SelvaRoot, "Tiles");

        // Un Tile por sub-sprite de Ground y Trunks (terreno modular para pintar).
        var tiles = new List<Tile>();
        foreach (var dir in new[] { GroundDir, TrunksDir })
        {
            if (!AssetDatabase.IsValidFolder(dir)) continue;
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { dir }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                foreach (var s in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>())
                {
                    string tilePath = $"{SelvaTiles}/{s.name}.asset";
                    var tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
                    if (tile == null)
                    {
                        tile = ScriptableObject.CreateInstance<Tile>();
                        tile.sprite = s;
                        tile.colliderType = Tile.ColliderType.Sprite;
                        AssetDatabase.CreateAsset(tile, tilePath);
                    }
                    tiles.Add(tile);
                }
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        bool paletteOk = TryCreateForestPalette(tiles);

        string msg = $"Generados {tiles.Count} tiles en {SelvaTiles}.\n\n";
        msg += paletteOk
            ? "Paleta 'Selva' creada en " + SelvaPaletteDir + ".\n\nAbre Window > 2D > Tile Palette y elígela."
            : "No pude crear la paleta automáticamente.\n\nAbre Window > 2D > Tile Palette > Create New " +
              "Palette y arrastra la carpeta '" + SelvaTiles + "' (o el atlas Ground) para llenarla.";
        Debug.Log("[Onix] " + msg);
        EditorUtility.DisplayDialog("Onix", msg, "OK");
    }

    /// <summary>
    /// Crea (best-effort) un prefab de Tile Palette con los tiles colocados en rejilla y lo marca
    /// como paleta añadiéndole un GridPalette como sub-asset. Si algo falla, devuelve false y el
    /// usuario crea la paleta a mano (los tiles ya están generados).
    /// </summary>
    static bool TryCreateForestPalette(List<Tile> tiles)
    {
        if (tiles.Count == 0) return false;
        try
        {
            if (!AssetDatabase.IsValidFolder(SelvaPaletteDir))
                AssetDatabase.CreateFolder(SelvaRoot, "Palette");

            var root = new GameObject("Selva Palette", typeof(Grid));
            root.GetComponent<Grid>().cellSize = new Vector3(0.32f, 0.32f, 0f);
            var layer = new GameObject("Layer1", typeof(Tilemap), typeof(TilemapRenderer));
            layer.transform.SetParent(root.transform, false);
            var tm = layer.GetComponent<Tilemap>();

            const int cols = 8;
            for (int i = 0; i < tiles.Count; i++)
                tm.SetTile(new Vector3Int(i % cols, -(i / cols), 0), tiles[i]);

            PrefabUtility.SaveAsPrefabAsset(root, SelvaPalettePath);
            Object.DestroyImmediate(root);

            // Marcar el prefab como paleta (GridPalette sub-asset) si no lo tiene ya.
            if (!AssetDatabase.LoadAllAssetsAtPath(SelvaPalettePath).OfType<GridPalette>().Any())
            {
                var gp = ScriptableObject.CreateInstance<GridPalette>();
                gp.name = "Selva Palette";
                gp.cellSizing = GridPalette.CellSizing.Automatic;
                AssetDatabase.AddObjectToAsset(gp, SelvaPalettePath);
                AssetDatabase.ImportAsset(SelvaPalettePath);
            }
            AssetDatabase.SaveAssets();
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[Onix] No pude crear la paleta automáticamente: " + e.Message);
            return false;
        }
    }

    [MenuItem("Onix/Selva - 3) Preparar escena Level2 (grid + fondo)")]
    static void PrepareSelvaScene()
    {
        var scene = SceneManager.GetActiveScene();
        EnsureSelvaTilemap();
        EnsureSelvaBackground();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Onix] Escena de selva preparada: Grid/Tilemap_Suelo (capa Floor) + Escenario/Background.");
        EditorUtility.DisplayDialog("Onix",
            "Listo. En la Hierarchy verás 'Grid/Tilemap_Suelo' y 'Escenario/Background'.\n\n" +
            "Abre Window > 2D > Tile Palette, elige la paleta 'Selva' y pinta el suelo sobre " +
            "'Tilemap_Suelo'. Borra 'Floor_Basic' cuando termines.", "OK");
    }

    /// <summary>Grid (cellSize 0.32 como Level1) + Tilemap del suelo en capa Floor con collider.</summary>
    static void EnsureSelvaTilemap()
    {
        var gridGO = GameObject.Find("Grid");
        if (gridGO == null || gridGO.GetComponent<Grid>() == null)
        {
            gridGO = new GameObject("Grid", typeof(Grid));
            gridGO.GetComponent<Grid>().cellSize = new Vector3(0.32f, 0.32f, 0f);
        }

        if (gridGO.transform.Find("Tilemap_Suelo") != null) return;

        var tm = new GameObject("Tilemap_Suelo",
            typeof(Tilemap), typeof(TilemapRenderer), typeof(TilemapCollider2D));
        tm.transform.SetParent(gridGO.transform, false);
        int floor = LayerMask.NameToLayer("Floor");
        if (floor >= 0) tm.layer = floor;     // para que el Player lo detecte como suelo
        tm.tag = "floor";
        tm.GetComponent<TilemapRenderer>().sortingOrder = 0;
    }

    /// <summary>Fondo de selva (Background 01) detrás de todo, escalado para cubrir la cámara.</summary>
    static void EnsureSelvaBackground()
    {
        var parent = GameObject.Find("Escenario") ?? new GameObject("Escenario");
        var existing = parent.transform.Find("Background");
        GameObject bg = existing != null ? existing.gameObject : new GameObject("Background", typeof(SpriteRenderer));
        if (existing == null) bg.transform.SetParent(parent.transform, false);

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SelvaBg)
                     ?? AssetDatabase.LoadAllAssetsAtPath(SelvaBg).OfType<Sprite>().FirstOrDefault();
        if (sprite == null)
        {
            Debug.LogWarning($"[Onix] No encontré el sprite de fondo en {SelvaBg}.");
            return;
        }

        var sr = bg.GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = -10;

        var cam = Camera.main;
        float worldH = (cam != null && cam.orthographic) ? cam.orthographicSize * 2f : 6f;
        float spriteH = sprite.bounds.size.y;
        if (spriteH > 0.01f)
        {
            float scale = worldH / spriteH;
            bg.transform.localScale = new Vector3(scale, scale, 1f);
        }
        bg.transform.position = cam != null
            ? new Vector3(cam.transform.position.x, cam.transform.position.y, 0f)
            : Vector3.zero;
    }

    [MenuItem("Onix/Selva - Hacer solida la seleccion (plataforma)")]
    static void MakeSelectionSolid()
    {
        int n = 0;
        int floor = LayerMask.NameToLayer("Floor");
        foreach (var go in Selection.gameObjects)
        {
            if (go.GetComponent<SpriteRenderer>() == null) continue;
            if (go.GetComponent<Collider2D>() == null)
                go.AddComponent<BoxCollider2D>(); // se auto-ajusta al sprite al añadirse
            if (floor >= 0) go.layer = floor;
            n++;
        }

        if (n == 0)
        {
            EditorUtility.DisplayDialog("Onix",
                "Selecciona en la Hierarchy uno o más objetos con SpriteRenderer " +
                "(p. ej. un Preset que arrastraste a la escena).", "OK");
            return;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"[Onix] {n} objeto(s) convertidos en plataforma sólida (BoxCollider2D + capa Floor).");
    }

    // Sprite blanco reutilizable para fondos sólidos (piso, placeholders). El color se aplica
    // como tinte en el SpriteRenderer.color de cada usuario.
    static Sprite cachedSolid;
    static Sprite MakeSolidSprite()
    {
        if (cachedSolid == null)
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var cols = Enumerable.Repeat<Color>(Color.white, 16).ToArray();
            tex.SetPixels(cols);
            tex.Apply();
            cachedSolid = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
        }
        return cachedSolid;
    }
}
