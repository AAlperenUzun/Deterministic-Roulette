using System.Collections.Generic;
using Roulette.Core;
using TMPro;
using UnityEngine;

namespace Roulette.Game
{
    // Procedurally assembles the wheel: stationary bowl + rim (stator), spinning pockets/frets/labels (rotor),
    // centre turret and ball. Re-runnable so it can rebuild when the player switches European/American.
    public static class RouletteWheelBuilder
    {
        private const float PocketInnerR = 0.46f;
        private const float PocketOuterR = 0.80f;
        private const float PocketFloorY = 0.0f;
        private const float PocketWallH = 0.05f;
        private const float FretH = 0.08f;
        private const float TrackRadius = 0.86f;
        private const float TrackY = 0.18f;
        private const float BallRestR = 0.63f;
        private const float BallRestY = 0.055f;
        private const float HubBaseR = 0.44f;
        private const float HubTopR = 0.06f;
        private const float HubH = 0.14f;
        private const float BallSize = 0.055f;

        public static WheelRig Build(Transform root, RouletteWheel wheel)
        {
            ClearChildren(root);

            int n = wheel.PocketCount;
            float step = 360f / n;

            Material wood = MaterialFactory.Create(MaterialFactory.Wood, 0f, 0.35f, doubleSided: true);
            Material gold = MaterialFactory.Create(MaterialFactory.Gold, 0.9f, 0.7f);
            Material steel = MaterialFactory.Create(MaterialFactory.Steel, 0.9f, 0.78f, doubleSided: true);
            Material red = MaterialFactory.Create(MaterialFactory.RouletteRed, 0.1f, 0.5f, doubleSided: true);
            Material black = MaterialFactory.Create(MaterialFactory.RouletteBlack, 0.1f, 0.5f, doubleSided: true);
            Material green = MaterialFactory.Create(MaterialFactory.RouletteGreen, 0.1f, 0.5f, doubleSided: true);
            Material ballMat = MaterialFactory.Create(MaterialFactory.Ivory, 0.25f, 0.88f);

            Transform stator = NewChild(root, "Stator");
            AddMesh(stator, "Bowl", MeshFactory.Revolve(BowlProfile(), 96, flip: true, name: "Bowl"), wood);
            AddMeshObject(stator, "RimBand", MeshFactory.Annulus(0.99f, 1.07f, 96, 0.30f, name: "RimBand"), gold);

            Transform rotor = NewChild(root, "Rotor");
            Transform[] labels = BuildPocketRing(rotor, wheel, step, red, black, green, steel);
            AddMesh(rotor, "Hub", MeshFactory.Cone(HubBaseR, HubTopR, HubH, 64, name: "Hub"), gold);
            BuildTurret(rotor, gold, steel);

            Transform ball = CreatePrimitive(PrimitiveType.Sphere, stator, "Ball", Vector3.one * BallSize, ballMat);
            ball.localPosition = WheelRig.Direction(0f) * TrackRadius + Vector3.up * TrackY;

            return new WheelRig
            {
                Root = root,
                Stator = stator,
                Rotor = rotor,
                Ball = ball,
                NumberLabels = labels,
                Wheel = wheel,
                PocketCount = n,
                AnglePerPocket = step,
                PocketRadius = BallRestR,
                PocketY = BallRestY,
                TrackRadius = TrackRadius,
                TrackY = TrackY
            };
        }

        private static Transform[] BuildPocketRing(Transform rotor, RouletteWheel wheel, float step,
            Material red, Material black, Material green, Material steel)
        {
            int n = wheel.PocketCount;
            var verts = new List<Vector3>();
            var subTris = new[] { new List<int>(), new List<int>(), new List<int>(), new List<int>() };
            var labels = new Transform[n];

            void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, int sub)
            {
                int i = verts.Count;
                verts.Add(a); verts.Add(b); verts.Add(c); verts.Add(d);
                subTris[sub].Add(i); subTris[sub].Add(i + 1); subTris[sub].Add(i + 2);
                subTris[sub].Add(i); subTris[sub].Add(i + 2); subTris[sub].Add(i + 3);
            }

            Vector3 P(float r, float angle, float y) => WheelRig.Direction(angle) * r + Vector3.up * y;
            float fretHalf = step * 0.06f;

