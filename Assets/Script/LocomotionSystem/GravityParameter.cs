using UnityEngine;

namespace Framework {
    public class GravityParameter {
        Vector3 gravityAccelVector;
        public float scale;

        public GravityParameter(Vector3 gravityVector, float scale) {
            this.gravityAccelVector = gravityVector;
            this.scale = scale;
        }

        public Vector3 GetScaledGravityAccel() {
            return gravityAccelVector * scale;
        }
    }
}
