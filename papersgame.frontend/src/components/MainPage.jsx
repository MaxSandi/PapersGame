import { MDBContainer } from "mdb-react-ui-kit";
import { Button, Container } from "react-bootstrap";
import { Link } from "react-router-dom";

export default function MainPage() {
    return (
        <div>
            <div className="position-absolute top-50 start-50 translate-middle">
                <Container>                 
                    <Link to="/game-create">
                        <Button variant="primary" className="mr-1">Create game</Button>
                    </Link>
                    <Link to="/game-join">
                        <Button variant="primary" className="ml-1">Join to game</Button>
                    </Link>
                </Container>
            </div>
        </div>
    )
}