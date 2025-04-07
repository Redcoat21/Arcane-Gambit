using UnityEngine;

namespace Components.Movements
{
    /// <summary>
    /// Interface that defines the movement of an entity
    /// </summary>
    public interface IMovementComponent
    {
        public void Move(Vector2 direction);
    }
}
