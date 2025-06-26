using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SpaceDefence.Engine
{
    public class Grid
    {
        public int CellSize;

        private Dictionary<GridCoordinate, List<GameObject>> _gridObjects;

        public Grid(int cellSize)
        {
            _gridObjects = new Dictionary<GridCoordinate, List<GameObject>>();
            CellSize = cellSize;
        }

        public void Add(GameObject gameObject)
        {
            GridCoordinate gridPosition = GetGridCoordinate(gameObject.GetPosition().Center);
            if (!_gridObjects.ContainsKey(gridPosition))
            {
                _gridObjects[gridPosition] = new List<GameObject>();
            }
            _gridObjects[gridPosition].Add(gameObject);
        }

        public void Remove(GameObject gameObject)
        {
            GridCoordinate gridPosition = GetGridCoordinate(gameObject.GetPosition().Center);
            if (_gridObjects.ContainsKey(gridPosition))
            {
                _gridObjects[gridPosition].Remove(gameObject);
                if (_gridObjects[gridPosition].Count == 0)
                {
                    _gridObjects.Remove(gridPosition);
                }
            }
        }

        public List<GameObject> GetObjectsInAndAroundGridCoordinate(GridCoordinate gridCoordinate)
        {
            List<GameObject> objects = new List<GameObject>();
            for (int x = gridCoordinate.X - 1; x <= gridCoordinate.X + 1; x++)
            {
                for (int y = gridCoordinate.Y - 1; y <= gridCoordinate.Y + 1; y++)
                {
                    var coordinate = new GridCoordinate(x, y);
                    if (!_gridObjects.ContainsKey(coordinate))
                        continue;
                    var gridObjects = _gridObjects[coordinate];
                    objects.AddRange(gridObjects);
                }
            }
            return objects;
        }

        public List<GameObject> GetObjectsAt(GridCoordinate gridCoordinate)
        {
            if (_gridObjects.TryGetValue(gridCoordinate, out var objects))
            {
                return objects;
            }
            return new List<GameObject>();
        }

        public GridCoordinate GetGridCoordinate(Point position)
        {
            return new GridCoordinate(
                position.X / CellSize,
                position.Y / CellSize);
        }
    }

    public record GridCoordinate(int X, int Y);
}
