using UnityEngine;

public class PhysicsUtils : MonoBehaviour
{
    public static float GetFlightTime(float startY, float startYVel, float acceleration, float targetY) {
        const float eps = 1e-6f;

        float dy = startY - targetY; // (x0 - xf)

        // Handle zero (or near-zero) acceleration: linear motion
        if (Mathf.Abs(acceleration) < eps)
        {
            if (Mathf.Abs(startYVel) < eps)
                return Mathf.Abs(dy) < eps ? 0f : float.NaN;

            float t = (targetY - startY) / startYVel;
            return t >= 0f ? t : float.NaN;
        }

        // Solve: 0.5*a*t^2 + v0*t + (startY - targetY) = 0
        float disc = startYVel * startYVel - 2f * acceleration * dy; // v0^2 - 2a(x0-xf)
        if (disc < 0f)
            return float.NaN;

        float sqrtDisc = Mathf.Sqrt(disc);

        float t1 = (-startYVel - sqrtDisc) / acceleration;
        float t2 = (-startYVel + sqrtDisc) / acceleration;

        // Pick the earliest non-negative time
        float best = float.PositiveInfinity;

        if (t1 >= 0f && t1 < best) best = t1;
        if (t2 >= 0f && t2 < best) best = t2;

        return float.IsPositiveInfinity(best) ? float.NaN : best;
    }

    public static Vector3 GetPositionAtTime(GridPath path, float time, float currentPositionAlongPath, float speed)
    {
        float distanceToTravel = speed * time;
        float targetPositionAlongPath = currentPositionAlongPath + distanceToTravel;
        targetPositionAlongPath = Mathf.Min(targetPositionAlongPath, path.PathLength);
        targetPositionAlongPath = Mathf.Max(targetPositionAlongPath, 0.0f);
        return path.GetWorldPositionAt(targetPositionAlongPath);
    }
}
