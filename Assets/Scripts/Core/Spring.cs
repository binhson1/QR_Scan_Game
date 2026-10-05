using UnityEngine;

public struct SpringState
{
    public float Value;
    public float Velocity;
    public float Target;
    public float Omega;
    public float Zeta;

    public static SpringState Make(float value, float omega = 16f, float zeta = 0.62f)
    {
        return new SpringState { Value = value, Target = value, Omega = omega, Zeta = zeta };
    }

    public void Step(float dt)
    {
        if (dt <= 0f) return;
        dt = Mathf.Min(dt, 1f / 30f);
        float f = Omega * Omega;
        float d = 2f * Zeta * Omega;
        Velocity += ((Target - Value) * f - Velocity * d) * dt;
        Value += Velocity * dt;
    }
}

public struct Spring2
{
    public Vector2 Value;
    public Vector2 Velocity;
    public Vector2 Target;
    public float Omega;
    public float Zeta;

    public static Spring2 Make(Vector2 value, float omega = 12f, float zeta = 0.72f)
    {
        return new Spring2 { Value = value, Target = value, Omega = omega, Zeta = zeta };
    }

    public void Step(float dt)
    {
        if (dt <= 0f) return;
        dt = Mathf.Min(dt, 1f / 30f);
        float f = Omega * Omega;
        float d = 2f * Zeta * Omega;
        Velocity += ((Target - Value) * f - Velocity * d) * dt;
        Value += Velocity * dt;
    }
}
