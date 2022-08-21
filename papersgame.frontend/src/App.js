import { useState } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import './App.css';
import 'bootstrap/dist/css/bootstrap.min.css';
import { SignalRAPI } from './api/endpointConstants';
import { store } from './store';
import SignalRTest from './components/signalR/SignalRTest';
import { Provider } from 'react-redux';
import { ClientMainPageContainer } from './components/mainPage/client/clientMainPageContainer';

const App = () => {
    const [connection, setConnection] = useState();

    const startConnection = async () => {
        try {
            const connection = new HubConnectionBuilder()
                .withUrl(SignalRAPI.BuildGetConnection)
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
        <Provider store={store}>
            <ClientMainPageContainer/>
            <h2>TestSignalR</h2>
            <hr className='line' />
            <SignalRTest startConnection={startConnection} closeConnection={closeConnection} />
        </Provider>
    </div>
}

export default App;
