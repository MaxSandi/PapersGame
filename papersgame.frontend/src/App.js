import { useEffect, useState } from 'react';
import { HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { Route, Routes, useNavigate } from 'react-router-dom';
import './App.css';
import 'bootstrap/dist/css/bootstrap.min.css';
import GameCreatePage from "./components/GameCreatePage";
import GameJoinPage from "./components/GameJoinPage";
import MainPage from "./components/MainPage";
import LobbyPage from "./components/LobbyPage";

const App = () => {

    const navigate = useNavigate();
    const [connection, setConnection] = useState();
    const [currentGame, setCurrentGame] = useState();

    useEffect(() => {

        const connection = new HubConnectionBuilder()
            .withUrl("https://localhost:44354/gameHub")
            .configureLogging(LogLevel.Information)
            .build();

        connection.onclose(e => {
            setConnection();
            console.log("connection closed!");
        });

        connection.start();
        setConnection(connection);
        console.log("setConnection");

    }, [])

    const createGame = async (username, gameName, playerCount) => {
        try {

            await connection.invoke("CreateGame", gameName, playerCount);
            console.log("GameHub - CreateGame");

            await joinGame(username);

        } catch (e) {
            console.log(e);
        }
    }

    const joinGame = async (username) => {
        try
        {
            connection.on("IGameJoined", game => {
                setCurrentGame(game);
                console.log("IGameJoined");

                navigate('/lobby=' + game.id, { replace: true });
            });

            await connection.invoke("JoinToGame", username, "111");
            console.log("GameHub - JoinToGame");

        } catch (e) {
            console.log(e);
        }
    }

    return <div className='app'>
        <h2>PapersGame</h2>
        <hr className='line' />
        <Routes>
            <Route path="/" element={<MainPage />} />
            <Route path='/game-create' element={<GameCreatePage createGame={createGame}/>} />
            <Route path='/game-join' element={<GameJoinPage joinGame={joinGame}/>} />
            <Route path='/lobby=:id' element={<LobbyPage connection={connection} currentGame={currentGame}/>} />
        </Routes>
    </div>
}

export default App;
