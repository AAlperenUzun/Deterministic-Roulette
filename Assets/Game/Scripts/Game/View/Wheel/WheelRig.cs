using Roulette.Core;
using UnityEngine;

namespace Roulette.Game
{
    public sealed class WheelRig
    {
        public Transform Root;
        public Transform Stator;
        public Transform Rotor;
        public Transform Ball;
        public Transform[] NumberLabels;

        public RouletteWheel Wheel;
        public int PocketCount;
        public float AnglePerPocket;

        public float PocketRadius;        // where the ball rests inside a pocket
        public float PocketY;
        public float TrackRadius;         // where the ball orbits before dropping
        public float TrackY;

        // Unit direction for an angle measured clockwise around Y from +Z.
        public static Vector3 Direction(float angleDeg)
        {
            float a = angleDeg * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a));
        }

        public float PocketAngle(int index) => index * AnglePerPocket;

        public int IndexOfKey(int key) => Wheel.IndexOfKey(key);
    }
}
