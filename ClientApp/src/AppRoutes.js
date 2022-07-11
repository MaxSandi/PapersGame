import { GameCreate } from "./components/GameCreate";
import { GameJoin } from "./components/GameJoin";
import { Main } from "./components/Main";

const AppRoutes = [
  {
    index: true,
    element: <Main />
  },
  {
    path: '/game-create',
      element: <GameCreate />
  },
  {
    path: '/game-join',
    element: <GameJoin />
  }
];

export default AppRoutes;
