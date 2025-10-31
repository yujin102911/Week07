using UnityEngine;

public static class PlayerConstant
{
    public const string PlayerTag = "Player";

    // Layer Masks
    public const string CarryableMask = "Carryable";
    public const string InteractableMask = "Interactable";
    public const string ObstacleMask = "Obstacle";
    public static LayerMask CarryableLayerMask = LayerMask.GetMask(CarryableMask);
    public static LayerMask InteractableLayerMask = LayerMask.GetMask(InteractableMask);
    public static LayerMask ObstacleLayerMask = LayerMask.GetMask(ObstacleMask);

    public const float InteractableRange = 1.5f;
    public const int CarryableMaxCount = 99;
    public const float InteractCoolTime = 0.5f;
    public static Vector2 HoldPointOffset = new Vector2(0, 0.5f);
}