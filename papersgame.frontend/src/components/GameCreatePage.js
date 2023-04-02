import { useState } from "react";
import { Button, Col, Container, Form, Row } from "react-bootstrap";

export default function GameCreatePage(props) {
    const [playerName, setPlayerName] = useState("Vasia");
    const [gameName, setGameName] = useState("Test game");
    const [playerCount, setplayerCount] = useState(4);

    return (
        <Container>
            <Row className="justify-content-center">
                <Col sm={6}>
                    <Form>
                        <Form.Group controlId="formGameName">
                            <Form.Label>Имя игрока</Form.Label>
                            <Form.Control
                                name="playerName"
                                type="text"
                                placeholder=""
                                value={playerName}
                                onChange={(event) =>
                                    setPlayerName(event.target.value)
                                }
                            />
                        </Form.Group>

                        <Form.Group controlId="formGameName">
                            <Form.Label>Название игры</Form.Label>
                            <Form.Control
                                name="name"
                                type="text"
                                placeholder=""
                                value={gameName}
                                onChange={(event) =>
                                    setGameName(event.target.value)
                                }
                            />
                        </Form.Group>

                        <Form.Group controlId="formPlayerCount">
                            <Form.Label>Количество игроков</Form.Label>
                            <Form.Control
                                name="playerCount"
                                type="number"
                                min="2"
                                max="6"
                                placeholder=""
                                value={playerCount}
                                onChange={(event) =>
                                    setplayerCount(event.target.valueAsNumber)
                                }
                            />
                        </Form.Group>

                        <Button
                            variant="primary"
                            className="mt-1"
                            onClick={() =>
                                props.createGame(
                                    playerName,
                                    gameName,
                                    playerCount
                                )
                            }
                        >
                            Start game
                        </Button>
                    </Form>
                </Col>
            </Row>
        </Container>
    );
}
