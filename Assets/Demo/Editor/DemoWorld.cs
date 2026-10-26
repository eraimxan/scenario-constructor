using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;

namespace ScenarioKit.Demo.Editor
{
    /// <summary>Builds the 3D part of the demo: room, zones, interactive objects, player, camera.</summary>
    sealed class DemoWorld
    {
        const string MaterialsFolder = "Assets/Demo/Materials";

        public PowerSwitch PowerSwitch;
        DemoAssets assets;
        Font font;

        public static DemoWorld Build(DemoAssets assets)
        {
            AssetDatabase.DeleteAsset(MaterialsFolder);
            Directory.CreateDirectory(MaterialsFolder);
            AssetDatabase.Refresh();

            var world = new DemoWorld { assets = assets, font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") };
            world.BuildRoom();
            world.BuildZones();
            world.BuildObjects();
            world.BuildPlayerAndCamera();
            return world;
        }

        void BuildRoom()
        {
            var room = new GameObject("Room").transform;
            Box("Floor", room, new Vector3(0, -0.1f, 0), new Vector3(16, 0.2f, 14), Mat("Floor", new Color(0.32f, 0.33f, 0.35f)));
            var wall = Mat("Wall", new Color(0.18f, 0.19f, 0.21f));
            Box("Wall North", room, new Vector3(0, 0.5f, 7.1f), new Vector3(16.4f, 1, 0.2f), wall);
            Box("Wall South", room, new Vector3(0, 0.5f, -7.1f), new Vector3(16.4f, 1, 0.2f), wall);
            Box("Wall West", room, new Vector3(-8.1f, 0.5f, 0), new Vector3(0.2f, 1, 14.4f), wall);
            Box("Wall East", room, new Vector3(8.1f, 0.5f, 0), new Vector3(0.2f, 1, 14.4f), wall);
        }

        void BuildZones()
        {
            var zones = new GameObject("Zones").transform;
            Zone(zones, "Зона A", new Vector2(-5, 3.5f), new Vector2(3, 3), new Color(0.95f, 0.8f, 0.2f), "zone.a");
            Zone(zones, "Зона B", new Vector2(0, 4.6f), new Vector2(3, 2.5f), new Color(0.25f, 0.5f, 0.95f), "zone.b");
            Zone(zones, "Зона C", new Vector2(5, 3.5f), new Vector2(3, 3), new Color(0.3f, 0.8f, 0.35f), "zone.c");
            Zone(zones, "ОПАСНО", new Vector2(0, 0.6f), new Vector2(3.2f, 2.2f), new Color(0.85f, 0.15f, 0.15f), "zone.danger");
            Zone(zones, "База", new Vector2(-5.5f, -4.8f), new Vector2(3, 2.6f), new Color(0.45f, 0.55f, 0.65f), "zone.base");
            Zone(zones, "Погрузка", new Vector2(5.6f, -1), new Vector2(2.6f, 2.6f), new Color(0.95f, 0.55f, 0.15f), "dock.box", "Box");
        }

        void Zone(Transform parent, string title, Vector2 position, Vector2 size, Color color, string action, string actorTag = "Player")
        {
            var zone = new GameObject(title);
            zone.transform.SetParent(parent);
            zone.transform.position = new Vector3(position.x, 0, position.y);
            var trigger = zone.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(size.x, 2, size.y);
            trigger.center = new Vector3(0, 1, 0);
            var source = zone.AddComponent<TriggerActionSource>();
            DemoWiring.Set(source, "onEnter", assets.Actions[action]);
            DemoWiring.Set(source, "actorTag", actorTag);

            var plate = Box("Plate", zone.transform, new Vector3(0, 0.01f, 0), new Vector3(size.x, 0.02f, size.y), Mat("Zone " + action, color));
            Object.DestroyImmediate(plate.GetComponent<Collider>());
            Label(zone.transform, title, new Vector3(0, 0.05f, 0), new Color(0.08f, 0.08f, 0.1f));
        }

        void BuildObjects()
        {
            var objects = new GameObject("Objects").transform;

            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "Стойка";
            pillar.transform.SetParent(objects);
            pillar.transform.position = new Vector3(-5, 1, -0.5f);
            pillar.transform.localScale = new Vector3(0.9f, 1, 0.9f);
            pillar.GetComponent<Renderer>().sharedMaterial = Mat("Pillar", new Color(0.6f, 0.6f, 0.62f));
            DemoWiring.Set(pillar.AddComponent<CollisionActionSource>(), "onCollision", assets.Actions["pillar.touch"]);
            Label(objects, "Стойка", new Vector3(-5, 2.05f, -1.6f));

            var terminal = Box("Терминал", objects, new Vector3(6.6f, 0.6f, -5.6f), new Vector3(1.1f, 1.2f, 1.1f), Mat("Terminal", new Color(0.2f, 0.45f, 0.9f)));
            DemoWiring.Set(terminal.AddComponent<CollisionActionSource>(), "onCollision", assets.Actions["terminal.touch"]);
            Label(objects, "Терминал", new Vector3(6.6f, 1.25f, -4.6f));

            var crate = Box("Ящик", objects, new Vector3(2.6f, 0.4f, -1), new Vector3(0.8f, 0.8f, 0.8f), Mat("Crate", new Color(0.55f, 0.35f, 0.18f)));
            crate.tag = "Box";
            var body = crate.AddComponent<Rigidbody>();
            body.mass = 2;
            body.linearDamping = 4;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            Label(objects, "Ящик", new Vector3(2.6f, 0.85f, -1.75f));

            // Custom component with UnityEvents, connected to the scenario only through persistent listeners.
            var lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lamp.name = "Рубильник (лампа)";
            lamp.transform.SetParent(objects);
            lamp.transform.position = new Vector3(-7, 0.5f, 6.2f);
            lamp.transform.localScale = Vector3.one * 0.7f;
            Object.DestroyImmediate(lamp.GetComponent<Collider>());
            lamp.GetComponent<Renderer>().sharedMaterial = Mat("Lamp", Color.gray);
            Label(objects, "Питание", new Vector3(-5.6f, 0.05f, 6.2f));

            PowerSwitch = lamp.AddComponent<PowerSwitch>();
            DemoWiring.Set(PowerSwitch, "indicator", lamp.GetComponent<Renderer>());
            var relay = lamp.AddComponent<UnityEventActionSource>();
            var flag = lamp.AddComponent<StateFlag>();
            DemoWiring.Set(flag, "key", assets.PowerOn);
            UnityEventTools.AddObjectPersistentListener<ActionKey>(PowerSwitch.onTurnedOn, relay.RaiseKey, assets.Actions["switch.on"]);
            UnityEventTools.AddObjectPersistentListener<ActionKey>(PowerSwitch.onTurnedOff, relay.RaiseKey, assets.Actions["switch.off"]);
            UnityEventTools.AddPersistentListener(PowerSwitch.onChanged, new UnityAction<bool>(flag.Set));
        }

        void BuildPlayerAndCamera()
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.tag = "Player";
            player.transform.position = new Vector3(0, 0.85f, -4.5f);
            player.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            player.GetComponent<Renderer>().sharedMaterial = Mat("Player", new Color(0.95f, 0.95f, 0.95f));
            var body = player.AddComponent<Rigidbody>();
            body.constraints = RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            player.AddComponent<PlayerMover>();

            var camera = Object.FindAnyObjectByType<Camera>();
            camera.transform.SetPositionAndRotation(new Vector3(0, 20, 0), Quaternion.Euler(90, 0, 0));
            camera.orthographic = true;
            camera.orthographicSize = 7.6f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.11f);
            // The 3D view lives between the left task panel and the right control panel.
            camera.rect = new Rect(0.225f, 0, 0.655f, 1);

            var light = Object.FindAnyObjectByType<Light>();
            light.transform.rotation = Quaternion.Euler(60, -30, 0);
        }

        GameObject Box(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent);
            box.transform.localPosition = position;
            box.transform.localScale = scale;
            box.GetComponent<Renderer>().sharedMaterial = material;
            return box;
        }

        void Label(Transform parent, string text, Vector3 localPosition) => Label(parent, text, localPosition, Color.white);

        void Label(Transform parent, string text, Vector3 localPosition, Color color)
        {
            var label = new GameObject("Label");
            label.transform.SetParent(parent);
            label.transform.localPosition = localPosition;
            label.transform.rotation = Quaternion.Euler(90, 0, 0);
            var mesh = label.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.font = font;
            mesh.fontSize = 48;
            mesh.characterSize = 0.08f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = color;
            label.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        }

        static Material Mat(string name, Color color)
        {
            var path = $"{MaterialsFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;
            material = new Material(Shader.Find("Standard")) { color = color };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
