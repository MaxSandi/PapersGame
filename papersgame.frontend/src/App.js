import { useEffect, useState } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { Route, Routes, useNavigate } from "react-router-dom";
import "./App.css";
import "bootstrap/dist/css/bootstrap.min.css";
import GameCreatePage from "./components/GameCreatePage";
import GameJoinPage from "./components/GameJoinPage";
import MainPage from "./components/MainPage";
import LobbyPage from "./components/LobbyPage";
import Cookies from "universal-cookie";

const App = () => {
    const navigate = useNavigate();
    const [connection, setConnection] = useState();
    const [isConnectionReady, setIsConnectionReady] = useState(false);
    const [currentGame, setCurrentGame] = useState();
    const [currentPlayerId, setCurrentPlayerId] =
        useState();
    const cookies = new Cookies();

    useEffect(() => {
        const connection = new HubConnectionBuilder()
            .withUrl("https://localhost:44354/gameHub")
            .configureLogging(LogLevel.Information)
            .build();

        connection.start().then(() => {
            setIsConnectionReady(true);
            setConnection(connection);
        });
    }, []);

    useEffect(() => {
        if (
            connection &&
            isConnectionReady &&
            cookies.get("gameConnectionInfo")?.gameId
        ) {
            connection?.invoke(
                "CanReconnectGame",
                cookies.get("gameConnectionInfo").gameId,
                cookies.get("gameConnectionInfo").playerId)
                .then((res) => {
                    if (res) {
                        connection.on("IGameReconnected", (game) => {
                            console.log(game, "IGameReconnected");
                            setCurrentGame(game);
                            setCurrentPlayerId(cookies.get("gameConnectionInfo").playerId);
                            navigate("/lobby=" + cookies.get("gameConnectionInfo").gameId, {
                                replace: true,
                            });
                        });

                        connection.invoke(
                            "ReconnectGame",
                            cookies.get("gameConnectionInfo").gameId,
                            cookies.get("gameConnectionInfo").playerId
                        );
                        console.log(cookies.get("gameConnectionInfo")?.gameId, "Reconnect game");
                    }
                });

        }
    }, [connection]);

    const createGame = async (username, gameName, playerCount) => {
        try {
            connection.on("IGameCreated", async (game) => {
                console.log("IGameCreated", game);

                await joinGame(username, game.id);
            });
            await connection.invoke("CreateGame", gameName, playerCount);
            console.log("GameHub - CreateGame");
        } catch (e) {
            console.log(e);
        }
    };

    const joinGame = async (username) => {
        try {
            connection.on("IGameJoined", (game, player) => {
                setCurrentGame(game);
                setCurrentPlayerId(player.id);

                cookies.set(
                    "gameConnectionInfo",
                    {
                        gameId: game?.id,
                        playerId: player?.id,
                    },
                    { path: "/" }
                );
                navigate("/lobby=" + game?.id, { replace: true });
            });

            await connection.invoke(
                "JoinGame",
                "",//TODO: replace to game id
                username
            );
        } catch (e) {
            console.log(e);
        }
    };

    return (
        <div className="app">
            <h2>PapersGame</h2>
            <hr className="line" />
            <Routes>
                <Route path="/" element={<MainPage />} />
                <Route
                    path="/game-create"
                    element={<GameCreatePage createGame={createGame} />}
                />
                <Route
                    path="/game-join"
                    element={<GameJoinPage joinGame={joinGame} />}
                />
                {currentGame && connection && currentPlayerId && (
                    <Route
                        path="/lobby=:id"
                        element={
                            <LobbyPage
                                connection={connection}
                                currentGame={currentGame}
                                currentPlayerId={
                                    currentPlayerId
                                }
                                setCurrentGame={setCurrentGame}
                            />
                        }
                    />
                )}
            </Routes>
        </div>
    );
};

export default App;
