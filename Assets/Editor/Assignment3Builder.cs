using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class Assignment3Builder
{
    public static void BuildProject()
    {
        CreateFolders();
        EnsureTag("Enemy");
        EnsureTag("Powerup");
        ConfigurePipeline();
        ConfigureAudio();
        CreateMaterials();
        CreatePhysicsMaterial();
        CreateParticles();
        CreateAnimator();
        CreatePrefabs();
        CreateScene();
        WriteReport();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void CreateFolders()
    {
        foreach (string path in new[] {"Assets/Scenes", "Assets/Scripts", "Assets/Prefabs", "Assets/Materials", "Assets/Physics Materials", "Assets/Audio", "Assets/Animations", "Assets/Settings"})
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
                string name = System.IO.Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }

    private static void EnsureTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tag)
            {
                return;
            }
        }
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }

    private static void ConfigurePipeline()
    {
        UniversalRenderPipelineAsset asset = UniversalRenderPipelineAsset.Create();
        AssetDatabase.CreateAsset(asset, "Assets/Settings/Assignment3_URP.asset");
        GraphicsSettings.defaultRenderPipeline = asset;
        QualitySettings.renderPipeline = asset;
    }

    private static Material Lit(string name, Color color, float metallic, float smoothness, bool emission = false)
    {
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.name = name;
        material.SetColor("_BaseColor", color);
        material.SetFloat("_Metallic", metallic);
        material.SetFloat("_Smoothness", smoothness);
        if (emission)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 2.2f);
        }
        AssetDatabase.CreateAsset(material, $"Assets/Materials/{name}.mat");
        return material;
    }

    private static void CreateMaterials()
    {
        Lit("Player_Normal_Blue", new Color(0.1f, 0.45f, 1f), 0f, 0.55f);
        Lit("Player_Powered_Emission", new Color(0.1f, 0.95f, 1f), 0f, 0.88f, true);
        Lit("Enemy_Red_Metal", new Color(0.95f, 0.12f, 0.15f), 0.35f, 0.75f);
        Lit("Arena_Runtime_Tint", new Color(0.08f, 0.2f, 0.35f), 0f, 0.42f);
        Lit("Powerup_Gold_Emission", new Color(1f, 0.72f, 0.12f), 0.4f, 0.9f, true);
        Lit("Indicator_Glow", new Color(0.25f, 1f, 0.9f), 0f, 0.9f, true);
        Lit("Arena_Edge_Emission", new Color(0.04f, 0.65f, 1f), 0f, 0.8f, true);
    }

    private static void CreatePhysicsMaterial()
    {
        PhysicsMaterial material = new PhysicsMaterial("Bouncy_Multiply");
        material.bounciness = 1.15f;
        material.bounceCombine = PhysicsMaterialCombine.Multiply;
        material.dynamicFriction = 0.25f;
        material.staticFriction = 0.25f;
        AssetDatabase.CreateAsset(material, "Assets/Physics Materials/Bouncy_Multiply.physicsMaterial");
    }

    private static void ConfigureAudio()
    {
        AssetDatabase.ImportAsset("Assets/Audio/Powerup.wav");
        AssetDatabase.ImportAsset("Assets/Audio/Hit.wav");
        AssetDatabase.ImportAsset("Assets/Audio/ArenaLoop.wav");
    }

    private static void CreateParticles()
    {
        SaveParticlePrefab("FX_PowerupCollect", "Assets/Prefabs/FX_PowerupCollect.prefab", new Color(0.2f, 1f, 1f), 36, 2.9f, true);
        SaveParticlePrefab("FX_EnemyKnockout", "Assets/Prefabs/FX_EnemyKnockout.prefab", new Color(1f, 0.25f, 0.1f), 46, 4.5f, true);
        GameObject trail = new GameObject("FX_PlayerTrail");
        ParticleSystem ps = trail.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = ps.main;
        main.loop = true;
        main.playOnAwake = false;
        main.startLifetime = 0.35f;
        main.startSpeed = 0.25f;
        main.startSize = 0.18f;
        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 26f;
        ParticleSystem.ColorOverLifetimeModule color = ps.colorOverLifetime;
        color.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(new[] { new GradientColorKey(Color.cyan, 0), new GradientColorKey(Color.blue, 1) },
            new[] { new GradientAlphaKey(0.8f, 0), new GradientAlphaKey(0, 1) });
        color.color = gradient;
        PrefabUtility.SaveAsPrefabAsset(trail, "Assets/Prefabs/FX_PlayerTrail.prefab");
        Object.DestroyImmediate(trail);
    }

    private static void SaveParticlePrefab(string name, string path, Color tint, short count, float speed, bool burst)
    {
        GameObject obj = new GameObject(name);
        ParticleSystem ps = obj.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = 0.75f;
        main.startSpeed = speed;
        main.startSize = 0.18f;
        main.maxParticles = 90;
        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0, count) });
        ParticleSystem.ShapeModule shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.25f;
        ParticleSystem.ColorOverLifetimeModule color = ps.colorOverLifetime;
        color.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(new[] { new GradientColorKey(tint, 0), new GradientColorKey(Color.white, 1) },
            new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) });
        color.color = gradient;
        PrefabUtility.SaveAsPrefabAsset(obj, path);
        Object.DestroyImmediate(obj);
    }

    private static void CreateAnimator()
    {
        AnimationClip idle = new AnimationClip { name = "IndicatorIdle", frameRate = 30 };
        AnimationUtility.SetEditorCurve(idle, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.x"), AnimationCurve.Constant(0, 1, 1));
        AnimationUtility.SetEditorCurve(idle, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.y"), AnimationCurve.Constant(0, 1, 1));
        AnimationUtility.SetEditorCurve(idle, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.z"), AnimationCurve.Constant(0, 1, 1));
        AssetDatabase.CreateAsset(idle, "Assets/Animations/IndicatorIdle.anim");

        AnimationClip pulse = new AnimationClip { name = "IndicatorPoweredPulse", frameRate = 30 };
        SerializedObject pulseSo = new SerializedObject(pulse);
        pulseSo.FindProperty("m_AnimationClipSettings.m_LoopTime").boolValue = true;
        pulseSo.ApplyModifiedProperties();
        AnimationCurve scale = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(0.35f, 1.32f), new Keyframe(0.7f, 1f));
        AnimationUtility.SetEditorCurve(pulse, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.x"), scale);
        AnimationUtility.SetEditorCurve(pulse, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.y"), scale);
        AnimationUtility.SetEditorCurve(pulse, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.z"), scale);
        AssetDatabase.CreateAsset(pulse, "Assets/Animations/IndicatorPoweredPulse.anim");

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath("Assets/Animations/PowerupIndicator.controller");
        controller.AddParameter("Powered", AnimatorControllerParameterType.Bool);
        AnimatorState idleState = controller.layers[0].stateMachine.AddState("Idle");
        idleState.motion = idle;
        AnimatorState pulseState = controller.layers[0].stateMachine.AddState("Powered Pulse");
        pulseState.motion = pulse;
        controller.layers[0].stateMachine.defaultState = idleState;
        AnimatorStateTransition toPulse = idleState.AddTransition(pulseState);
        toPulse.hasExitTime = false;
        toPulse.AddCondition(AnimatorConditionMode.If, 0, "Powered");
        AnimatorStateTransition toIdle = pulseState.AddTransition(idleState);
        toIdle.hasExitTime = false;
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "Powered");
    }

    private static void CreatePrefabs()
    {
        PhysicsMaterial bouncy = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>("Assets/Physics Materials/Bouncy_Multiply.physicsMaterial");

        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        enemy.name = "Enemy";
        enemy.tag = "Enemy";
        enemy.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Enemy_Red_Metal.mat");
        enemy.GetComponent<Collider>().material = bouncy;
        Rigidbody enemyRb = enemy.AddComponent<Rigidbody>();
        enemyRb.mass = 1.2f;
        enemy.AddComponent<Enemy>().speed = 4.2f;
        PrefabUtility.SaveAsPrefabAsset(enemy, "Assets/Prefabs/Enemy.prefab");
        Object.DestroyImmediate(enemy);

        GameObject powerup = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        powerup.name = "Powerup";
        powerup.tag = "Powerup";
        powerup.transform.localScale = new Vector3(0.8f, 0.25f, 0.8f);
        powerup.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Powerup_Gold_Emission.mat");
        Collider powerupCollider = powerup.GetComponent<Collider>();
        powerupCollider.isTrigger = true;
        Animation anim = powerup.AddComponent<Animation>();
        AnimationClip spin = new AnimationClip { name = "PowerupFloatSpin", frameRate = 30 };
        SerializedObject so = new SerializedObject(spin);
        so.FindProperty("m_AnimationClipSettings.m_LoopTime").boolValue = true;
        so.ApplyModifiedProperties();
        AnimationUtility.SetEditorCurve(spin, EditorCurveBinding.FloatCurve("", typeof(Transform), "localEulerAnglesRaw.y"),
            new AnimationCurve(new Keyframe(0, 0), new Keyframe(1.2f, 360f)));
        AnimationUtility.SetEditorCurve(spin, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.y"),
            new AnimationCurve(new Keyframe(0, 0.5f), new Keyframe(0.6f, 0.85f), new Keyframe(1.2f, 0.5f)));
        AssetDatabase.CreateAsset(spin, "Assets/Animations/PowerupFloatSpin.anim");
        anim.AddClip(spin, spin.name);
        anim.clip = spin;
        anim.playAutomatically = true;
        PrefabUtility.SaveAsPrefabAsset(powerup, "Assets/Prefabs/Powerup.prefab");
        Object.DestroyImmediate(powerup);
    }

    private static void CreateScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        PhysicsMaterial bouncy = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>("Assets/Physics Materials/Bouncy_Multiply.physicsMaterial");

        GameObject arena = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        arena.name = "Floating Arena - Runtime Tint Material";
        arena.transform.localScale = new Vector3(8.2f, 0.35f, 8.2f);
        arena.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Arena_Runtime_Tint.mat");
        GameObjectUtility.SetStaticEditorFlags(arena, StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic);

        GameObject edge = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        edge.name = "Emissive Arena Edge";
        edge.transform.localScale = new Vector3(8.4f, 0.08f, 8.4f);
        edge.transform.position = new Vector3(0, 0.32f, 0);
        edge.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Arena_Edge_Emission.mat");

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0, 1f, 0);
        player.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Player_Normal_Blue.mat");
        player.GetComponent<Collider>().material = bouncy;
        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.mass = 1f;
        AudioSource playerAudio = player.AddComponent<AudioSource>();
        playerAudio.spatialBlend = 0.2f;
        PlayerController pc = player.AddComponent<PlayerController>();
        pc.normalMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Player_Normal_Blue.mat");
        pc.poweredMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Player_Powered_Emission.mat");
        pc.powerupClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Powerup.wav");
        pc.hitClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Hit.wav");

        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        indicator.name = "Powerup Indicator - Code Driven Animator";
        indicator.transform.SetParent(player.transform);
        indicator.transform.localPosition = Vector3.zero;
        indicator.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        indicator.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Indicator_Glow.mat");
        Animator indicatorAnimator = indicator.AddComponent<Animator>();
        indicatorAnimator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/PowerupIndicator.controller");
        pc.powerupIndicator = indicator;

        ParticleSystem collect = (PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FX_PowerupCollect.prefab")) as GameObject).GetComponent<ParticleSystem>();
        collect.name = "FX_PowerupCollect_Runtime";
        pc.collectParticles = collect;
        ParticleSystem knockout = (PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FX_EnemyKnockout.prefab")) as GameObject).GetComponent<ParticleSystem>();
        knockout.name = "FX_EnemyKnockout_Runtime";
        pc.knockoutParticles = knockout;
        GameObject trailObject = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FX_PlayerTrail.prefab")) as GameObject;
        trailObject.name = "FX_PlayerTrail_Runtime";
        trailObject.transform.SetParent(player.transform);
        trailObject.transform.localPosition = Vector3.zero;
        pc.moveTrail = trailObject.GetComponent<ParticleSystem>();

        GameObject focalPoint = new GameObject("Focal Point");
        focalPoint.transform.position = Vector3.zero;
        focalPoint.AddComponent<RotateCamera>();

        Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.transform.SetParent(focalPoint.transform);
        camera.transform.localPosition = new Vector3(0, 7f, -10f);
        camera.transform.localRotation = Quaternion.Euler(35f, 0, 0);
        CameraPolish polish = camera.gameObject.AddComponent<CameraPolish>();
        polish.player = pc;
        AudioSource music = camera.gameObject.AddComponent<AudioSource>();
        music.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/ArenaLoop.wav");
        music.loop = true;
        music.volume = 0.18f;
        music.playOnAwake = true;
        camera.gameObject.AddComponent<MusicGameOverResponder>();

        GameObject manager = new GameObject("Game Manager");
        GameManager gm = manager.AddComponent<GameManager>();
        gm.player = player.transform;

        GameObject spawner = new GameObject("Spawn Manager");
        SpawnManager spawnManager = spawner.AddComponent<SpawnManager>();
        spawnManager.enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy.prefab");
        spawnManager.powerupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Powerup.prefab");
        polish.spawnManager = spawnManager;

        GameObject lightRig = new GameObject("Lights");
        Light directional = new GameObject("Directional Light - Dramatic Shadows").AddComponent<Light>();
        directional.transform.SetParent(lightRig.transform);
        directional.type = LightType.Directional;
        directional.transform.rotation = Quaternion.Euler(50, -35, 0);
        directional.color = new Color(0.62f, 0.74f, 1f);
        directional.intensity = 1f;
        directional.shadows = LightShadows.Soft;
        Light waveLight = new GameObject("Point Light - Wave Reactive").AddComponent<Light>();
        waveLight.transform.SetParent(lightRig.transform);
        waveLight.transform.position = new Vector3(0, 4.5f, 0);
        waveLight.type = LightType.Point;
        waveLight.range = 13f;
        waveLight.intensity = 1.4f;
        ArenaLightController lightController = lightRig.AddComponent<ArenaLightController>();
        lightController.dynamicLight = waveLight;
        lightController.arenaRenderer = arena.GetComponent<Renderer>();

        GameObject volumeObject = new GameObject("Global Volume - Bloom Vignette Color");
        Volume volume = volumeObject.AddComponent<Volume>();
        volume.isGlobal = true;
        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(profile, "Assets/Settings/CreativeCorePostProcessing.asset");
        Bloom bloom = profile.Add<Bloom>(true);
        bloom.intensity.Override(0.8f);
        bloom.threshold.Override(0.9f);
        Vignette vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(0.28f);
        ColorAdjustments color = profile.Add<ColorAdjustments>(true);
        color.saturation.Override(12f);
        color.contrast.Override(8f);
        volume.sharedProfile = profile;

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/KnockoutArenaPolished.unity", true) };
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/KnockoutArenaPolished.unity");
    }

    private static void WriteReport()
    {
        System.IO.File.WriteAllText("README.md",
@"# SWE-402 Assignment 3 - Creative Core Polish

Project: Knockout Arena - Creative Core Polish
Student: Abdulaziz Almoghairi

Open Assets/Scenes/KnockoutArenaPolished.unity and press Play. Move with W/S or arrow keys and orbit the camera with A/D or left/right arrows.

## Section 1: Shaders and Materials
The arena, player, enemies, powerup, indicator, and arena edge all use custom URP/Lit materials in Assets/Materials. The player switches from Player_Normal_Blue to Player_Powered_Emission at runtime in PlayerController when a powerup is collected, and ArenaLightController shifts the arena tint as waves increase.

## Section 2: Lighting
The scene has a dramatic Directional Light with soft shadows plus a Point Light named Point Light - Wave Reactive. ArenaLightController subscribes to the wave event and increases the light intensity/color each wave so the arena becomes more urgent as difficulty rises.

## Section 3: Animation
The powerup prefab has a looping keyframe float/spin animation, and the player indicator uses PowerupIndicator.controller with Idle and Powered Pulse states. PlayerController drives the Animator bool parameter with SetBool when the powerup starts and expires.

## Section 4: VFX
FX_PowerupCollect, FX_EnemyKnockout, and FX_PlayerTrail are separate Particle Systems saved as prefabs. PlayerController calls Play or Stop from code for collection, enemy impact, and movement trail behavior.

## Section 5: Cameras
The camera remains a child of the required Focal Point orbit object and adds CameraPolish for smooth wave intro movement, wave-based FOV zoom, and a powered-up FOV boost. The orbit still follows the Worksheet 5 requirement that movement uses focalPoint.transform.forward.

## Section 6: Post-Processing
The scene includes a Global Volume named Global Volume - Bloom Vignette Color with Bloom, Vignette, and Color Adjustments in Assets/Settings/CreativeCorePostProcessing.asset. Bloom supports the emissive materials while vignette/color grading reinforce the polished arena mood.

## Worksheet 5 Base Features Included
The project includes player AddForce movement, camera-relative controls, bouncy physics material, enemy AI prefab, powerup coroutine, wave spawning with FindObjectsByType<Enemy>(), event/delegate based GameManager.OnGameOver, and two subscribers: SpawnManager and MusicGameOverResponder.
");
    }
}
