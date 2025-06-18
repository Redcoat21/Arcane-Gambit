using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Edgar.GraphBasedGenerator.Grid2D;
using UnityEngine;

namespace Edgar.Unity
{
    /// <summary>
    /// Creates an input for the generator from a given level graph.
    /// </summary>
    internal class FixedLevelGraphInputTaskGrid2D : PipelineTask<DungeonGeneratorPayloadGrid2D>
    {
        private readonly FixedLevelGraphConfigGrid2D config;

        public FixedLevelGraphInputTaskGrid2D(FixedLevelGraphConfigGrid2D config)
        {
            this.config = config;
        }

        public override IEnumerator Process()
        {
            LevelGraph selectedGraph = GetLevelGraphForCurrentLevel();

            if (selectedGraph == null)
            {
                throw new ConfigurationException("No valid LevelGraph found for current level.");
            }

            if (selectedGraph.Rooms.Count == 0)
            {
                throw new ConfigurationException($"Each level graph must contain at least one room. Please add some rooms to the level graph called \"{selectedGraph.name}\".");
            }

            var levelDescription = new LevelDescriptionGrid2D();

            // Setup individual rooms
            foreach (var room in selectedGraph.Rooms)
            {
                var roomTemplates = InputSetupUtils.GetRoomTemplates(room, selectedGraph.DefaultRoomTemplateSets, selectedGraph.DefaultIndividualRoomTemplates);

                if (roomTemplates.Count == 0)
                {
                    throw new ConfigurationException($"There are no room templates for the room \"{room.GetDisplayName()}\" and also no room templates in the default set of room templates. Please make sure that the room has at least one room template available.");
                }

                levelDescription.AddRoom(room, roomTemplates);
            }

            var typeOfRooms = selectedGraph.Rooms.First().GetType();

            // Add passages
            foreach (var connection in selectedGraph.Connections)
            {
                if (config.UseCorridors)
                {
                    var corridorRoom = (RoomBase)ScriptableObject.CreateInstance(typeOfRooms);

                    if (corridorRoom is Room basicRoom)
                    {
                        basicRoom.Name = "Corridor";
                    }

                    levelDescription.AddCorridorConnection(connection, corridorRoom,
                        InputSetupUtils.GetRoomTemplates(connection, selectedGraph.CorridorRoomTemplateSets, selectedGraph.CorridorIndividualRoomTemplates));
                }
                else
                {
                    levelDescription.AddConnection(connection);
                }
            }

            InputSetupUtils.CheckIfDirected(levelDescription, selectedGraph);

            Payload.LevelDescription = levelDescription;

            yield return null;
        }

        private LevelGraph GetLevelGraphForCurrentLevel()
        {
            int level = LevelManager.LevelCounter;

            if (level == 1 || level == 2) return config.LevelGraph;
            if (level == 3) return config.LevelGraph2;
            if (level == 4 || level == 5) return config.LevelGraph3;
            if (level == 6) return config.LevelGraph4;
            if (level == 7 || level == 8) return config.LevelGraph5;
            if (level == 9) return config.LevelGraph6;

            return null; // fallback (shouldn’t occur)
        }
    }
}