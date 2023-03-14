import { useEffect, useState } from "react";
import {
    HubConnectionBuilder,
    LogLevel,
} from "@microsoft/signalr";
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
    const cookies = new Cookies();

    useEffect(() => {
        const connection = new HubConnectionBuilder()
            .withUrl("https://localhost:44354/gameHub")
            .configureLogging(LogLevel.Information)
            .build();

        // connection.onclose(e => {
        //     setConnection();
        //     console.log("connection closed!");
        // });

        connection.start().then(() => {
            setIsConnectionReady(true);
            setConnection(connection);
        });
    }, []);

    useEffect(() => {
        console.log(
            connection,
            isConnectionReady,
            cookies.get("gameConnection")?.gameId
        );
        if (
            connection &&
            isConnectionReady &&
            cookies.get("gameConnection")?.gameId
        ) {
            connection.on("IGameJoined", (game) => {
                console.log(game, "IGameJoined");
                setCurrentGame(game);
                navigate("/lobby=" + cookies.get("gameConnection").gameId, {
                    replace: true,
                });
            });
            console.log(
                connection,
                isConnectionReady,
                cookies.get("gameConnection")?.gameId,
                cookies.get("gameConnection").userName
            );
            console.log(
                cookies.get("gameConnection").userName,
                cookies.get("gameConnection").gameId
            );
            connection.invoke(
                "JoinGame",
                cookies.get("gameConnection").userName,
                `${cookies.get("gameConnection").gameId}, true`
            );
            console.log(cookies.get("gameConnection")?.gameId, "Join game");
        }
    }, [connection]);

    const createGame = async (username, gameName, playerCount) => {
        try {
            await connection.invoke("CreateGame", gameName, playerCount);
            console.log("GameHub - CreateGame");

            await joinGame(username);
        } catch (e) {
            console.log(e);
        }
    };

    const joinGame = async (username) => {
        try {
            connection.on("IGameJoined", (game) => {
                console.log("IGameJoined", game);
                setCurrentGame(game);

                cookies.set(
                    "gameConnection",
                    {
                        userName: username,
                        gameId: game?.id,
                        connectionId: game?.connectionId,
                    },
                    { path: "/" }
                );
                navigate("/lobby=" + game?.id, { replace: true });
            });

            await connection.invoke("JoinGame", username, "111, false");
            console.log("GameHub - JoinGame");

            console.log(cookies.get("gameConnection"));
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
                <Route
                    path="/lobby=:id"
                    element={
                        <LobbyPage
                            connection={connection}
                            currentGame={currentGame}
                        />
                    }
                />
            </Routes>
        </div>
    );
};

export default App;
