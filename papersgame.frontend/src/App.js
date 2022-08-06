import { useState } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import SignalRTest from './components/SignalRTest';
import './App.css';
import 'bootstrap/dist/css/bootstrap.min.css';

const App = () => {
    const [connection, setConnection] = useState();

    const startConnection = async () => {
        try {
            const connection = new HubConnectionBuilder()
                .withUrl("https://localhost:44354/gameHub")
                .configureLogging(LogLevel.Information)
                .build();

            
            connection.onclose(e => {
                setConnection();
            });

            await connection.start();
            setConnection(connection);
        } catch (e) {
            console.log(e);
        }
    }

    const closeConnection = async () => {
        try {
            await connection.stop();
        } catch (e) {
            console.log(e);
        }
    }

    return <div className='app'>
        <h2>TestSignalR</h2>
        <hr className='line' />
        <SignalRTest startConnection={startConnection} closeConnection={closeConnection}/>
    </div>
}

export default App;
