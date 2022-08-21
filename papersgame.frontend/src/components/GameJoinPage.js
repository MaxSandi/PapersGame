import { useState } from "react";
import { Button, Col, Container, Form, Row } from "react-bootstrap";

export default function GameJoinPage(props) {
    const [playerName, setPlayerName] = useState("Petia");

    return (
        <Container>
            <Row className="justify-content-center">
                <Col sm={6}>
                    <Form>
                        <Form.Group controlId="formGameName">
                            <Form.Label>Имя игрока</Form.Label>
                            <Form.Control name="playerName" type="text" placeholder=""
                                value={playerName} onChange={(event) => setPlayerName(event.target.value)} />
                        </Form.Group>

                        <Button variant="primary" className="mt-1" onClick={() => props.joinGame(playerName)}>
                            Join to game
                        </Button>
                    </Form>
                </Col>
            </Row>

        </Container>
    )
}