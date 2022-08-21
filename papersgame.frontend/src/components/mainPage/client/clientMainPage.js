export const ClientMainPage = ({count, handleIncrementClick, handleDecrementClick }) => {
    return <div>
      <h1>Helloworld React & Redux! {count}</h1>
      <button onClick={handleDecrementClick}>Decrement</button>
      <button onClick={handleIncrementClick}>Increment</button>
    </div>
};

