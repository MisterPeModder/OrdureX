using UnityEngine;

namespace OrdureX.Grid
{
    public enum Side : uint
    {
        Top,
        Bottom,
        Right,
        Left
    }

    public static class SideExt
    {
        public static Side GetOpposite(this Side side)
        {
            return side switch
            {
                Side.Top => Side.Bottom,
                Side.Bottom => Side.Top,
                Side.Right => Side.Left,
                Side.Left => Side.Right,
                _ => throw new System.ArgumentOutOfRangeException(nameof(side), side, null)
            };
        }

        public static Vector2Int GetDirection(this Side side)
        {
            return side switch
            {
                Side.Top => Vector2Int.up,
                Side.Bottom => Vector2Int.down,
                Side.Right => Vector2Int.right,
                Side.Left => Vector2Int.left,
                _ => throw new System.ArgumentOutOfRangeException(nameof(side), side, null)
            };
        }

        public static int GetRotation(this Side side)
        {
            return side switch
            {
                Side.Top => 0,
                Side.Right => 90,
                Side.Bottom => 180,
                Side.Left => 270,
                _ => throw new System.ArgumentOutOfRangeException(nameof(side), side, null)
            };
        }
    }

}
