﻿using Si.GameEngine.Core;
using Si.Library.Payload;
using Si.MultiplayClient;


namespace Si.IndependentLobbyHostWithAI
{
    /// <summary>
    /// Uses the Si.Multiplay and Si.GameEngine modules to allow for hosting lobbies a situation server which includes human and AI players.
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            var engine = CreateGameAndLobby("Boaring AI Hoast", 1, 10, 30);

            //TODO: Add situation load logic.


            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();
        }

        private static GameEngineCore CreateGameAndLobby(string lobbyName, int minPlayers, int maxPlayers, int autoStartSeconds)
        {
            Console.WriteLine("Starting multiplay...");
            var multiplay = new EngineMultiplayManager();
            Console.WriteLine("Success!");

            Console.WriteLine("Starting game engine...");
            var engine = new GameEngineCore(multiplay);
            Console.WriteLine("Success!");

            //multiplay.OnReceivedSituationLayout += Multiplay_OnReceivedSituationLayout;
            //multiplay.OnNeedSituationLayout += Multiplay_OnNeedSituationLayout;
            //multiplay.OnSpriteVectorsUpdated += Multiplay_OnSpriteVectorsUpdated;

            Console.WriteLine("Configuring server connection...");
            multiplay.ConfigureConnection();
            Console.WriteLine("Success!");

            Console.WriteLine($"Creating lobby: '{lobbyName}'...");
            multiplay.CreateLobby(new SiLobbyConfiguration(lobbyName, minPlayers, maxPlayers, autoStartSeconds));
            Console.WriteLine("Success!");

            return engine;
        }
    }
}
