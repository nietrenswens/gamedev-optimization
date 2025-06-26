using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpaceDefence.Engine;
using System;
using System.Collections.Generic;

namespace SpaceDefence
{
    public class GameManager
    {
        private static GameManager gameManager;

        public List<Ship> Team1Ships { get; private set; }
        public List<Ship> Team2Ships { get; private set; }
        public List<GameObject> CollidableGameObjects { get; private set; }
        public Grid CollidableGameObjectsGrid { get; private set; }
        public Grid NearestShipGrid { get; private set; }

        private List<GameObject> _gameObjects;
        private List<GameObject> _bullets;
        private List<GameObject> _nonBullets;
        private List<GameObject> _toBeRemoved;
        private List<GameObject> _toBeAdded;
        private ContentManager _content;
        private Effect _teamColorEffect;
        public Matrix WorldMatrix { get; set; }

        public Random RNG { get; private set; }
        public InputManager InputManager { get; private set; }
        public Game Game { get; private set; }

        public static GameManager GetGameManager()
        {
            if (gameManager == null)
                gameManager = new GameManager();
            return gameManager;
        }
        public GameManager()
        {
            _gameObjects = new List<GameObject>();
            _toBeRemoved = new List<GameObject>();
            _toBeAdded = new List<GameObject>();
            _bullets = new List<GameObject>();
            CollidableGameObjectsGrid = new(128);
            _nonBullets = new List<GameObject>();
            Team1Ships = new List<Ship>();
            Team2Ships = new List<Ship>();
            CollidableGameObjects = new();
            NearestShipGrid = new(1500);
            InputManager = new InputManager();
            RNG = new Random();
            WorldMatrix = Matrix.CreateScale(.3f);
            //WorldMatrix = Matrix.CreateScale(1f) * Matrix.CreateTranslation(0, -800, 0);
        }

        public void Initialize(ContentManager content, Game game)
        {
            Game = game;
            _content = content;
        }

        public void Load(ContentManager content)
        {
            _teamColorEffect = content.Load<Effect>("TeamColors");
            foreach (GameObject gameObject in _gameObjects)
            {
                gameObject.Load(content);
            }
        }

        public void HandleInput(InputManager inputManager)
        {
            foreach (GameObject gameObject in _gameObjects)
            {
                gameObject.HandleInput(this.InputManager);
            }
        }

        public void CheckCollision()
        {
            // Checks once for every pair of 2 GameObjects if the collide.
            for (int i = 0; i < _bullets.Count; i++)
            {
                var gridCoordinate = CollidableGameObjectsGrid.GetGridCoordinate(_bullets[i].GetPosition().Center);
                var possibleShips = CollidableGameObjectsGrid.GetObjectsInAndAroundGridCoordinate(gridCoordinate);
                for (int j = 0; j < possibleShips.Count; j++)
                {
                    if (_bullets[i].CheckCollision(possibleShips[j]))
                    {
                        _bullets[i].OnCollision(possibleShips[j]);
                        possibleShips[j].OnCollision(_bullets[i]);
                    }
                }
            }

        }

        public void Update(GameTime gameTime)
        {
            InputManager.Update();

            // Handle input
            HandleInput(InputManager);


            // Update
            foreach (GameObject gameObject in _gameObjects)
            {
                gameObject.Update(gameTime);
            }

            // Check Collission
            CheckCollision();

            foreach (GameObject gameObject in _toBeAdded)
            {
                gameObject.Load(_content);
                _gameObjects.Add(gameObject);
                if (gameObject.CollisionType.HasFlag(CollisionType.Solid))
                    CollidableGameObjects.Add(gameObject);
                if (gameObject is Bullet)
                {
                    _bullets.Add(gameObject);
                }
                else if (gameObject.CollisionType.HasFlag(CollisionType.Solid))
                {
                    _nonBullets.Add(gameObject);
                    if (gameObject is Ship ship)
                    {
                        if (ship.CollisionType.HasFlag(CollisionType.Team1))
                        {
                            Team1Ships.Add(ship);
                        }
                        else if (ship.CollisionType.HasFlag(CollisionType.Team2))
                        {
                            Team2Ships.Add(ship);
                        }
                    }
                }
            }
            _toBeAdded.Clear();

            foreach (GameObject gameObject in _toBeRemoved)
            {
                gameObject.Destroy();
                _gameObjects.Remove(gameObject);
                if (gameObject.CollisionType.HasFlag(CollisionType.Solid))
                    CollidableGameObjects.Remove(gameObject);
                if (gameObject is Bullet)
                {
                    _bullets.Remove(gameObject);
                }
                else
                {
                    _nonBullets.Remove(gameObject);
                    if (gameObject is Ship ship)
                    {
                        if (ship.CollisionType.HasFlag(CollisionType.Team1))
                        {
                            Team1Ships.Remove(ship);
                        }
                        else if (ship.CollisionType.HasFlag(CollisionType.Team2))
                        {
                            Team2Ships.Remove(ship);
                        }

                        NearestShipGrid.Remove(ship);
                        CollidableGameObjectsGrid.Remove(ship);
                    }
                }
            }
            _toBeRemoved.Clear();
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(transformMatrix: WorldMatrix, effect: _teamColorEffect);
            foreach (GameObject gameObject in _gameObjects)
            {
                gameObject.Draw(gameTime, spriteBatch);
            }
            spriteBatch.End();
        }

        /// <summary>
        /// Add a new GameObject to the GameManager. 
        /// The GameObject will be added at the start of the next Update step. 
        /// Once it is added, the GameManager will ensure all steps of the game loop will be called on the object automatically. 
        /// </summary>
        /// <param name="gameObject"> The GameObject to add. </param>
        public void AddGameObject(GameObject gameObject)
        {
            _toBeAdded.Add(gameObject);
        }

        /// <summary>
        /// Remove GameObject from the GameManager. 
        /// The GameObject will be removed at the start of the next Update step and its Destroy() mehtod will be called.
        /// After that the object will no longer receive any updates.
        /// </summary>
        /// <param name="gameObject"> The GameObject to Remove. </param>
        public void RemoveGameObject(GameObject gameObject)
        {
            _toBeRemoved.Add(gameObject);
        }

        public List<GameObject> GetGameObjects()
        {
            return _gameObjects;
        }

        /// <summary>
        /// Get a random location on the screen.
        /// </summary>
        public Vector2 RandomScreenLocation()
        {
            return new Vector2(
                RNG.Next(0, Game.GraphicsDevice.Viewport.Width),
                RNG.Next(0, Game.GraphicsDevice.Viewport.Height));
        }
    }
}
