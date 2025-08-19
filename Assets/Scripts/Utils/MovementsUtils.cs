using UnityEngine;

public static class MovementUtils
{
    // Déplacement de 'from' vers 'to' avec vitesse constante
    public static Vector3 LerpConstant(Vector3 from, Vector3 to, float speed)
    {
        Vector3 direction = to - from;
        float distance = direction.magnitude;

        if (distance <= 0.0001f) // si on est déjà sur place
            return to;

        Vector3 move = direction.normalized * speed;

        // On ne dépasse pas la cible
        if (move.magnitude > distance)
            return to;

        return from + move;
    }
}
