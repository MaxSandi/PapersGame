import { Button } from 'react-bootstrap';

const SignalRTest = ({ startConnection, closeConnection}) => <div>
    <div className='test-signalr'>
        <Button variant='success' onClick={() => startConnection()}>Start</Button>
        <Button variant='danger' onClick={() => closeConnection()}>Stop</Button>
    </div>
</div>

export default SignalRTest;