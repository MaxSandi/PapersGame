import { useEffect, useState } from "react";
import {
    Button,
    Col,
    Row,
    Container,
    Form,
    ListGroup,
    ListGroupItem,
    InputGroup,
} from "react-bootstrap";
import { Card } from "react-bootstrap";
import { useParams, useNavigate } from "react-router-dom";
import Cookies from "universal-cookie";
import "./LobbyPage.css";

export default function LobbyPage(props) {
    let { id } = useParams();

    const connection = props.connection;
    const currentGame = props.currentGame;
    const currentPlayerConnectionId = props.currentPlayerConnectionId;

    const navigate = useNavigate();
    const [players, setPlayers] = useState([]);
    const [currentPlayer, setCurrentPlayer] = useState(0);

    const [characterName, setCharacterName] = useState("");
    const [isPlayerReady, setIsPlayerReady] = useState(false);
    const [isPlayerAdmin, setIsPlayerAdmin] = useState(false);
    const [lobbyStatus, setLobbyStatus] = useState("preparing");
    const cookies = new Cookies();

    useEffect(() => {
        if (currentGame) {
            setPlayers(currentGame.players);

            if (
                currentGame.players.find((p) => p.connectionId == currentPlayerConnectionId)
                    ?.isReady
            ) {
                setIsPlayerReady(true);
            }

            if (currentGame.isStarted) {
                setLobbyStatus("running");
            }
        }

        connection
            ?.invoke("CheckPlayerIsAdmin", currentPlayerConnectionId)
            .then((res) => {
                setIsPlayerAdmin(res);
            });

        connection?.on("ReceivePlayersList", (p) => {
            
            setPlayers(p);
            console.log("GameHub - ReceivePlayersList");
        });

        connection?.on("ReceiveCurrentPlayer", (p) => {
            setCurrentPlayer(p);
            console.log("GameHub - ReceiveCurrentPlayer");
        });

        connection?.on("IGameStarted", (game) => {
            setPlayers(game.players);
            setLobbyStatus("running");

            console.log("GameHub - IGameStarted");
        });

        connection?.on("IGameStopped", (_) => {
            cookies.remove("gameConnection");
            props.setCurrentGame(null);
            navigate("/", {
                replace: true,
            });
            console.log("GameHub - IGameStopped");
        });
    }, []);

    useEffect(() => {
        if (currentGame?.players) {
            setPlayers(currentGame.players);
        }
    }, [currentGame]);

    const playersList = () => {
        const currentPlayers = players;
        const list = currentPlayers.map((player, index) => (
            <ListGroupItem key={player.name}>
                <div className="d-flex justify-content-between">
                    {player.name}
                    {player.isReady ? <div>✔</div> : <div />}
                </div>
            </ListGroupItem>
        ));

        return <ListGroup>{list}</ListGroup>;
    };

    const playerBoard = () => {
        const currentPlayers = players;
        const list = currentPlayers.map((player, index) => (
            <Col key={player.name}>
                {index == currentPlayer ? (
                    <Card border="success">
                        <Card.Body>
                            <Card.Title>{player.name}</Card.Title>
                            <Card.Text>
                                {player.connectionId ==
                                currentPlayerConnectionId
                                    ? "?"
                                    : player.character}
                            </Card.Text>
                        </Card.Body>
                    </Card>
                ) : (
                    <Card>
                        <Card.Body>
                            <Card.Title>{player.name}</Card.Title>
                            <Card.Text>
                                {player.connectionId ==
                                currentPlayerConnectionId
                                    ? "?"
                                    : player.character}
                            </Card.Text>
                        </Card.Body>
                    </Card>
                )}
            </Col>
        ));

        return <Row xs={3}>{list}</Row>;
    };

    const setPlayerReady = async (characterName) => {
        try {
            console.log("setPlayerReady id", id);
            console.log(currentPlayerConnectionId);
            await connection.invoke(
                "SetPlayerReady",
                characterName,
                currentPlayerConnectionId
            );
            console.log("GameHub - SetPlayerReady");

            setIsPlayerReady(true);
        } catch (e) {
            console.log(e);
        }
    };

    const canStartGame = () => {
        try {
            var result = players.every((x) => x.isReady);
            console.log("canStartGame");
            console.log(result);

            return result;
        } catch (e) {
            console.log(e);
        }
    };

    const setPlayerUnready = async () => {
        try {
            await connection.invoke(
                "SetPlayerUnready",
                currentPlayerConnectionId
            );
            console.log("GameHub - SetPlayerUnready");

            setIsPlayerReady(false);
        } catch (e) {
            console.log(e);
        }
    };

    const startGame = async () => {
        try {
            await connection.invoke("StartGame", currentGame.id);
            console.log("GameHub - StartGame");
        } catch (e) {
            console.log(e);
        }
    };

    const stopGame = async () => {
        try {
            await connection.invoke("StopGame", currentGame.id);
            console.log("GameHub - StopGame");
        } catch (e) {
            console.log(e);
        }
    };

    const setTurnNext = async () => {
        try {
            await connection.invoke("SetTurnNext", id);
            console.log("GameHub - SetTurnNext");
        } catch (e) {
            console.log(e);
        }
    };

    const setTurnPrev = async () => {
        try {
            await connection.invoke("SetTurnPrev", id);
            console.log("GameHub - SetTurnPrev");
        } catch (e) {
            console.log(e);
        }
    };

    return (
        <div>
            {!currentGame ? (
                <div>
                    <h3>Id: {id}. Game does not exist!</h3>
                </div>
            ) : (
                <div>
                    <h3>
                        Game name: {currentGame.name}. Id {currentGame.id}
                    </h3>
                    {
                        {
                            preparing: (
                                <Container fluid>
                                    <Row>
                                        <Col>
                                            <h4>Список игроков:</h4>
                                            {playersList()}
                                        </Col>

                                        <Col>
                                            {isPlayerAdmin ? (
                                                <Row>
                                                    <Button
                                                        variant="danger"
                                                        className="ml-1"
                                                        onClick={() =>
                                                            stopGame()
                                                        }
                                                    >
                                                        Завершить игру
                                                    </Button>
                                                </Row>
                                            ) : (
                                                <div />
                                            )}

                                            <Row>
                                                <h2>
                                                    Chat doesn't exist yet :)
                                                </h2>
                                            </Row>
                                            <Row className="bottom-aligment">
                                                <InputGroup className="mb-3">
                                                    <Form.Label className="mr-3 mt-1">
                                                        Имя персонажа:
                                                    </Form.Label>
                                                    <Form.Control
                                                        name="setCharacter"
                                                        type="text"
                                                        placeholder=""
                                                        value={characterName}
                                                        onChange={(event) =>
                                                            setCharacterName(
                                                                event.target
                                                                    .value
                                                            )
                                                        }
                                                    />
                                                    {!isPlayerReady ? (
                                                        <Button
                                                            variant="outline-success"
                                                            className="ml-1"
                                                            onClick={() =>
                                                                setPlayerReady(
                                                                    characterName
                                                                )
                                                            }
                                                        >
                                                            Готов
                                                        </Button>
                                                    ) : (
                                                        <Button
                                                            variant="success"
                                                            className="ml-1"
                                                            onClick={() =>
                                                                setPlayerUnready()
                                                            }
                                                        >
                                                            Готов
                                                        </Button>
                                                    )}
                                                </InputGroup>
                                                {isPlayerAdmin ? (
                                                    <Button
                                                        className="mt-1"
                                                        disabled={
                                                            !canStartGame()
                                                        }
                                                        onClick={() =>
                                                            startGame()
                                                        }
                                                    >
                                                        Начать игру
                                                    </Button>
                                                ) : (
                                                    <div />
                                                )}
                                            </Row>
                                        </Col>
                                    </Row>
                                </Container>
                            ),
                            running: (
                                <Container fluid>
                                    <Row>{playerBoard()}</Row>
                                    {isPlayerAdmin && (
                                        <Row className="bottom-aligment">
                                            <InputGroup>
                                                <Button
                                                    variant="light"
                                                    className="ml-1"
                                                    style={{ width: "18rem" }}
                                                    onClick={() =>
                                                        setTurnPrev()
                                                    }
                                                >
                                                    Предыдущий ход
                                                </Button>
                                                <Button
                                                    variant="light"
                                                    className="ml-1"
                                                    style={{ width: "18rem" }}
                                                    onClick={() =>
                                                        setTurnNext()
                                                    }
                                                >
                                                    Следующий ход
                                                </Button>
                                                <Button
                                                    variant="danger"
                                                    className="ml-5"
                                                    onClick={() => stopGame()}
                                                >
                                                    Завершить игру
                                                </Button>
                                            </InputGroup>
                                        </Row>
                                    )}
                                </Container>
                            ),
                        }[lobbyStatus]
                    }
                </div>
            )}
        </div>
    );
}
