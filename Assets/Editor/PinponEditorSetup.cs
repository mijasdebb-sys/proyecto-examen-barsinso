using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class PinponEditorSetup
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string BartPath = "Assets/Sprites/Bart.png";
    private const string BrickPath = "Assets/Sprites/Ladrillo.png";
    private const string PlatformPath = "Assets/Sprites/Piso.png";
    private const string HitAudioPath = "Assets/Resources/sonidohit.mp3";

    [MenuItem("Pinpon/Crear juego")]
    public static void CreateGame()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        ConfigureSprite(BartPath, 80f);
        ConfigureSprite(BrickPath, 64f);
        ConfigureSprite(PlatformPath, 64f);
        AssetDatabase.ImportAsset(HitAudioPath, ImportAssetOptions.ForceSynchronousImport);

        Sprite bartSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BartPath);
        Sprite brickSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BrickPath);
        Sprite platformSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PlatformPath);
        AudioClip hitClip = AssetDatabase.LoadAssetAtPath<AudioClip>(HitAudioPath);

        if (bartSprite == null || brickSprite == null || platformSprite == null || hitClip == null)
            throw new FileNotFoundException("No se pudieron importar el Bart, los bloques y el sonido de Pinpon.");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SceneManager.SetActiveScene(scene);

        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0.2f, -10f);
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5.5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.32f, 0.75f, 0.95f);

        var lightObject = new GameObject("Global Light 2D", typeof(Light2D));
        var globalLight = lightObject.GetComponent<Light2D>();
        globalLight.lightType = Light2D.LightType.Global;
        globalLight.intensity = 1f;

        CreateBlock("Piso izquierdo", platformSprite, new Vector3(-4.75f, -2.5f, 0f), new Vector3(6.5f, 0.8f, 1f));
        CreateBlock("Piso derecho", platformSprite, new Vector3(4.75f, -2.5f, 0f), new Vector3(6.5f, 0.8f, 1f));

        CreateBlock("Ladrillo 1", brickSprite, new Vector3(-2.6f, -1.55f, 0f), Vector3.one * 1.1f);
        CreateBlock("Ladrillo 2", brickSprite, new Vector3(3.0f, -1.55f, 0f), Vector3.one * 1.1f);
        CreateBlock("Ladrillo 3", brickSprite, new Vector3(4.25f, -0.35f, 0f), Vector3.one * 1.1f);

        var playerObject = new GameObject("Bart", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CapsuleCollider2D), typeof(PlatformerPlayer));
        playerObject.transform.position = new Vector3(-5.7f, 0f, 0f);
        SpriteRenderer renderer = playerObject.GetComponent<SpriteRenderer>();
        renderer.sprite = bartSprite;
        renderer.sortingOrder = 5;

        Rigidbody2D body = playerObject.GetComponent<Rigidbody2D>();
        body.gravityScale = 3f;
        body.mass = 1f;

        CapsuleCollider2D collider = playerObject.GetComponent<CapsuleCollider2D>();
        collider.size = new Vector2(0.5f, 1.12f);
        collider.offset = new Vector2(0f, 0.02f);

        PlatformerPlayer player = playerObject.GetComponent<PlatformerPlayer>();
        player.brickHitClip = hitClip;
        player.fallLimit = -5f;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();

        Debug.Log("PINPON_SETUP_OK | escena=" + ScenePath + " | pisos=2 | ladrillos=3 | audio=sonidohit.mp3");
    }

    private static void ConfigureSprite(string path, float pixelsPerUnit)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            throw new FileNotFoundException("No se encontró el sprite: " + path);

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    private static GameObject CreateBlock(string name, Sprite sprite, Vector3 position, Vector3 scale)
    {
        var block = new GameObject(name, typeof(SpriteRenderer), typeof(BoxCollider2D));
        block.transform.position = position;
        block.transform.localScale = scale;
        block.GetComponent<SpriteRenderer>().sprite = sprite;
        block.GetComponent<BoxCollider2D>().size = Vector2.one;
        return block;
    }
}