            for (int i = 0; i < n; i++)
            {
                RoulettePocket pocket = wheel.Pockets[i];
                int sub = pocket.Color switch { PocketColor.Red => 0, PocketColor.Black => 1, _ => 2 };
                float center = i * step;
                float a0 = center - step * 0.5f + fretHalf;
                float a1 = center + step * 0.5f - fretHalf;

                // floor
                Quad(P(PocketInnerR, a0, PocketFloorY), P(PocketOuterR, a0, PocketFloorY),
                     P(PocketOuterR, a1, PocketFloorY), P(PocketInnerR, a1, PocketFloorY), sub);
                // outer wall
                Quad(P(PocketOuterR, a0, PocketFloorY), P(PocketOuterR, a0, PocketFloorY + PocketWallH),
                     P(PocketOuterR, a1, PocketFloorY + PocketWallH), P(PocketOuterR, a1, PocketFloorY), sub);

                // fret divider — a thin solid wall (two offset faces + a top), so no coincident geometry
                float b = center + step * 0.5f;
                float d = step * 0.025f; // half-thickness in degrees
                Quad(P(PocketInnerR, b - d, PocketFloorY), P(PocketOuterR, b - d, PocketFloorY),
                     P(PocketOuterR, b - d, PocketFloorY + FretH), P(PocketInnerR, b - d, PocketFloorY + FretH), 3);
                Quad(P(PocketInnerR, b + d, PocketFloorY + FretH), P(PocketOuterR, b + d, PocketFloorY + FretH),
                     P(PocketOuterR, b + d, PocketFloorY), P(PocketInnerR, b + d, PocketFloorY), 3);
                Quad(P(PocketInnerR, b - d, PocketFloorY + FretH), P(PocketOuterR, b - d, PocketFloorY + FretH),
                     P(PocketOuterR, b + d, PocketFloorY + FretH), P(PocketInnerR, b + d, PocketFloorY + FretH), 3);

                labels[i] = CreateLabel(rotor, pocket.Label, center);
            }

            var mesh = new Mesh { name = "Pockets", subMeshCount = 4 };
            mesh.SetVertices(verts);
            for (int s = 0; s < 4; s++) mesh.SetTriangles(subTris[s], s);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var go = new GameObject("Pockets", typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(rotor, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterials = new[] { red, black, green, steel };
            return labels;
        }

        private static Transform CreateLabel(Transform rotor, string text, float angle)
        {
            var go = new GameObject($"Label_{text}");
            go.transform.SetParent(rotor, false);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 0.05f;
            tmp.fontSizeMax = 0.5f;
            tmp.rectTransform.sizeDelta = new Vector2(0.3f, 0.22f);
            go.transform.localRotation = Quaternion.Euler(90f, angle, 0f);
            go.transform.localPosition = WheelRig.Direction(angle) * BallRestR + Vector3.up * (PocketWallH + 0.01f);
            return go.transform;
        }

        private static void BuildTurret(Transform rotor, Material gold, Material steel)
        {
            CreatePrimitive(PrimitiveType.Cylinder, rotor, "TurretStem", new Vector3(0.05f, 0.09f, 0.05f), gold)
                .localPosition = Vector3.up * (HubH + 0.06f);
            Transform knob = CreatePrimitive(PrimitiveType.Sphere, rotor, "Knob", Vector3.one * 0.09f, gold);
            knob.localPosition = Vector3.up * (HubH + 0.17f);

            Transform barA = CreatePrimitive(PrimitiveType.Cube, rotor, "BarA", new Vector3(0.62f, 0.018f, 0.05f), steel);
            barA.localPosition = Vector3.up * (HubH + 0.04f);
            Transform barB = CreatePrimitive(PrimitiveType.Cube, rotor, "BarB", new Vector3(0.05f, 0.018f, 0.62f), steel);
            barB.localPosition = Vector3.up * (HubH + 0.04f);
        }

        private static IReadOnlyList<Vector2> BowlProfile() => new[]
        {
            new Vector2(0.78f, 0.16f),  // inner open lip (pockets show through the hole inside this)
            new Vector2(0.82f, 0.175f),
            new Vector2(0.86f, 0.165f), // ball-track valley
            new Vector2(0.92f, 0.20f),
            new Vector2(0.99f, 0.295f), // rim crest
            new Vector2(1.05f, 0.27f),
            new Vector2(1.07f, 0.06f),
            new Vector2(1.07f, -0.14f),
            new Vector2(1.00f, -0.18f),
            new Vector2(0.0f, -0.18f)   // base disc
        };

        private static Transform NewChild(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static void AddMesh(Transform parent, string name, Mesh mesh, Material material) =>
            AddMeshObject(parent, name, mesh, material);

        private static Transform AddMeshObject(Transform parent, string name, Mesh mesh, Material material)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            return go.transform;
        }

        private static Transform CreatePrimitive(PrimitiveType type, Transform parent, string name, Vector3 scale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            var collider = go.GetComponent<Collider>();
            if (Application.isPlaying) Object.Destroy(collider);
            else Object.DestroyImmediate(collider);
            go.transform.SetParent(parent, false);
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            return go.transform;
        }

        private static void ClearChildren(Transform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                GameObject child = root.GetChild(i).gameObject;
                if (Application.isPlaying) Object.Destroy(child);
                else Object.DestroyImmediate(child);
            }
        }
    }
}
