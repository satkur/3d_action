using UnityEngine.AI;

namespace Framework {
    public static class ConstNavMeshArea {
        public static int Walkable { get; private set; } = NavMesh.GetAreaFromName("Walkable");
    }
}
