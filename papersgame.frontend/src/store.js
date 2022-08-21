import { configureStore } from '@reduxjs/toolkit';
import { countReducer } from './redux/reducers/mainPageReducer';

export const store = configureStore({reducer: countReducer});