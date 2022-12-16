using UnityEngine;


public static class MathHelper {
    public static Vector3 GetRollPitchYaw( Quaternion pQuaternion ) {
        float x = pQuaternion.x, y = pQuaternion.y, z = pQuaternion.z, w = pQuaternion.w;

        float roll = Mathf.Atan2( 2 * y * w - 2 * x * z, 1 - 2 * y * y - 2 * z * z );
        float pitch = Mathf.Atan2( 2 * x * w - 2 * y * z, 1 - 2 * x * x - 2 * z * z );
        float yaw = Mathf.Asin( 2 * x * y + 2 * z * w );

        return new Vector3( roll, pitch, yaw );
    }
}